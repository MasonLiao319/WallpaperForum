using Microsoft.AspNetCore.Identity;
using WallpaperForum.Models;


namespace WallpaperForum.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public bool IsForHire { get; set; }
        public string Location { get; set; } = string.Empty;
        public string ImageFilename { get; set; } = string.Empty;

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Discussion> Discussions { get; set; } = new List<Discussion>();
    }

}