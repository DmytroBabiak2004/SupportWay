using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportWay.Data.Models
{
    public class HelpRequest
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal TargetAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedById { get; set; }
        public User User { get; set; }
        public int? LocationId { get; set; }
        public Location Location { get; set; }
        public int RequestStatusId { get; set; }
        public RequestStatus RequestStatus { get; set; }
        public ICollection<RequestItem> RequestItems { get; set; }
        public ICollection<Payment> Payments { get; set; }
        public ICollection<PostLike> Likes { get; set; }
        public ICollection<PostComment> Comments { get; set; }

    }

}
