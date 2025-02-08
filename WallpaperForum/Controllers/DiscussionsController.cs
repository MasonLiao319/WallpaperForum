using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WallpaperForum.Data;
using WallpaperForum.Models;

namespace WallpaperForum.Controllers
{
    public class DiscussionsController : Controller
    {
        private readonly WallpaperForumContext _context;

        public DiscussionsController(WallpaperForumContext context)
        {
            _context = context;
        }

        // GET: Discussions

        public async Task<IActionResult> Index()
        {
            var discussions = await _context.Discussion
                .Include(d => d.Comments) 
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
                .FirstOrDefaultAsync(m => m.DiscussionId == id);
            if (discussion == null)
            {
                return NotFound();
            }

            return View(discussion);
        }

        // GET: Discussions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Discussions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content")] Discussion discussion, IFormFile Image)
        {
            if (ModelState.IsValid)
            {
                if (Image != null && Image.Length > 0)
                {
                    
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);

                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", uniqueFileName);

                    
                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await Image.CopyToAsync(stream);
                    }
     
                    discussion.ImageFilename = uniqueFileName;
                }

                discussion.CreateDate = DateTime.Now; 
                _context.Add(discussion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(discussion);
        }


        // GET: Discussions/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var discussion = await _context.Discussion.FindAsync(id);
            if (discussion == null)
            {
                return NotFound();
            }
            return View(discussion);
        }


        // POST: Discussions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DiscussionId,Title,Content")] Discussion discussion, IFormFile Image)
        {
            if (id != discussion.DiscussionId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(discussion);
            }

            try
            {
                var existingDiscussion = await _context.Discussion.FindAsync(id);
                if (existingDiscussion == null)
                {
                    return NotFound();
                }

                bool isUpdated = false;

                
                if (existingDiscussion.Title != discussion.Title || existingDiscussion.Content != discussion.Content)
                {
                    existingDiscussion.Title = discussion.Title;
                    existingDiscussion.Content = discussion.Content;
                    _context.Entry(existingDiscussion).Property(x => x.Title).IsModified = true;
                    _context.Entry(existingDiscussion).Property(x => x.Content).IsModified = true;
                    isUpdated = true;
                }

                if (Image != null && Image.Length > 0)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", uniqueFileName);

                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await Image.CopyToAsync(stream);
                    }

                    if (!string.IsNullOrEmpty(existingDiscussion.ImageFilename))
                    {
                        string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", existingDiscussion.ImageFilename);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    
                    existingDiscussion.ImageFilename = uniqueFileName;
                    _context.Entry(existingDiscussion).Property(x => x.ImageFilename).IsModified = true;
                    isUpdated = true;
                }

                if (isUpdated)
                {
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Discussion.Any(e => e.DiscussionId == discussion.DiscussionId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch
            {
                return View(discussion);
            }
        }



        // GET: Discussions/Delete/5 
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discussion = await _context.Discussion
                .FirstOrDefaultAsync(m => m.DiscussionId == id);

            if (discussion == null)
            {
                return NotFound();
            }

            return View(discussion);
        }

        // POST: Discussions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var discussion = await _context.Discussion.FindAsync(id);

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
    }
}
