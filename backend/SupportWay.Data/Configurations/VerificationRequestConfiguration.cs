using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportWay.Data.Models;

namespace SupportWay.Data.Configurations
{
    public class VerificationRequestConfiguration : IEntityTypeConfiguration<VerificationRequest>
    {
        public void Configure(EntityTypeBuilder<VerificationRequest> builder)
        {
            builder.HasKey(v => v.Id);

            builder.HasOne(v => v.User)
                   .WithMany()
                   .HasForeignKey(v => v.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(v => new { v.UserId, v.Status });

            builder.Property(v => v.VerificationType)
                   .HasConversion<int>();

            builder.Property(v => v.Status)
                   .HasConversion<int>();
        }
    }
}