# 🎬 TrimVideos (ShortsPrep)

> **TrimVideos** est un outil Windows gratuit et open-source qui vous aide à monter, rogner et préparer vos vidéos pour TikTok, Instagram Reels et YouTube Shorts — sans perte de qualité, en quelques clics, et avec un aperçu toujours lisible même pour les codecs exotiques (HEVC, etc.).

![Version](https://img.shields.io/badge/version-1.2.0-blue.svg)
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

## 🆕 Quoi de neuf dans la v1.2.0 ?

Cette version résout le gros point noir du lecteur Windows intégré et offre une expérience visuelle complètement repensée :

- 🛠️ **Correctif du bug "codec non supporté"** : un **proxy de prévisualisation** (H.264 baseline + AAC, basse résolution) est désormais généré en interne avant l'affichage. Résultat : **l'aperçu fonctionne pour quasiment tous les fichiers**, y compris HEVC, ProRes, et autres codecs exotiques que le lecteur Windows intégré ne sait pas décoder nativement. Le proxy ne sert qu'à la prévisualisation — l'export final travaille toujours sur le fichier source en pleine qualité.
- 🎞️ **Timeline visuelle dans l'éditeur avancé** : une vraie barre de montage dessine la vidéo, vos segments (en dégradé corail/or) et la tête de lecture. Cliquable, comme dans un logiciel de montage classique.
- 🎨 **Interface repensée** : nouvelle palette chaleureuse (bruns / anthracite + accent corail-or), sections numérotées en cartes, en-tête avec icônes. Plus lisible, plus intuitive.
- 📐 **Fenêtres redimensionnées** : éditeur avancé et fenêtre de recadrage plus spacieux pour laisser respirer les contrôles et la timeline.
- 🧹 **Nettoyage automatique des fichiers proxy temporaires** à la fermeture des fenêtres d'aperçu — aucun résidu sur votre disque.

> 💡 Sous le capot : nouvelle méthode `CreateCompatiblePreviewAsync` dans `VideoProcessor`, refonte complète de `VideoEditorWindow` (UI + code-behind avec dessin vectoriel via `System.Windows.Shapes`), et adaptation de `TrimWindow` au même système de proxy.

---

## 🚀 Démarrage rapide

1. Téléchargez la dernière version depuis la page [**Releases**](../../releases).
2. Lancez **`TrimVideos.exe`** — aucune installation requise, c'est portable.
3. Choisissez une vidéo **ou** une image + un son.
4. (Optionnel) Cliquez sur **"Éditeur avancé (coupes multiples + effets)..."** pour monter votre vidéo sans perte avant traitement — la timeline visuelle vous guide.
5. Cliquez sur **Démarrer** et laissez FFmpeg faire le travail.
6. Le fichier est prêt, ouvert automatiquement, et la page d'upload s'ouvre dans votre navigateur.

> Au premier lancement, FFmpeg (~100 Mo) est téléchargé automatiquement depuis les builds officiels BtbN/FFmpeg-Builds (GPL, à jour en continu). Connexion Internet requise uniquement pour cette étape.

---

## 🔧 Fonctionnalités détaillées

### 🎬 Éditeur vidéo avancé sans perte

Une fenêtre dédiée accessible depuis le bouton **"Éditeur avancé (coupes multiples + effets)..."** :

- **Aperçu toujours lisible** grâce au proxy H.264/AAC généré en interne : fonctionne même pour les fichiers en HEVC ou codecs exotiques.
- **Timeline visuelle** : la vidéo, les segments marqués (en dégradé corail/or) et la tête de lecture sont dessinés sur une vraie barre de montage cliquable, façon logiciel de montage classique.
- **Coupes multiples** : "Marquer début" → "Marquer fin" → "+ Ajouter" pour empiler les passages à garder. Réorganisables (monter / descendre) et supprimables individuellement.
- **Effets disponibles** : luminosité (slider -1..+1), contraste (0..2), saturation (0..3), rotation (90° / 180° / 270°), plus le mouvement de caméra Ken Burns et la réactivité aux basses déjà présents dans le mode image + son.
- **Export rapide (par défaut)** : coupe + recolle par copie de flux pure — zéro ré-encodage, donc zéro perte garantie. Sortie en `.mp4` (codec audio d'origine conservé tel quel). Coupes alignées sur l'image clé la plus proche.
- **Export précis** : coupes exactes à la frame près, ré-encodage vidéo CRF 0 + audio FLAC, conteneur `.mkv`. Activé automatiquement dès qu'un effet est appliqué (une copie de flux ne peut pas être filtrée).
- **Overlay de chargement** pendant la préparation du proxy, avec message clair si l'aperçu n'est pas disponible.

### 📥 Deux modes d'entrée

- **Mode vidéo** : partez d'une vidéo existante (mp4, mov, mkv, avi, webm).
- **Mode image + son** : choisissez une image fixe et un fichier audio — la vidéo générée dure exactement la durée du son. Idéal pour publier un morceau avec une pochette animée.
- **Mode "commande simple"** : reproduit exactement `ffmpeg -loop 1 -i image -i son -shortest -c:a copy -strict -2 sortie.mp4`. Aucun ré-encodage de l'audio (copie brute du flux d'origine). Le traitement se fait en deux étapes — "Choisir le nom du fichier..." puis "Démarrer" — pour laisser le temps de vérifier avant de lancer l'encodage.

### ✂️ Éditeur de recadrage temporel

Une fenêtre dédiée avec aperçu vidéo (lui aussi alimenté par le proxy v1.2.0, donc lisible dans presque tous les cas) et deux curseurs (début/fin) pour sélectionner précisément la portion à conserver. Fonctionne aussi bien pour la vidéo que pour l'audio en mode image+son. Si l'aperçu ne s'affiche pas malgré tout, un message clair l'indique et la sélection reste utilisable à partir de la durée détectée.

### 🎨 Mouvement de caméra & réactivité à la musique (mode image + son)

- **Ken Burns** : zoom lent continu + léger travelling, intensité réglable.
- **Réactif aux basses** : le zoom pulse sur les basses/kicks détectés dans l'audio (analyse d'énergie de la bande ~80 Hz par fenêtres de 50 ms, détection de pics). Sensibilité réglable.
- Les deux effets se combinent. Techniquement : l'image est mise à l'échelle avec 30 % de marge, puis zoomée/déplacée dynamiquement par des expressions FFmpeg évaluées image par image, avant un recadrage final — jamais de bord vide visible, même en mouvement.
- **Ces effets s'appliquent aussi à une vidéo existante** (mode vidéo, portrait ou paysage) — y compris dans l'éditeur avancé.

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
  -p:AssemblyName=TrimVideos `
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
- ✅ L'**éditeur avancé** préserve la qualité d'origine dans les deux modes d'export (rapide par copie de flux, ou précis via CRF 0 + FLAC).
- ✅ Le **proxy de prévisualisation** (v1.2.0) reste local et temporaire : il est supprimé automatiquement à la fermeture de la fenêtre, et l'export final n'utilise que le fichier source en pleine qualité.

---

## 🗺️ Feuille de route

- [x] ✅ **Éditeur vidéo avancé sans perte** (coupes multiples, effets, deux modes d'export) — livré en v1.1.0.
- [x] ✅ **Proxy de prévisualisation** + **timeline visuelle** + **interface repensée** — livré en v1.2.0.
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
