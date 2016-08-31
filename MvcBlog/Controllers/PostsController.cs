using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MvcBlog.Models;
using PagedList;

namespace MvcBlog.Controllers
{

    public class PostsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Posts
        [Authorize]
        public ActionResult Index()
        {
            //Shows all posts for the Administrator or shows posts of the current user
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

        public ActionResult List(string categorySelected, int page = 1, int pageSize = 4)
        {
            //Lists the posts by selected category, then orders them by date descending
      
            List<Post> postsByCategory;
            if (categorySelected == null)
            {
               postsByCategory = db.Posts.OrderByDescending(p=>p.Date).ToList();
            }
            else
            {
                postsByCategory = db.Posts.Where(p => p.Category == categorySelected).OrderByDescending(p => p.Date).ToList();
            }
            
            //All categories to be displayed in the sidebar
            List<Category> allCategories = db.Categories.ToList();
            ViewBag.Categories = allCategories;

            //Top five posts to be displayed in the sidebar
            List<Post> topFivePosts = db.Posts.OrderByDescending(p => p.Visits).Take(5).ToList();
            ViewBag.TopPosts = topFivePosts;

            //Posts are put in a paged list in order to display them with pagination
            PagedList<Post> pagedPosts = new PagedList<Post>(postsByCategory, page, pageSize);
          
            return View(pagedPosts);
        }

        // GET: Posts/Details/5
        public ActionResult Details(int? id)
        {
            //Displays the post like an article with comments below

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }

            //Simple logic to increment the visits for current post
            int currentVisits = post.Visits;
            currentVisits++;
            post.Visits = currentVisits;
            db.Entry(post).State = EntityState.Modified;
            db.SaveChanges();

            //Post's id should be send to the view for creating the hyperlink for edit/add comment
            int postId = post.Id;
            ViewBag.Id = postId;

            //Extracting the current user to check whether he is the admin/author of the post. If true, post can be edited.
            var author = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            bool editAvailable = false;
            if (post.Author == author)
            {
                editAvailable =true;
            }
            ViewBag.EditAvailable = editAvailable;

            //Some logic for the comments section under the post. If the current user is the author of any of the comments
            //or if the current user is Administrator, he can edit/delete them. 
            if (author != null)
            {
                ViewBag.CurrentUser = author.FullName;
            }

            if (User.IsInRole("Administrator"))
            {
                ViewBag.Admin = true;
            }
            else
            {
                ViewBag.Admin = false;
            }

            //Extracting all comments for the current post
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
                //Adding the category of the newly created post to database set of all categories
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
               
                //Adding post author
                post.Author = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
                post.AuthorName = post.Author.FullName;
                db.Posts.Add(post);
                db.SaveChanges();
                return RedirectToAction("/Details/" + post.Id);
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

            //Edit available only for admin/author of current post
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
                //After edit, the post's author is not binded. Therefore the author is added here after validation.
                //Because a post can be edited only by author or admin, the AuthorName ramains the same or if edited by admin,
                //the AuthorName is set to "Last edited by" + author.FullName (admin).
                //However the author of current post remains the same. AuthorName is a property only to be displayed in some cases like Details.
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
                return Redirect("~/Posts/Details/" + post.Id);
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

            //Delete available only for admin/author of current post
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
