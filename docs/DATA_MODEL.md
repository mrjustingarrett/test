# Data Model (Firestore)

All timestamps are Firestore `Timestamp`. Money in minor units (cents).

## `users/{uid}`
Profile + denormalized stats + currency + entitlement flags. `currency.hard` and `flags.*` are server-only-writable.

```
{
  uid, displayName, handle, photoURL, createdAt, lastSeenAt, locale,
  privacy: { profilePublic, libraryPublic },
  stats: { booksRead, booksOwned, reviewsCount, streakDays },
  currency: { soft, hard },
  flags: { isPremium, activeSeasonPassId }
}
```

## `users/{uid}/books/{bookId}`
`bookId` = stable hash of `isbn13` if available, else of `(title|author|year)`.

```
{
  bookId, source: "openlibrary"|"manual"|"goodreads",
  olWorkKey, olEditionKey, isbn10, isbn13,
  title, authors[], coverUrl, publisher, publishYear, pageCount,
  genres[], series: { name, index } | null,
  status: "want_to_read"|"reading"|"read"|"dnf",
  shelves[], addedAt, startedAt, finishedAt,
  rating, progress: { page, percent } | null
}
```

## `users/{uid}/reviews/{reviewId}`
```
{ reviewId, bookId, body (markdown), rating, spoiler, createdAt, updatedAt,
  visibility: "private"|"friends"|"public" }
```

## `users/{uid}/ownedItems/{itemId}`
`itemId` = catalog SKU (e.g. `furn_oak_bookshelf_01`, `seasonal_xmas_tree_2026`).
```
{ itemId, acquiredAt, source: "achievement"|"purchase_soft"|"purchase_hard"|"season_pass"|"gift",
  seasonPassId, variantId, quantity }
```

## `users/{uid}/roomLayouts/{layoutId}`
```
{
  layoutId, templateId, name, isActive,
  placements: [{ itemId, variantId, position{x,y,z}, rotationY, gridCell{x,z}, scale }],
  updatedAt, schemaVersion
}
```

## `users/{uid}/achievements/{achievementId}`
```
{ achievementId, progress, target, unlockedAt, rewardClaimedAt, rewardItemIds[] }
```

## `users/{uid}/seasonPassProgress/{seasonId}`
```
{ seasonId, tier, xp, isPremium, claimedFreeTiers[], claimedPremiumTiers[],
  startedAt, expiresAt }
```

## Top-Level Catalogs (server-authored, public-read)
- `catalog/furniture/{itemId}` — display data for shop UI
- `catalog/seasons/{seasonId}` — tier rewards, dates
- `catalog/achievements/{achievementId}` — definitions
- `catalog/iap/{sku}` — server-side SKU registry

## `bookCache/{bookId}`
Global Open Library cache, written via Cloud Functions. Reduces per-user calls and survives Open Library outages.

## Security Rules (sketch)
- `users/{uid}/**` — read/write iff `request.auth.uid == uid`, **except** `currency.hard` and `flags.*` (Functions only)
- `catalog/**` — public read, no client write
- `bookCache/**` — public read, write via Functions only
