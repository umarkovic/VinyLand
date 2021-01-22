using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VinyLand.Models
{
    public class Label
    {
        [Required(ErrorMessage = "This field is required")]
        public string NameOfLabel { get; set; }
        public string Country { get; set; }
        public List<Record> Records { get; set; }
        public List<Record> Releases { get; set; }
    }
}