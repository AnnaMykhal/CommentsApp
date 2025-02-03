using System.Net;
using Microsoft.EntityFrameworkCore;
using CommentsApp.Data;
using CommentsApp.Entities;
using Microsoft.AspNetCore.Identity;
using CommentsApp.Services;
using CommentsApp.Services.RabbitMQ;
using CommentsApp.Services.BackgroundTasks;
using CommentsApp.Interfaces;
using CommentsApp.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using SixLabors.ImageSharp;
using RabbitMQ.Client;
using Microsoft.Extensions.FileProviders;


var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(IPAddress.Any, 5017); // Використовуємо порт 5017
});

// Configure services
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
}

ConfigureMiddleware(app);

app.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    var jwtSettingsSection = configuration.GetSection("JwtSettings");
    var jwtSettings = jwtSettingsSection.Get<JwtSettings>();

    services.Configure<JwtSettings>(jwtSettingsSection);

    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    services.AddIdentity<User, IdentityRole<Guid>>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

    services.AddCors(options =>
    {
        options.AddPolicy("AllowAngular",
            policy =>
            {
                policy.WithOrigins("http://localhost:4200") 
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
    });

    // Налаштування RabbitMQ через DI
    services.AddSingleton<IConnection>(sp =>
    {
        var factory = new ConnectionFactory()
        {
            HostName = "rabbitmq_container",  
            Port = 5672,            
            UserName = "guest",     
            Password = "guest",     
            RequestedConnectionTimeout = TimeSpan.FromSeconds(30),
            HandshakeContinuationTimeout = TimeSpan.FromSeconds(30),
            ContinuationTimeout = TimeSpan.FromSeconds(30)
        };
        return factory.CreateConnection();  // Підключення до RabbitMQ
    });

    services.AddSingleton<IModel>(sp =>
    {
        var connection = sp.GetRequiredService<IConnection>();
        return connection.CreateModel();  // Створення каналу для обміну повідомленнями
    });

    // Додавання вашого сервісу для обробки черги
    services.AddSingleton<FileProcessingQueue>();


    services.AddHttpContextAccessor();

    services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
    });


    services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

    services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),

            NameClaimType = ClaimTypes.NameIdentifier
        };
    })
    .AddCookie();   

    services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminPolicy", policy =>
            policy.RequireRole("Admin"));

        options.AddPolicy("UserPolicy", policy =>
            policy.RequireRole("User", "Admin"));
    });

    services.Configure<PasswordOptions>(options =>
    {
        options.RequireDigit = true;
        options.RequireNonAlphanumeric = true;
        options.RequiredLength = 10;
    });

    services.AddScoped<TokenService>();
    services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
    
    services.AddScoped<IFileService, FileService>();
    services.AddScoped<ICommentService, CommentService>();
    services.AddScoped<IUserService, UserService>();
    services.AddSingleton<FileProcessingQueue>();
    services.AddSingleton<FileProcessorService>();


    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

   
    services.AddRazorPages();
}

void ConfigureMiddleware(WebApplication app)
{
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("AllowAngular");

    app.UseHttpsRedirection();
    //app.UseStaticFiles();
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
        RequestPath = "/profileImages"
    });
    app.UseRouting();

    app.Use(async (context, next) =>
    {
        Console.WriteLine($"Request path: {context.Request.Path}");
        await next.Invoke();
    });
    
    app.UseSession();

    app.UseCookiePolicy(new CookiePolicyOptions
    {
        MinimumSameSitePolicy = SameSiteMode.Lax,
        Secure = CookieSecurePolicy.Always,
    });

    /// Запуск обробки файлів на фоні
    var fileProcessorService = app.Services.GetRequiredService<FileProcessorService>();
    fileProcessorService.StartProcessing(); 

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapRazorPages();
    app.MapControllers();
}


