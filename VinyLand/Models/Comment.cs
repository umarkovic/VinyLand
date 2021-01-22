using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VinyLand.Models
{
    public class Comment
    {
        public string Content { get; set; }
        public User Subject { get; set; }
        public User Object { get; set; }
        public DateTime DateTime { get; set; }
       
    }
}