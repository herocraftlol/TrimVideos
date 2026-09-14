# 🎨 TrimVideos v1.2.0 — Proxy d'aperçu, timeline visuelle & interface repensée 🛠️

Bienvenue dans la v1.2.0 de **TrimVideos** (anciennement ShortsPrep) ! Cette release s'attaque au plus gros point noir de la v1.1.0 (le lecteur Windows intégré qui refuse d'afficher certains fichiers) et offre une refonte visuelle complète de l'éditeur avancé.

---

## 🆕 Nouveautés de la v1.2.0

### 🛠️ Correctif majeur : proxy de prévisualisation toujours compatible

Le lecteur Windows intégré (`MediaElement` WPF) ne sait décoder qu'un nombre limité de codecs (H.264 de préférence). Avec une vidéo en **HEVC, ProRes, ou tout autre codec exotique**, l'aperçu restait noir avec un simple message "codec non supporté" — alors que le montage et l'export fonctionnaient parfaitement.

La v1.2.0 corrige ça : avant l'affichage, l'application **génère en interne un proxy** (H.264 baseline + AAC, 480p, encodage ultra-rapide) dans un fichier temporaire, puis lit ce proxy avec `MediaElement`. Résultat : l'aperçu fonctionne pour **quasiment tous les fichiers**, même ceux en HEVC ou codec exotique.

- Le proxy est généré via la nouvelle méthode `CreateCompatiblePreviewAsync(inputPath, outputPath)` dans `VideoProcessor` — elle utilise `ffmpeg` avec `-c:v libx264 -preset ultrafast -crf 28 -profile:v baseline -level 3.0 -c:a aac -b:a 128k -vf "scale=480:-2"`.
- L'export final **n'utilise jamais le proxy** : il travaille toujours sur le fichier d'origine en pleine qualité.
- Le proxy est stocké dans le dossier temporaire Windows (`Path.GetTempPath()`) et **supprimé automatiquement** à la fermeture de la fenêtre (`OnClosed` → `File.Delete`), donc aucun résidu sur votre disque.
- Le système est appliqué à **deux endroits** : l'éditeur avancé (`VideoEditorWindow`) et la fenêtre de recadrage simple (`TrimWindow`).

### 🎞️ Timeline visuelle dans l'éditeur avancé

L'éditeur avancé embarque désormais une vraie **barre de montage cliquable** dessinée avec `System.Windows.Shapes` :

- La **vidéo entière** est affichée en arrière-plan (couleur de fond neutre).
- Les **segments ajoutés** apparaissent par-dessus en **dégradé corail/or**, pour les repérer d'un coup d'œil.
- La **tête de lecture** (position actuelle) est dessinée en temps réel — elle suit la lecture de l'aperçu.
- La timeline est **cliquable** : cliquer dessus déplace la tête de lecture à l'endroit cliqué, comme dans un logiciel de montage classique.

### 🎨 Interface complètement repensée

- Nouvelle **palette chaleureuse** : bruns / anthracite (`#1A1512`, `#241D19`, `#2B231E`, `#3D322A`) + accent **corail-or** pour les éléments actifs et les segments de timeline.
- **Sections numérotées en cartes** : chaque bloc de l'interface principale est présenté comme une carte arrondie avec un numéro de section visible.
- **En-têtes avec icônes** : émojis 🎬 / 🎞️ / 🎚️ / 🎨 / 📦 / 🚀 à côté du titre de chaque section, pour une lecture plus intuitive.
- **Fenêtres redimensionnées** : éditeur avancé (880 × 1100) et fenêtre de recadrage (600 × 680) plus spacieux pour laisser respirer les contrôles.

### 🧹 Qualité de vie

- **Nettoyage automatique** des fichiers proxy temporaires à la fermeture de chaque fenêtre d'aperçu (`OnClosed` + `try/catch best effort`).
- **Messages d'erreur plus clairs** quand l'aperçu échoue : on précise que le montage / export reste possible sans prévisualisation.
- **Slider de position rafraîchi à 150 ms** (au lieu de 200 ms) dans l'éditeur : lecture plus fluide de la tête de lecture sur la timeline.

---

## ✨ Rappel : à quoi sert TrimVideos ?

TrimVideos est un **outil Windows gratuit et open-source** (WPF / .NET 8) qui automatise toute la partie fastidieuse de la publication d'une vidéo courte sur les réseaux sociaux : rognage, conversion au format portrait 9:16, choix de la qualité, montage sans perte, et ouverture des pages d'upload.

**Trois modes d'entrée** sont proposés :

- **Une vidéo existante** (mp4, mov, mkv, avi, webm), avec possibilité de la monter sans perte dans l'éditeur avancé.
- **Une image fixe + un son** — la vidéo générée dure exactement la durée du son, idéal pour publier un morceau avec une pochette animée.
- **Mode "commande simple"** : `ffmpeg -loop 1 -i image -i son -shortest -c:a copy -strict -2 sortie.mp4` reproduit tel quel, sans ré-encoder l'audio.

---

## 📥 Téléchargements

| Fichier | Description |
|---------|-------------|
| **`TrimVideos.exe`** | Exécutable portable Windows (zéro installation, ~155 Mo self-contained). |
| **`TrimVideos-1.2.0-source.zip`** | Code source complet de la version 1.2.0. |

### Utilisation

1. Téléchargez `TrimVideos.exe`.
2. Lancez-le — aucune installation, c'est portable.
3. Au premier lancement, FFmpeg (~100 Mo) est téléchargé automatiquement. Connexion Internet requise uniquement pour cette étape.
4. Choisissez une vidéo **ou** une image + un son.
5. **(Nouveau v1.2.0)** Cliquez sur **"Éditeur avancé (coupes multiples + effets)..."** si vous voulez monter la vidéo sans perte avant traitement — la timeline visuelle vous guide, et l'aperçu fonctionne désormais avec virtually tous les codecs.
6. Cliquez sur **Démarrer** et laissez FFmpeg travailler.
7. Le fichier est prêt, ouvert automatiquement, et la page d'upload s'ouvre dans votre navigateur. Il ne reste plus qu'à glisser le fichier !

---

## 🔧 Prérequis techniques

- **OS** : Windows 10 ou Windows 11 (64 bits).
- **Aucune dépendance** : FFmpeg est intégré au binaire ou téléchargé automatiquement.

---

## 🛠️ Pour les développeurs

```powershell
git clone https://github.com/herocraftlol/TrimVideos.git
cd TrimVideos
dotnet build ShortsPrep/ShortsPrep.csproj -c Release
dotnet run --project ShortsPrep/ShortsPrep.csproj
```

Pour publier un exécutable portable :

```powershell
dotnet publish ShortsPrep/ShortsPrep.csproj `
  -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:AssemblyName=TrimVideos `
  -o publish
```

Prérequis : [SDK .NET 8](https://dotnet.microsoft.com/download) (Windows).

---

## ❓ Pourquoi "semi-automatique" ?

L'upload **entièrement automatisé** (sans aucun clic) vers TikTok/Instagram/YouTube nécessite leurs API officielles, avec des contraintes réelles :

- **YouTube Shorts** : API Google Cloud accessible — sera ajoutée dans une V2.
- **TikTok** : validation d'app obligatoire (dossier de review, délais).
- **Instagram** : compte Business/Creator + app Meta validée requis.

TrimVideos prépare donc tout (fichier prêt, bon format, bonne qualité) et ouvre la bonne page — il ne reste qu'à glisser le fichier. C'est le meilleur compromis simplicité / automatisation aujourd'hui.

---

## 🐛 Problèmes connus & retours

Vous avez trouvé un bug ou vous avez une suggestion ? Ouvrez une [**issue**](../../issues) sur le dépôt, c'est le meilleur moyen de nous aider à améliorer l'outil.

---

## 📜 Crédits & licences

- **Code source** : MIT.
- **FFmpeg** : inclus automatiquement, sous licence GPL "full" — merci à [BtbN/FFmpeg-Builds](https://github.com/BtbN/FFmpeg-Builds).

Bon montage, et bonne publication ! 🎬✨
