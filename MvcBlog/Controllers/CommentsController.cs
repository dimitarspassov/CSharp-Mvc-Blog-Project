using System;
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
    public class CommentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Comments
        [Authorize]
        public ActionResult Index()
        {
            //Comments index view is only available for the Administrator. Users can edit/delete their own comments, 
            //but they do not need to see a page with all existing comments for each of the posts.
            List<Comment> comments = new List<Comment>();
            var author = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (User.IsInRole("Administrator"))
            {
                return View(db.Comments.ToList());
            }
            else
            {
                return Redirect("/");
            }
        }

        // GET: Comments/Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Comments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Body")] Comment comment, int? id)
        {

            if (ModelState.IsValid)
            {
                //It is neccessary to add comment.Author, comment.AuthorName and comment.PostId, because they are not binded
                //Moreover the return statement must redirect back to the post, which is commented. Here comes the Post.Id
                comment.Author = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
                comment.AuthorName = comment.Author.FullName;
                comment.PostId = (int)id;
                db.Comments.Add(comment);
                db.SaveChanges();
                return Redirect("~/Posts/Details/" + comment.PostId);
            }

            return View(comment);
        }

        // GET: Comments/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }

            //Edit available only for Administrator/author of comment
            var author = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (comment.Author != author)
            {
                if (!User.IsInRole("Administrator"))
                {
                    return HttpNotFound();
                }
            }
            return View(comment);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Body,PostId")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                //After edit, the comment's AuthorName is not binded. Therefore the Authorname is added here after validation.
                //Because a comment can be edited only by author or admin, the AuthorName ramains the same or if edited by admin,
                //the AuthorName is set to "Last edited by" + author.FullName (admin).              
                var author = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (User.IsInRole("Administrator"))
                {
                    comment.AuthorName = "Last edited by " + author.FullName;
                }
                else
                {
                    comment.AuthorName = author.FullName;
                }
                db.Entry(comment).State = EntityState.Modified;
                db.SaveChanges();
                return Redirect("~/Posts/Details/" + comment.PostId);
            }
            return View(comment);
        }

        // GET: Comments/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }

            //Delete available only for Administrator/author of comment
            var author = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (comment.Author != author)
            {
                if (!User.IsInRole("Administrator"))
                {
                    return HttpNotFound();
                }
            }
            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Comment comment = db.Comments.Find(id);
            db.Comments.Remove(comment);
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
