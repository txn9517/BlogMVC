using BlogMVC.Data;
using BlogMVC.Extensions;
using BlogMVC.Models;
using BlogMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogMVC.Services
{
    public class BlogPostService : IBlogPostService
    {
        // inject database
        private readonly ApplicationDbContext _context;

        public BlogPostService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ValidateSlugAsync(string title, int blogPostId)
        {
            try
            {
                string newSlug = title.Slugify();

                if (blogPostId == 0)
                {
                    // check to see if there's any alike blog entries
                    return !(await _context.BlogPosts.AnyAsync(b => b.Slug == newSlug));
                }
                else
                {
                    BlogPost? blogPost = await _context.BlogPosts.AsNoTracking().FirstOrDefaultAsync(b => b.Id == blogPostId);

                    string? oldSlug = blogPost?.Slug;

                    // check to see if the new Title/newSlug is the same as the old Title/oldSlug
                    if (!string.Equals(newSlug, oldSlug))
                    {
                        // now check to see if the Title/newSlug already exists in the db
                        return !(await _context.BlogPosts.AnyAsync(b => b.Id != blogPostId && b.Slug == newSlug));
                    }
                }

                return true;

            } catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<BlogPost>> GetAllBlogPostsAsync()
        {
            try
            {
                List<BlogPost> blogPosts = await _context.BlogPosts
                                                        .Include(b => b.Comments)
                                                        .Include(b => b.Category)
                                                        .Include(b => b.Tags)
                                                        .OrderByDescending(b => b.DateCreated)
                                                        .ToListAsync();

                return blogPosts;

            } catch (Exception)
            {
                throw;
            }
        }
    }
}
