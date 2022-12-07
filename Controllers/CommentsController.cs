using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogMVC.Data;
using BlogMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace BlogMVC.Controllers
{
    public class CommentsController : Controller
    {
        // injections
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BlogUser> _userManager;

        public CommentsController(ApplicationDbContext context,
                                  UserManager<BlogUser> userManager)
        {
            // assign injected values
            _context = context;
            _userManager = userManager;
        }

        // GET: Comments
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Index()
        {
            List<Comment> comments = await _context.Comments
                                                    .Include(c => c.Author)
                                                    .Include(c => c.BlogPost)
                                                    .ToListAsync();

            return View(comments);
        }

        // GET: Comments/Details/5
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Comment? comment = await _context.Comments
                .Include(c => c.Author)
                .Include(c => c.BlogPost)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // GET: Comments/Create
        public IActionResult Create()
        {
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["BlogPostId"] = new SelectList(_context.BlogPosts, "Id", "Content");
            return View();
        }

        // POST: Comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,BlogPostId,Body")] Comment comment, string? slug)
        {
            // remove the author id
            ModelState.Remove("AuthorId");

            if (ModelState.IsValid)
            {
                comment.AuthorId = _userManager.GetUserId(User);

                // DateCreated
                comment.DateCreated = DateTime.UtcNow;

                // LastUpdated
                comment.LastUpdated = DateTime.UtcNow;

                _context.Add(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "BlogPosts", new { slug });
            }
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", comment.AuthorId);
            ViewData["BlogPostId"] = new SelectList(_context.BlogPosts, "Id", "Content", comment.BlogPostId);
            return View(comment);
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Comment? comment = await _context.Comments.FindAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            // if user is admin, mod, or comment author, see comment edit form
            if (User.IsInRole("Administrator") || User.IsInRole("Moderator") || comment.AuthorId == _userManager.GetUserId(User))
            {
                ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", comment.AuthorId);
                ViewData["BlogPostId"] = new SelectList(_context.BlogPosts, "Id", "Content", comment.BlogPostId);

                return View(comment);
            }
            else
            {
                return Unauthorized();
            }
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BlogPostId,AuthorId,DateCreated,LastUpdated,UpdateReason,Body")] Comment comment)
        {
            if (id != comment.Id)
            {
                return NotFound();
            }

            // if user is NOT admin or mod
            // if user wrote comment, let them edit
            // return error
            if (!(User.IsInRole("Administrator")) && !(User.IsInRole("Moderator")) && !(comment.AuthorId == _userManager.GetUserId(User)))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // set DateCreated
                    comment.DateCreated = DateTime.SpecifyKind(comment.DateCreated, DateTimeKind.Utc);

                    // check this
                    comment.LastUpdated = DateTime.SpecifyKind(comment.LastUpdated!.Value, DateTimeKind.Utc);

                    _context.Update(comment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "BlogPosts");
            }
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", comment.AuthorId);
            ViewData["BlogPostId"] = new SelectList(_context.BlogPosts, "Id", "Content", comment.BlogPostId);
            return View(comment);
        }

        // GET: Comments/Delete/5
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Comments == null)
            {
                return NotFound();
            }

            Comment? comment = await _context.Comments
                .Include(c => c.Author)
                .Include(c => c.BlogPost)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Comments == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Comments'  is null.");
            }

            Comment? comment = await _context.Comments.FindAsync(id);

            if (comment != null)
            {
                _context.Comments.Remove(comment);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CommentExists(int id)
        {
          return _context.Comments.Any(e => e.Id == id);
        }
    }
}
