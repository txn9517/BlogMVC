using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogMVC.Data;
using BlogMVC.Models;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;
using BlogMVC.Services.Interfaces;

namespace BlogMVC.Controllers
{
    public class TagsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TagsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tags
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
              return View(await _context.Tags.ToListAsync());
        }

        // GET: Tags/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id, int? pageNum, int tagId)
        {
            if (id == null)
            {
                return NotFound();
            }

            // add page list functionality
            int pageSize = 10;
            // if pageNum is null, set equal to 1
            int page = pageNum ?? 1;

            // get the blog posts for specific tag
            IPagedList<BlogPost> blogPosts = _context.BlogPosts
                                                    .Where(b => b.IsDeleted == false && b.IsPublished == true)
                                                    .Include(b => b.Tags)
                                                    .Include(b => b.Category)
                                                    .Include(b => b.Creator)
                                                    .Include(b => b.Comments)
                                                    .Where(b => b.Tags.Any(t => t.Id == id))
                                                    .OrderBy(b => b.DateCreated)
                                                    .ToPagedList(page, pageSize);

            //Tag? tag = await _context.Tags
            //                    .Include(t => t.BlogPosts)
            //                        .ThenInclude(b => b.Category)
            //                    .FirstOrDefaultAsync(m => m.Id == id);

            //if (tag == null)
            //{
            //    return NotFound();
            //}

            if (blogPosts == null)
            {
                return NotFound();
            }

            return View(blogPosts);
        }

        // GET: Tags/Create
        [Authorize(Roles = "Administrator,Moderator")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tags/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Create([Bind("Id,Name")] Tag tag)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tag);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tag);
        }

        // GET: Tags/Edit/5
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tags == null)
            {
                return NotFound();
            }

            Tag? tag = await _context.Tags.FindAsync(id);

            if (tag == null)
            {
                return NotFound();
            }
            return View(tag);
        }

        // POST: Tags/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Tag tag)
        {
            if (id != tag.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tag);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TagExists(tag.Id))
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
            return View(tag);
        }

        // GET: Tags/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Tags == null)
            {
                return NotFound();
            }

            Tag? tag = await _context.Tags
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tag == null)
            {
                return NotFound();
            }

            return View(tag);
        }

        // POST: Tags/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Tags == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tags'  is null.");
            }

            Tag? tag = await _context.Tags.FindAsync(id);

            if (tag != null)
            {
                _context.Tags.Remove(tag);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TagExists(int id)
        {
          return _context.Tags.Any(e => e.Id == id);
        }
    }
}
