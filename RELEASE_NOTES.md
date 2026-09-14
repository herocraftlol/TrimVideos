# ❄️ TrimVideos v1.5.0 — La préparation de l'aperçu ne semble plus jamais bloquée ❄️

Bienvenue dans la v1.5.0 de **TrimVideos** (anciennement ShortsPrep) ! Cette release s'attaque à un comportement qui pouvait sembler anormal : sur des vidéos un peu longues, la fenêtre de recadrage ou l'éditeur avancé affichait « Préparation de l'aperçu… » pendant un certain temps, sans indication visible de progression, et il arrivait que l'utilisateur se demande si l'application était plantée.

---

## 🆕 Nouveautés de la v1.5.0

### ❄️ Bugfix n°1 : la préparation de l'aperçu pouvait sembler bloquée indéfiniment

**Symptôme :** sur des vidéos longues ou dans certains cas limites, le bouton « Préparation de l'aperçu… » restait affiché sans aucune indication de progression pendant plusieurs dizaines de secondes — voire, dans de rares cas extrêmes, plus longtemps que prévu.

**Cause (trois facteurs combinés) :**

1. **Sortie standard non lue** — Le code redirigeait stdout de ffmpeg mais ne le lisait nulle part (`process.StandardOutput` n'était jamais ni drainé ni lu). En .NET, c'est un piège classique : si le tampon système (typiquement 4 Ko côté pipe) venait à se remplir, ffmpeg se bloquait en écriture, attendant qu'on lise — et comme personne ne le faisait, le process restait bloqué silencieusement. C'est un blocage purement logiciel, pas un blocage ffmpeg réel.

2. **Aucune progression remontée à l'UI** — Même quand l'encodage avançait vraiment, rien ne le disait à l'utilisateur. Le texte « Préparation de l'aperçu… » restait figé.

3. **Aucune limite de temps** — Si jamais le process ne se terminait vraiment pas (cas extrême), il n'y avait aucun garde-fou.

**Correctif v1.5.0 — trois points :**

1. **Drainage de stdout** — On attache un handler vide et on appelle `BeginOutputReadLine()` pour que le tampon système soit vidé en continu. ffmpeg ne peut plus se bloquer sur stdout :

   ```csharp
   // vider stdout en continu (sinon le tampon système peut se remplir
   // et bloquer ffmpeg silencieusement)
   process.OutputDataReceived += (_, _) => { };
   // ... et après process.Start() :
   process.BeginOutputReadLine();
   ```

2. **Lecture parallèle de stdout et stderr** — Au lieu de lire l'un après l'autre (ce qui peut bloquer si l'autre se remplit entre-temps), on lance les deux en parallèle et on attend les deux ensemble avec `Task.WhenAll`. C'est appliqué aussi à `BassAnalyzer.cs` :

   ```csharp
   var stdoutTask = process.StandardOutput.ReadToEndAsync();
   var stderrTask = process.StandardError.ReadToEndAsync();
   await Task.WhenAll(stdoutTask, stderrTask);
   ```

3. **Progression réelle affichée à l'utilisateur** — `CreateCompatiblePreviewAsync` accepte désormais un `IProgress<int>?` en troisième paramètre :

   ```csharp
   public async Task CreateCompatiblePreviewAsync(
       string inputPath, string outputPath, IProgress<int>? percentProgress = null)
   ```

   La fenêtre de recadrage ET l'éditeur avancé créent un `Progress<int>` qui met à jour le message « Préparation de l'aperçu (43%)… » en direct. Sur une vidéo de 30 secondes, on voit le pourcentage monter jusqu'à 100%.

4. **Timeout de sécurité 2 minutes** — `RunAsync` accepte désormais un `TimeSpan? timeout` :

   ```csharp
   if (timeout is not null)
   {
       var waitTask = process.WaitForExitAsync();
       var completed = await Task.WhenAny(waitTask, Task.Delay(timeout.Value));
       if (completed != waitTask)
       {
           try { process.Kill(entireProcessTree: true); } catch { /* déjà terminé entre-temps */ }
           throw new InvalidOperationException(
               $"FFmpeg n'a pas terminé après {timeout.Value.TotalSeconds:F0}s (arrêté par sécurité). " +
               "Le fichier est peut-être très volumineux/long, ou dans un format inhabituel.");
       }
   }
   ```

   La préparation de l'aperçu est appelée avec `timeout: TimeSpan.FromMinutes(2)`. Au-delà, l'utilisateur a un message clair au lieu d'attendre indéfiniment.

**Conséquence utilisateur :** la fenêtre de recadrage et l'éditeur avancé affichent maintenant un pourcentage qui avance, on voit en temps réel que le proxy se construit. Dans tous les cas extrêmes où l'encodage ne se terminerait pas vraiment, l'application se débloque au bout de 2 minutes avec un message clair.

---

### 🤖 Nouveauté : workflow GitHub Actions

Pour faciliter la contribution, une workflow **`.github/workflows/build.yml`** est ajoutée : à chaque push sur `main` ou `master`, ou manuellement via l'onglet Actions, un runner **Windows** compile l'exécutable portable et l'upload en tant qu'artifact `ShortsPrep-portable` (contenant `ShortsPrep.exe`). Pratique pour vérifier rapidement qu'un patch ne casse pas la build, sans avoir à installer .NET localement.

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
- **v1.4.0** — Contraintes H.264 profile/level retirées du proxy, l'aperçu fonctionne désormais sur **toutes** les vidéos (notamment portrait téléphone).
- **v1.5.0** (cette version) — Suppression du risque de blocage silencieux + progression visible + timeout 2 min + workflow GitHub Actions.

---

## 📥 Téléchargements

| Fichier | Description |
|---------|-------------|
| **`TrimVideos.exe`** | Exécutable portable Windows (zéro installation, ~155 Mo self-contained). |
| **`TrimVideos-1.5.0-source.zip`** | Code source complet de la version 1.5.0. |

### Utilisation

1. Téléchargez `TrimVideos.exe`.
2. Lancez-le — aucune installation, c'est portable.
3. Au premier lancement, FFmpeg (~100 Mo) est téléchargé automatiquement.
4. Choisissez une vidéo **ou** une image + un son.
5. (Optionnel) Cliquez sur **« Éditeur avancé (coupes multiples + effets)... »** — la fenêtre affiche désormais un pourcentage qui avance pendant la préparation de l'aperçu.
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
- [x] ✅ Contraintes H.264 profile/level retirées du proxy — v1.4.0.
- [x] ✅ Suppression du risque de blocage silencieux + progression visible + timeout 2 min + workflow CI — **v1.5.0**.
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
