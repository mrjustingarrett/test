# Architecture

## Overview
Cross-platform mobile app (iOS + Android) built in **Unity 2022.3 LTS** (C#, URP) with **Firebase** as the backend.

The app is a **library tracker + 3D room builder game**: users add books (search, ISBN scan, manual, Goodreads CSV import), and reading them unlocks furniture, wall art, and seasonal cosmetics for a 3D library room they can decorate. Monetized as freemium with IAP and a season pass.

## Engine
- Unity **2022.3 LTS** pinned via `ProjectSettings/ProjectVersion.txt`
- **URP** mobile renderer; Color Space Linear; Metal (iOS) / Vulkan + GLES3 (Android)
- **UI Toolkit** for all 2D UI (library list, search, reviews, inventory, shop, season pass); uGUI only for diegetic in-world labels

## Backend (Firebase)
- **Auth** — Apple, Google, Email-link, Anonymous (with upgrade-link)
- **Firestore** — primary data store; rules in `firebase/firestore.rules`
- **Cloud Functions** (Node 20 + TS) — server-authoritative for currency, entitlements, IAP receipt validation, season pass grants, Goodreads import
- **Remote Config** — feature flags + tunables (XP rates, season id, prices)
- **Crashlytics + Analytics** — crash reporting + typed events that double as achievement triggers
- **App Check** — DeviceCheck (iOS) / Play Integrity (Android); debug provider during dev

## Service Layout
- `Bootstrap` (00_Boot scene) wires a single persistent prefab containing all services
- `ServiceLocator` exposes them with typed lookup; no `Singleton<T>` proliferation
- Services live under `Assets/_Project/Scripts/Services/{Auth, Save, Books, Inventory, Room, Achievements, SeasonPass, Weather, IAP, RemoteConfig, Analytics}`

## Save Strategy
Two-tier:
- `LocalSaveStore` — JSON in `Application.persistentDataPath`, write-through
- `CloudSaveStore` — Firestore
- Conflict policy: last-write-wins per document; `roomLayouts.placements` merged as a set keyed by `(itemId, gridCell)`

## Scenes
- `00_Boot` → init, branch
- `01_Auth` → sign-in
- `02_MainHub` → 3D room (persistent for session) + UITK HUD
- `03_Loading` → transitional

## Build / Dependency Notes
- Firebase comes via `.unitypackage` imports, not UPM
- EDM4U resolves Android deps every build — commit `Assets/Plugins/Android/*.aar` only after a clean resolve
- Addressables for cosmetic packs / seasonal content; keep first-install lean (<200 MB on cellular per Apple guidance)
