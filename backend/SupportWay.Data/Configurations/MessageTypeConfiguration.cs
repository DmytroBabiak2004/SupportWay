using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportWay.Data.Models;

namespace SupportWay.Data.Configurations
{
    public class MessageTypeConfiguration : IEntityTypeConfiguration<MessageType>
    {
        public void Configure(EntityTypeBuilder<MessageType> builder)
        {
            builder.Property(x => x.NameOfType)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasMany(mt => mt.Messages)
                .WithOne(m => m.MessageType)
                .HasForeignKey(m => m.MessageTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
