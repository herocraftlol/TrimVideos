# 🎬 TrimVideos (ShortsPrep)

> **TrimVideos** est un outil Windows gratuit et open-source qui vous aide à préparer et rogner vos vidéos pour TikTok, Instagram Reels et YouTube Shorts — sans perte de qualité, en quelques clics.

![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)
![Plateforme](https://img.shields.io/badge/platform-Windows-0078d4.svg)
![.NET](https://img.shields.io/badge/.NET-8.0--windows-purple.svg)
![Licence](https://img.shields.io/badge/license-MIT-green.svg)

---

## ✨ À quoi ça sert ?

TrimVideos vous simplifie tout le pipeline de publication d'une vidéo courte sur les réseaux sociaux :

1. 🎞️ **Rognez** la partie qui vous intéresse (éditeur visuel avec aperçu et deux curseurs).
2. 🎚️ **Préparez** automatiquement votre vidéo au format portrait 9:16 parfait pour TikTok, Reels et Shorts.
3. 🎨 **Animez** vos visuels statiques (image + son) avec un effet Ken Burns et une réactivité aux basses.
4. 📦 **Exportez** une copie "maître" sans aucune perte, ainsi qu'un MP4 optimisé pour chaque plateforme.
5. 🚀 **Ouvrez** directement la page d'upload de la plateforme choisie pour publier en un glisser-déposer.

---

## 🚀 Démarrage rapide

1. Téléchargez la dernière version depuis la page [**Releases**](../../releases).
2. Lancez **`TrimVideos.exe`** — aucune installation requise, c'est portable.
3. Choisissez une vidéo **ou** une image + un son.
4. Cliquez sur **Démarrer** et laissez FFmpeg faire le travail.
5. Le fichier est prêt, ouvert automatiquement, et la page d'upload s'ouvre dans votre navigateur.

> Au premier lancement, FFmpeg (~100 Mo) est téléchargé automatiquement depuis les builds officiels BtbN/FFmpeg-Builds (GPL, à jour en continu). Connexion Internet requise uniquement pour cette étape.

---

## 🔧 Fonctionnalités détaillées

### 📥 Deux modes d'entrée

- **Mode vidéo** : partez d'une vidéo existante (mp4, mov, mkv, avi, webm).
- **Mode image + son** : choisissez une image fixe et un fichier audio — la vidéo générée dure exactement la durée du son. Idéal pour publier un morceau avec une pochette animée.

### ✂️ Éditeur de recadrage temporel

Une fenêtre dédiée avec aperçu vidéo et deux curseurs (début/fin) pour sélectionner précisément la portion à conserver. Fonctionne aussi bien pour la vidéo que pour l'audio en mode image+son. Si l'aperçu ne s'affiche pas (codec non lu par le lecteur Windows intégré), un message clair l'indique.

### 🎨 Mouvement de caméra & réactivité à la musique (mode image + son)

- **Ken Burns** : zoom lent continu + léger travelling, intensité réglable.
- **Réactif aux basses** : le zoom pulse sur les basses/kicks détectés dans l'audio (analyse d'énergie de la bande ~80 Hz par fenêtres de 50 ms, détection de pics). Sensibilité réglable.
- Les deux effets se combinent. Techniquement : l'image est mise à l'échelle avec 30 % de marge, puis zoomée/déplacée dynamiquement par des expressions FFmpeg évaluées image par image, avant un recadrage final — jamais de bord vide visible, même en mouvement.
- **Ces effets s'appliquent aussi à une vidéo existante** (mode vidéo, portrait ou paysage), pas seulement au mode image + son.

### 🎚️ Trois niveaux de qualité vidéo

| Mode | CRF | Cas d'usage |
|------|-----|-------------|
| **Visuellement sans perte** *(recommandé)* | 16 | Aucune perte visible à l'écran, taille raisonnable |
| **100 % sans perte** | 0 (x264) | Qualité mathématiquement parfaite, fichiers énormes |
| **Copie du flux** | — | Aucune ré-encodage si déjà au bon format — zéro perte, ultra rapide |

### 📐 Gestion automatique du ratio

- Source **plus large que 9:16** → recadrage centré (aucune déformation).
- Source **plus étroite que 9:16** → fond flou + image centrée.
- Limite automatique à 60 s pour les sorties portrait (sauf si vous décochez la case pour conserver la durée d'origine).

### 📦 Sortie maître sans perte

En plus du MP4 optimisé, vous pouvez générer un **fichier maître `.mkv` 100 % sans perte** (vidéo CRF 0 + audio FLAC intégré) — c'est votre archive personnelle à conserver pour un usage musical ou une ré-encodage ultérieur sans repartir de zéro. ⚠️ Ce fichier n'est pas accepté par les plateformes (qui veulent du .mp4) ; il sert d'archive.

### 🚀 Publication en un clic

- Ouverture automatique du dossier de sortie.
- Cases à cocher pour ouvrir directement les pages d'upload de TikTok, Instagram et YouTube Studio.
- Barre de progression avec pourcentage réel (calculé en comparant la progression FFmpeg à la durée totale attendue).

---

## 🛠️ Build (développeurs)

### Prérequis
- [SDK .NET 8](https://dotnet.microsoft.com/download) (Windows)
- Windows 10/11 (WPF est une technologie Windows uniquement)

### Compilation en ligne de commande

```powershell
git clone https://github.com/herocraftlol/TrimVideos.git
cd TrimVideos
dotnet build ShortsPrep/ShortsPrep.csproj -c Release
dotnet run --project ShortsPrep/ShortsPrep.csproj
```

### Publication d'un exécutable portable

```powershell
dotnet publish ShortsPrep/ShortsPrep.csproj `
  -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o publish
```

L'exécutable `TrimVideos.exe` est généré dans le dossier `publish/`.

Vous pouvez aussi ouvrir `ShortsPrep.csproj` dans Visual Studio 2022 et lancer en F5.

---

## ❓ Pourquoi "semi-automatique" ?

L'upload **entièrement automatisé** (sans aucun clic) vers TikTok/Instagram/YouTube nécessite leurs API officielles, avec des contraintes réelles :

- **YouTube Shorts** : API Google Cloud accessible, upload automatisable — la V2 la plus simple à ajouter si vous voulez aller plus loin.
- **TikTok Content Posting API** : nécessite une validation d'app par TikTok (dossier de review, délais, refus possible pour un usage personnel/petit compte).
- **Instagram Reels (Graph API)** : nécessite un compte Business/Creator + app Meta validée — process assez lourd pour un usage perso.

TrimVideos prépare donc tout (fichier prêt, bon format, bonne qualité) et ouvre la bonne page — il ne reste plus qu'à glisser le fichier.

---

## 🎵 Sur la question du "zéro perte audio"

Important à savoir : **TikTok, Instagram et YouTube ré-encodent toujours l'audio en AAC compressé côté serveur**, quoi que vous leur envoyiez — c'est une limite des plateformes elles-mêmes, aucun logiciel ne peut l'éviter.

Ce que TrimVideos garantit :

- ✅ La copie **WAV/FLAC extraite en local est bit-exacte** par rapport à l'original — c'est votre version "maître" à conserver (DAW, SoundCloud qui accepte le FLAC sans transcodage destructeur imposé, etc.).
- ✅ L'audio intégré dans le MP4 envoyé aux plateformes est encodé en **AAC 320 kbps** (maximum utile — au-delà l'oreille ne fait plus la différence et les plateformes replafonnent de toute façon).
- ✅ Le fichier maître `.mkv` (FLAC + CRF 0) sert d'archive personnelle sans aucune perte, ou pour être ré-encodé plus tard sans repartir de zéro.

---

## 🗺️ Feuille de route

- [ ] Upload automatique réel vers YouTube Shorts via l'API Google.
- [ ] Watermark / recadrage ajustable à la souris (aperçu avant traitement).
- [ ] File d'attente pour traiter plusieurs vidéos d'un coup.
- [ ] Version Android (nécessiterait une réécriture — FFmpeg via `FFmpegKit`, interface en Kotlin/Jetpack Compose ; projet distinct du WPF).

---

## 📜 Licence & crédits

- **Code source** : MIT.
- **FFmpeg** : inclus automatiquement au premier lancement, sous licence GPL "full" avec tous les codecs — merci à [BtbN/FFmpeg-Builds](https://github.com/BtbN/FFmpeg-Builds).

---

## 📥 Téléchargements

Rendez-vous sur la page [**Releases**](../../releases) pour télécharger :

- 🪟 **`TrimVideos.exe`** — exécutable portable Windows (zéro installation).
- 📦 **`TrimVideos-X.Y.Z-source.zip`** — code source complet de la version X.Y.Z.

Bon montage ! 🎬
