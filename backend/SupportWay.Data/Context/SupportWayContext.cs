using SupportWay.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Data;

namespace SupportWay.Data.Context
{
    public class SupportWayContext : IdentityDbContext<User>

    {
        public SupportWayContext(DbContextOptions<SupportWayContext> options) : base(options) { }

        public DbSet<Profile> Profiles { get; set; }
        public DbSet<HelpRequest> HelpRequests { get; set; }
        public DbSet<RequestItem> RequestItems { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<RequestStatus> RequestStatuses { get; set; }
        public DbSet<SupportType> SupportTypes { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentProvider> PaymentProviders { get; set; }
        public DbSet<PaymentStatus> PaymentStatuses { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User - Profile 1:M
            modelBuilder.Entity<Profile>()
                .HasOne(p => p.User)
                .WithMany(u => u.Profiles)
                .HasForeignKey(p => p.UserId);

            // User - HelpRequest 1:M
            modelBuilder.Entity<HelpRequest>()
                .HasOne(r => r.User)
                .WithMany(u => u.HelpRequests)
                .HasForeignKey(r => r.CreatedById);

            // User - Payment 1:M
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId);

            // HelpRequest - Location 1:1
            modelBuilder.Entity<HelpRequest>()
                .HasOne(r => r.Location)
                .WithMany()
                .HasForeignKey(r => r.LocationId)
                .OnDelete(DeleteBehavior.SetNull);

            // HelpRequest - RequestStatus
            modelBuilder.Entity<HelpRequest>()
                .HasOne(r => r.RequestStatus)
                .WithMany()
                .HasForeignKey(r => r.RequestStatusId);

            // RequestItem - HelpRequest
            modelBuilder.Entity<RequestItem>()
                .HasOne(i => i.HelpRequest)
                .WithMany(r => r.RequestItems)
                .HasForeignKey(i => i.HelpRequestId);

            // RequestItem - SupportType
            modelBuilder.Entity<RequestItem>()
                .HasOne(i => i.SupportType)
                .WithMany()
                .HasForeignKey(i => i.SupportTypeId);

            // Payment - HelpRequest
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.HelpRequest)
                .WithMany(r => r.Payments)
                .HasForeignKey(p => p.HelpRequestId)
                .OnDelete(DeleteBehavior.SetNull);

            // Payment - PaymentStatus
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.PaymentStatus)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.PaymentStatusId);

            // Payment - PaymentProvider
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.PaymentProvider)
                .WithMany(pp => pp.Payments)
                .HasForeignKey(p => p.PaymentProviderId);

            // ChatMessage - Conversation
            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId);

            // Conversation - Participants (many-to-many via linking table)
            modelBuilder.Entity<Conversation>()
                .HasMany<User>(/* navigation */)
                .WithMany(/* navigation */)
                .UsingEntity(j => j.ToTable("UserConversations"));


        }
    }
}
