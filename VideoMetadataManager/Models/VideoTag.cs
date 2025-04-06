using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VideoMetadataManager.Models
{
    /// <summary>
    /// Représente un tag qui peut être attribué à un fichier vidéo
    /// </summary>
    public class VideoTag
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        // Couleur associée au tag (stockée au format hexadécimal)
        public string Color { get; set; } = "#0078D7";
        
        // Date de création du tag
        public DateTime Created { get; set; } = DateTime.Now;
        
        // Liste des fichiers vidéo associés à ce tag
        public virtual ICollection<VideoFile> Videos { get; set; } = new List<VideoFile>();
    }
}
