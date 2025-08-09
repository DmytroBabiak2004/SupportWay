using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportWay.Data.Models
{
    public class PaymentStatus
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string NameOfStatus { get; set; }
        public ICollection<Payment> Payments { get; set; }
    }
}
