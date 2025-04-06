using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VideoMetadataManager.Settings
{
    /// <summary>
    /// Paramètres de configuration de l'application
    /// </summary>
    public class AppSettings
    {
        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "VideoMetadataManager",
            "settings.json");

        // Paramètres par défaut
        private static readonly AppSettings DefaultSettings = new AppSettings
        {
            LastOpenedFolders = new List<string>(),
            ScanSubdirectories = true,
            AutoExtractMetadata = true,
            SupportedExtensions = new List<string> { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv" },
            DarkMode = false,
            FFmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg")
        };

        /// <summary>
        /// Liste des derniers dossiers ouverts
        /// </summary>
        public List<string> LastOpenedFolders { get; set; } = new List<string>();

        /// <summary>
        /// Indique si les sous-dossiers doivent être analysés
        /// </summary>
        public bool ScanSubdirectories { get; set; } = true;

        /// <summary>
        /// Indique si les métadonnées doivent être extraites automatiquement lors de l'ouverture d'un dossier
        /// </summary>
        public bool AutoExtractMetadata { get; set; } = true;

        /// <summary>
        /// Extensions de fichiers vidéo prises en charge
        /// </summary>
        public List<string> SupportedExtensions { get; set; } = new List<string>();

        /// <summary>
        /// Indique si le mode sombre est activé
        /// </summary>
        public bool DarkMode { get; set; } = false;

        /// <summary>
        /// Chemin vers le répertoire FFmpeg
        /// </summary>
        public string FFmpegPath { get; set; }

        /// <summary>
        /// Charge les paramètres à partir du fichier de configuration
        /// </summary>
        /// <returns>Les paramètres chargés ou les paramètres par défaut en cas d'erreur</returns>
        public static AppSettings Load()
        {
            try
            {
                // Vérifier si le fichier existe
                if (!File.Exists(SettingsFilePath))
                {
                    // Créer le répertoire si nécessaire
                    var directory = Path.GetDirectoryName(SettingsFilePath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Enregistrer et retourner les paramètres par défaut
                    DefaultSettings.Save();
                    return DefaultSettings;
                }

                // Lire le fichier
                string json = File.ReadAllText(SettingsFilePath);
                
                // Désérialiser les paramètres
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                
                return settings ?? DefaultSettings;
            }
            catch (Exception)
            {
                // En cas d'erreur, retourner les paramètres par défaut
                return DefaultSettings;
            }
        }

        /// <summary>
        /// Enregistre les paramètres dans le fichier de configuration
        /// </summary>
        public void Save()
        {
            try
            {
                // Créer le répertoire si nécessaire
                var directory = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Sérialiser les paramètres avec indentation pour une meilleure lisibilité
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                
                string json = JsonSerializer.Serialize(this, options);
                
                // Écrire dans le fichier
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception)
            {
                // Ignorer les erreurs d'écriture
            }
        }

        /// <summary>
        /// Ajoute un dossier à la liste des derniers dossiers ouverts
        /// </summary>
        public void AddLastOpenedFolder(string folderPath)
        {
            // Supprimer le dossier s'il existe déjà
            LastOpenedFolders.Remove(folderPath);
            
            // Ajouter le dossier au début de la liste
            LastOpenedFolders.Insert(0, folderPath);
            
            // Limiter la liste à 10 dossiers
            if (LastOpenedFolders.Count > 10)
            {
                LastOpenedFolders.RemoveAt(LastOpenedFolders.Count - 1);
            }
            
            // Enregistrer les modifications
            Save();
        }
    }
}
