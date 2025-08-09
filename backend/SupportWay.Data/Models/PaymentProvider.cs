using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportWay.Data.Models
{
    public class PaymentProvider
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string NameOfProvider { get; set; }

        public ICollection<Payment> Payments { get; set; }
    }

}
