using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportWay.Data.Models;

namespace SupportWay.Data.Configurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasOne(m => m.Chat)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(m => m.Content)
                .HasMaxLength(4000)
                .HasDefaultValue(string.Empty);

            builder.Property(m => m.MessageType)
                .HasConversion<int>();

            builder.HasIndex(m => m.ChatId);
            builder.HasIndex(m => m.SharedPostId);
            builder.HasIndex(m => m.SharedHelpRequestId);
        }
    }
}