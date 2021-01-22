using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VinyLand.Models
{
    public class Offer
    {
        public string RecordTitleB { get; set; }
        public string RecordTitleS { get; set; }
        public string Type { get; set; }

        public string Status { get; set; }
        public float Price { get; set; }
        public User Buyer { get; set; }
        public int Id { get; set; }
    }
}