using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportWay.Data.Models;

namespace SupportWay.Data.Configurations
{
    public class ProfileRatingConfiguration : IEntityTypeConfiguration<ProfileRating>
    {
        public void Configure(EntityTypeBuilder<ProfileRating> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Value)
                   .IsRequired();

            builder.Property(x => x.RatedAt)
                   .IsRequired();

            builder.HasOne(x => x.RaterUser)
                   .WithMany()
                   .HasForeignKey(x => x.RaterUserId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.RatedProfile)
                   .WithMany(p => p.ProfileRatings)
                   .HasForeignKey(x => x.RatedProfileId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => new { x.RaterUserId, x.RatedProfileId })
                   .IsUnique();
        }
    }

}
