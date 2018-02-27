using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using VkurseStats.Models.DB;

namespace VkurseStats.Controllers
{
    public class GraphController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}