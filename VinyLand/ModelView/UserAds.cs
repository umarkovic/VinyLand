using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VinyLand.Models;

namespace VinyLand.ModelView
{
    public class UserAds
    {
        public List<Ad> Ads { get; set; }
        public User User { get; set; }
        public int Following { get; set; }
        public int Followers { get; set; }
        public int OfferNum { get; set; }
        public string FavGenre { get; set; }
        public List<Comment> Comments { get; set; }
        public Comment comm { get; set; }
    }
}