using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcBlog.Models;

namespace MvcBlog.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var posts = db.Posts.OrderByDescending(p => p.Date).Take(5);

            List<Category> allCategories = db.Categories.ToList();
            ViewBag.Categories = allCategories;

            List<Post> topFivePosts = db.Posts.OrderByDescending(p => p.Visits).Take(5).ToList();
            ViewBag.TopPosts = topFivePosts;
            return View(posts.ToList());        
        }
    }
}
