using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VideoMetadataManager.Models;
using VideoMetadataManager.Data;
using Xabe.FFmpeg;
using Microsoft.EntityFrameworkCore;

namespace VideoMetadataManager.Services
{
    /// <summary>
    /// Service pour extraire et gérer les métadonnées des fichiers vidéo
    /// </summary>
    public class VideoMetadataService
    {
        private readonly ApplicationDbContext _dbContext;

        public VideoMetadataService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            
            // Configurer le chemin de FFmpeg
            FFmpeg.SetExecutablesPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg"));
        }

        /// <summary>
        /// Extrait les métadonnées d'un fichier vidéo et les retourne dans un objet VideoFile
        /// </summary>
        public async Task<VideoFile> ExtractMetadataAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Le fichier vidéo n'existe pas.", filePath);
            }

            // Vérifiez d'abord si le fichier est déjà dans la base de données
            var existingFile = await _dbContext.VideoFiles
                .FirstOrDefaultAsync(v => v.FilePath == filePath);

            if (existingFile != null)
            {
                return existingFile;
            }

            var fileInfo = new FileInfo(filePath);
            
            var videoFile = new VideoFile
            {
                FileName = fileInfo.Name,
                FilePath = filePath,
                FileSize = fileInfo.Length,
                LastModified = fileInfo.LastWriteTime,
                Title = Path.GetFileNameWithoutExtension(filePath) // Titre par défaut
            };

            try
            {
                // Utiliser FFmpeg pour extraire les métadonnées
                var mediaInfo = await FFmpeg.GetMediaInfo(filePath);
                
                // Extraire les informations vidéo
                var videoStream = mediaInfo.VideoStreams.FirstOrDefault();
                if (videoStream != null)
                {
                    videoFile.Duration = mediaInfo.Duration;
                    videoFile.Resolution = $"{videoStream.Width}x{videoStream.Height}";
                    videoFile.VideoCodec = videoStream.Codec;
                    videoFile.FrameRate = videoStream.Framerate;
                    videoFile.Bitrate = videoStream.Bitrate / 1000; // Convertir en kbps
                }

                // Extraire les informations audio
                var audioStream = mediaInfo.AudioStreams.FirstOrDefault();
                if (audioStream != null)
                {
                    videoFile.AudioCodec = audioStream.Codec;
                }

                // Essayer d'extraire la date de prise de vue si disponible
                var creationTime = File.GetCreationTime(filePath);
                videoFile.DateTaken = creationTime;

                return videoFile;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de l'extraction des métadonnées: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sauvegarde un fichier vidéo et ses métadonnées dans la base de données
        /// </summary>
        public async Task<VideoFile> SaveVideoFileAsync(VideoFile videoFile)
        {
            var existingFile = await _dbContext.VideoFiles
                .FirstOrDefaultAsync(v => v.FilePath == videoFile.FilePath);

            if (existingFile != null)
            {
                // Mettre à jour les propriétés existantes
                existingFile.FileSize = videoFile.FileSize;
                existingFile.LastModified = videoFile.LastModified;
                existingFile.Duration = videoFile.Duration;
                existingFile.Resolution = videoFile.Resolution;
                existingFile.VideoCodec = videoFile.VideoCodec;
                existingFile.AudioCodec = videoFile.AudioCodec;
                existingFile.FrameRate = videoFile.FrameRate;
                existingFile.Bitrate = videoFile.Bitrate;
                existingFile.Title = videoFile.Title;
                existingFile.Description = videoFile.Description;
                existingFile.DateTaken = videoFile.DateTaken;
                existingFile.LastUpdated = DateTime.Now;
                
                _dbContext.Update(existingFile);
                await _dbContext.SaveChangesAsync();
                return existingFile;
            }
            else
            {
                // Ajouter un nouveau fichier vidéo
                _dbContext.VideoFiles.Add(videoFile);
                await _dbContext.SaveChangesAsync();
                return videoFile;
            }
        }

        /// <summary>
        /// Analyse un dossier pour trouver tous les fichiers vidéo et extraire leurs métadonnées
        /// </summary>
        public async Task<List<VideoFile>> ScanDirectoryAsync(string directoryPath, bool includeSubdirectories = true)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException("Le dossier spécifié n'existe pas.");
            }

            // Extensions vidéo prises en charge
            string[] videoExtensions = { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv" };
            
            var searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var videoFiles = Directory.GetFiles(directoryPath, "*.*", searchOption)
                .Where(file => videoExtensions.Contains(Path.GetExtension(file).ToLower()))
                .ToList();

            var results = new List<VideoFile>();
            
            foreach (var filePath in videoFiles)
            {
                try
                {
                    // Extraire les métadonnées
                    var videoFile = await ExtractMetadataAsync(filePath);
                    
                    // Sauvegarder dans la base de données
                    var savedFile = await SaveVideoFileAsync(videoFile);
                    
                    results.Add(savedFile);
                }
                catch (Exception ex)
                {
                    // Log l'erreur mais continue avec les autres fichiers
                    Console.WriteLine($"Erreur lors du traitement du fichier {filePath}: {ex.Message}");
                }
            }

            return results;
        }

        /// <summary>
        /// Obtient tous les fichiers vidéo stockés dans la base de données
        /// </summary>
        public async Task<List<VideoFile>> GetAllVideoFilesAsync()
        {
            return await _dbContext.VideoFiles
                .Include(v => v.Tags)
                .Include(v => v.CustomMetadata)
                .ToListAsync();
        }

        /// <summary>
        /// Obtient un fichier vidéo par son ID
        /// </summary>
        public async Task<VideoFile> GetVideoFileByIdAsync(int id)
        {
            return await _dbContext.VideoFiles
                .Include(v => v.Tags)
                .Include(v => v.CustomMetadata)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        /// <summary>
        /// Ajoute ou met à jour les tags pour un fichier vidéo
        /// </summary>
        public async Task UpdateTagsAsync(int videoId, List<string> tagNames)
        {
            var videoFile = await _dbContext.VideoFiles
                .Include(v => v.Tags)
                .FirstOrDefaultAsync(v => v.Id == videoId);

            if (videoFile == null)
            {
                throw new Exception($"Fichier vidéo avec ID {videoId} non trouvé.");
            }

            // Obtenir ou créer les tags
            var tags = new List<VideoTag>();
            foreach (var tagName in tagNames)
            {
                var tag = await _dbContext.VideoTags
                    .FirstOrDefaultAsync(t => t.Name == tagName);

                if (tag == null)
                {
                    tag = new VideoTag { Name = tagName };
                    _dbContext.VideoTags.Add(tag);
                    await _dbContext.SaveChangesAsync();
                }

                tags.Add(tag);
            }

            // Mettre à jour les tags du fichier vidéo
            videoFile.Tags.Clear();
            foreach (var tag in tags)
            {
                videoFile.Tags.Add(tag);
            }

            videoFile.LastUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Recherche des fichiers vidéo basée sur un terme de recherche
        /// </summary>
        public async Task<List<VideoFile>> SearchVideoFilesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllVideoFilesAsync();
            }

            var lowerSearchTerm = searchTerm.ToLower();

            return await _dbContext.VideoFiles
                .Include(v => v.Tags)
                .Include(v => v.CustomMetadata)
                .Where(v => v.FileName.ToLower().Contains(lowerSearchTerm) ||
                            v.Title.ToLower().Contains(lowerSearchTerm) ||
                            v.Description.ToLower().Contains(lowerSearchTerm) ||
                            v.Tags.Any(t => t.Name.ToLower().Contains(lowerSearchTerm)))
                .ToListAsync();
        }
    }
}
