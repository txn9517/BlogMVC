using System.ComponentModel.DataAnnotations;

namespace BlogMVC.Models
{
    // blueprint for a comment model
    public class Comment
    {
        // primary key
        public int Id { get; set; }

        // foreign key
        public int BlogPostId { get; set; }

        // foreign key
        [Required]
        public string? AuthorId { get; set; }

        [Display(Name = "Date Created")]
        [DataType(DataType.DateTime)]
        public DateTime DateCreated { get; set; }

        [Display(Name = "Last Updated")]
        [DataType(DataType.DateTime)]
        public DateTime? LastUpdated { get; set; }

        [Display(Name = "Update Reason")]
        public string? UpdateReason { get; set; }

        [Required]
        [StringLength(5000, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long", MinimumLength = 2)]
        public string? Body { get; set; }

        // navigation properties
        [Display(Name = "Blog Post")]
        public virtual BlogPost? BlogPost { get; set; }

        // add relationship to BlogUser
        public virtual BlogUser? Author { get; set; }
    }
}
