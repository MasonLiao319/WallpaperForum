using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WallpaperForum.Data;
using WallpaperForum.Models;

namespace WallpaperForum.Controllers
{
    public class HomeController : Controller
    {
        
        private readonly WallpaperForumContext _context;
        private readonly UserManager<ApplicationUser> _userManager; 

        public HomeController(
            ILogger<HomeController> logger,
            WallpaperForumContext context,
            UserManager<ApplicationUser> userManager) 
        {
            
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var discussions = await _context.Discussion
                .Include(d => d.ApplicationUser) 
                .OrderByDescending(d => d.CreateDate)
                .ToListAsync();
            return View(discussions);
        }

        public async Task<IActionResult> GetDiscussion(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var discussion = await _context.Discussion
                .Include(d => d.ApplicationUser)
                .Include(d => d.Comments)
                    .ThenInclude(c => c.ApplicationUser)
                .FirstOrDefaultAsync(m => m.DiscussionId == id);
            if (discussion == null)
            {
                return NotFound();
            }
            return View(discussion);
        }

        // GET: Home/Profile/{username}
        
        public async Task<IActionResult> Profile(string username, string id = null)
        {
            ApplicationUser user = null;

            if (!string.IsNullOrEmpty(username))
            {
                user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Name == username);
            }

            
            if (user == null && !string.IsNullOrEmpty(id))
            {
                user = await _userManager.FindByIdAsync(id);
            }

            if (user == null)
            {
                return NotFound();
            }

            
            var userDiscussions = await _context.Discussion
                .Where(d => d.ApplicationUserId == user.Id)
                .OrderByDescending(d => d.CreateDate)
                .ToListAsync();

            
            ViewBag.UserDiscussions = userDiscussions;

            return View(user);
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