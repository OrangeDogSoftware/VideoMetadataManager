====== INSTALLATION DE FFMPEG ======

Pour que l'application fonctionne correctement, vous devez installer FFmpeg dans ce dossier.

1. Téléchargez la dernière version de FFmpeg pour Windows sur https://ffmpeg.org/download.html
   (Choisissez la version "Full" ou "Essentials" pour Windows)

2. Extrayez les fichiers suivants du dossier bin/ dans ce répertoire :
   - ffmpeg.exe
   - ffprobe.exe
   - Les fichiers DLL associés

3. Redémarrez l'application VideoMetadataManager

====== VÉRIFICATION DE L'INSTALLATION ======

Si les fichiers FFmpeg sont correctement installés, l'application pourra extraire automatiquement
les métadonnées des fichiers vidéo. Si vous rencontrez des erreurs liées à FFmpeg, vérifiez que
les fichiers exécutables sont bien présents dans ce dossier.

Note : Le package Xabe.FFmpeg utilisé par cette application nécessite ces fichiers exécutables
pour analyser les métadonnées des fichiers vidéo.
