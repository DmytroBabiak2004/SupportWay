using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportWay.Data.Models;

namespace SupportWay.Data.Configurations
{
    public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
    {
        public void Configure(EntityTypeBuilder<Profile> builder)
        {
            builder.HasOne(p => p.User)
                   .WithOne(u => u.Profile)
                   .HasForeignKey<Profile>(p => p.UserId);
            builder.Property(p => p.IsVerified).HasDefaultValue(false);
            builder.Property(p => p.VerifiedAs).HasConversion<int?>();
        }
    }
}
