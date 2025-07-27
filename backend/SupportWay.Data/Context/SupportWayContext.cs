using SupportWay.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SupportWay.Data.Context
{
    public class SupportWayContext : IdentityDbContext<IdentityUser>

    {
        public SupportWayContext(DbContextOptions<SupportWayContext> options) : base(options) { }     
        
        public DbSet<HelpRequest> HelpRequests { get; set; }
        public DbSet<HelpType> HelpTypes { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<HelpRequest>()
                .HasKey(h => h.Id);

            modelBuilder.Entity<HelpType>()
                .HasKey(t => t.Id);

         

            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "1", Name = "Military", NormalizedName = "MILITARY" },
                new IdentityRole { Id = "2", Name = "Volunteer", NormalizedName = "VOLUNTEER" },
                new IdentityRole { Id = "3", Name = "User", NormalizedName = "USER" },
                new IdentityRole { Id = "4", Name = "Admin", NormalizedName = "ADMIN" }
            );
        }
    }
}
