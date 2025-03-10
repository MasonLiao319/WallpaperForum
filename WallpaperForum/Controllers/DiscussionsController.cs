using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WallpaperForum.Data;
using WallpaperForum.Models;

namespace WallpaperForum.Controllers
{
    [Authorize]
    public class DiscussionsController : Controller
    {
        private readonly WallpaperForumContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DiscussionsController(WallpaperForumContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Discussions
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var discussions = await _context.Discussion
                .Where(m => m.ApplicationUserId == userId)
                .Include(d => d.Comments)
                .Include(d => d.ApplicationUser) 
                .AsNoTracking()
                .OrderByDescending(d => d.CreateDate)
                .ToListAsync();

            return View(discussions);
        }

        // GET: Discussions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discussion = await _context.Discussion
                .Include(d => d.ApplicationUser) 
                .Include(d => d.Comments)        
                .FirstOrDefaultAsync(m => m.DiscussionId == id);

            if (discussion == null)
            {
                return NotFound();
            }

            
            if (discussion.Comments != null && discussion.Comments.Any())
            {
                foreach (var comment in discussion.Comments)
                {
                    var commentAuthor = await _context.Users
                        .FirstOrDefaultAsync(u => u.Id == comment.ApplicationUserId);

                    if (commentAuthor != null)
                    {
                        comment.ApplicationUser = commentAuthor as ApplicationUser;
                    }
                }
            }

            SetUserViewData(discussion);
            return View(discussion);
        }

        // GET: Discussions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Discussions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
     
        public async Task<IActionResult> Create([Bind("Title,Content")] Discussion discussion, IFormFile Image)
        {
            try
            {
               
                ModelState.Remove("ApplicationUser");

                if (ModelState.IsValid)
                {
                    // Set user ID
                    discussion.ApplicationUserId = _userManager.GetUserId(User);

                    // Get current user
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser == null)
                    {
                        ModelState.AddModelError("", "Unable to retrieve current user information");
                        return View(discussion);
                    }

                    // Set creation date
                    discussion.CreateDate = DateTime.Now;

                    // Handle image upload
                    if (Image != null && Image.Length > 0)
                    {
                        try
                        {
                            string uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                            if (!Directory.Exists(uploadDirectory))
                            {
                                Directory.CreateDirectory(uploadDirectory);
                            }

                            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                            string uploadPath = Path.Combine(uploadDirectory, uniqueFileName);

                            using (var stream = new FileStream(uploadPath, FileMode.Create))
                            {
                                await Image.CopyToAsync(stream);
                            }

                            discussion.ImageFilename = uniqueFileName;
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("Image", $"Failed to upload file: {ex.Message}");
                            return View(discussion);
                        }
                    }

                    // Add to database
                    _context.Add(discussion);

                    // Attach the user manually to ensure relationship is properly set
                    _context.Entry(currentUser).State = EntityState.Unchanged;

                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateException dbEx)
                    {
                        // Log detailed database error
                        var errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                        ModelState.AddModelError("", $"Database error when saving: {errorMessage}");
                        System.Diagnostics.Debug.WriteLine($"Database error: {errorMessage}");
                        return View(discussion);
                    }
                }

                // Log validation errors 
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Count > 0)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            System.Diagnostics.Debug.WriteLine($"Field {state.Key} error: {error.ErrorMessage}");
                        }
                    }
                }

                return View(discussion);
            }
            catch (Exception ex)
            {
                
                System.Diagnostics.Debug.WriteLine($"Error creating discussion: {ex.Message}");
                ModelState.AddModelError("", $"Failed to create discussion: {ex.Message}");
                return View(discussion);
            }
        }

        // GET: Discussions/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var discussion = await _context.Discussion
                .Include(d => d.ApplicationUser)
                .FirstOrDefaultAsync(m => m.DiscussionId == id && m.ApplicationUserId == userId);

            if (discussion == null)
            {
                return NotFound();
            }
            return View(discussion);
        }

        // POST: Discussions/Edit/5
        // DiscussionsController.cs
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DiscussionId,Title,Content,ImageFilename,CreateDate,ApplicationUserId")] Discussion discussion, IFormFile Image)
        {
            if (id != discussion.DiscussionId)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var existingDiscussion = await _context.Discussion.FindAsync(id);

            if (existingDiscussion == null || existingDiscussion.ApplicationUserId != userId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Keep original name
                    string originalImageFilename = existingDiscussion.ImageFilename;

                    // Update info
                    existingDiscussion.Title = discussion.Title;
                    existingDiscussion.Content = discussion.Content;

                    // Process newImage
                    if (Image != null && Image.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                        string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", uniqueFileName);

                        using (var stream = new FileStream(uploadPath, FileMode.Create))
                        {
                            await Image.CopyToAsync(stream);
                        }

                        // Delete oldImage
                        if (!string.IsNullOrEmpty(originalImageFilename))
                        {
                            string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", originalImageFilename);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        existingDiscussion.ImageFilename = uniqueFileName;
                    }

                    // Update db
                    _context.Update(existingDiscussion);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Discussion.Any(e => e.DiscussionId == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(discussion);
        }

        // GET: Discussions/Delete/5 
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var discussion = await _context.Discussion
                .Include(d => d.ApplicationUser) 
                .FirstOrDefaultAsync(m => m.DiscussionId == id && m.ApplicationUserId == userId);

            if (discussion == null)
            {
                return NotFound();
            }

            SetUserViewData(discussion); 
            return View(discussion);
        }


        // POST: Discussions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var discussion = await _context.Discussion
                .Where(m => m.ApplicationUserId == userId)
                .FirstOrDefaultAsync(m => m.DiscussionId == id);

            if (discussion != null)
            {
                if (!string.IsNullOrEmpty(discussion.ImageFilename))
                {
                    string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", discussion.ImageFilename);

                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Discussion.Remove(discussion);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DiscussionExists(int id)
        {
            return _context.Discussion.Any(e => e.DiscussionId == id);
        }

        
        private string GetUserProfileImageUrl(ApplicationUser user)
        {
            if (user == null || string.IsNullOrEmpty(user.ImageFilename))
            {
                
                return "/images/default-avatar.png";
            }

            
            return $"/profile_img/{user.ImageFilename}";
        }

       
        private void SetUserViewData(Discussion discussion)
        {
            if (discussion?.ApplicationUser != null)
            {
                ViewData["AuthorName"] = !string.IsNullOrEmpty(discussion.ApplicationUser.Name)
                    ? discussion.ApplicationUser.Name
                    : "Anonymous";
                ViewData["AuthorImageUrl"] = GetUserProfileImageUrl(discussion.ApplicationUser);
            }
        }
    }
}