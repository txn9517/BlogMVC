using BlogMVC.Models;

namespace BlogMVC.Services.Interfaces
{
    // actions for working with blog posts
    public interface IBlogPostService
    {
        // get all blog posts, published
        public Task<List<BlogPost>> GetAllBlogPostsPubAsync();

        // get all blog posts, unpublished
        public Task<List<BlogPost>> GetAllBlogPostsUnpubAsync();

        // get deleted blog posts, published
        public Task<List<BlogPost>> GetDeletedBlogPostsPubAsync();

        // get deleted blog posts, unpublished
        public Task<List<BlogPost>> GetDeletedBlogPostsUnpubAsync();

        // get blog post details
        public Task<BlogPost> GetBlogPostDetails(string? slug);

        // get blog post edit
        public Task<BlogPost> GetBlogPostEditDetails(int? id);

        // get blog post delete
        public Task<BlogPost> GetBlogPostDeleteDetails(int? id);

        // Slug functionality
        public Task<bool> ValidateSlugAsync(string title, int blogPostId);

        // get all blog posts
        public Task<List<BlogPost>> GetAllBlogPostsAsync();

        // get most popular blog post
        public Task<List<BlogPost>> GetMostPopularBlogPostsAsync();

        // get recent blog posts
        public Task<List<BlogPost>> GetRecentBlogPostsAsync(int count);

        // get categories
        public Task<List<Category>> GetCategoriesAsync();

        // get tags
        public Task<List<Tag>> GetTagsAsync();

        // add tags to blog posts
        public Task AddTagsToBlogPostsAsync(IEnumerable<int> tagIds, int blogPostId);

        // add tags to blog posts - Overloaded Method
        public Task AddTagsToBlogPostsAsync(string tagNames, int blogPostId);

        // remove all tags
        public Task RemoveAllBlogPostTagsAsync(int blogPostId);

        // search blog posts
        public IEnumerable<BlogPost> SearchBlogPosts(string searchStr);

        // get blog posts with tags
    }
}
