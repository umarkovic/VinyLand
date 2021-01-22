using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Neo4jClient;
using Neo4jClient.Cypher;
using VinyLand.Models;

namespace VinyLand.Controllers
{
    public class HomeController : Controller
    {
        private GraphClient client;
        public HomeController()
        {
            client = DataLayer.GetSession();
        }
        public ActionResult Index()
        {
            var q = new Neo4jClient.Cypher.CypherQuery("match (n:Record) where n.title = 'Berlin Calling' return n", new Dictionary<string, object>(), CypherResultMode.Set);
            List<Record> records = ((IRawGraphClient)client).ExecuteGetCypherResults<Record>(q).ToList();
            return View();

            
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
    
}