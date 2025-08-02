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

            builder.HasOne(r => r.RequestStatus)
                .WithMany()
                .HasForeignKey(r => r.RequestStatusId);
        }
    }
}
