using System;
using System.ComponentModel.DataAnnotations;

namespace SupportWay.Data.Models
{
    public class BadgeType
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}