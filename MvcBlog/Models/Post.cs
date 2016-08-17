using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Security.Principal;

namespace MvcBlog.Models
{
    public class Post
    {
        
        public Post()
        {
            this.Date = DateTime.Now;
            this.Comments = new HashSet<Comment>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Body { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public ApplicationUser Author { get; set; }

        public int Visits { get; set; }  

        public string Category { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
    }
}