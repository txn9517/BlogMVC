using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogMVC.Models
{
    // the parts of a category
    public class Category
    {
        // primary key
        public int Id { get; set; }

        // foreign key
        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long", MinimumLength = 2)]
        public string? Name { get; set; }

        [StringLength(2000, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long", MinimumLength = 2)]
        public string? Description { get; set; }

        public byte[]? ImageData { get; set; }

        public string? ImageType { get; set; }

        [NotMapped]
        public IFormFile? CategoryImage { get; set; }

        // navigation properties
        public virtual ICollection<BlogPost> BlogPosts { get; set; } = new HashSet<BlogPost>();
    }
}
