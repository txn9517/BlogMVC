using BlogMVC.Services.Interfaces;

namespace BlogMVC.Services
{
    public class BlogPostService : IBlogPostService
    {
        public async Task<bool> ValidateSlugAsync(string title, int blogPostId)
        {
            throw new NotImplementedException();
        }
    }
}
