﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MvcBlog.Models;

namespace MvcBlog.Controllers
{

    public class PostsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Posts
        [Authorize]
        public ActionResult Index()
        {
            List<Post> posts = new List<Post>();
            var author = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (User.IsInRole("Administrator"))
            {
                return View(db.Posts.ToList());
            }
            else
            {
                string currentUser = User.Identity.Name;
                foreach (var post in db.Posts)
                {
                    if (post.Author == author)
                    {
                        posts.Add(post);
                    }


                }
                return View(posts);
            }
        }

        public ActionResult List(string categorySelected)
        {
            List<Post> postsByCategory;
            if (categorySelected == null)
            {
               postsByCategory = db.Posts.OrderByDescending(p=>p.Date).ToList();
            }
            else
            {
                postsByCategory = db.Posts.Where(p => p.Category == categorySelected).OrderByDescending(p => p.Date).ToList();
            }
            
            List<Category> allCategories = db.Categories.ToList();
            ViewBag.Categories = allCategories;

            List<Post> topFivePosts = db.Posts.OrderByDescending(p => p.Visits).Take(5).ToList();
            ViewBag.TopPosts = topFivePosts;

            return View(postsByCategory);
        }

        // GET: Posts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }

            int currentVisits = post.Visits;
            currentVisits++;
            post.Visits = currentVisits;
            db.Entry(post).State = EntityState.Modified;
            db.SaveChanges();

            int postId = post.Id;
            ViewBag.Id = postId;

            var author = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            bool editAvailable = false;
            if (post.Author == author)
            {
                editAvailable =true;
            }
            ViewBag.EditAvailable = editAvailable;


                List<Comment>postComments = new List<Comment>();

            foreach (var comment in db.Comments)
            {
                if (postId == comment.PostId)
                {
                    postComments.Add(comment);
                }
            }
            postComments = postComments.OrderByDescending(c => c.Date).ToList();
            ViewBag.Comments = postComments;
            return View(post);
        }

        // GET: Posts/Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Body,Visits,Category")] Post post)
        {
            if (ModelState.IsValid)
            {
                Category category = new Category { Name = post.Category.ToUpper()};

                bool exists = false;
                foreach (Category c in db.Categories)
                {
                    if (c.Name == category.Name)
                    {
                        exists = true;
                        break;
                    }
                }
                if (!exists)
                {
                    db.Categories.Add(category);
                }
               
                post.Author = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
                post.AuthorName = post.Author.FullName;
                db.Posts.Add(post);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(post);
        }

        // GET: Posts/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }

            var author = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (post.Author != author)
            {
                if (!User.IsInRole("Administrator"))
                {
                    return HttpNotFound();
                }
            }
            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Body,Date,Visits,Category")] Post post)
        {

            if (ModelState.IsValid)
            {
                var author = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
                if(User.IsInRole("Administrator"))
                {
                    post.AuthorName = "Last edited by " + author.FullName;
                }
                else
                {
                    post.AuthorName = author.FullName;
                }
            
                db.Entry(post).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(post);
        }

        // GET: Posts/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            var author = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (post.Author != author)
            {
                if (!User.IsInRole("Administrator"))
                {
                    return HttpNotFound();
                }          
            }
            return View(post);
        }

        // POST: Posts/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Post post = db.Posts.Find(id);
            db.Posts.Remove(post);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
