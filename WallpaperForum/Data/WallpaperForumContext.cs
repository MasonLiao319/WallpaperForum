using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WallpaperForum.Models;
using WallpaperForum.Data;

namespace WallpaperForum.Data
{
    public class WallpaperForumContext : IdentityDbContext<ApplicationUser>
    {
        public WallpaperForumContext (DbContextOptions<WallpaperForumContext> options)
            : base(options)
        {
        }

        public DbSet<WallpaperForum.Models.Discussion> Discussion { get; set; } = default!;
        public DbSet<WallpaperForum.Models.Comment> Comment { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Discussion>()
                .HasOne(d => d.ApplicationUser)
                .WithMany(u => u.Discussions)
                .HasForeignKey(d => d.ApplicationUserId);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Discussion)
                .WithMany(d => d.Comments)
                .HasForeignKey(c => c.DiscussionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ApplicationUser)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.ApplicationUserId)
                .OnDelete(DeleteBehavior.NoAction); 
        }

    }
}
