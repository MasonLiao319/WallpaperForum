using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WallpaperForum.Models;

namespace WallpaperForum.Data
{
    public class WallpaperForumContext : DbContext
    {
        public WallpaperForumContext (DbContextOptions<WallpaperForumContext> options)
            : base(options)
        {
        }

        public DbSet<WallpaperForum.Models.Discussion> Discussion { get; set; } = default!;
        public DbSet<WallpaperForum.Models.Comment> Comment { get; set; } = default!;
    }
}
