# 🎬 TrimVideos v1.4.0 — Aperçu des vidéos portrait sans crash 🎬

Bienvenue dans la v1.4.0 de **TrimVideos** (anciennement ShortsPrep) ! Cette release résout un bug d'encodage discret mais frustrant qui touchait les utilisateurs de vidéos portrait enregistrées au téléphone.

---

## 🆕 Nouveautés de la v1.4.0

### 🎬 Bugfix : l'aperçu échouait sur certaines vidéos portrait de téléphone

**Symptôme :** sur certains fichiers vidéo portrait (typiquement ceux filmés à la verticale sur iPhone/Android, mais aussi d'autres cas moins évidents), la génération du proxy de prévisualisation plantait avec une erreur cryptique du type « *x264 [error]: open: Invalid argument* » ou « *could not open encoder* ». Conséquence : pas d'aperçu dans la fenêtre de recadrage ni dans l'éditeur avancé, même si le montage / export final restait possible.

**Cause :** l'encodeur `libx264` du proxy imposait `profile:v baseline -level 3.0`, deux contraintes pensées pour la compatibilité de très vieux appareils. Sur certaines combinaisons résolution / fréquence d'images (typiquement : portrait 9:16 + fréquence d'images élevée = grand nombre de macroblocs/seconde), le débit calculé dépassait la limite autorisée par le niveau 3.0 et l'encodeur refusait carrément d'ouvrir la session d'encodage.

**Correctif v1.4.0 :** les contraintes `-profile:v baseline -level 3.0` sont retirées, et remplacées par `-pix_fmt yuv420p` :

```diff
- $"-c:v libx264 -preset ultrafast -crf 28 -profile:v baseline -level 3.0 " +
+ $"-c:v libx264 -preset ultrafast -crf 28 -pix_fmt yuv420p " +
```

**Pourquoi ce changement est correct :**

- Le `MediaElement` WPF décode **tous les profils H.264** sans restriction (high, main, baseline…) — il n'a jamais eu besoin qu'on lui force baseline.
- Le `MediaElement` WPF exige en revanche **`yuv420p`** comme pixel format : un autre pixel format (yuv444p, yuvj420p…) fait apparaître un écran noir sans message d'erreur. C'est la *seule* contrainte vraiment nécessaire.
- Retirer `-level 3.0` supprime la limite de débit macroblocs/seconde qui faisait planter l'encodeur.

**Conséquence utilisateur :** le proxy de prévisualisation fonctionne désormais pour **toutes les vidéos**, sans exception. Pas de message d'erreur cryptique, pas d'aperçu noir — la fenêtre de recadrage et l'éditeur avancé affichent le contenu immédiatement.

---

## ✨ Rappel : à quoi sert TrimVideos ?

TrimVideos est un **outil Windows gratuit et open-source** (WPF / .NET 8) qui automatise toute la partie fastidieuse de la publication d'une vidéo courte sur les réseaux sociaux : rognage, conversion au format portrait 9:16, choix de la qualité, montage sans perte, et ouverture des pages d'upload.

**Trois modes d'entrée** sont proposés :

- **Une vidéo existante** (mp4, mov, mkv, avi, webm), avec possibilité de la monter sans perte dans l'éditeur avancé (timeline visuelle + coupes multiples + effets + deux modes d'export rapide/précis).
- **Une image fixe + un son** — la vidéo générée dure exactement la durée du son, idéal pour publier un morceau avec une pochette animée (effet Ken Burns + basses disponible).
- **Mode « commande simple »** : `ffmpeg -loop 1 -i image -i son -shortest -c:a copy -strict -2 sortie.mp4` reproduit tel quel, sans ré-encoder l'audio.

### Ce qui a été ajouté au fil des versions

- **v1.0.0** — Première publication officielle : tronc de rognage, conversion portrait 9:16, gestion de FFmpeg.
- **v1.1.0** — Éditeur vidéo avancé sans perte : coupes multiples, effets colorimétriques / rotation / Ken Burns, deux modes d'export rapide / précis, sortie `.mp4` ou `.mkv`.
- **v1.2.0** — Proxy de prévisualisation H.264/AAC (le lecteur Windows intégré affiche enfin l'aperçu sur quasi tous les codecs, y compris HEVC), timeline visuelle dans l'éditeur, interface complètement repensée (palette bruns / corail-or).
- **v1.3.0** — Pochettes audio MP3/FLAC supportées + messages d'erreur FFmpeg plus lisibles.
- **v1.4.0** (cette version) — Contraintes H.264 profile/level retirées du proxy, l'aperçu fonctionne désormais sur **toutes** les vidéos (notamment portrait téléphone).

---

## 📥 Téléchargements

| Fichier | Description |
|---------|-------------|
| **`TrimVideos.exe`** | Exécutable portable Windows (zéro installation, ~155 Mo self-contained). |
| **`TrimVideos-1.4.0-source.zip`** | Code source complet de la version 1.4.0. |

### Utilisation

1. Téléchargez `TrimVideos.exe`.
2. Lancez-le — aucune installation, c'est portable.
3. Au premier lancement, FFmpeg (~100 Mo) est téléchargé automatiquement.
4. Choisissez une vidéo **ou** une image + un son.
5. (Optionnel) Cliquez sur **« Éditeur avancé (coupes multiples + effets)... »** pour monter la vidéo sans perte avant traitement — l'aperçu s'affiche désormais pour toutes les vidéos.
6. Cliquez sur **Démarrer** et laissez FFmpeg travailler.
7. Le fichier est prêt, ouvert automatiquement, et la page d'upload s'ouvre dans votre navigateur. Il ne reste plus qu'à glisser le fichier !

---

## 🔧 Prérequis techniques

- **OS** : Windows 10 ou Windows 11 (64 bits).
- **Aucune dépendance** : FFmpeg est téléchargé automatiquement au premier lancement.

---

## 🛠️ Pour les développeurs

```powershell
git clone https://github.com/herocraftlol/TrimVideos.git
cd TrimVideos
dotnet build ShortsPrep/ShortsPrep.csproj -c Release
dotnet run --project ShortsPrep/ShortsPrep.csproj
```

Publication d'un exécutable portable :

```powershell
dotnet publish ShortsPrep/ShortsPrep.csproj `
  -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:AssemblyName=TrimVideos `
  -o publish
```

Prérequis : [SDK .NET 8](https://dotnet.microsoft.com/download) (Windows).

> ⚠️ Note technique : `VideoEditorWindow.xaml.cs` utilise `using Path = System.IO.Path;` pour lever l'ambiguïté entre `System.IO.Path` (utilisé par `Path.Combine`, `Path.GetTempPath()`, etc.) et `System.Windows.Shapes.Path` (importé pour `Rectangle` dans la timeline visuelle). Sans cet alias, la build échoue.

---

## 🗺️ Feuille de route

- [x] ✅ Éditeur vidéo avancé sans perte (coupes multiples, effets, deux modes d'export) — v1.1.0.
- [x] ✅ Proxy de prévisualisation + timeline visuelle + interface repensée — v1.2.0.
- [x] ✅ Pochettes intégrées + messages FFmpeg plus clairs — v1.3.0.
- [x] ✅ Contraintes H.264 profile/level retirées du proxy, l'aperçu fonctionne désormais sur toutes les vidéos — **v1.4.0**.
- [ ] Upload automatique réel vers YouTube Shorts via l'API Google.
- [ ] Watermark / recadrage ajustable à la souris (aperçu avant traitement).
- [ ] File d'attente pour traiter plusieurs vidéos d'un coup.
- [ ] Version Android (nécessiterait une réécriture — FFmpeg via `FFmpegKit`, interface en Kotlin/Jetpack Compose ; projet distinct du WPF).

---

## ❓ Pourquoi « semi-automatique » ?

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
- **FFmpeg** : inclus automatiquement, sous licence GPL « full » — merci à [BtbN/FFmpeg-Builds](https://github.com/BtbN/FFmpeg-Builds).

Bon montage, et bonne publication ! 🎬✨
