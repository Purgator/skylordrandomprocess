using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using ITI.SkyLord.TestAvecEntity.Models;

namespace ITI.SkyLord.TestAvecEntity.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {

            var t = View("Index");
            return t;
        }

        public IActionResult AddMessage()
        {
            using (var dal = new Dal())
            {
                dal.AddMessage("test", "testmessage");
            }

            return View();
        }
        public IActionResult ViewMessages()
        {
            using (var dal = new Dal())
            {
                var messages = dal.GetAllMessage();
                messages.Add(new Tchat() { Message = "Kikoo", Personne = "Antoine" });
                ViewBag.messages = messages;
                
            }
            
            return View();
        }

        
    }
}
