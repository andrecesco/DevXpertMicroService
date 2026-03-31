using EduOnline.Auth.ApiRest.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EduOnline.Auth.ApiRest.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<EduOnlineUser>(options)
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }
}
