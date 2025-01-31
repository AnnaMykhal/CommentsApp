using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CommentsApp.Entities;
using Microsoft.AspNetCore.Identity;


namespace CommentsApp.Data;
public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public DbSet<Comment> Comments { get; set; }
    public DbSet<User> Users { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Зв'язок між коментарем і користувачем
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.User)  // Коментар має одного користувача
            .WithMany(u => u.Comments)  // Користувач має багато коментарів
            .HasForeignKey(c => c.UserId);  // Зовнішній ключ до користувача

        // Зв'язок між коментарем і батьківським коментарем
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.ParentComment)  // Коментар має один батьківський коментар
            .WithMany(p => p.Replies)  // Батьківський коментар має багато відповідей
            .HasForeignKey(c => c.ParentCommentId)  // Зовнішній ключ до батьківського коментаря
            .OnDelete(DeleteBehavior.Restrict);  // Уникнути видалення батьківського коментаря, якщо є відповіді

        // Настроювання для IdentityUserRole<Guid>
        modelBuilder.Entity<IdentityUserRole<Guid>>()
            .HasKey(r => new { r.UserId, r.RoleId });

        // Настроювання для IdentityUserLogin<Guid>
        modelBuilder.Entity<IdentityUserLogin<Guid>>()
            .HasKey(l => new { l.UserId, l.LoginProvider, l.ProviderKey });

        // Настроювання для IdentityUserClaim<Guid>
        modelBuilder.Entity<IdentityUserClaim<Guid>>()
            .HasKey(c => c.Id);

        // Настроювання для IdentityUserToken<Guid>
        modelBuilder.Entity<IdentityUserToken<Guid>>()
            .HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

        // Настроювання для IdentityRole<Guid>
        modelBuilder.Entity<IdentityRole<Guid>>()
            .HasKey(r => r.Id);
    }
}