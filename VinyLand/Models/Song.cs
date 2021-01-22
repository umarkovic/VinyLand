using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VinyLand.Models
{
    public class Song
    {
        
        public string NameOfSong { get; set; }
        public string Duration { get; set; }
    }
}