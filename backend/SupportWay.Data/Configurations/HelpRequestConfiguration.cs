using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using SupportWay.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SupportWay.Data.Configurations
{
    public class HelpRequestConfiguration : IEntityTypeConfiguration<HelpRequest>
    {
        public void Configure(EntityTypeBuilder<HelpRequest> builder)
        {
            builder.HasOne(r => r.User)
                .WithMany(u => u.HelpRequests)
                .HasForeignKey(r => r.CreatedById);

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
