using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportWay.Data.Models
{
    public class RequestItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public Guid SupportTypeId { get; set; }
        public SupportType SupportType { get; set; }
        public Guid HelpRequestId { get; set; }
        public HelpRequest HelpRequest { get; set; }
    }

}
