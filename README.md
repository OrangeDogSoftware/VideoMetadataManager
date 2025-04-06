# Gestionnaire de Métadonnées Vidéo

Application C# pour gérer les métadonnées de fichiers vidéos stockés sur un disque local.

## Fonctionnalités

- Parcourir et analyser des fichiers vidéo à partir d'un répertoire local
- Extraire et afficher les métadonnées techniques (résolution, codec, durée, etc.)
- Modifier et sauvegarder des métadonnées personnalisées (titre, description, tags, etc.)
- Interface utilisateur intuitive basée sur WPF
- Prise en charge de différents formats vidéo (.mp4, .avi, .mkv, .mov, etc.)
- Organisation des fichiers vidéo à l'aide de tags
- Stockage local des métadonnées dans une base de données SQLite

## Captures d'écran

*Les captures d'écran seront ajoutées une fois l'application complétée.*

## Prérequis

- .NET 7.0 ou supérieur
- Visual Studio 2022 (recommandé)
- FFmpeg (inclus via le package Xabe.FFmpeg)

## Installation

1. Clonez ce dépôt
```bash
git clone https://github.com/OrangeDogSoftware/VideoMetadataManager.git
```

2. Ouvrez la solution dans Visual Studio
```bash
cd VideoMetadataManager
start VideoMetadataManager.sln
```

3. Restaurez les packages NuGet
```bash
dotnet restore
```

4. Compilez et exécutez l'application
```bash
dotnet build
dotnet run --project VideoMetadataManager
```

5. Installation de FFmpeg
   - Téléchargez la dernière version de FFmpeg pour Windows sur [le site officiel](https://ffmpeg.org/download.html)
   - Extrayez les fichiers exécutables (ffmpeg.exe et ffprobe.exe) dans le dossier `ffmpeg` de l'application

## Guide d'utilisation

1. Lancez l'application
2. Cliquez sur "Fichier" > "Choisir dossier..." pour sélectionner un dossier contenant des fichiers vidéo
3. Naviguez dans la liste des fichiers vidéo dans le panneau de gauche
4. Sélectionnez un fichier pour afficher ses métadonnées
5. Utilisez l'onglet "Édition" pour modifier les métadonnées personnalisées
6. Cliquez sur "Enregistrer" pour sauvegarder les modifications

## Technologies utilisées

- C# / .NET 7.0
- Windows Presentation Foundation (WPF)
- Entity Framework Core avec SQLite
- Xabe.FFmpeg pour l'analyse de métadonnées vidéo

## Structure du projet

- `Models/` - Classes de modèles représentant les fichiers vidéo et leurs métadonnées
- `ViewModels/` - ViewModels pour la liaison de données dans l'architecture MVVM
- `Views/` - Composants d'interface utilisateur WPF
- `Services/` - Services pour l'extraction et la gestion des métadonnées
- `Data/` - Contexte de base de données et migrations Entity Framework Core
- `Settings/` - Gestion des paramètres de l'application

## Contribution

Les contributions sont les bienvenues ! N'hésitez pas à proposer des améliorations ou à signaler des bugs.

1. Forkez le dépôt
2. Créez une branche pour votre fonctionnalité (`git checkout -b feature/amazing-feature`)
3. Commitez vos changements (`git commit -m 'Add some amazing feature'`)
4. Poussez vers la branche (`git push origin feature/amazing-feature`)
5. Ouvrez une Pull Request

## Licence

Ce projet est sous licence MIT. Voir le fichier `LICENSE` pour plus de détails.
