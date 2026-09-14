# 🛠️ TrimVideos v1.3.0 — Pochettes intégrées audio, messages FFmpeg plus clairs 🛠️

Bienvenue dans la v1.3.0 de **TrimVideos** (anciennement ShortsPrep) ! Cette release est plus modeste que les précédentes — deux corrections ciblées sur des points qui fâchaient au quotidien, mais dont l'absence était vraiment pénible.

---

## 🆕 Nouveautés de la v1.3.0

### 🎵 Bugfix : MP3 / FLAC avec pochette intégrée

**Symptôme :** un fichier `.mp3` ou `.flac` contenant une pochette (`cover art` embarquée dans le flux lui-même) faisait planter la génération du proxy de prévisualisation. Le mode image + son restait fonctionnel sur ces fichiers, mais la fenêtre de l'éditeur avancé ou celle de recadrage se fermait avec une erreur disgracieuse.

**Cause :** `ffprobe` voit la pochette intégrée comme un flux `video` d'une seule image, marquée `disposition.attached_pic=1`. Pour `ffmpeg`, c'est du flux vidéo « normal », mais quand le proxy essaie d'y appliquer `-vf "scale=480:-2"` puis d'écrire dans un conteneur `.mp4`, le conteneur refuse de stocker une vidéo H.264 à la place d'une image de couverture — d'où l'échec.

**Correctif v1.3.0 :** dans `VideoProcessor.ProbeAsync`, on détecte maintenant ce cas précis :

```csharp
bool isAttachedPic = stream.TryGetProperty("disposition", out var disposition)
    && disposition.TryGetProperty("attached_pic", out var attachedPic)
    && attachedPic.GetInt32() == 1;

if (type == "video" && width == 0 && !isAttachedPic)
    // ... pris comme vidéo réelle
```

Les « vidéos » de pochette sont désormais ignorées, et la branche audio (`-vn`) de `CreateCompatiblePreviewAsync` reçoit en plus un `-vn` explicite pour exclure toute pochette intégrée en sortie, ceinture & bretelles.

**Conséquence utilisateur :** un `.mp3` avec cover art s'ouvre maintenant correctement dans l'éditeur et la fenêtre de recadrage. Le fichier reste intact (la pochette n'est jamais modifiée), et le rendu final conserve tout son audio en qualité AAC / FLAC.

### 📜 Bugfix : messages d'erreur FFmpeg illisibles

**Symptôme :** en cas d'erreur FFmpeg (encode qui plante, codec non disponible, fichier corrompu…), l'exception affichée contenait **toute** la sortie stderr de `ffmpeg` : la longue bannière de version (« `ffmpeg version 7.0.2 Copyright (c) 2000-2024 the FFmpeg developers` »), la liste des options de compilation (« `--enable-gpl --enable-libx264 --enable-libfdk_aac ... »), etc. La vraie cause de l'erreur se noie au milieu de ces 50+ premières lignes techniques sans intérêt pour 99 % des utilisateurs.

**Correctif v1.3.0 :** la sortie est désormais filtrée avec `TakeLast(8)` — on ne garde que les 8 dernières lignes du journal, qui contiennent presque toujours la vraie cause (erreur de codec, fichier introuvable, format non supporté, etc.) :

```csharp
var lines = stderrLog.ToString()
    .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
var tail = string.Join('\n', lines.TakeLast(8));
throw new InvalidOperationException($"FFmpeg a échoué (code {process.ExitCode}) :\n{tail}");
```

**Conséquence utilisateur :** en cas de problème, le message affiché est directement lisible et exploitable — plus besoin de scroller dans un log de 200 lignes pour trouver la vraie raison.

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
- **v1.3.0** (cette version) — Pochettes audio + messages FFmpeg plus clairs.

---

## 📥 Téléchargements

| Fichier | Description |
|---------|-------------|
| **`TrimVideos.exe`** | Exécutable portable Windows (zéro installation, ~155 Mo self-contained). |
| **`TrimVideos-1.3.0-source.zip`** | Code source complet de la version 1.3.0. |

### Utilisation

1. Téléchargez `TrimVideos.exe`.
2. Lancez-le — aucune installation, c'est portable.
3. Au premier lancement, FFmpeg (~100 Mo) est téléchargé automatiquement.
4. Choisissez une vidéo **ou** une image + un son (y compris maintenant un `.mp3` / `.flac` avec pochette intégrée, qui s'ouvre sans planter).
5. (Optionnel) Cliquez sur **« Éditeur avancé (coupes multiples + effets)... »** si vous voulez monter la vidéo sans perte avant traitement.
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

---

## 🗺️ Feuille de route

- [x] ✅ Éditeur vidéo avancé sans perte (coupes multiples, effets, deux modes d'export) — v1.1.0.
- [x] ✅ Proxy de prévisualisation + timeline visuelle + interface repensée — v1.2.0.
- [x] ✅ Correctif pochettes intégrées + messages FFmpeg plus clairs — **v1.3.0**.
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
