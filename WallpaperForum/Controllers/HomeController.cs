using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WallpaperForum.Data;
using WallpaperForum.Models;

namespace WallpaperForum.Controllers
{
    public class HomeController : Controller
    {
        
        private readonly WallpaperForumContext _context;

        public HomeController(ILogger<HomeController> logger, WallpaperForumContext context)
        {
            
            _context = context;
        }

        
           public async Task<IActionResult> Index()
        {
            var discussions = await _context.Discussion
                .OrderByDescending(d => d.CreateDate) 
                .ToListAsync();

            return View(discussions);
        }
        

        public async Task<IActionResult> GetDiscussion(int id)
        {
            // Fetch the discussion and its related comments in descending order by create date
            var discussion = await _context.Discussion
                .Include(d => d.Comments)
                .FirstOrDefaultAsync(d => d.DiscussionId == id);

            if (discussion == null)
            {
                return NotFound();
            }

            // Order comments by CreateDate descending
            discussion.Comments = discussion.Comments?.OrderByDescending(c => c.CreateDate).ToList();

            return View(discussion); 
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
