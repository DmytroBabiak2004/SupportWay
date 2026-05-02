using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportWay.Data.Models;

namespace SupportWay.Data.Configurations
{
    public class HelpRequestConfiguration : IEntityTypeConfiguration<HelpRequest>
    {
        public void Configure(EntityTypeBuilder<HelpRequest> builder)
        {
            builder.HasOne(r => r.Location)
                   .WithMany()
                   .HasForeignKey(r => r.LocationId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.Property(r => r.TargetAmount)
                   .HasColumnType("numeric(18,2)")
                   .HasDefaultValue(0m);

            builder.Property(r => r.CollectedAmount)
                   .HasColumnType("numeric(18,2)")
                   .HasDefaultValue(0m);

            builder.Property(r => r.IsActive)
                   .HasDefaultValue(true);

            builder.Property(r => r.PreferredDonationMethod)
                   .HasMaxLength(64);

            builder.Property(r => r.DonationRecipientName)
                   .HasMaxLength(200);

            builder.Property(r => r.DonationRecipientCardNumber)
                   .HasMaxLength(32);

            builder.Property(r => r.DonationRecipientIban)
                   .HasMaxLength(64);

            builder.Property(r => r.DonationPaymentLink)
                   .HasMaxLength(500);

            builder.Property(r => r.DonationNotes)
                   .HasMaxLength(1000);

            builder.HasIndex(r => new { r.IsActive, r.CreatedAt });
        }
    }
}
