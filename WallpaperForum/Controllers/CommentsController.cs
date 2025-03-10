using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WallpaperForum.Data;
using WallpaperForum.Models;

[Authorize]
public class CommentsController : Controller
{
    private readonly WallpaperForumContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CommentsController(WallpaperForumContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Comments/Create/5

    public IActionResult Create(int? discussionId)
    {

        System.Diagnostics.Debug.WriteLine($"GET Create called with discussionId: {discussionId}");

        if (discussionId == null || discussionId <= 0)
        {

            return RedirectToAction("Index", "Home");
        }


        var discussion = _context.Discussion.FirstOrDefault(d => d.DiscussionId == discussionId);
        if (discussion == null)
        {
            return NotFound("Discussion not found");
        }

        var comment = new Comment
        {
            DiscussionId = discussionId.Value
        };

        return View(comment);
    }

    // POST: Comments/Create/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Content,DiscussionId")] Comment comment)
    {
        try
        {
            
            System.Diagnostics.Debug.WriteLine($"POST Create called with DiscussionId: {comment.DiscussionId}, Content: {comment.Content}");

           
            if (comment.DiscussionId == 0)
            {
                ModelState.AddModelError("", "Discussion ID is required");
                return View(comment);
            }

          
            var discussion = await _context.Discussion.FindAsync(comment.DiscussionId);
            if (discussion == null)
            {
                ModelState.AddModelError("", "Discussion not found");
                return View(comment);
            }

            if (ModelState.IsValid)
            {
              
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError("", "User not authenticated");
                    return View(comment);
                }

                comment.ApplicationUserId = userId;
                comment.CreateDate = DateTime.Now;

               
                _context.Comment.Add(comment);
                await _context.SaveChangesAsync();


                return RedirectToAction("GetDiscussion", "Home", new { id = comment.DiscussionId });
            }

           
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

            return View(comment);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating comment: {ex.Message}");
            ModelState.AddModelError("", $"Error: {ex.Message}");
            return View(comment);
        }
    }


    // GET: Comments/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var comment = await _context.Comment
            .Include(c => c.Discussion)
            .FirstOrDefaultAsync(m => m.CommentId == id);

        if (comment == null)
        {
            return NotFound();
        }

       
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (comment.ApplicationUserId != userId)
        {
            return Forbid();
        }

        return View(comment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int discussionId)
    {
        try
        {
            var comment = await _context.Comment.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            // Verify if the current user is the comment author
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (comment.ApplicationUserId != userId)
            {
                return Forbid(); // If not the author, forbid the operation
            }

            // Verify if the discussionId matches 
            if (comment.DiscussionId != discussionId)
            {
                return BadRequest("Comment ID does not match Discussion ID");
            }

            
            _context.Comment.Remove(comment);
            await _context.SaveChangesAsync();

            
            return RedirectToAction("GetDiscussion", "Home", new { id = discussionId });
        }
        catch (Exception ex)
        {
           
            Console.WriteLine($"Error deleting comment: {ex.Message}");
            return RedirectToAction("GetDiscussion", "Home", new { id = discussionId });
        }
    }
}
