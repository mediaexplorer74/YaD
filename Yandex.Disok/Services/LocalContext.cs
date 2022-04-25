using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ya.D.Models;

namespace Ya.D.Services
{
    public class LocalContext : DbContext
    {
        public DbSet<DiskItem> Items { get; set; }
        public DbSet<PlayList> PlayLists { get; set; }
        public DbSet<ItemList> ItemsInPlaylist { get; set; }
        public DbSet<PlayListType> PlayListTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ItemList>()
                .HasKey(t => new { t.ItemID, t.PlayListID });

            modelBuilder.Entity<ItemList>()
                .HasOne(pt => pt.Item)
                .WithMany(p => p.PlayLists)
                .HasForeignKey(pt => pt.ItemID);

            modelBuilder.Entity<ItemList>()
                .HasOne(pt => pt.PlayList)
                .WithMany(t => t.Items)
                .HasForeignKey(pt => pt.PlayListID);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=yad.db");
#if DEBUG
            optionsBuilder.EnableSensitiveDataLogging();
#endif
        }
    }

    public static class LocalContextExtinsion
    {
        public static async Task InitMigrateAsync(this LocalContext context, bool clear = false)
        {
            if (clear)
            {
                // context.ItemsInPlaylist.RemoveRange(context.ItemsInPlaylist.Select(l => l));
                context.Items.RemoveRange(context.Items.Select(i => i));
                context.PlayLists.RemoveRange(context.PlayLists.Select(l => l));
                context.PlayListTypes.RemoveRange(context.PlayListTypes.Select(l => l));
            }
            var testTypes = await context.PlayListTypes.FirstOrDefaultAsync();
            if (testTypes == null)
            {
                await context.PlayListTypes.AddRangeAsync(new List<PlayListType>()
                {
                    new PlayListType() { ID = 1, Name = "Audios" },
                    new PlayListType() { ID = 2, Name = "Videos" }
                });
                await context.SaveChangesAsync();
            }

            var testPlayLists = await context.PlayLists.FirstOrDefaultAsync();
            if (testPlayLists == null)
            {
                await context.PlayLists.AddRangeAsync(new List<PlayList>()
                {
                    new PlayList() { ID = 1, Name = "Audios", Type = context.PlayListTypes.FirstOrDefault(t => t.ID == 1) },
                    new PlayList() { ID = 2, Name = "Videos", Type = context.PlayListTypes.FirstOrDefault(t => t.ID == 2) }
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
