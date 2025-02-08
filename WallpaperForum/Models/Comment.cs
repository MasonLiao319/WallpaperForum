namespace WallpaperForum.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreateDate { get; set; } = DateTime.Now;

        // Foreign key for Discussion
        public int DiscussionId { get; set; }

        // Navigation property to represent the relationship with Discussion
        public Discussion? Discussion { get; set; }
    }
}
