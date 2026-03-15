using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportWay.Data.Models;

namespace SupportWay.Data.Configurations
{
    public class BadgeConfiguration : IEntityTypeConfiguration<Badge>
    {
        public void Configure(EntityTypeBuilder<Badge> builder)
        {
            // Зв'язок: Один BadgeType має багато Badge
            builder.HasOne(b => b.BadgeType)
                   .WithMany() // Залишаємо порожнім, бо в класі BadgeType немає колекції бейджів
                   .HasForeignKey(b => b.BadgeTypeId);
        }
    }
}