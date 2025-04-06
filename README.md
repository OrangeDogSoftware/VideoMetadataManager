# Gestionnaire de Métadonnées Vidéo

Application C# pour gérer les métadonnées de fichiers vidéos stockés sur un disque local.

## Fonctionnalités

- Parcourir et analyser des fichiers vidéo à partir d'un répertoire local
- Extraire et afficher les métadonnées techniques (résolution, codec, durée, etc.)
- Modifier et sauvegarder des métadonnées personnalisées (titre, description, tags, etc.)
- Interface utilisateur intuitive basée sur WPF
- Prise en charge de différents formats vidéo (.mp4, .avi, .mkv, .mov, etc.)

## Prérequis

- .NET 7.0 ou supérieur
- Visual Studio 2022 (recommandé)
- FFmpeg (inclus via le package Xabe.FFmpeg)

## Installation

1. Clonez ce dépôt
2. Ouvrez la solution dans Visual Studio
3. Restaurez les packages NuGet
4. Compilez et exécutez l'application

## Technologies utilisées

- C# / .NET 7.0
- Windows Presentation Foundation (WPF)
- Entity Framework Core avec SQLite
- Xabe.FFmpeg pour l'analyse de métadonnées vidéo
