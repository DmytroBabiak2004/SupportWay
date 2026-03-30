using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportWay.Data.Models;

namespace SupportWay.Data.Configurations
{
    public class HelpRequestConfiguration : IEntityTypeConfiguration<HelpRequest>
    {
        public void Configure(EntityTypeBuilder<HelpRequest> builder)
        {
            // Існуючий зв'язок з локацією — залишаємо
            builder.HasOne(r => r.Location)
                   .WithMany()
                   .HasForeignKey(r => r.LocationId)
                   .OnDelete(DeleteBehavior.SetNull);

            // Нові поля
            builder.Property(r => r.TargetAmount)
                   .HasColumnType("numeric(18,2)")
                   .HasDefaultValue(0m);

            builder.Property(r => r.CollectedAmount)
                   .HasColumnType("numeric(18,2)")
                   .HasDefaultValue(0m);

            builder.Property(r => r.IsActive)
                   .HasDefaultValue(true);

            // Складений індекс для швидкої фільтрації на карті
            builder.HasIndex(r => new { r.IsActive, r.CreatedAt });
        }
    }
}