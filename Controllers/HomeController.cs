using BlogMVC.Data;
using BlogMVC.Models;
using BlogMVC.Services.Interfaces;
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

        public HomeController(ILogger<HomeController> logger,
                                ApplicationDbContext context,
                                IBlogPostService blogPostService)
        {
            // assign injected values
            _logger = logger;
            _context = context;
            _blogPostService = blogPostService;
        }

        public async Task<IActionResult> Index(int? pageNum)
        {
            // add page list functionality
            int pageSize = 5;
            // if pageNum is null, set equal to 1
            int page = pageNum ?? 1;

            IPagedList<BlogPost> model = (await _blogPostService.GetAllBlogPostsAsync())
                                                                    .Where(b => b.IsDeleted == false && b.IsPublished == true)
                                                                    .ToPagedList(page, pageSize);

            return View(model);
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