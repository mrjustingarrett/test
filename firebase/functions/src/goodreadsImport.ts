import * as functions from "firebase-functions/v2/https";
import * as admin from "firebase-admin";

interface GoodreadsImportRequest {
  csv: string;     // raw legacy Goodreads CSV
}

/**
 * Parses legacy Goodreads CSV (export was removed late 2024 — users supply old
 * exports or third-party scraper output) and writes to users/{uid}/books.
 *
 * Legacy schema columns:
 *   Book Id, Title, Author, ISBN, ISBN13, My Rating, Average Rating,
 *   Publisher, Binding, Number of Pages, Year Published,
 *   Original Publication Year, Date Read, Date Added, Bookshelves,
 *   Bookshelves with positions, Exclusive Shelf, My Review, Spoiler,
 *   Private Notes, Read Count, Owned Copies
 *
 * M7 TODO:
 *  - Parse CSV (ISBNs come quoted like ="0743273567" — strip the = and quotes)
 *  - Resolve missing metadata via Open Library
 *  - Batch writes in groups of 50 (Firestore batch limit is 500 ops; stay safe)
 *  - Idempotent: bookId = hash(isbn13 || title|author|year)
 */
export const goodreadsImport = functions.onCall(
  { enforceAppCheck: true, timeoutSeconds: 300, memory: "512MiB" },
  async (request) => {
    if (!request.auth) {
      throw new functions.HttpsError("unauthenticated", "Sign in required.");
    }
    const { csv } = request.data as GoodreadsImportRequest;
    if (!csv) {
      throw new functions.HttpsError("invalid-argument", "Missing csv.");
    }
    return { status: "stub", uid: request.auth.uid, bytes: csv.length };
  }
);

void admin;
