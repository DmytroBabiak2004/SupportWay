using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportWay.Data.Models
{
    public class User : IdentityUser
    {
        public ICollection<Profile> Profiles { get; set; }
        public ICollection<HelpRequest> HelpRequests { get; set; }
        public ICollection<Payment> Payments { get; set; }
    }
}