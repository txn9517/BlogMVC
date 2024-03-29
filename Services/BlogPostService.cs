﻿using BlogMVC.Data;
using BlogMVC.Extensions;
using BlogMVC.Models;
using BlogMVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace BlogMVC.Services
{
    // implementations of the action methods
    public class BlogPostService : IBlogPostService
    {
        // inject database
        private readonly ApplicationDbContext _context;

        public BlogPostService(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<List<BlogPost>> GetAllBlogPostsPubAsync()
        {
            List<BlogPost> blogPosts = new List<BlogPost>();

            try
            {
                // get all posts published
                blogPosts = await _context.BlogPosts
                                          .Where(b => b.IsPublished == true && b.IsDeleted == false)
                                          .Include(b => b.Creator)
                                          .Include(b => b.Category)
                                          .Include(b => b.Comments)
                                          .Include(b => b.Tags)
                                          .OrderBy(b => b.DateCreated)
                                          .ToListAsync();
                return blogPosts;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<BlogPost>> GetAllBlogPostsUnpubAsync()
        {
            List<BlogPost> blogPosts = new List<BlogPost>();

            try
            {
                // get all posts unpublished
                blogPosts = await _context.BlogPosts
                                          .Where(b => b.IsPublished == false && b.IsDeleted == false)
                                          .Include(b => b.Category)
                                          .Include(b => b.Creator)
                                          .Include(b => b.Comments)
                                          .Include(b => b.Tags)
                                          .OrderBy(b => b.DateCreated)
                                          .ToListAsync();
                return blogPosts;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<BlogPost>> GetDeletedBlogPostsPubAsync()
        {
            List<BlogPost> blogPosts = new List<BlogPost>();

            try
            {
                // get deleted posts
                blogPosts = await _context.BlogPosts
                                    .Where(b => b.IsDeleted == true && b.IsPublished == true)
                                    .Include(b => b.Category)
                                    .Include(b => b.Creator)
                                    .Include(b => b.Comments)
                                    .Include(b => b.Tags)
                                    .OrderBy(b => b.DateCreated)
                                    .ToListAsync();

                return blogPosts;

            } catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<BlogPost>> GetDeletedBlogPostsUnpubAsync()
        {
            List<BlogPost> blogPosts = new List<BlogPost>();

            try
            {
                // get deleted posts
                blogPosts = await _context.BlogPosts
                                    .Where(b => b.IsDeleted == true && b.IsPublished == false)
                                    .Include(b => b.Category)
                                    .Include(b => b.Creator)
                                    .Include(b => b.Comments)
                                    .Include(b => b.Tags)
                                    .OrderBy(b => b.DateCreated)
                                    .ToListAsync();

                return blogPosts;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<BlogPost> GetBlogPostDetails(string? slug)
        {
            try
            {
                BlogPost? blogPost = await _context.BlogPosts
                                            .Include(b => b.Category)
                                            .Include(b => b.Comments)
                                                .ThenInclude(c => c.Author)
                                            .Include(b => b.Tags)
                                            .FirstOrDefaultAsync(m => m.Slug == slug);
                return blogPost!;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<BlogPost> GetBlogPostEditDetails(int? id)
        {
            try
            {
                BlogPost? blogPost = await _context.BlogPosts
                                            .Include(b => b.Tags)
                                            .FirstOrDefaultAsync(b => b.Id == id);

                return blogPost!;

            } catch (Exception)
            {
                throw;
            }
        }

        public async Task<BlogPost> GetBlogPostDeleteDetails(int? id)
        {
            try
            {
                BlogPost? blogPost = await _context.BlogPosts
                                                    .Include(b => b.Creator)
                                                    .Include(b => b.Category)
                                                    .Include(b => b.Comments)
                                                    .FirstOrDefaultAsync(m => m.Id == id);

                return blogPost!;

            } catch (Exception)
            {
                throw;
            }
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
                // get all blog posts from db
                List<BlogPost> blogPosts = await _context.BlogPosts
                                                        .Include(b => b.Comments)
                                                        .Include(b => b.Category)
                                                        .Include(b => b.Tags)
                                                        .Include(b => b.Creator)
                                                        .OrderBy(b => b.DateCreated)
                                                        .ToListAsync();

                return blogPosts;

            } catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<BlogPost>> GetMostPopularBlogPostsAsync()
        {
            try
            {
                List<BlogPost> blogPosts = await _context.BlogPosts
                                                            .Where(b => b.IsDeleted == false && b.IsPublished == true)
                                                            .Include(b => b.Comments)
                                                                .ThenInclude(c => c.Author)
                                                            .Include(b => b.Category)
                                                            .Include(b => b.Tags)
                                                            .ToListAsync();

                // .Take(3) for top 3 blog posts
                return blogPosts.OrderByDescending(b => b.Comments.Count).ToList();

            } catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<BlogPost>> GetRecentBlogPostsAsync(int count)
        {
            try
            {
                List<BlogPost> blogPosts = await _context.BlogPosts
                                                            .Where(b => b.IsDeleted == false && b.IsPublished == true)
                                                            .Include(b => b.Comments)
                                                                .ThenInclude(c => c.Author)
                                                            .Include(b => b.Category)
                                                            .Include(b => b.Tags)
                                                            .ToListAsync();

                return blogPosts.OrderByDescending(b=> b.DateCreated).Take(count).ToList();

            } catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            try
            {
                List<Category> categories = await _context.Categories
                                                                .Include(c => c.BlogPosts)
                                                                .ToListAsync();

                return categories;

            } catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Tag>> GetTagsAsync()
        {
            try
            {
                return await _context.Tags
                                        .Include(t => t.BlogPosts)
                                        .ToListAsync();

            } catch (Exception)
            {
                throw;
            }
        }

        public async Task AddTagsToBlogPostsAsync(IEnumerable<int> tagIds, int blogPostId)
        {
            try
            {
                BlogPost? blogPost = await _context.BlogPosts.FindAsync(blogPostId);

                foreach(int tagId in tagIds)
                {
                    Tag? tag = await _context.Tags.FindAsync(tagId);

                    if (blogPost != null && tag != null)
                    {
                        blogPost.Tags.Add(tag);
                    }
                }

                await _context.SaveChangesAsync();

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task AddTagsToBlogPostsAsync(string tagNames, int blogPostId)
        {
            try
            {
                BlogPost? blogPost = await _context.BlogPosts.FindAsync(blogPostId);

                // guard statement
                if (blogPost == null) return;

                // comma delimited list
                foreach(string tagName in tagNames.Split(","))
                {
                    if (string.IsNullOrEmpty(tagName.Trim())) continue;

                    Tag? tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name.Trim().ToLower() == tagName.Trim().ToLower());

                    if (tag != null)
                    {
                        blogPost.Tags.Add(tag);
                    } 
                    else
                    {
                        Tag newTag = new Tag() { Name = tagName.Trim() };

                        blogPost.Tags.Add(newTag);
                    }
                }

                await _context.SaveChangesAsync();

            } catch (Exception)
            {
                throw;
            }
        }

        public async Task RemoveAllBlogPostTagsAsync(int blogPostId)
        {
            try
            {
                BlogPost? blogPost = await _context.BlogPosts
                                                 .Include(b => b.Tags)
                                                 .FirstOrDefaultAsync(b => b.Id == blogPostId);

                blogPost!.Tags.Clear();
                _context.Update(blogPost);
                await _context.SaveChangesAsync();

            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<BlogPost> SearchBlogPosts(string searchStr)
        {
            try
            {
                IEnumerable<BlogPost> blogPosts = new List<BlogPost>();

                if (string.IsNullOrEmpty(searchStr))
                {
                    return blogPosts;
                }
                else
                {
                    // removes any leading or trailing whitespace
                    searchStr = searchStr.Trim().ToLower();

                    // attach the following attributes to the search
                    blogPosts = _context.BlogPosts.Where(b => b.Title!.ToLower().Contains(searchStr) ||
                                                        b.Abstract!.ToLower().Contains(searchStr) ||
                                                        b.Content!.ToLower().Contains(searchStr) ||
                                                        b.Category!.Name!.ToLower().Contains(searchStr) ||
                                                        b.Comments.Any(c => c.Body!.ToLower().Contains(searchStr) ||
                                                                            c.Author!.FirstName!.ToLower().Contains(searchStr) ||
                                                                            c.Author!.LastName!.ToLower().Contains(searchStr) ) ||
                                                        b.Tags!.Any(t => t.Name!.ToLower().Contains(searchStr) ) )
                                                    .Where(b => b.IsDeleted == false && b.IsPublished == true)
                                                    .Include(b => b.Comments)
                                                        .ThenInclude(c => c.Author)
                                                    .Include(b => b.Category)
                                                    .Include(b => b.Tags)
                                                    .Include(b => b.Creator)
                                                    .AsNoTracking()
                                                    .OrderBy(b => b.DateCreated)
                                                    .AsEnumerable();

                    return blogPosts;
                }

            } catch (Exception)
            {
                throw;
            }
        }
    }
}
