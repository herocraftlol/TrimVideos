# 🎬 TrimVideos (ShortsPrep)

> **TrimVideos** est un outil Windows gratuit et open-source qui vous aide à monter, rogner et préparer vos vidéos pour TikTok, Instagram Reels et YouTube Shorts — sans perte de qualité, en quelques clics, et avec un aperçu toujours lisible même pour les codecs exotiques (HEVC, etc.).

![Version](https://img.shields.io/badge/version-1.3.0-blue.svg)
![Plateforme](https://img.shields.io/badge/platform-Windows-0078d4.svg)
![.NET](https://img.shields.io/badge/.NET-8.0--windows-purple.svg)
![Licence](https://img.shields.io/badge/license-MIT-green.svg)

---

## ✨ À quoi ça sert ?

TrimVideos vous simplifie tout le pipeline de publication d'une vidéo courte sur les réseaux sociaux :

1. ✂️ **Montez** vos vidéos dans un éditeur sans perte : coupez plusieurs passages, réorganisez-les, appliquez des effets (luminosité, contraste, saturation, rotation) et gardez la qualité mathématique parfaite.
2. 🎞️ **Rognez** la partie qui vous intéresse avec un éditeur visuel simple à deux curseurs et aperçu.
3. 🎚️ **Préparez** automatiquement votre vidéo au format portrait 9:16 parfait pour TikTok, Reels et Shorts.
4. 🎨 **Animez** vos visuels statiques (image + son) avec un effet Ken Burns et une réactivité aux basses.
5. 📦 **Exportez** une copie "maître" sans aucune perte, ainsi qu'un MP4 optimisé pour chaque plateforme.
6. 🚀 **Ouvrez** directement la page d'upload de la plateforme choisie pour publier en un glisser-déposer.

---

## 🆕 Quoi de neuf dans la v1.3.0 ?

Cette version corrige deux bugs spécifiques mais pénibles au quotidien :

- 🎵 **MP3 / FLAC avec pochette intégrée ne font plus planter l'aperçu**. Un fichier audio avec une image de couverture (cover art) est détecté par `ffprobe` comme ayant un « flux vidéo » (l'image elle-même, marquée `disposition.attached_pic=1`). Auparavant, ça faisait planter la génération du proxy de prévisualisation, parce que le conteneur `.mp4` refuse de stocker de la vidéo H.264 à la place d'une image de couverture. C'est maintenant détecté et ignoré correctement dans `VideoProcessor`.
- 📜 **Messages d'erreur FFmpeg plus lisibles**. En cas d'échec d'une opération `ffmpeg`, seules les **8 dernières lignes** du journal sont affichées (au lieu de toute la bannière de version / options de compilation, illisible et sans intérêt). En pratique, c'est toujours dans la queue du log que se trouve la vraie cause, donc le diagnostic est plus rapide.

> 💡 Sous le capot : nouvelle détection `isAttachedPic` dans `VideoProcessor.ProbeAsync` (flux `video` avec `width=0` + `disposition.attached_pic=1` est désormais ignoré), et nouveau `-vn` explicite sur la branche audio de `CreateCompatiblePreviewAsync` pour exclure toute pochette intégrée. Côté messages, le `throw new InvalidOperationException(...)` filtre la sortie via `TakeLast(8)`.

> ⚠️ Note technique : `VideoEditorWindow.xaml.cs` continue d'utiliser l'alias `using Path = System.IO.Path;` introduit en v1.2.0 pour lever l'ambiguïté avec `System.Windows.Shapes.Path`. La build reste impossible sans cet alias.

---

## 🚀 Démarrage rapide

1. Téléchargez la dernière version depuis la page [**Releases**](../../releases).
2. Lancez **`TrimVideos.exe`** — aucune installation requise, c'est portable.
3. Choisissez une vidéo **ou** une image + un son.
4. (Optionnel) Cliquez sur **"Éditeur avancé (coupes multiples + effets)..."** pour monter votre vidéo sans perte avant traitement.
5. Cliquez sur **Démarrer** et laissez FFmpeg faire le travail.
6. Le fichier est prêt, ouvert automatiquement, et la page d'upload s'ouvre dans votre navigateur.

> Au premier lancement, FFmpeg (~100 Mo) est téléchargé automatiquement depuis les builds officiels BtbN/FFmpeg-Builds (GPL, à jour en continu). Connexion Internet requise uniquement pour cette étape.

---

## 🔧 Fonctionnalités détaillées

### 🎬 Éditeur vidéo avancé sans perte

Une fenêtre dédiée accessible depuis le bouton **"Éditeur avancé (coupes multiples + effets)..."** :

- **Aperçu toujours lisible** grâce au proxy H.264/AAC généré en interne (v1.2.0) : fonctionne même pour les fichiers en HEVC, ProRes, etc.
- **Timeline visuelle** : la vidéo, les segments marqués (en dégradé corail/or) et la tête de lecture sont dessinés sur une vraie barre de montage cliquable.
- **Coupes multiples** : marquez autant de passages à garder, réorganisez-les, montez-les bout à bout.
- **Effets** : luminosité, contraste, saturation, rotation (90° / 180° / 270°), Ken Burns et basses.
- **Export rapide (par défaut)** : copie de flux pure, zéro ré-encodage, sortie `.mp4`.
- **Export précis** : coupes exactes à la frame près, vidéo CRF 0 + audio FLAC, sortie `.mkv`.

### 📥 Deux modes d'entrée

- **Mode vidéo** : vidéo existante (mp4, mov, mkv, avi, webm).
- **Mode image + son** : pochette + audio — la vidéo générée dure exactement la durée du son.
- **Mode "commande simple"** : reproduit exactement `ffmpeg -loop 1 -i image -i son -shortest -c:a copy -strict -2 sortie.mp4` (aucun ré-encodage audio).

### ✂️ Éditeur de recadrage temporel

Fenêtre dédiée avec aperçu vidéo (lui aussi alimenté par le proxy v1.2.0) et deux curseurs (début/fin). **v1.3.0** : fonctionne désormais correctement aussi sur les fichiers audio avec pochette intégrée.

### 🎨 Mouvement de caméra & réactivité à la musique

- **Ken Burns** : zoom lent + travelling.
- **Réactif aux basses** : le zoom pulse sur les basses/kicks détectés dans l'audio (analyse de l'énergie ~80 Hz par fenêtres de 50 ms).
- Combinables.
- Disponibles aussi sur une vidéo existante et dans l'éditeur avancé.

### 🎚️ Trois niveaux de qualité vidéo

| Mode | CRF | Cas d'usage |
|------|-----|-------------|
| **Visuellement sans perte** *(recommandé)* | 16 | Aucune perte visible à l'écran, taille raisonnable |
| **100 % sans perte** | 0 (x264) | Qualité mathématiquement parfaite, fichiers énormes |
| **Copie du flux** | — | Aucune ré-encodage si déjà au bon format — zéro perte, ultra rapide |

### 📐 Gestion automatique du ratio

- Source **plus large que 9:16** → recadrage centré (aucune déformation).
- Source **plus étroite que 9:16** → fond flou + image centrée.
- Limite automatique à 60 s pour les sorties portrait (sauf si vous décochez la case).

### 📦 Sortie maître sans perte

En plus du MP4 optimisé, vous pouvez générer un **fichier maître `.mkv` 100 % sans perte** (vidéo CRF 0 + audio FLAC intégré).

### 🚀 Publication en un clic

Ouverture automatique du dossier de sortie. Cases à cocher pour ouvrir directement les pages d'upload de TikTok, Instagram et YouTube Studio.

---

## 🛠️ Build (développeurs)

### Prérequis
- [SDK .NET 8](https://dotnet.microsoft.com/download) (Windows)
- Windows 10/11

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
  -p:AssemblyName=TrimVideos `
  -o publish
```

Ouvrez aussi `ShortsPrep.csproj` dans Visual Studio 2022 et lancez en F5.

---

## ❓ Pourquoi "semi-automatique" ?

L'upload **entièrement automatisé** vers TikTok/Instagram/YouTube nécessite leurs API officielles, avec des contraintes réelles :

- **YouTube Shorts** : API Google Cloud accessible — la V2 la plus simple à ajouter.
- **TikTok Content Posting API** : validation d'app obligatoire (dossier de review, délais).
- **Instagram Reels (Graph API)** : compte Business/Creator + app Meta validée.

TrimVideos prépare donc tout (fichier prêt, bon format, bonne qualité) et ouvre la bonne page.

---

## 🎵 Sur la question du "zéro perte audio"

Important à savoir : **TikTok, Instagram et YouTube ré-encodent toujours l'audio en AAC compressé côté serveur**, quoi que vous leur envoyiez.

Ce que TrimVideos garantit :

- ✅ La copie **WAV/FLAC extraite en local est bit-exacte** par rapport à l'original.
- ✅ L'audio intégré dans le MP4 envoyé aux plateformes est encodé en **AAC 320 kbps** (maximum utile).
- ✅ Le fichier maître `.mkv` (FLAC + CRF 0) sert d'archive personnelle sans aucune perte.
- ✅ L'**éditeur avancé** préserve la qualité d'origine dans les deux modes d'export.
- ✅ Le **proxy de prévisualisation** (v1.2.0) reste local et temporaire, supprimé à la fermeture de la fenêtre.

---

## 🗺️ Feuille de route

- [x] ✅ Éditeur vidéo avancé sans perte (coupes multiples, effets, deux modes d'export) — livré en v1.1.0.
- [x] ✅ Proxy de prévisualisation + timeline visuelle + interface repensée — livré en v1.2.0.
- [x] ✅ Correctif pochettes intégrées MP3/FLAC + messages d'erreur FFmpeg plus lisibles — **livré en v1.3.0**.
- [ ] Upload automatique réel vers YouTube Shorts via l'API Google.
- [ ] Watermark / recadrage ajustable à la souris (aperçu avant traitement).
- [ ] File d'attente pour traiter plusieurs vidéos d'un coup.
- [ ] Version Android (nécessiterait une réécriture — FFmpeg via `FFmpegKit`, interface en Kotlin/Jetpack Compose ; projet distinct du WPF).

---

## 📜 Licence & crédits

- **Code source** : MIT.
- **FFmpeg** : inclus automatiquement, sous licence GPL "full" avec tous les codecs — merci à [BtbN/FFmpeg-Builds](https://github.com/BtbN/FFmpeg-Builds).

---

## 📥 Téléchargements

Rendez-vous sur la page [**Releases**](../../releases) pour télécharger :

- 🪟 **`TrimVideos.exe`** — exécutable portable Windows (zéro installation).
- 📦 **`TrimVideos-X.Y.Z-source.zip`** — code source complet de la version X.Y.Z.

Bon montage ! 🎬
