using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportWay.Data.Models;

namespace SupportWay.Data.Configurations
{
    public class BadgeTypeConfiguration : IEntityTypeConfiguration<BadgeType>
    {
        public void Configure(EntityTypeBuilder<BadgeType> builder)
        {
            builder.HasKey(bt => bt.Id);
        }
    }
}