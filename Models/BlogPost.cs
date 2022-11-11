using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogMVC.Models
{
    // the parts of a blog post
    public class BlogPost
    {
        // primary key
        public int Id { get; set; }

        [Required]
        public string? CreatorId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Title { get; set; }

        [Required]
        public string? Content { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateCreated { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? LastUpdated { get; set; }

        // foreign key
        public int CategoryId { get; set; }

        public string? Slug { get; set; }

        public string? Abstract { get; set; }

        public bool IsDeleted { get; set; }

        [DisplayName("Published")]
        public bool IsPublished { get; set; }

        public byte[]? ImageData { get; set; }

        public string? ImageType { get; set; }

        [NotMapped]
        public IFormFile? BlogPostImage { get; set; }

        // add relationship to Category model
        public virtual Category? Category { get; set; }

        // add relationship to Comment model
        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();

        // add relationship to Tag model
        public virtual ICollection<Tag> Tags { get; set; } = new HashSet<Tag>();

        // virtual object that will receive property based on CreatorId
        public virtual BlogUser? Creator { get; set; }
    }
}
