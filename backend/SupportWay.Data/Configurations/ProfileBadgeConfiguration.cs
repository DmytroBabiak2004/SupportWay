using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportWay.Data.Models;

namespace SupportWay.Data.Configurations
{
    public class ProfileBadgeConfiguration : IEntityTypeConfiguration<ProfileBadge>
    {
        public void Configure(EntityTypeBuilder<ProfileBadge> builder)
        {
            builder.HasOne(pb => pb.Profile)
                   .WithMany(p => p.ProfileBadges)
                   .HasForeignKey(pb => pb.ProfileId);

            builder.HasOne(pb => pb.Badge)
                   .WithMany(b => b.ProfileBadges)
                   .HasForeignKey(pb => pb.BadgeId);
        }
    }
}