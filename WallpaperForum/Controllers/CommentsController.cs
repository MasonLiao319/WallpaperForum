using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc;
using WallpaperForum.Data;
using WallpaperForum.Models;

namespace WallpaperForum.Controllers
{
    public class CommentsController : Controller
    {
        private readonly WallpaperForumContext _context;

        public CommentsController(WallpaperForumContext context)
        {
            _context = context;
        }

        // GET: Comments/Create
        public IActionResult Create(int discussionId)
        {
            if (discussionId <= 0)
            {
                return NotFound(); 
            }

           
            var comment = new Comment
            {
                DiscussionId = discussionId 
            };

            return View(comment); 
        }

        // POST: Comments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Content,DiscussionId")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                // Save the comment
                _context.Add(comment);
                await _context.SaveChangesAsync();

                // Redirect to the discussion page
                return RedirectToAction("GetDiscussion", "Home", new { id = comment.DiscussionId });
            }

            // If model state is invalid, return to the form
            return View(comment);
        }

    }
}
