using BlogMVC.Models;

namespace BlogMVC.Services.Interfaces
{
    // actions for working with blog posts
    public interface IBlogPostService
    {
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

        // remove all tags
        public Task RemoveAllBlogPostTagsAsync(int blogPostId);
    }
}
