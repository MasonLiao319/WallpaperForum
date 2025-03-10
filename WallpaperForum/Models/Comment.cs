using WallpaperForum.Data;
namespace WallpaperForum.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreateDate { get; set; }
        public int DiscussionId { get; set; }
        
        public Discussion? Discussion { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        
        public ApplicationUser? ApplicationUser { get; set; }
    }
}
