using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OneVietnam.Controllers
{
    public class ErrorController : Controller
    {
        public ViewResult Index()
        {
            return View();
        }
        public ViewResult PageNotFound()
        {
            Response.StatusCode = 404;  //you may want to set this to 200
            return View();
        }
    }
}