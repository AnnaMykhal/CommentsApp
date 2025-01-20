using System.Net;
using Microsoft.EntityFrameworkCore;
using CommentsApp;
using System;
using CommentsApp.Data;
using CommentsApp.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();



builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(IPAddress.Any, 5017); // Використовуємо порт 5017
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
