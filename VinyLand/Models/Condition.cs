using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VinyLand.Models
{
    public class Condition
    {
        public Record Record { get; set; }
        public User User { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public string condition { get; set; }
        
        public bool Replacement { get; set; }
    }
}