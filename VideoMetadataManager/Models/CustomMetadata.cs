using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VideoMetadataManager.Models
{
    /// <summary>
    /// Représente des métadonnées personnalisées supplémentaires qui peuvent être ajoutées aux fichiers vidéo
    /// </summary>
    public class CustomMetadata
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Key { get; set; }
        
        public string Value { get; set; }
        
        // Type de la valeur (string, int, date, etc.)
        public string ValueType { get; set; } = "string";
        
        // Date de création/modification
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Modified { get; set; } = DateTime.Now;
        
        // Relation avec la vidéo
        public int VideoFileId { get; set; }
        
        [ForeignKey("VideoFileId")]
        public virtual VideoFile VideoFile { get; set; }
    }
}
