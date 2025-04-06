using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using VideoMetadataManager.Data;
using VideoMetadataManager.Models;
using VideoMetadataManager.Services;
using VideoMetadataManager.ViewModels;

namespace VideoMetadataManager
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly VideoMetadataService _metadataService;

        public MainWindow()
        {
            InitializeComponent();

            // Initialisation des services et du ViewModel
            var dbContext = new ApplicationDbContext();
            _metadataService = new VideoMetadataService(dbContext);
            _viewModel = new MainViewModel(_metadataService);
            
            // Définir le DataContext pour le binding
            DataContext = _viewModel;
            
            // Associer les événements aux contrôles
            VideoFilesList.SelectionChanged += (s, e) => 
            {
                if (VideoFilesList.SelectedItem != null)
                {
                    string selectedFileName = VideoFilesList.SelectedItem.ToString();
                    var selectedFile = _viewModel.VideoFiles.FirstOrDefault(v => v.FileName == selectedFileName);
                    if (selectedFile != null)
                    {
                        _viewModel.SelectedVideoFile = selectedFile;
                        UpdateUIWithSelectedVideo(selectedFile);
                    }
                }
            };
            
            SearchBox.TextChanged += (s, e) => 
            {
                _viewModel.SearchText = SearchBox.Text;
            };
        }

        private async void ChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog
            {
                Description = "Sélectionnez le dossier contenant vos fichiers vidéo",
                UseDescriptionForTitle = true
            };

            var result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // Charger les fichiers vidéo du dossier sélectionné
                string selectedPath = dialog.SelectedPath;
                await _viewModel.ScanDirectoryCommand.Execute(selectedPath);
                UpdateFilesList();
            }
        }

        private void UpdateFilesList()
        {
            VideoFilesList.Items.Clear();
            foreach (var videoFile in _viewModel.VideoFiles)
            {
                VideoFilesList.Items.Add(videoFile.FileName);
            }
            
            StatusText.Text = $"{_viewModel.VideoFiles.Count} fichiers vidéo trouvés";
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private async void ScanAllFiles_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.VideoFiles.Count > 0)
            {
                var result = System.Windows.MessageBox.Show(
                    "Voulez-vous analyser à nouveau tous les fichiers? Cela peut prendre du temps.",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                    
                if (result == MessageBoxResult.Yes)
                {
                    var distinctPaths = _viewModel.VideoFiles
                        .Select(v => Path.GetDirectoryName(v.FilePath))
                        .Distinct()
                        .ToList();
                        
                    foreach (var path in distinctPaths)
                    {
                        await _viewModel.ScanDirectoryCommand.Execute(path);
                    }
                    
                    UpdateFilesList();
                }
            }
            else
            {
                System.Windows.MessageBox.Show(
                    "Veuillez d'abord choisir un dossier contenant des fichiers vidéo.",
                    "Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            // Implémentation future: Dialogue des réglages
            System.Windows.MessageBox.Show(
                "Cette fonctionnalité sera implémentée prochainement.",
                "Information",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show(
                "VideoMetadataManager\nVersion 1.0\n\nUne application pour gérer les métadonnées des fichiers vidéo.",
                "À propos",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void UpdateUIWithSelectedVideo(VideoFile videoFile)
        {
            if (videoFile == null)
            {
                // Réinitialiser les champs si aucun fichier n'est sélectionné
                FileName.Text = string.Empty;
                FilePath.Text = string.Empty;
                FileSize.Text = string.Empty;
                Duration.Text = string.Empty;
                Resolution.Text = string.Empty;
                Codec.Text = string.Empty;
                CreationDate.Text = string.Empty;
                
                // Réinitialiser les champs d'édition
                TitleEdit.Text = string.Empty;
                DescriptionEdit.Text = string.Empty;
                TagsEdit.Text = string.Empty;
                DateTakenEdit.SelectedDate = null;
                
                // Vider la grille de métadonnées avancées
                AdvancedMetadataGrid.ItemsSource = null;
                
                return;
            }
            
            // Mettre à jour les informations générales
            FileName.Text = videoFile.FileName;
            FilePath.Text = videoFile.FilePath;
            FileSize.Text = FormatFileSize(videoFile.FileSize);
            Duration.Text = FormatDuration(videoFile.Duration);
            Resolution.Text = videoFile.Resolution;
            Codec.Text = videoFile.VideoCodec;
            CreationDate.Text = videoFile.DateTaken?.ToString("dd/MM/yyyy HH:mm:ss") ?? "Non disponible";
            
            // Mettre à jour les champs d'édition
            TitleEdit.Text = videoFile.Title;
            DescriptionEdit.Text = videoFile.Description;
            DateTakenEdit.SelectedDate = videoFile.DateTaken;
            
            // Formater les tags
            var tagNames = videoFile.Tags.Select(t => t.Name);
            TagsEdit.Text = string.Join(", ", tagNames);
            
            // Préparer les métadonnées avancées pour l'affichage en grille
            var advancedMetadata = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("ID", videoFile.Id.ToString()),
                new KeyValuePair<string, string>("Chemin", videoFile.FilePath),
                new KeyValuePair<string, string>("Taille", FormatFileSize(videoFile.FileSize)),
                new KeyValuePair<string, string>("Dernière modification", videoFile.LastModified.ToString()),
                new KeyValuePair<string, string>("Durée", FormatDuration(videoFile.Duration)),
                new KeyValuePair<string, string>("Résolution", videoFile.Resolution),
                new KeyValuePair<string, string>("Codec vidéo", videoFile.VideoCodec),
                new KeyValuePair<string, string>("Codec audio", videoFile.AudioCodec),
                new KeyValuePair<string, string>("Framerate", $"{videoFile.FrameRate} fps"),
                new KeyValuePair<string, string>("Bitrate", $"{videoFile.Bitrate} kbps"),
                new KeyValuePair<string, string>("Date de prise", videoFile.DateTaken?.ToString() ?? "Non disponible"),
                new KeyValuePair<string, string>("Date d'ajout", videoFile.DateAdded.ToString()),
                new KeyValuePair<string, string>("Dernière mise à jour", videoFile.LastUpdated.ToString())
            };
            
            // Ajouter les métadonnées personnalisées
            foreach (var metadata in videoFile.CustomMetadata)
            {
                advancedMetadata.Add(new KeyValuePair<string, string>(metadata.Key, metadata.Value));
            }
            
            AdvancedMetadataGrid.ItemsSource = advancedMetadata;
        }
        
        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "o", "Ko", "Mo", "Go", "To" };
            int counter = 0;
            decimal number = bytes;
            
            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;
            }
            
            return $"{number:n1} {suffixes[counter]}";
        }
        
        private string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalHours >= 1)
            {
                return $"{duration.Hours}h {duration.Minutes}m {duration.Seconds}s";
            }
            else
            {
                return $"{duration.Minutes}m {duration.Seconds}s";
            }
        }

        private void VideoFilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Déjà géré par le binding
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Déjà géré par le binding
        }

        private async void SaveMetadata_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.SaveMetadataCommand.Execute(null);
        }

        private void ResetMetadata_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ResetMetadataCommand.Execute(null);
        }
    }
}
