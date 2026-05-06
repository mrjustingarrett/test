# Runbook

Operational procedures for the Library Game app. Fill in as systems come online.

## Local Setup
1. Install Unity Hub and Unity **2022.3 LTS** with iOS + Android Build Support.
2. Clone repo (Git LFS required: `git lfs install`).
3. Open project root in Unity Hub.
4. Import Firebase Unity SDK `.unitypackage`s (Auth, Firestore, Functions, RemoteConfig, Crashlytics, Analytics, Storage, AppCheck) from <https://firebase.google.com/download/unity>.
5. Place `google-services.json` (Android) and `GoogleService-Info.plist` (iOS) under `Assets/` — these are gitignored; pull from secrets vault.
6. Run **External Dependency Manager → Android Resolver → Force Resolve** before first Android build.
7. Cloud Functions: `cd firebase/functions && npm install`.
8. Firebase emulator suite: `cd firebase && firebase emulators:start --only auth,firestore,functions`.

## Branching
- Feature work on `claude/<topic>-<id>` or `feat/<topic>` branches.
- Main branch protected; PRs require CI green + one review.

## CI
- GitHub Actions workflow: `.github/workflows/unity-ci.yml`
- Uses `game-ci/unity-builder` for Android smoke build on every PR
- Firestore rules tests run via Firebase emulator (Functions emulator + `@firebase/rules-unit-testing`)

## Releases
TBD — flesh out at M9 (Store readiness).

## Incident Response
TBD — flesh out before v1.0 launch (M10).

## Live-Ops
TBD — season rotation procedures, content pack publishing via Addressables remote, Remote Config rollout.

## Useful Commands
```
# Unity command-line build (CI uses this via game-ci)
Unity -batchmode -quit -projectPath . -buildTarget Android -executeMethod BuildScript.BuildAndroid

# Firebase Functions deploy
cd firebase && firebase deploy --only functions

# Firestore rules deploy
cd firebase && firebase deploy --only firestore:rules

# Run rules unit tests
cd firebase/functions && npm run test:rules
```
