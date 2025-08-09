using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportWay.Data.Models
{
    public class Payment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TransactionId { get; set; }
        public string Comment { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public Guid? HelpRequestId { get; set; }
        public HelpRequest HelpRequest { get; set; }
        public Guid PaymentStatusId { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public Guid PaymentProviderId { get; set; }
        public PaymentProvider PaymentProvider { get; set; }
    }

}
