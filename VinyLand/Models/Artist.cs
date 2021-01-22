using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VinyLand.Models
{
    public class Artist
    {
        [Required(ErrorMessage = "This field is required")]
        public string Name { get;  set;}
        public string Country { get; set; }
        public string ImageURL { get; set; }
        public List<Record> Discography { get; set; }
        public Genre Genre { get; set; }
    }
}