using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportWay.Data.Models
{
    public class PaymentStatus
    {
        public int Id { get; set; }
        public string NameOfStatus { get; set; }
        public ICollection<Payment> Payments { get; set; }
    }
}
