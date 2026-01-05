using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Models;

namespace SocialMediaApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>
        options)
        : base(options)
        {

        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Follows> Follows { get; set; }
        public DbSet<GroupMessage> GroupMessages { get; set; }
        public DbSet<GroupUser> GroupUsers { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Likes> Likes { get; set; } 
        public DbSet<Post> Posts { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<GroupJoinRequest> GroupJoinRequests { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // chei compuse configurate ca in cursul 10, pag 7

            modelBuilder.Entity<Follows>()
                .HasKey(uf => new { uf.FollowerId, uf.FollowedId });

            modelBuilder.Entity<Follows>()
                .HasOne(uf => uf.Follower)
                .WithMany(u => u.Follows)
                .HasForeignKey(uf => uf.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Follows>()
                .HasOne(uf => uf.Followed)
                .WithMany(u => u.Followers)
                .HasForeignKey(uf => uf.FollowedId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Likes>()
                .HasKey(l => new { l.UserId, l.PostId });

            modelBuilder.Entity<Likes>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId);

            modelBuilder.Entity<Likes>()
                .HasOne(l => l.Post)
                .WithMany(p => p.WhoLiked)
                .HasForeignKey(l => l.PostId);

            // index unic ca sa nu poata un user sa dea like de doua ori la aceeasi postare
            modelBuilder.Entity<Likes>()
                .HasIndex(l => new { l.UserId, l.PostId })
                .IsUnique();

            modelBuilder.Entity<GroupUser>()
                .HasKey(gu => new { gu.UserId, gu.GroupId });

            modelBuilder.Entity<GroupUser>()
                .HasOne(gu => gu.User)
                .WithMany(u => u.Groups)
                .HasForeignKey(gu => gu.UserId);

            modelBuilder.Entity<GroupUser>()
                .HasOne(gu => gu.Group)
                .WithMany(g => g.Users)
                .HasForeignKey(g => g.GroupId);

            modelBuilder.Entity<GroupMessage>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.SetNull);//mesaj cu user null
        }
    }
}
