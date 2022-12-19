using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogMVC.Data;
using BlogMVC.Models;
using BlogMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using BlogMVC.Extensions;
using BlogMVC.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing.Printing;
using X.PagedList;

namespace BlogMVC.Controllers
{
    public class BlogPostsController : Controller
    {
        // injections
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly IBlogPostService _blogPostService;
        private readonly UserManager<BlogUser> _userManager;

        public BlogPostsController(ApplicationDbContext context,
                                   IImageService imageService,
                                   IBlogPostService blogPostService,
                                   UserManager<BlogUser> userManager)
        {
            // assign injected values
            _context = context;
            _imageService = imageService;
            _blogPostService = blogPostService;
            _userManager = userManager;
        }

        // GET: BlogPosts
        [AllowAnonymous]
        public async Task<IActionResult> Index(int? pageNum)
        {
            // add page list functionality
            int pageSize = 10;
            // if pageNum is null, set equal to 1
            int page = pageNum ?? 1;

            IPagedList<BlogPost> blogPosts = (await _blogPostService.GetAllBlogPostsPubAsync()).ToPagedList(page, pageSize);

            return View(blogPosts);
        }

        // GET: BlogPosts/PostsUnpub
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> PostsUnpub(int? pageNum)
        {
            // add page list functionality
            int pageSize = 10;
            // if pageNum is null, set equal to 1
            int page = pageNum ?? 1;

            IPagedList<BlogPost> blogPosts = (await _blogPostService.GetAllBlogPostsUnpubAsync()).ToPagedList(page, pageSize);

            return View(blogPosts);
        }

        // GET: BlogPosts/DeletedPostsPub
        [Authorize(Roles=("Administrator"))]
        public async Task<IActionResult> DeletedPostsPub(int? pageNum)
        {
            // add page list functionality
            int pageSize = 10;
            // if pageNum is null, set equal to 1
            int page = pageNum ?? 1;

            // call service to view deleted blogs
            IPagedList<BlogPost> blogPosts = (await _blogPostService.GetDeletedBlogPostsPubAsync()).ToPagedList(page, pageSize);

            return View(blogPosts);
        }

        // GET: BlogPosts/DeletedPostsUnpub
        [Authorize(Roles=("Administrator"))]
        public async Task<IActionResult> DeletedPostsUnpub(int? pageNum)
        {
            // add page list functionality
            int pageSize = 10;
            // if pageNum is null, set equal to 1
            int page = pageNum ?? 1;

            // call service to view deleted blogs
            IPagedList<BlogPost> blogPosts = (await _blogPostService.GetDeletedBlogPostsUnpubAsync()).ToPagedList(page, pageSize);

            return View(blogPosts);
        }

        // GET: BlogPosts/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(string? slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            BlogPost? blogPost = await _blogPostService.GetBlogPostDetails(slug);

            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
        }

        // GET: BlogPosts/Create
        [Authorize(Roles="Administrator,Moderator")]
        public async Task<IActionResult> Create()
        {
            BlogPost blogPost = new BlogPost();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["BlogPostTags"] = new MultiSelectList(await _blogPostService.GetTagsAsync(), "Id", "Name");

            return View(blogPost);
        }

        // POST: BlogPosts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,CategoryId,Abstract,IsDeleted,IsPublished,BlogPostImage")] BlogPost blogPost, string stringTags)
        {
            ModelState.Remove("CreatorId");

            if (ModelState.IsValid)
            {
                // assign CreatorId
                blogPost.CreatorId = _userManager.GetUserId(User);

                // check to see if there is a Slug
                if (!await _blogPostService.ValidateSlugAsync(blogPost.Title!, blogPost.Id))
                {
                    ModelState.AddModelError("Title", "A similar Title or Slug has already been used!");
                    ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);
                    return View(blogPost);
                }
                blogPost.Slug = blogPost.Title!.Slugify();

                // set dateCreated
                blogPost.DateCreated = DateTime.UtcNow;

                // set the ImageData and ImageType of image file being uploaded
                if (blogPost.BlogPostImage != null)
                {
                    // convert file to a byte array
                    blogPost.ImageData = await _imageService.ConvertFileToByteArrayAsync(blogPost.BlogPostImage);
                    // get the raw Content-Type header of the uploaded file
                    blogPost.ImageType = blogPost.BlogPostImage.ContentType;
                }

                _context.Add(blogPost);
                await _context.SaveChangesAsync();

                // TODO: add tags selected by the end user
                await _blogPostService.AddTagsToBlogPostsAsync(stringTags, blogPost.Id);

                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);
            ViewData["BlogPostTags"] = new MultiSelectList(await _blogPostService.GetTagsAsync(), "Id", "Name");

            return View(blogPost);
        }

        // GET: BlogPosts/Edit/5
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            BlogPost? blogPost = await _blogPostService.GetBlogPostEditDetails(id);

            if (blogPost == null)
            {
                return NotFound();
            }

            // could use a service here
            IEnumerable<int> blogPostTags = blogPost.Tags.Select(t => t.Id);

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);
            ViewData["BlogPostTags"] = new MultiSelectList(await _blogPostService.GetTagsAsync(), "Id", "Name", blogPostTags);

            return View(blogPost);
        }

        // POST: BlogPosts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CreatorId,Title,Content,DateCreated,Abstract,CategoryId,IsDeleted,IsPublished,ImageData,ImageType,BlogPostImage")] BlogPost blogPost, string stringTags)
        {
            if (id != blogPost.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // check to see if there is a Slug
                    if (!await _blogPostService.ValidateSlugAsync(blogPost.Title!, blogPost.Id))
                    {
                        ModelState.AddModelError("Title", "A similar Title or Slug has already been used!");
                        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);
                        return View(blogPost);
                    }
                    blogPost.Slug = blogPost.Title!.Slugify();

                    // set DateCreated
                    blogPost.DateCreated = DateTime.SpecifyKind(blogPost.DateCreated, DateTimeKind.Utc);

                    // set the ImageData and ImageType of image file being uploaded
                    if (blogPost.BlogPostImage != null)
                    {
                        // convert file to a byte array
                        blogPost.ImageData = await _imageService.ConvertFileToByteArrayAsync(blogPost.BlogPostImage);
                        // get the raw Content-Type header of the uploaded file
                        blogPost.ImageType = blogPost.BlogPostImage.ContentType;
                    }

                    _context.Update(blogPost);
                    await _context.SaveChangesAsync();

                    // remove current blog post tags
                    await _blogPostService.RemoveAllBlogPostTagsAsync(blogPost.Id);

                    // add tags selected to the blog post
                    await _blogPostService.AddTagsToBlogPostsAsync(stringTags, blogPost.Id);

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogPostExists(blogPost.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);

            return View(blogPost);
        }

        // GET: BlogPosts/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BlogPosts == null)
            {
                return NotFound();
            }

            //BlogPost? blogPost = await _context.BlogPosts
            //    .Include(b => b.Category)
            //    .FirstOrDefaultAsync(m => m.Id == id);

            BlogPost? blogPost = await _blogPostService.GetBlogPostDeleteDetails(id);

            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
        }

        // POST: BlogPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BlogPosts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.BlogPosts'  is null.");
            }

            BlogPost? blogPost = await _context.BlogPosts.FindAsync(id);

            if (blogPost != null)
            {
                // set deleted blog post to true
                blogPost.IsDeleted = true;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BlogPostExists(int id)
        {
            return _context.BlogPosts.Any(e => e.Id == id);
        }
    }
}
