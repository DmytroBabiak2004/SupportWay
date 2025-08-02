using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportWay.Data.Models
{
    public class HelpRequest : Post
    {
        public int LocationId { get; set; }
        public Location Location { get; set; }
        public int RequestStatusId { get; set; }
        public RequestStatus RequestStatus { get; set; }
        public ICollection<RequestItem> RequestItems { get; set; }
        public ICollection<Payment> Payments { get; set; }

    }

}
