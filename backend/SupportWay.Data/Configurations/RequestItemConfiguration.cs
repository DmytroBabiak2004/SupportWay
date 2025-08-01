using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportWay.Data.Models;

namespace SupportWay.Data.Configurations
{
    public class RequestItemConfiguration : IEntityTypeConfiguration<RequestItem>
    {
        public void Configure(EntityTypeBuilder<RequestItem> builder) 
        {
            builder.HasOne(i => i.HelpRequest)
                .WithMany(r => r.RequestItems)
                .HasForeignKey(i => i.HelpRequestId);

            builder.HasOne(i => i.SupportType)
                .WithMany()
                .HasForeignKey(i => i.SupportTypeId);
        }
    }
}
