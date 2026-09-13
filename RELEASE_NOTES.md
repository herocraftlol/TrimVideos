# 🚀 TrimVideos v1.1.0 — Éditeur vidéo avancé sans perte 🎬

Bienvenue dans la v1.1.0 de **TrimVideos** (anciennement ShortsPrep) ! Cette release ajoute un **éditeur vidéo avancé complet**, entièrement sans perte, qui transforme l'application en un véritable petit studio de montage dédié aux vidéos courtes.

---

## 🆕 Nouveautés de la v1.1.0

### 🎬 Éditeur vidéo avancé sans perte (gros morceau de cette release)

Une nouvelle fenêtre **"Éditeur avancé (coupes multiples + effets)..."** accessible depuis le mode vidéo, qui permet :

- **Coupes multiples** : marquez autant de passages à garder que vous le souhaitez, dans l'ordre que vous voulez, et ils seront collés bout à bout à l'export. Réorganisables (monter / descendre), supprimables individuellement.
- **Effets colorimétriques et rotation** : luminosité (slider -1..+1), contraste (0..2), saturation (0..3), rotation (90° / 180° / 270°).
- **Mouvement de caméra Ken Burns** + **réactivité aux basses** : les mêmes effets que ceux déjà disponibles dans le mode image + son, désormais accessibles aussi depuis l'éditeur.
- **Deux modes d'export au choix** :
  - **Rapide** *(par défaut)* : copie de flux pure (zéro ré-encodage, donc zéro perte garantie). Coupes alignées sur l'image clé la plus proche, comme *LosslessCut*. Sortie `.mp4`, ultra rapide.
  - **Précis (sans perte)** : coupes exactes à la frame près, effets appliqués, vidéo CRF 0 + audio FLAC → 100 % sans perte mathématique. Sortie `.mkv`.
- Bascule **automatique vers le mode précis** dès qu'un effet (colorimétrie, rotation, mouvement de caméra ou basses) est actif — une copie de flux ne peut pas être filtrée.
- Lecture vidéo intégrée (aperçu, pause, scrubber de défilement) avec gestion propre d'un échec d'aperçu (message clair si le codec n'est pas lu par le lecteur Windows intégré, montage / export restant possible).

### 🛠️ Améliorations techniques

- Nouveaux types `EditSegment`, `EditEffects` et `CutExportMode` (`ShortsPrep/EditEffects.cs`).
- Nouvelles méthodes `EditVideoAsync` et `EditByStreamCopyAsync` dans `VideoProcessor` — pipeline FFmpeg dédié au montage multi-segments avec ou sans filtre.
- L'analyse des basses (utilisée pour l'effet "réactif aux basses") est désormais réutilisable aussi dans l'éditeur.
- Refactorisation légère de `MainWindow.xaml` : ajout du bouton **"Éditeur avancé (coupes multiples + effets)..."** dans le panneau du mode vidéo (activé dès qu'une vidéo est sélectionnée).
- Version de l'assembly bumpée à **1.1.0** (`ShortsPrep.csproj`).

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
| **`TrimVideos-1.1.0-source.zip`** | Code source complet de la version 1.1.0. |

### Utilisation

1. Téléchargez `TrimVideos.exe`.
2. Lancez-le — aucune installation, c'est portable.
3. Au premier lancement, FFmpeg (~100 Mo) est téléchargé automatiquement. Connexion Internet requise uniquement pour cette étape.
4. Choisissez une vidéo **ou** une image + un son.
5. **(Nouveau)** Cliquez sur **"Éditeur avancé (coupes multiples + effets)..."** si vous voulez monter la vidéo sans perte avant traitement.
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
dotnet build ShortsPrep/ShortsPrep.csproj -c Release -p:EnableWindowsTargeting=true
dotnet run --project ShortsPrep/ShortsPrep.csproj -p:EnableWindowsTargeting=true
```

Pour publier un exécutable portable :

```powershell
dotnet publish ShortsPrep/ShortsPrep.csproj `
  -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableWindowsTargeting=true `
  -p:AssemblyName=TrimVideos `
  -o publish
```

Prérequis : [SDK .NET 8](https://dotnet.microsoft.com/download) (Windows, ou Linux/macOS avec le flag `-p:EnableWindowsTargeting=true` pour le développement).

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
