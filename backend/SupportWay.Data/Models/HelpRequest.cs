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
      [Key]
      public int Id { get; set; }
        [MaxLength(50)]
    public int HelpTypeId { get; set; }
    public HelpType HelpType { get; set; }

    }
}
