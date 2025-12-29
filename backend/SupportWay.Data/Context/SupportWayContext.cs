using SupportWay.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SupportWay.Data.Configurations;

namespace SupportWay.Data.Context
{
    public class SupportWayContext : IdentityDbContext<User>
    {
        public SupportWayContext(DbContextOptions<SupportWayContext> options) : base(options) { }
        public DbSet<DefaultAvatar> DefaultAvatars { get; set; }
        public DbSet<ProfileRating> ProfileRatings { get; set; }
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
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<MessageStatus> MessageStatuses { get; set; }
        public DbSet<MessageType> MessageTypes { get; set; }
        public DbSet<UserChat> UserChats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ChatConfiguration());
            modelBuilder.ApplyConfiguration(new UserChatConfiguration());
            modelBuilder.ApplyConfiguration(new ChatMessageConfiguration());
            modelBuilder.ApplyConfiguration(new FollowConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentConfiguration());
            modelBuilder.ApplyConfiguration(new PostConfiguration());
            modelBuilder.ApplyConfiguration(new HelpRequestConfiguration());
            modelBuilder.ApplyConfiguration(new PostLikeConfiguration());
            modelBuilder.ApplyConfiguration(new PostCommentConfiguration());
            modelBuilder.ApplyConfiguration(new ProfileConfiguration());
            modelBuilder.ApplyConfiguration(new RequestItemConfiguration());
            modelBuilder.ApplyConfiguration(new ProfileRatingConfiguration());
            modelBuilder.ApplyConfiguration(new MessageTypeConfiguration());
            modelBuilder.ApplyConfiguration(new MessageStatusConfiguration());

            modelBuilder.Entity<IdentityRole>().HasData(
              new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
              new IdentityRole { Id = "2", Name = "User", NormalizedName = "USER" },
              new IdentityRole { Id = "3", Name = "Volunteer", NormalizedName = "VOLUNTEER" },
              new IdentityRole { Id = "4", Name = "Military", NormalizedName = "MILITARY" }
          );
        }
    }
}
