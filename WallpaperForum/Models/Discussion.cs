namespace WallpaperForum.Models
{
    public class Discussion
    {
        public int DiscussionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string ImageFilename { get; set; } = string.Empty;
        public DateTime CreateDate { get; set; } = DateTime.Now;

        // Navigation property to represent the relationship with Comment
        public ICollection<Comment>? Comments { get; set; }

        public int CommentCount => Comments?.Count ?? 0;
    }
}
