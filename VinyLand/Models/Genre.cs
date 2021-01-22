using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VinyLand.Models
{
    public class Genre
    {
        [Required(ErrorMessage = "This field is required")]
        public string genre { get; set; }
        
    }
}