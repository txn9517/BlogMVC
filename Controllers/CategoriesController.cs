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
using BlogMVC.Services;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;

namespace BlogMVC.Controllers
{
    public class CategoriesController : Controller
    {
        // injections
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;

        public CategoriesController(ApplicationDbContext context, 
                                    IImageService imageService)
        {
            // assign injected values
            _context = context;
            _imageService = imageService;
        }

        // GET: Categories
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
              return View(await _context.Categories.ToListAsync());
        }

        // GET: Categories/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id, int? pageNum)
        {
            if (id == null)
            {
                return NotFound();
            }

            // add page list functionality
            int pageSize = 10;
            // if pageNum is null, set equal to 1
            int page = pageNum ?? 1;

            // get the blog posts for specific category
            IPagedList<BlogPost> blogPosts = _context.BlogPosts
                                                    .Where(b => b.CategoryId == id && b.IsDeleted == false && b.IsPublished == true)
                                                    .Include(b => b.Creator)
                                                    .Include(b => b.Comments)
                                                    .Include(b => b.Category)
                                                    .Include(b => b.Tags)
                                                    .OrderBy(b => b.DateCreated)
                                                    .ToPagedList(page, pageSize);

            if (blogPosts == null)
            {
                return NotFound();
            }

            return View(blogPosts);
        }

        // GET: Categories/Create
        [Authorize(Roles = "Administrator,Moderator")]
        public IActionResult Create()
        {
            return View(new Category());
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,CategoryImage")] Category category)
        {
            if (ModelState.IsValid)
            {
                // set the ImageData and ImageType of image file being uploaded
                if (category.CategoryImage != null)
                {
                    // convert file to a byte array
                    category.ImageData = await _imageService.ConvertFileToByteArrayAsync(category.CategoryImage);
                    // get the raw Content-Type header of the uploaded file
                    category.ImageType = category.CategoryImage.ContentType;
                }

                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            Category? category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ImageData,ImageType,CategoryImage")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // set the ImageData and ImageType of image file being uploaded
                    if (category.CategoryImage != null)
                    {
                        // convert file to a byte array
                        category.ImageData = await _imageService.ConvertFileToByteArrayAsync(category.CategoryImage);
                        // get the raw Content-Type header of the uploaded file
                        category.ImageType = category.CategoryImage.ContentType;
                    }

                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            return View(category);
        }

        // GET: Categories/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            Category? category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Categories'  is null.");
            }

            Category? category = await _context.Categories.FindAsync(id);

            if (category != null)
            {
                _context.Categories.Remove(category);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
          return _context.Categories.Any(e => e.Id == id);
        }
    }
}
