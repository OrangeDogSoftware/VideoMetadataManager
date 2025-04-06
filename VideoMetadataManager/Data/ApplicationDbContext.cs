using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using VideoMetadataManager.Models;

namespace VideoMetadataManager.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Définition des DbSets pour chaque entité
        public DbSet<VideoFile> VideoFiles { get; set; }
        public DbSet<VideoTag> VideoTags { get; set; }
        public DbSet<CustomMetadata> CustomMetadata { get; set; }

        // Constructeur par défaut
        public ApplicationDbContext()
        {
            // Assurez-vous que le répertoire de données existe
            var dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VideoMetadataManager");
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var dbPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "VideoMetadataManager",
                    "videometadata.db");

                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration de la relation many-to-many entre VideoFile et VideoTag
            modelBuilder.Entity<VideoFile>()
                .HasMany(v => v.Tags)
                .WithMany(t => t.Videos)
                .UsingEntity(j => j.ToTable("VideoFileTags"));

            // Indexation pour de meilleures performances
            modelBuilder.Entity<VideoFile>()
                .HasIndex(v => v.FilePath)
                .IsUnique();

            modelBuilder.Entity<VideoTag>()
                .HasIndex(t => t.Name)
                .IsUnique();

            modelBuilder.Entity<CustomMetadata>()
                .HasIndex(c => new { c.VideoFileId, c.Key })
                .IsUnique();
        }
    }
}
