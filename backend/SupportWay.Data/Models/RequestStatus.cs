using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportWay.Data.Models
{
    public class RequestStatus
    {
        [Key]
        public Guid Id { get; set; }
        [MaxLength(100)]
        public string NameOfStatus { get; set; }
    }
}
