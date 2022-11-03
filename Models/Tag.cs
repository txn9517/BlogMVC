using System.ComponentModel.DataAnnotations;

namespace BlogMVC.Models
{
    // blueprint for a tag
    public class Tag
    {
        // primary key
        public int Id { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long", MinimumLength = 2)]
        public string? Name { get; set; }

        // navigation properties
        public virtual ICollection<BlogPost> BlogPosts { get; set; } = new HashSet<BlogPost>();
    }
}
