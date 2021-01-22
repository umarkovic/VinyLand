using Neo4jClient;
using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VinyLand.Models;
using Neo4j.AspNet.Identity;
using System.Security.Principal;
using VinyLand.ModelView;
using Microsoft.Ajax.Utilities;
using System.IO;

namespace VinyLand.Controllers
{
    public class UserController : Controller
    {
        private GraphClient client;

        public string GlobalKey { get; private set; }

        public ActionResult UserSignedUp()
        {
            return View();
        }
        public UserController()
        {
            client = DataLayer.GetSession();
        }

        public ActionResult Index()
        {

            return View();
        }
        [HttpPost]
        public ActionResult AddRecord(Ad ad)
        {
            string recordPicture;
            HttpPostedFileBase objFiles = Request.Files["upload"];
            if (objFiles.ContentLength != 0)
            {
                var filename = Path.GetFileName(objFiles.FileName);
                var path = Path.Combine(Server.MapPath("~/Content/Pictures/"), filename);
                objFiles.SaveAs(path);
            }


            if (!objFiles.FileName.Equals(""))
            {
                recordPicture = "~/Content/Pictures/" + objFiles.FileName;
            }
            else
            {
                recordPicture = "~/Content/Pictures/defaultVinyl1.png";
            }
            doesLabelExist(ad.Label);
            doesArtistExist(ad.Artist);
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            string userName = User.Identity.Name;
            int key = DataLayer.getKey();


            if (doesLabelExist(ad.Label) && doesArtistExist(ad.Artist))
            {
                var query = new Neo4jClient.Cypher.CypherQuery("MATCH (u:User) WHERE u.Email='" + userName + "' MATCH (z:Genre) WHERE z.genre='" + ad.Genre.genre + "' MATCH (l:Label) WHERE l.NameOfLabel='" + ad.Label.NameOfLabel + "' MATCH(a:Artist) WHERE a.Name='" + ad.Artist.Name + "' CREATE (m:Record { Title:'" + ad.Record.Title + "', Id:'" + key + "' , Format:'" + ad.Record.Format + "', Price:'" + ad.Record.Price + "', Picture:'" + recordPicture + "' })<-[ r:SELLING {condition: '" + ad.Condition.condition + "', Replacement: '" + ad.Condition.Replacement + "'}]-(u) Create(a) -[q: OWNS]->(m) Create (l)-[h:RELEASE {Date:'11 / 02 / 2020'}]->(m)  CREATE (z)<-[k:BELONGS_TO]-(m) ", queryDict, CypherResultMode.Set);
                ((IRawGraphClient)client).ExecuteCypher(query);
                foreach (Song song in ad.Songs)
                {
                    if (song.NameOfSong == null) return RedirectToAction("UserProfile", new { email = User.Identity.Name });
                    else
                    {
                        var query1 = new Neo4jClient.Cypher.CypherQuery("match (r:Record) where r.Id='" + key + "' create (s:Song {NameOfSong:'" + song.NameOfSong + "', Duration:'" + song.Duration + "' })<-[v:INCLUDE ]-(r)", new Dictionary<string, object>(), CypherResultMode.Set);
                        ((IRawGraphClient)client).ExecuteCypher(query1);
                    }
                }
            }


            return RedirectToAction("UserProfile", new { email = User.Identity.Name });


        }
        [HttpGet]
        public ActionResult AddRecord()
        {

            Ad ad = new Ad();
            ad.slcond = new List<SelectListItem>();
            ad.slcond.Add(new SelectListItem { Text = "New", Value = "New" });
            ad.slcond.Add(new SelectListItem { Text = "Used", Value = "Used" });

            ad.slgen = new List<SelectListItem>();
            List<string> genres = getAllGenres();
            foreach (string item in genres)
                ad.slgen.Add(new SelectListItem { Text = item, Value = item });
            return View(ad);
        }
        public ActionResult UserProfile(string email)
        {
            var qu = new Neo4jClient.Cypher.CypherQuery("match (u{Email:'" + User.Identity.Name + "'})-[f:FOLLOW]->(m{Email:'" + email + "'}) return true", new Dictionary<string, object>(), CypherResultMode.Set);
            bool b = ((IRawGraphClient)client).ExecuteGetCypherResults<bool>(qu).FirstOrDefault();
            List<Ad> ads = new List<Ad>();
            var query = new Neo4jClient.Cypher.CypherQuery("match(u:User) where u.Email='" + email + "'	 return u", new Dictionary<string, object>(), CypherResultMode.Set);
            User us = ((IRawGraphClient)client).ExecuteGetCypherResults<User>(query).FirstOrDefault();
            var queryy = new Neo4jClient.Cypher.CypherQuery("match(u:User) where u.Email='" + User.Identity.Name + "' return u", new Dictionary<string, object>(), CypherResultMode.Set);
            User usSubject = ((IRawGraphClient)client).ExecuteGetCypherResults<User>(queryy).FirstOrDefault();
            if (b) us.SecurityStamp = "1";
            var qq = new Neo4jClient.Cypher.CypherQuery("match (u:User)-[o:OFFER]->(m{Email:'" + User.Identity.Name + "'}) return count(o)", new Dictionary<string, object>(), CypherResultMode.Set);
            int offernum = ((IRawGraphClient)client).ExecuteGetCypherResults<int>(qq).FirstOrDefault();
            var query1 = new Neo4jClient.Cypher.CypherQuery("match(u{ Email:'" + email + "'})-[SELLING]->(r:Record)   return r", new Dictionary<string, object>(), CypherResultMode.Set);
            List<Record> records = ((IRawGraphClient)client).ExecuteGetCypherResults<Record>(query1).ToList();
            foreach (Record rec in records)
            {
                var q = client.Cypher.Match("(r:Record {Id:'" + rec.Id + "'})<-[RELEASE]-(l:Label),(r:Record {Id:'" + rec.Id + "'})<-[OWNS]-(a:Artist),(r:Record {Id:'" + rec.Id + "'})<-[s:SELLING]-(u:User)  ").Return((l, a, u, s) => new { Label = l.As<Label>(), Artist = a.As<Artist>(), User = u.As<User>(), Condition = s.As<Condition>() }); ;
                foreach (var result in q.Results)
                {
                    ads.Add(new Ad { Artist = result.Artist, Record = rec, Label = result.Label, User = result.User, Songs = getAllRecordSongs(rec.Id), Condition = result.Condition });

                }
            }
            var q1 = new Neo4jClient.Cypher.CypherQuery("match(u{Email:'" + email + "'})-[f:FOLLOW]->() return count(f)", new Dictionary<string, object>(), CypherResultMode.Set);
            int numOfFollowing = ((IRawGraphClient)client).ExecuteGetCypherResults<int>(q1).FirstOrDefault();
            var q2 = new Neo4jClient.Cypher.CypherQuery("match(u{Email:'" + email + "'})<-[f:FOLLOW]-() return count(f)", new Dictionary<string, object>(), CypherResultMode.Set);
            int numOfFollowers = ((IRawGraphClient)client).ExecuteGetCypherResults<int>(q2).FirstOrDefault();
            var q3 = client.Cypher.Match("(u{Email:'" + email + "'})<-[c:COMMENT]-(m)").Return((u, c, m) => new { Object = u.As<User>(), Content = c.As<Comment>(), Subject = m.As<User>() }); ;
            List<Comment> tmp = new List<Comment>();
            foreach (var item in q3.Results)
                tmp.Add(new Comment { Content = item.Content.Content, Object = item.Object, Subject = item.Subject, DateTime = item.Content.DateTime });

            Comment comm = new Comment { Object = us, Subject = usSubject };
            UserAds userAds = new UserAds { Ads = ads, User = us, Followers = numOfFollowers, Following = numOfFollowing, OfferNum = offernum, FavGenre = getFavouriteGenre(email), comm = comm, Comments = tmp };
            return View(userAds);
        }


        public bool doesLabelExist(Label l)
        {
            var query1 = new Neo4jClient.Cypher.CypherQuery("match (l:Label) where l.NameOfLabel='" + l.NameOfLabel + "' return l", new Dictionary<string, object>(), CypherResultMode.Set);
            List<Label> label = ((IRawGraphClient)client).ExecuteGetCypherResults<Label>(query1).ToList();
            if (label.Count == 0)
            {
                var query2 = new Neo4jClient.Cypher.CypherQuery("create (l:Label {NameOfLabel:'" + l.NameOfLabel + "', Country:'" + l.Country + "'})", new Dictionary<string, object>(), CypherResultMode.Set);
                ((IRawGraphClient)client).ExecuteCypher(query2);
                return false;
            }
            else
                return true;
        }

        public bool doesArtistExist(Artist a)
        {
            var query1 = new Neo4jClient.Cypher.CypherQuery("match (a:Artist) where a.Name='" + a.Name + "' return a", new Dictionary<string, object>(), CypherResultMode.Set);
            List<Artist> label = ((IRawGraphClient)client).ExecuteGetCypherResults<Artist>(query1).ToList();
            if (label.Count == 0)
            {
                var query2 = new Neo4jClient.Cypher.CypherQuery("create (a:Artist {Name:'" + a.Name + "'})", new Dictionary<string, object>(), CypherResultMode.Set);
                ((IRawGraphClient)client).ExecuteCypher(query2);
                return false;
            }
            else
                return true;
        }


        public ActionResult GetFeedSuggestions()
        {
            List<Ad> adsFollowing = new List<Ad>();
            List<Ad> ads = new List<Ad>();
            string userEmial = User.Identity.Name;
            var query1 = new Neo4jClient.Cypher.CypherQuery("match(u{Email:'" + userEmial + "'})-[]-(record)-[]->(g:Genre) MATCH (r:Record),(u:User { Email: '" + userEmial + "' }) WHERE NOT (u)-->(r) match (r)-[]->(g) return distinct r", new Dictionary<string, object>(), CypherResultMode.Set);
            List<Record> sugestedRecords = ((IRawGraphClient)client).ExecuteGetCypherResults<Record>(query1).ToList();
            foreach (Record rec in sugestedRecords)
            {
                var q = client.Cypher.Match("(r:Record {Id:'" + rec.Id + "'})<-[RELEASE]-(l:Label),(r:Record {Id:'" + rec.Id + "'})<-[OWNS]-(a:Artist),(r:Record {Id:'" + rec.Id + "'})<-[s:SELLING]-(u:User),(r:Record {Id:'" + rec.Id + "'})-[BELONGS_TO]->(g:Genre)  ").Return((l, a, u, s, g) => new { Label = l.As<Label>(), Artist = a.As<Artist>(), User = u.As<User>(), Condition = s.As<Condition>(), Genre = g.As<Genre>() }); ;
                foreach (var result in q.Results)
                {
                    ads.Add(new Ad { Artist = result.Artist, Record = rec, Label = result.Label, User = result.User, Songs = getAllRecordSongs(rec.Id), Condition = result.Condition, Genre = result.Genre });

                }

            }


            var q1 = new Neo4jClient.Cypher.CypherQuery("match(u{Email:'" + userEmial + "'})-[f:FOLLOW]-(m:User)-[SELLING]-(r:Record) return  r", new Dictionary<string, object>(), CypherResultMode.Set);
            List<Record> records = ((IRawGraphClient)client).ExecuteGetCypherResults<Record>(q1).ToList();
            foreach (Record rec in records)
            {
                var q2 = client.Cypher.Match("(r:Record {Id:'" + rec.Id + "'})<-[RELEASE]-(l:Label),(r:Record {Id:'" + rec.Id + "'})<-[OWNS]-(a:Artist),(r:Record {Id:'" + rec.Id + "'})<-[s:SELLING]-(u:User), (r:Record {Id:'" + rec.Id + "'})-[BELONGS_TO]->(g:Genre)  ").Return((l, a, u, s, g) => new { Label = l.As<Label>(), Artist = a.As<Artist>(), User = u.As<User>(), Condition = s.As<Condition>(), Genre = g.As<Genre>() }); ;
                foreach (var result in q2.Results)
                {
                    ads.Add(new Ad { Artist = result.Artist, Record = rec, Label = result.Label, User = result.User, Songs = getAllRecordSongs(rec.Id), Condition = result.Condition, Genre = result.Genre });

                }

            }
            List<Ad> distinctList = ads.DistinctBy(x => x.Record.Id).ToList();

            var q3 = new Neo4jClient.Cypher.CypherQuery("match(u{Email:'" + userEmial + "'})-[f:FOLLOW]-(m:User)-[SELLING]-(r:Record) return  r", new Dictionary<string, object>(), CypherResultMode.Set);
            List<Record> records2 = ((IRawGraphClient)client).ExecuteGetCypherResults<Record>(q3).ToList();
            foreach (Record rec in records2)
            {
                var q2 = client.Cypher.Match("(r:Record {Id:'" + rec.Id + "'})<-[RELEASE]-(l:Label),(r:Record {Id:'" + rec.Id + "'})<-[OWNS]-(a:Artist),(r:Record {Id:'" + rec.Id + "'})<-[s:SELLING]-(u:User), (r:Record {Id:'" + rec.Id + "'})-[BELONGS_TO]->(g:Genre)  ").Return((l, a, u, s, g) => new { Label = l.As<Label>(), Artist = a.As<Artist>(), User = u.As<User>(), Condition = s.As<Condition>(), Genre = g.As<Genre>() }); ;
                foreach (var result in q2.Results)
                {
                    adsFollowing.Add(new Ad { Artist = result.Artist, Record = rec, Label = result.Label, User = result.User, Songs = getAllRecordSongs(rec.Id), Condition = result.Condition, Genre = result.Genre });

                }

            }
            List<Ad> distinctList2 = adsFollowing.DistinctBy(x => x.Record.Id).ToList();
            HomeSuggestions hs = new HomeSuggestions();
            hs.Ads = distinctList;
            hs.AdsFollowing = distinctList2;

            return View(hs);
        }
        public ActionResult FollowUser(string idObject)
        {
            string idSubject = User.Identity.Name;
            var query1 = new Neo4jClient.Cypher.CypherQuery("match (u:User),(m:User) where u.Email='" + idSubject + "' and  m.Email='" + idObject + "'  create (u)-[f:FOLLOW]->(m) ", new Dictionary<string, object>(), CypherResultMode.Set);
            ((IRawGraphClient)client).ExecuteCypher(query1);
            return RedirectToAction("UserProfile", new { email = idObject });
        }
        public ActionResult UnFollowUser(string idObject)
        {
            string idSubject = User.Identity.Name;
            var query1 = new Neo4jClient.Cypher.CypherQuery("match(u{ Email:'" + idSubject + "'})-[f: FOLLOW]->(m{Email:'" + idObject + "'}) delete f", new Dictionary<string, object>(), CypherResultMode.Set);
            ((IRawGraphClient)client).ExecuteCypher(query1);
            return RedirectToAction("UserProfile", new { email = idObject });
        }
        public ActionResult UserFollowers(string email)
        {
            var query1 = new Neo4jClient.Cypher.CypherQuery("match(u{Email:'" + email + "'})<-[f:FOLLOW]-(m:User) return  m", new Dictionary<string, object>(), CypherResultMode.Set);
            List<User> users = ((IRawGraphClient)client).ExecuteGetCypherResults<User>(query1).ToList();
            //return Json(new { data = users }, JsonRequestBehavior.AllowGet);
            return View(users);
        }
        public ActionResult UserFollowing(string email)
        {
            var query1 = new Neo4jClient.Cypher.CypherQuery("match(u{Email:'" + email + "'})-[f:FOLLOW]->(m:User) return  m", new Dictionary<string, object>(), CypherResultMode.Set);
            List<User> users = ((IRawGraphClient)client).ExecuteGetCypherResults<User>(query1).ToList();
            return View(users);


        }

        public ActionResult UserFollowersJson(string email)
        {
            var query1 = new Neo4jClient.Cypher.CypherQuery("match(u{Email:'" + email + "'})<-[f:FOLLOW]-(m:User) return  m", new Dictionary<string, object>(), CypherResultMode.Set);
            List<User> users = ((IRawGraphClient)client).ExecuteGetCypherResults<User>(query1).ToList();
            return Json(new { data = users }, JsonRequestBehavior.AllowGet);
            //return View(users);
        }
        public ActionResult UserFollowingJson(string email)
        {
            var query1 = new Neo4jClient.Cypher.CypherQuery("match(u{Email:'" + email + "'})-[f:FOLLOW]->(m:User) return  m", new Dictionary<string, object>(), CypherResultMode.Set);
            List<User> users = ((IRawGraphClient)client).ExecuteGetCypherResults<User>(query1).ToList();
            //return View(users);
            return Json(new { data = users }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public ActionResult FilterRecords()
        {
            

            AdFilter ad = new AdFilter();

            List<string> genres = getAllGenres();
            ViewBag.slgen = new List<SelectListItem>();
            foreach (string item in genres)
                ViewBag.slgen.Add(new SelectListItem { Text = item, Value = item });


            List<string> artists = getAllArtists();
            ViewBag.slart = new List<SelectListItem>();
            foreach (string item in artists)
                ViewBag.slart.Add(new SelectListItem { Text = item, Value = item });

            


            return View(ad);
        }
        [HttpPost]
        public ActionResult FilterRecords(AdFilter ad)
        {
            string genre = ad.genre;
            string artist = ad.artist;
            List<Record> records = new List<Record>();
            List<Ad> ads = new List<Ad>();
            AdFilter adf = new AdFilter();
            if (!genre.IsNullOrWhiteSpace() & !artist.IsNullOrWhiteSpace())
            {
                var query1 = new Neo4jClient.Cypher.CypherQuery("match (a{Name:'" + artist + "'})-[OWNS]->(record)-[BELONGS_TO]->(g{genre:'" + genre + "'}) return record", new Dictionary<string, object>(), CypherResultMode.Set);
                records = ((IRawGraphClient)client).ExecuteGetCypherResults<Record>(query1).ToList();
            } else if (!genre.IsNullOrWhiteSpace() & artist.IsNullOrWhiteSpace())
            {
                var query1 = new Neo4jClient.Cypher.CypherQuery("match (a)-[OWNS]->(record)-[BELONGS_TO]->(g{genre:'" + genre + "'}) return record", new Dictionary<string, object>(), CypherResultMode.Set);
                records = ((IRawGraphClient)client).ExecuteGetCypherResults<Record>(query1).ToList();
            } else if (genre.IsNullOrWhiteSpace() & !artist.IsNullOrWhiteSpace())
            {
                var query1 = new Neo4jClient.Cypher.CypherQuery("match (a{Name:'" + artist + "'})-[OWNS]->(record)-[BELONGS_TO]->(g) return record", new Dictionary<string, object>(), CypherResultMode.Set);
                records = ((IRawGraphClient)client).ExecuteGetCypherResults<Record>(query1).ToList();
            }
            else
            {
                var query1 = new Neo4jClient.Cypher.CypherQuery("match (a:Artist)-[OWNS]->(record)-[BELONGS_TO]->(g:Genre) return record", new Dictionary<string, object>(), CypherResultMode.Set);
                records = ((IRawGraphClient)client).ExecuteGetCypherResults<Record>(query1).ToList();
            }

            foreach (var rec in records)
            {
                var q = client.Cypher.Match("(r:Record {Id:'" + rec.Id + "'})<-[RELEASE]-(l:Label),(r:Record {Id:'" + rec.Id + "'})<-[OWNS]-(a:Artist),(r:Record {Id:'" + rec.Id + "'})<-[s:SELLING]-(u:User),(r:Record {Id:'" + rec.Id + "'})-[BELONGS_TO]->(g:Genre)  ").Return((l, a, u, s, g) => new { Label = l.As<Label>(), Artist = a.As<Artist>(), User = u.As<User>(), Condition = s.As<Condition>(), Genre = g.As<Genre>() }); ;
                foreach (var result in q.Results)
                {
                    ads.Add(new Ad { Artist = result.Artist, Record = rec, Label = result.Label, User = result.User, Condition = result.Condition, Genre = result.Genre });

                }
            }
            adf.ads = ads.DistinctBy(x => x.Record.Id).ToList();

            List<string> genres = getAllGenres();
            ViewBag.slgen = new List<SelectListItem>();
             foreach (string item in genres)
              ViewBag.slgen.Add(new SelectListItem { Text = item, Value = item });


            List<string> artists = getAllArtists();
            ViewBag.slart = new List<SelectListItem>();
            foreach (string item in artists)
                ViewBag.slart.Add(new SelectListItem { Text = item, Value = item });



            return View(adf);
}



        [HttpGet]
        public ActionResult SearchRecords()
        {
            AdFilter ad = new AdFilter();
            return View(ad);
        }
        [HttpPost]
        public ActionResult SearchRecords(AdFilter ad)
        {
            
            Dictionary<string, object> queryDict = new Dictionary<string, object>();
            List<Ad> ads = new List<Ad>();
            AdFilter adf = new AdFilter();
           


            string searchWord = ad.inputUser + ".*";
            string new1 = searchWord.ToLower();
            queryDict.Add("new1", new1);
            var query = new Neo4jClient.Cypher.CypherQuery("match (r:Record) where toLower(r.Title)=~{new1} return r", queryDict, CypherResultMode.Set);


        
            List<Record> records = ((IRawGraphClient)client).ExecuteGetCypherResults<Record>(query).ToList();
            foreach (Record rec in records)
            {
                var q = client.Cypher.Match("(r:Record {Id:'" + rec.Id + "'})<-[RELEASE]-(l:Label),(r:Record {Id:'" + rec.Id + "'})<-[OWNS]-(a:Artist),(r:Record {Id:'" + rec.Id + "'})<-[s:SELLING]-(u:User),(r:Record {Id:'" + rec.Id + "'})-[BELONGS_TO]->(g:Genre)  ").Return((l, a, u, s, g) => new { Label = l.As<Label>(), Artist = a.As<Artist>(), User = u.As<User>(), Condition = s.As<Condition>(), Genre = g.As<Genre>() }); ;
                foreach (var result in q.Results)
                {
                    ads.Add(new Ad { Artist = result.Artist, Record = rec, Label = result.Label, User = result.User, Condition = result.Condition, Genre = result.Genre });

                }
                
            }
            adf.ads = ads.DistinctBy(x => x.Record.Id).ToList();
           
            adf.inputUser = ad.inputUser;

            return View(adf);
        }
        public ActionResult DeleteRecord(string RecordId)
        {
            var query1 = new Neo4jClient.Cypher.CypherQuery("match (u{Id:'"+ RecordId + "'})  detach delete u", new Dictionary<string, object>(), CypherResultMode.Set);
            ((IRawGraphClient)client).ExecuteCypher(query1);
            return RedirectToAction("UserProfile", new { email = User.Identity.Name }) ;
           

        }
        public ActionResult RecordProfile(string ad)
        {
            Ad ad1 = Ad.Deserialize(ad);
            List<Song> songs = getAllRecordSongs(ad1.Record.Id);
            ad1.Songs = songs;
            return View(ad1);
        }
        [HttpGet]
        public ActionResult BuyRecord(string ad)
        {
            string email = User.Identity.Name;
            Ad adRecord = Ad.Deserialize(ad);
            RecBuy recBuy = new RecBuy();
            recBuy.slreplace = new List<SelectListItem>();
            if (adRecord.Condition.Replacement)
            {
                var query1 = new Neo4jClient.Cypher.CypherQuery("match (m{Email:'"+email+"'})-[s{Replacement:'True'}]->(r:Record) return r", new Dictionary<string, object>(), CypherResultMode.Set);
                List<Record> records = ((IRawGraphClient)client).ExecuteGetCypherResults<Record>(query1).ToList();
                foreach (var item in records)
                    recBuy.slreplace.Add(new SelectListItem { Text = item.Title, Value = item.Title });
                
            }
            recBuy.Record = adRecord.Record;
            recBuy.Record.Condition = adRecord.Condition;
            recBuy.User = adRecord.User;
            recBuy.recordSeller = adRecord.Record.Title;
            return View(recBuy);
        }
        [HttpPost]
        public ActionResult BuyRecord(RecBuy recBuy)
        {
            string emailb = User.Identity.Name;
            string emails = recBuy.User.Email;
            string typeOffer = recBuy.radio;
            string recordBuy = recBuy.Record.Title;
            string recordSel = recBuy.recordSeller;
            float price = recBuy.Record.Price;
            int idOffer = DataLayer.getKeyOffer();
            var query1 = new Neo4jClient.Cypher.CypherQuery("match (u{Email:'"+emails+ "'}),(m{Email:'"+emailb+ "'}) create (m)-[:OFFER{Type:'"+ typeOffer + "', RecordTitleB:'"+ recordBuy + "',RecordTitleS:'"+ recordSel+"',Status:'Pending', Price:'"+price+"',Id:'"+idOffer+"' }]->(u) ", new Dictionary<string, object>(), CypherResultMode.Set);
            ((IRawGraphClient)client).ExecuteCypher(query1);
            return RedirectToAction("GetFeedSuggestions");
        }

        public ActionResult UserOffers(string email)
        {
            List<Offer> offers = new List<Offer>();
            var q= client.Cypher.Match("(u{ Email:'"+ email +"'})<-[o:OFFER]-(m)").Return((o,m) => new { Offer = o.As<Offer>(), Buyer = m.As<User>() }); ;
            foreach (var item in q.Results)
                offers.Add(new Offer { RecordTitleB = item.Offer.RecordTitleB, RecordTitleS = item.Offer.RecordTitleS, Type = item.Offer.Type, Buyer = item.Buyer, Price=item.Offer.Price,Status=item.Offer.Status, Id=item.Offer.Id });
            return View(offers);
        }
        public ActionResult AcceptOffer(int idOffer, string RecordTitleB, string RecordTitleS, string type)
        {
            string email = User.Identity.Name;
            var q = new Neo4jClient.Cypher.CypherQuery("match (u{Email:'"+email+ "'}) return u.SoldRecords", new Dictionary<string, object>(), CypherResultMode.Set);
            int soldRecords = ((IRawGraphClient)client).ExecuteGetCypherResults<int>(q).FirstOrDefault();
            soldRecords++;
            if (type == "Trade")
            {

                var query1 = new Neo4jClient.Cypher.CypherQuery("match (r{Title:'" + RecordTitleB + "'}), (m{Title:'" + RecordTitleS + "'}), (u{Email:'" + email + "'})<-[o{Id:'" + idOffer + "'}]-(s) set u.SoldRecords="+soldRecords+ " set o.Status='Accepted'", new Dictionary<string, object>(), CypherResultMode.Set);
                ((IRawGraphClient)client).ExecuteCypher(query1);
            }
            else
            {
                var query1 = new Neo4jClient.Cypher.CypherQuery("match (m{Title:'" + RecordTitleS + "'}), (u{Email:'" + email + "'})<-[o{Id:'" + idOffer + "'}]-(s) set u.SoldRecords=" + soldRecords + "  set o.Status='Accepted' ", new Dictionary<string, object>(), CypherResultMode.Set);
                ((IRawGraphClient)client).ExecuteCypher(query1);
            }

            return RedirectToAction("UserOffers", new { email = email });
            
        }
        public ActionResult DeclineOffer(int idOffer, string RecordTitleS, string type)
        {
             string email = User.Identity.Name;
             var query1 = new Neo4jClient.Cypher.CypherQuery("match (u{Email:'" + email + "'})<-[o{Id:'" + idOffer + "'}]-(s)  set o.Status='Declined' ", new Dictionary<string, object>(), CypherResultMode.Set);
             ((IRawGraphClient)client).ExecuteCypher(query1);
            return RedirectToAction("UserOffers", new { email = email });
        }
        [HttpGet]
        public ActionResult EditProfile(string email)
        {
            var query1 = new Neo4jClient.Cypher.CypherQuery("match (u:User) where u.Email='"+email+"' return u", new Dictionary<string, object>(), CypherResultMode.Set);
            User user = ((IRawGraphClient)client).ExecuteGetCypherResults<User>(query1).FirstOrDefault();
            return View(user);
        }
        [HttpPost]
        public ActionResult EditProfile(User user)
        {
            string recordPicture;
            HttpPostedFileBase objFiles = Request.Files["upload"];
            if (objFiles.ContentLength != 0)
            {
                var filename = Path.GetFileName(objFiles.FileName);
                var path = Path.Combine(Server.MapPath("~/Content/Pictures/"), filename);
                objFiles.SaveAs(path);
            }
            if (!objFiles.FileName.Equals(""))
            {
                recordPicture = "~/Content/Pictures/" + objFiles.FileName;
            }
            else
            {
                var q = new Neo4jClient.Cypher.CypherQuery("match (u{Email: '" + User.Identity.Name + "'}) return u.Picture", new Dictionary<string, object>(), CypherResultMode.Set);
                string OldPicture = ((IRawGraphClient)client).ExecuteGetCypherResults<String>(q).FirstOrDefault();
                recordPicture = OldPicture;
                
            }
            var query1 = new Neo4jClient.Cypher.CypherQuery("match (u{Email:'"+user.Email+"'}) set u.PersonName='"+user.PersonName+"', u.PersonSurName='"+user.PersonSurName+"', u.Nickname='"+user.Nickname+"', u.Picture='"+ recordPicture + "'", new Dictionary<string, object>(), CypherResultMode.Set);
            ((IRawGraphClient)client).ExecuteCypher(query1);
            return RedirectToAction("UserProfile", new { email = user.Email });
        }
        public ActionResult EditRecord(int RecordId)
        {
            Ad ad = new Ad();
            var q = client.Cypher.Match("(r:Record {Id:'" + RecordId + "'})<-[RELEASE]-(l:Label),(r:Record {Id:'" + RecordId + "'})<-[OWNS]-(a:Artist),(r:Record {Id:'" + RecordId + "'})<-[s:SELLING]-(u:User),(r:Record {Id:'" + RecordId + "'})-[BELONGS_TO]->(g:Genre)  ").Return((l, a, u, s, g, r) => new { Label = l.As<Label>(), Artist = a.As<Artist>(), User = u.As<User>(), Condition = s.As<Condition>(), Genre = g.As<Genre>(), Record = r.As<Record>() }); ;
            foreach (var result in q.Results)
            {
                ad = new Ad { Artist = result.Artist, Record = result.Record, Label = result.Label, User = result.User, Songs = getAllRecordSongs(RecordId), Condition = result.Condition, Genre = result.Genre };

            }
            ad.slcond = new List<SelectListItem>();
            ad.slcond.Add(new SelectListItem { Text = "New", Value = "New" });
            ad.slcond.Add(new SelectListItem { Text = "Used", Value = "Used" });

            ad.slgen = new List<SelectListItem>();
            List<string> genres = getAllGenres();
            foreach (string item in genres)
                ad.slgen.Add(new SelectListItem { Text = item, Value = item });

            return View(ad);
        }
        [HttpPost]
        public ActionResult EditRecord(Ad ad)
        {
            string recordPicture;
            HttpPostedFileBase objFiles = Request.Files["upload"];
            if (objFiles.ContentLength != 0)
            {
                var filename = Path.GetFileName(objFiles.FileName);
                var path = Path.Combine(Server.MapPath("~/Content/Pictures/"), filename);
                objFiles.SaveAs(path);
            }
            if (!objFiles.FileName.Equals(""))
            {
                recordPicture = "~/Content/Pictures/" + objFiles.FileName;
            }
            else
            {
                var q = new Neo4jClient.Cypher.CypherQuery("match (r{Id: '" + ad.Record.Id + "'}) return r.Picture",new Dictionary<string, object>(), CypherResultMode.Set);
                string OldPicture = ((IRawGraphClient)client).ExecuteGetCypherResults<String>(q).FirstOrDefault();
                recordPicture = OldPicture;
            }

            var query1 = new Neo4jClient.Cypher.CypherQuery("match (u{Email:'" + User.Identity.Name + "'})-[s:SELLING]->(r{Id:'" + ad.Record.Id + "'}) set r.Format='" + ad.Record.Format + "', r.Title='" + ad.Record.Title + "', r.Price='" + ad.Record.Price + "', r.Picture='"+recordPicture+ "', s.condition='" + ad.Condition.condition + "', s.Replacement='" + ad.Condition.Replacement + "'", new Dictionary<string, object>(), CypherResultMode.Set);
            ((IRawGraphClient)client).ExecuteCypher(query1);
            var query2 = new Neo4jClient.Cypher.CypherQuery("match (r{Id:'" + ad.Record.Id + "'})-[:BELONGS_TO]->(g{genre:'" + ad.Genre.genre + "'}) return true", new Dictionary<string, object>(), CypherResultMode.Set);
            bool g = ((IRawGraphClient)client).ExecuteGetCypherResults<bool>(query2).FirstOrDefault();
            bool a = doesArtistExist(ad.Artist);
            bool l = doesLabelExist(ad.Label);

            if (!g)
            {
                var query3 = new Neo4jClient.Cypher.CypherQuery("match (r{Id:'" + ad.Record.Id + "'})-[b:BELONGS_TO]->(), (g{genre:'" + ad.Genre.genre + "'}) create (r)-[k:BELONGS_TO]->(g) delete b", new Dictionary<string, object>(), CypherResultMode.Set);
                ((IRawGraphClient)client).ExecuteCypher(query3);

            }
            if (!a)
            {
                var query3 = new Neo4jClient.Cypher.CypherQuery("match (r{Id:'" + ad.Record.Id + "'})<-[b:OWNS]-(), (a{Name:'" + ad.Artist.Name + "'}) create (a)-[k:OWNS]->(r) delete b", new Dictionary<string, object>(), CypherResultMode.Set);
                ((IRawGraphClient)client).ExecuteCypher(query3);
            }
            if (!l)
            {
                var query3 = new Neo4jClient.Cypher.CypherQuery("match (r{Id:'" + ad.Record.Id + "'})<-[b:RELEASE]-(), (l{NameOfLabel:'" + ad.Label.NameOfLabel + "'}) create (l)-[m:RELEASE]->(r) delete b", new Dictionary<string, object>(), CypherResultMode.Set);
                ((IRawGraphClient)client).ExecuteCypher(query3);
            }
            var query4 = new Neo4jClient.Cypher.CypherQuery("match (r{Id:'" + ad.Record.Id + "'})-[b:INCLUDE]->(s:Song) detach delete s", new Dictionary<string, object>(), CypherResultMode.Set);
            ((IRawGraphClient)client).ExecuteCypher(query4);
            foreach ( var item in ad.Songs)
            {
                if (item.NameOfSong != null)
                {
                    var query5 = new Neo4jClient.Cypher.CypherQuery("match (r{Id:'" + ad.Record.Id + "'}) create (r)-[m:INCLUDE]->(s:Song{NameOfSong:'" + item.NameOfSong + "', Duration:'" + item.Duration + "'}) ", new Dictionary<string, object>(), CypherResultMode.Set);
                    ((IRawGraphClient)client).ExecuteCypher(query5);
                }
            }

            return RedirectToAction("UserProfile", new { email = User.Identity.Name });
        }

        public string getFavouriteGenre(string email)
        {
            var query1 = new Neo4jClient.Cypher.CypherQuery("match(u{ Email: '"+email+"'})-[] - (record) -[]->(g: Genre) return g.genre" , new Dictionary<string, object>(), CypherResultMode.Set);
            List<string> favgenres = ((IRawGraphClient)client).ExecuteGetCypherResults<String>(query1).ToList();
            var fav = favgenres.GroupBy(item => item)
                      .Where(item => item.Count() >= 2)
                      .ToList();
            if (fav.Count==0)
                return "You dont have favourite genre yet";
            else
            return fav[0].Key;
        }
        [HttpPost]
        public ActionResult AddComment(UserAds model)
        {
            model.comm.DateTime = DateTime.Now;
            var query1 = new Neo4jClient.Cypher.CypherQuery("match(u{Email:'"+model.comm.Object.Email+"'}), (m{Email:'"+ model.comm.Subject.Email+"'}) create (m)-[c:COMMENT{Content:'"+model.comm.Content+"', DateTime:'"+ model.comm.DateTime + "'}]->(u) ", new Dictionary<string, object>(), CypherResultMode.Set);
            ((IRawGraphClient)client).ExecuteCypher(query1);
            return RedirectToAction("UserProfile", new { email = model.comm.Object.Email });
            
        }

        public ActionResult GetSentOffers()
        {
            var query1 = new Neo4jClient.Cypher.CypherQuery("match (u{Email:'"+User.Identity.Name+"'})-[o:OFFER]->(m) return o", new Dictionary<string, object>(), CypherResultMode.Set);
            List<Offer> sentOffers = ((IRawGraphClient)client).ExecuteGetCypherResults<Offer>(query1).ToList();
            return View(sentOffers);
        }

        public List<string> getAllArtists()
        {
            var query1 = new Neo4jClient.Cypher.CypherQuery("match (a:Artist) return a.Name", new Dictionary<string, object>(), CypherResultMode.Set);
            List<string> artists = ((IRawGraphClient)client).ExecuteGetCypherResults<String>(query1).ToList();
            return artists;

        }

        public List<string> getAllLabels()
        {
            var query1 = new Neo4jClient.Cypher.CypherQuery("match (l:Label) return l.NameOfLabel", new Dictionary<string, object>(), CypherResultMode.Set);
            List<string> labels = ((IRawGraphClient)client).ExecuteGetCypherResults<String>(query1).ToList();
            return labels;

        }
        public List<string> getAllGenres()
        {
            var query1 = new Neo4jClient.Cypher.CypherQuery("match (g:Genre) return g.genre", new Dictionary<string, object>(), CypherResultMode.Set);
            List<string> genres = ((IRawGraphClient)client).ExecuteGetCypherResults<String>(query1).ToList();
            return genres;

        }

        public Artist getRecordArtist(int recId)
        {
            var query1 = new Neo4jClient.Cypher.CypherQuery("match (r:Record {Id:'" + recId + "'})<-[OWNS]-(a:Artist) return a", new Dictionary<string, object>(), CypherResultMode.Set);
            List<Artist> artist = ((IRawGraphClient)client).ExecuteGetCypherResults<Artist>(query1).ToList();
            return artist [0];
        }
        public User getUserRecord(int recId)
        {
            var query1 = new Neo4jClient.Cypher.CypherQuery("match (r:Record {Id:'" + recId + "'})<-[SELLING]-(u:User) return u", new Dictionary<string, object>(), CypherResultMode.Set);
            List<User> user = ((IRawGraphClient)client).ExecuteGetCypherResults<User>(query1).ToList();
            return user[0];
        }
        
        public List<Song> getAllRecordSongs(int recId)
        {
            var query1 = new Neo4jClient.Cypher.CypherQuery("match (r:Record {Id:'" + recId + "'})-[INCLUDE]->(s:Song) return s", new Dictionary<string, object>(), CypherResultMode.Set);
            List<Song> songs = ((IRawGraphClient)client).ExecuteGetCypherResults<Song>(query1).ToList();
            return songs;
        }
        

    }
}