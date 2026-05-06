# Library Game

A mobile library tracker that's also a 3D room-builder game. Track your books (search, ISBN scan, manual entry, Goodreads CSV import), review them, and use your reading progress to unlock furniture, wall art, and seasonal cosmetics for a 3D library you decorate. Cross-platform (iOS + Android), freemium with a season pass.

Status: **scaffold**. See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) and the implementation plan in `/root/.claude/plans/` for the full design.

## Stack

- **Engine:** Unity **2022.3 LTS** (C#) with URP
- **UI:** UI Toolkit (UITK) for 2D screens; uGUI only for diegetic in-world labels
- **Backend:** Firebase — Auth, Firestore, Cloud Functions (Node 20 + TS), Remote Config, Crashlytics, Analytics, Storage, App Check
- **Book data:** Open Library API (free, no key)
- **IAP:** Unity IAP 4.11+ with server-side receipt validation in Cloud Functions
- **Barcode:** ZXing.Net DLL + WebCamTexture

## Repo layout

```
Assets/_Project/        first-party Unity content (art, scenes, prefabs, scripts, UI, SOs)
Packages/manifest.json  Unity package set (URP, Cinemachine, Input System, Addressables, IAP, ...)
ProjectSettings/        Unity project settings (force-text serialization)
firebase/               firestore.rules, indexes, storage rules, RC template
  functions/            Cloud Functions (TypeScript, Node 20)
.github/workflows/      CI (Android smoke build via game-ci/unity-builder)
docs/                   ARCHITECTURE, DATA_MODEL, RUNBOOK
```

## First-time local setup

1. **Install Unity Hub** and **Unity 2022.3.45f1** with iOS + Android Build Support modules.
2. **Git LFS** is required (`git lfs install`) — binary art/audio is tracked through LFS.
3. **Clone** and open the repo root in Unity Hub. On first import Unity will:
   - Resolve the package manifest
   - Generate the remaining `ProjectSettings/*.asset` defaults
   - Generate `.meta` files for all source files — **commit these**
4. In Unity, run **Library Game → Create Initial Scenes** (top menu) once. This creates `00_Boot`, `01_Auth`, `02_MainHub`, `03_Loading` under `Assets/_Project/Scenes/` and wires them into Build Settings.
5. **Firebase setup:**
   ```
   cd firebase
   firebase login
   firebase use --add        # select your Firebase project
   ```
   Then in `firebase/functions/`:
   ```
   npm install
   ```
6. **Firebase config files** — drop these into `Assets/` (gitignored):
   - `google-services.json` (Android)
   - `GoogleService-Info.plist` (iOS)
7. **Firebase Unity SDKs** — download from <https://firebase.google.com/download/unity> and import these `.unitypackage`s: Auth, Firestore, Functions, RemoteConfig, Crashlytics, Analytics, Storage, AppCheck. (They are not on UPM.)
8. **Android dependency resolve:** in Unity, **Assets → External Dependency Manager → Android Resolver → Force Resolve** before the first Android build.

## Running locally

- **Firebase emulators:** `cd firebase && firebase emulators:start --only auth,firestore,functions,storage` (UI on http://localhost:4000)
- **Cloud Functions watch:** `cd firebase/functions && npm run build:watch`

## CI

- `.github/workflows/unity-ci.yml` runs an Android smoke build on every PR via `game-ci/unity-builder`. Requires repo secrets `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`.
- Cloud Functions build + lint on every PR.

## Plan

The full implementation plan with milestones (M0 → M10), data model, and pitfalls is at `/root/.claude/plans/i-m-wanting-to-build-optimized-sunrise.md` plus the runtime docs in `docs/`.
