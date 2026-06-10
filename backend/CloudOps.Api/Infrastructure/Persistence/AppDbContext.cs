using CloudOps.Api.Modules.Notifications.Models;
using CloudOps.Api.Modules.Requests.Models;
using CloudOps.Api.Modules.Services.Models;
using CloudOps.Api.Modules.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudOps.Api.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        public DbSet<ServiceItem> Services => Set<ServiceItem>();

        public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();

        public DbSet<Notification> Notifications => Set<Notification>();
    }
}
