# 🎉 TrimVideos v1.0.0 — Première publication officielle

Bienvenue dans la toute première version publique de **TrimVideos** (anciennement ShortsPrep) ! 🎬

Cette release marque le début du projet. L'application est stable, fonctionnelle et prête à vous faire gagner du temps sur la préparation de vos vidéos courtes pour TikTok, Instagram Reels et YouTube Shorts.

---

## ✨ Qu'est-ce que TrimVideos ?

TrimVideos est un **outil Windows gratuit et open-source** (WPF / .NET 8) qui automatise toute la partie fastidieuse de la publication d'une vidéo courte sur les réseaux sociaux : rognage, conversion au format portrait 9:16, choix de la qualité, et ouverture des pages d'upload.

Deux modes d'entrée sont proposés :
- **Une vidéo existante** (mp4, mov, mkv, avi, webm).
- **Une image fixe + un son** — la vidéo générée dure exactement la durée du son, idéal pour publier un morceau avec une pochette animée.

---

## 🌟 Nouveautés de cette version

- ✅ **Éditeur de recadrage temporel** avec aperçu et deux curseurs (début/fin) pour sélectionner la portion à conserver avant traitement.
- ✅ **Deux modes d'entrée** : vidéo existante **ou** image fixe + son.
- ✅ **Trois niveaux de qualité vidéo** : visuellement sans perte (CRF 16, recommandé), 100 % sans perte (CRF 0), copie du flux si déjà compatible.
- ✅ **Gestion automatique du ratio 9:16** : recadrage centré ou fond flou + image centrée, sans aucune déformation.
- ✅ **Effet Ken Burns** (zoom lent + travelling) et **réactivité aux basses** (analyse de l'énergie de la bande ~80 Hz) pour dynamiser vos visuels statiques. Les deux se combinent et s'appliquent aussi aux vidéos existantes.
- ✅ **Extraction audio sans perte** (WAV 24-bit ou FLAC) en fichier séparé, pour votre archive musicale.
- ✅ **Fichier maître `.mkv` 100 % sans perte** (CRF 0 + FLAC) en option pour conservation.
- ✅ **Limite automatique à 60 s** pour les sorties portrait (contraintes plateformes), désactivable pour conserver la durée d'origine.
- ✅ **Publication en un clic** : ouverture automatique du dossier de sortie et des pages d'upload TikTok / Instagram / YouTube Studio.
- ✅ **FFmpeg maintenu à jour** automatiquement au lancement depuis les builds officiels BtbN (GPL, à jour en continu).
- ✅ **Barre de progression avec pourcentage réel** sur toutes les opérations + ouverture automatique du résultat.

---

## 📥 Téléchargements

| Fichier | Description |
|---------|-------------|
| **`TrimVideos.exe`** | Exécutable portable Windows (zéro installation, ~80 Mo self-contained). |
| **`TrimVideos-1.0.0-source.zip`** | Code source complet de la version 1.0.0. |

### Utilisation

1. Téléchargez `TrimVideos.exe`.
2. Lancez-le — aucune installation, c'est portable.
3. Au premier lancement, FFmpeg (~100 Mo) est téléchargé automatiquement. Connexion Internet requise uniquement pour cette étape.
4. Choisissez une vidéo **ou** une image + un son.
5. Cliquez sur **Démarrer** et laissez FFmpeg travailler.
6. Le fichier est prêt, ouvert automatiquement, et la page d'upload s'ouvre dans votre navigateur. Il ne reste plus qu'à glisser le fichier !

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
