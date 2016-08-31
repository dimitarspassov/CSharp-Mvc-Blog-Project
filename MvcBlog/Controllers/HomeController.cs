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

            //Latest 5 posts from the database
            var posts = db.Posts.OrderByDescending(p => p.Date).Take(5);

            //All existing categories
            List<Category> allCategories = db.Categories.ToList();
            ViewBag.Categories = allCategories;

            //Top 5 most visited posts
            List<Post> topFivePosts = db.Posts.OrderByDescending(p => p.Visits).Take(5).ToList();
            ViewBag.TopPosts = topFivePosts;
            return View(posts.ToList());        
        }
    }
}
