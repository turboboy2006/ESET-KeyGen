using Microsoft.EntityFrameworkCore;
using RankTracker.Core.Models;

namespace RankTracker.Core.Data
{
    public class RankTrackerDbContext : DbContext
    {
        public RankTrackerDbContext(DbContextOptions<RankTrackerDbContext> options) : base(options)
        {
        }

        public DbSet<Website> Websites { get; set; }
        public DbSet<Keyword> Keywords { get; set; }
        public DbSet<WebsiteKeyword> WebsiteKeywords { get; set; }
        public DbSet<RankingLog> RankingLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WebsiteKeyword>()
                .HasKey(wk => new { wk.WebsiteId, wk.KeywordId });

            modelBuilder.Entity<WebsiteKeyword>()
                .HasOne(wk => wk.Website)
                .WithMany(w => w.WebsiteKeywords)
                .HasForeignKey(wk => wk.WebsiteId);

            modelBuilder.Entity<WebsiteKeyword>()
                .HasOne(wk => wk.Keyword)
                .WithMany(k => k.WebsiteKeywords)
                .HasForeignKey(wk => wk.KeywordId);

            modelBuilder.Entity<RankingLog>()
                .HasOne(rl => rl.Website)
                .WithMany()
                .HasForeignKey(rl => rl.WebsiteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RankingLog>()
                .HasOne(rl => rl.Keyword)
                .WithMany()
                .HasForeignKey(rl => rl.KeywordId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
