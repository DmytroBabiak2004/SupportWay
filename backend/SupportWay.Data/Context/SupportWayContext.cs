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
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<PostComment> PostComments { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentProvider> PaymentProviders { get; set; }
        public DbSet<PaymentStatus> PaymentStatuses { get; set; }
        public DbSet<Chat> Conversations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Profile>()
                .HasOne(p => p.User)
                .WithMany(u => u.Profiles)
                .HasForeignKey(p => p.UserId);

            modelBuilder.Entity<HelpRequest>()
                .HasOne(r => r.User)
                .WithMany(u => u.HelpRequests)
                .HasForeignKey(r => r.CreatedById);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId);

            modelBuilder.Entity<HelpRequest>()
                .HasOne(r => r.Location)
                .WithMany()
                .HasForeignKey(r => r.LocationId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<HelpRequest>()
                .HasOne(r => r.RequestStatus)
                .WithMany()
                .HasForeignKey(r => r.RequestStatusId);

            modelBuilder.Entity<RequestItem>()
                .HasOne(i => i.HelpRequest)
                .WithMany(r => r.RequestItems)
                .HasForeignKey(i => i.HelpRequestId);

            modelBuilder.Entity<RequestItem>()
                .HasOne(i => i.SupportType)
                .WithMany()
                .HasForeignKey(i => i.SupportTypeId);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.HelpRequest)
                .WithMany(r => r.Payments)
                .HasForeignKey(p => p.HelpRequestId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.PaymentStatus)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.PaymentStatusId);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.PaymentProvider)
                .WithMany(pp => pp.Payments)
                .HasForeignKey(p => p.PaymentProviderId);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Chat)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatId);

            modelBuilder.Entity<Chat>()
                .HasMany<User>()
                .WithMany()
                .UsingEntity(j => j.ToTable("UserChats"));

            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId);

            modelBuilder.Entity<Follow>()
                .HasKey(f => new { f.FollowerId, f.FollowedId });

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Follower)
                .WithMany(u => u.Followings)
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Followed)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FollowedId)
                .OnDelete(DeleteBehavior.Restrict);

           modelBuilder.Entity<PostLike>()
                .HasKey(pl => pl.Id);

            modelBuilder.Entity<PostLike>()
                .HasOne(pl => pl.User)
                .WithMany()
                .HasForeignKey(pl => pl.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PostLike>()
                .HasOne(pl => pl.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(pl => pl.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PostLike>()
                .HasOne(pl => pl.HelpRequest)
                .WithMany()
                .HasForeignKey(pl => pl.RequestId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PostComment>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<PostComment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PostComment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PostComment>()
                .HasOne(c => c.Request)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
