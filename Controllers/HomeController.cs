using BlogMVC.Data;
using BlogMVC.Models;
using BlogMVC.Services;
using BlogMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using X.PagedList;

namespace BlogMVC.Controllers
{
    public class HomeController : Controller
    {
        // injections
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IBlogPostService _blogPostService;
        private readonly IEmailSender _emailService;

        public HomeController(ILogger<HomeController> logger,
                                ApplicationDbContext context,
                                IBlogPostService blogPostService,
                                IEmailSender emailService)
        {
            // assign injected values
            _logger = logger;
            _context = context;
            _blogPostService = blogPostService;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index(int? pageNum, string? swalMessage = null)
        {
            // sweet alert message
            ViewData["SwalMessage"] = swalMessage;

            // add page list functionality
            int pageSize = 10;
            // if pageNum is null, set equal to 1
            int page = pageNum ?? 1;

            IPagedList<BlogPost> model = (await _blogPostService.GetAllBlogPostsAsync())
                                            .Where(b => b.IsDeleted == false && b.IsPublished == true)
                                            .ToPagedList(page, pageSize);

            return View(model);
        }

        // add action for search
        public async Task<IActionResult> SearchIndex(string searchStr, int? pageNum)
        {
            // add page list functionality
            int pageSize = 10;
            // if pageNum is null, set equal to 1
            int page = pageNum ?? 1;

            ViewData["SearchStr"] = searchStr;

            IPagedList<BlogPost> model = _blogPostService.SearchBlogPosts(searchStr)
                                                        .ToPagedList(page, pageSize);

            return View(nameof(Index), model);
        }

        // add contact me page
        [Authorize]
        public async Task<IActionResult> ContactMe()
        {
            EmailData model = new()
            {
                EmailAddress = User.Identity!.Name,
                Subject = "Contact Me: From My Blog"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ContactMe(EmailData model)
        {
            string? swalMessage = string.Empty;

            if (ModelState.IsValid)
            {
                try
                {
                    await _emailService.SendEmailAsync(model.EmailAddress, model.Subject, model.Message);

                    swalMessage = "Success: Email Sent!";

                    return RedirectToAction("Index", "Home", new { swalMessage });

                } catch (Exception)
                {
                    swalMessage = "Error: Email Send Failed";

                    return RedirectToAction("Index", "Home", new { swalMessage });

                    throw;
                }
            }

            swalMessage = "Redirecting you back to Contact Me page...";
            return RedirectToAction("ContactMe", "Home", new { swalMessage });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}