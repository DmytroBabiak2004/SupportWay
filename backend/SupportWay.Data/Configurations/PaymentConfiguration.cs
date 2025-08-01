using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportWay.Data.Models;

namespace SupportWay.Data.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasOne(p => p.HelpRequest)
                .WithMany(r => r.Payments)
                .HasForeignKey(p => p.HelpRequestId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(p => p.PaymentStatus)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.PaymentStatusId);

            builder.HasOne(p => p.PaymentProvider)
                .WithMany(pp => pp.Payments)
                .HasForeignKey(p => p.PaymentProviderId);
            
            builder.HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId);
        }
    }
}
