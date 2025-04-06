using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VideoMetadataManager.Models
{
    /// <summary>
    /// Représente un fichier vidéo avec ses métadonnées
    /// </summary>
    public class VideoFile
    {
        [Key]
        public int Id { get; set; }
        
        // Métadonnées de base du fichier
        [Required]
        public string FileName { get; set; }
        
        [Required]
        public string FilePath { get; set; }
        
        public long FileSize { get; set; }
        
        public DateTime LastModified { get; set; }
        
        // Métadonnées techniques extraites
        public TimeSpan Duration { get; set; }
        
        public string Resolution { get; set; }
        
        public string VideoCodec { get; set; }
        
        public string AudioCodec { get; set; }
        
        public double FrameRate { get; set; }
        
        public int Bitrate { get; set; }
        
        // Métadonnées personnalisables
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public DateTime? DateTaken { get; set; }
        
        // Relations
        public virtual ICollection<VideoTag> Tags { get; set; } = new List<VideoTag>();
        
        public virtual ICollection<CustomMetadata> CustomMetadata { get; set; } = new List<CustomMetadata>();
        
        // Date d'ajout à la base de données
        public DateTime DateAdded { get; set; } = DateTime.Now;
        
        // Date de dernière mise à jour des métadonnées
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
