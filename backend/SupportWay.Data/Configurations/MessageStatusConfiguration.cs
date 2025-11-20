using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportWay.Data.Models;

namespace SupportWay.Data.Configurations
{
    public class MessageStatusConfiguration : IEntityTypeConfiguration<MessageStatus>
    {
        public void Configure(EntityTypeBuilder<MessageStatus> builder)
        {
            builder.HasOne(ms => ms.Message)
                .WithMany(m => m.Statuses)
                .HasForeignKey(ms => ms.Id)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
