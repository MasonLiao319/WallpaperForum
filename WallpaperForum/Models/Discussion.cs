using WallpaperForum.Data;
namespace WallpaperForum.Models
{
    public class Discussion
    {
        public int DiscussionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string ImageFilename { get; set; } = string.Empty;
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string ApplicationUserId { get; set; } = string.Empty;
       
        public ApplicationUser? ApplicationUser { get; set; }
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public int CommentCount => Comments?.Count ?? 0;
    }
}