using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VideoMetadataManager.Models;
using VideoMetadataManager.Services;

namespace VideoMetadataManager.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly VideoMetadataService _metadataService;
        
        private ObservableCollection<VideoFile> _videoFiles;
        private VideoFile _selectedVideoFile;
        private string _searchText;
        private string _statusMessage;
        private bool _isBusy;
        
        public ObservableCollection<VideoFile> VideoFiles
        {
            get => _videoFiles;
            set
            {
                _videoFiles = value;
                OnPropertyChanged(nameof(VideoFiles));
            }
        }
        
        public VideoFile SelectedVideoFile
        {
            get => _selectedVideoFile;
            set
            {
                _selectedVideoFile = value;
                OnPropertyChanged(nameof(SelectedVideoFile));
                // Charger les données détaillées quand une vidéo est sélectionnée
                LoadVideoDetails();
            }
        }
        
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                // Filtrer la liste lorsque le texte de recherche change
                FilterVideoFiles();
            }
        }
        
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }
        
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }
        
        // Propriétés d'édition pour les métadonnées
        private string _editTitle;
        private string _editDescription;
        private string _editTags;
        private DateTime? _editDateTaken;
        
        public string EditTitle
        {
            get => _editTitle;
            set
            {
                _editTitle = value;
                OnPropertyChanged(nameof(EditTitle));
            }
        }
        
        public string EditDescription
        {
            get => _editDescription;
            set
            {
                _editDescription = value;
                OnPropertyChanged(nameof(EditDescription));
            }
        }
        
        public string EditTags
        {
            get => _editTags;
            set
            {
                _editTags = value;
                OnPropertyChanged(nameof(EditTags));
            }
        }
        
        public DateTime? EditDateTaken
        {
            get => _editDateTaken;
            set
            {
                _editDateTaken = value;
                OnPropertyChanged(nameof(EditDateTaken));
            }
        }
        
        // Commandes
        public ICommand LoadDirectoryCommand { get; }
        public ICommand ScanDirectoryCommand { get; }
        public ICommand SaveMetadataCommand { get; }
        public ICommand ResetMetadataCommand { get; }
        
        public MainViewModel(VideoMetadataService metadataService)
        {
            _metadataService = metadataService;
            
            VideoFiles = new ObservableCollection<VideoFile>();
            
            // Initialiser les commandes
            LoadDirectoryCommand = new RelayCommand(async param => await LoadDirectory(param as string), _ => !IsBusy);
            ScanDirectoryCommand = new RelayCommand(async param => await ScanDirectory(param as string), _ => !IsBusy);
            SaveMetadataCommand = new RelayCommand(async _ => await SaveMetadata(), _ => SelectedVideoFile != null && !IsBusy);
            ResetMetadataCommand = new RelayCommand(_ => ResetMetadataEdits(), _ => SelectedVideoFile != null);
            
            StatusMessage = "Prêt";
        }
        
        private async Task LoadDirectory(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath))
                return;
                
            try
            {
                IsBusy = true;
                StatusMessage = "Chargement des fichiers vidéo...";
                
                var videos = await _metadataService.GetAllVideoFilesAsync();
                
                VideoFiles.Clear();
                foreach (var video in videos)
                {
                    VideoFiles.Add(video);
                }
                
                StatusMessage = $"{videos.Count} fichiers vidéo chargés";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur: {ex.Message}";
                MessageBox.Show($"Erreur lors du chargement des fichiers: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        private async Task ScanDirectory(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath))
                return;
                
            try
            {
                IsBusy = true;
                StatusMessage = "Analyse des fichiers vidéo...";
                
                var videos = await _metadataService.ScanDirectoryAsync(directoryPath);
                
                VideoFiles.Clear();
                foreach (var video in videos)
                {
                    VideoFiles.Add(video);
                }
                
                StatusMessage = $"{videos.Count} fichiers vidéo analysés";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur: {ex.Message}";
                MessageBox.Show($"Erreur lors de l'analyse des fichiers: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        private async void FilterVideoFiles()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    var allVideos = await _metadataService.GetAllVideoFilesAsync();
                    VideoFiles = new ObservableCollection<VideoFile>(allVideos);
                    return;
                }
                
                var filtered = await _metadataService.SearchVideoFilesAsync(SearchText);
                VideoFiles = new ObservableCollection<VideoFile>(filtered);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors de la recherche: {ex.Message}";
            }
        }
        
        private void LoadVideoDetails()
        {
            if (SelectedVideoFile == null)
            {
                ResetMetadataEdits();
                return;
            }
            
            // Charger les données dans les champs d'édition
            EditTitle = SelectedVideoFile.Title;
            EditDescription = SelectedVideoFile.Description;
            EditDateTaken = SelectedVideoFile.DateTaken;
            
            // Convertir les tags en chaîne de caractères séparée par des virgules
            var tagNames = SelectedVideoFile.Tags.Select(t => t.Name);
            EditTags = string.Join(", ", tagNames);
        }
        
        private async Task SaveMetadata()
        {
            if (SelectedVideoFile == null)
                return;
                
            try
            {
                IsBusy = true;
                StatusMessage = "Enregistrement des métadonnées...";
                
                // Mettre à jour les métadonnées modifiables
                SelectedVideoFile.Title = EditTitle;
                SelectedVideoFile.Description = EditDescription;
                SelectedVideoFile.DateTaken = EditDateTaken;
                
                // Sauvegarder les modifications
                await _metadataService.SaveVideoFileAsync(SelectedVideoFile);
                
                // Mettre à jour les tags
                if (!string.IsNullOrWhiteSpace(EditTags))
                {
                    var tagList = EditTags.Split(',')
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .ToList();
                        
                    await _metadataService.UpdateTagsAsync(SelectedVideoFile.Id, tagList);
                }
                
                StatusMessage = "Métadonnées enregistrées avec succès";
                
                // Recharger les détails pour afficher les modifications
                SelectedVideoFile = await _metadataService.GetVideoFileByIdAsync(SelectedVideoFile.Id);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur: {ex.Message}";
                MessageBox.Show($"Erreur lors de l'enregistrement des métadonnées: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        private void ResetMetadataEdits()
        {
            EditTitle = string.Empty;
            EditDescription = string.Empty;
            EditTags = string.Empty;
            EditDateTaken = null;
        }
        
        // Implémentation de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    // Classe utilitaire pour les commandes
    public class RelayCommand : ICommand
    {
        private readonly Func<object, Task> _executeAsync;
        private readonly Predicate<object> _canExecute;
        
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _executeAsync = param => {
                execute(param);
                return Task.CompletedTask;
            };
            _canExecute = canExecute;
        }
        
        public RelayCommand(Func<object, Task> executeAsync, Predicate<object> canExecute = null)
        {
            _executeAsync = executeAsync;
            _canExecute = canExecute;
        }
        
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }
        
        public async void Execute(object parameter)
        {
            await _executeAsync(parameter);
        }
        
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
