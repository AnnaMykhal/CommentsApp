using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CommentsApp.Entities;
using Microsoft.AspNetCore.Identity;


namespace CommentsApp.Data;
public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public DbSet<Comment> Comments { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    { }
}