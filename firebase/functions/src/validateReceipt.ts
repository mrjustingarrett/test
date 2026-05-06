import * as functions from "firebase-functions/v2/https";
import * as logger from "firebase-functions/logger";
import * as admin from "firebase-admin";

interface ValidateReceiptRequest {
  store: "apple" | "google";
  productId: string;
  receipt: string;          // base64 (Apple) or JSON (Google)
  signature?: string;       // Google only
}

/**
 * Server-side IAP receipt validation. Client never grants entitlements.
 *
 * M7 TODO:
 *  - Apple: POST receipt to https://buy.itunes.apple.com/verifyReceipt
 *           (sandbox fallback to https://sandbox.itunes.apple.com/verifyReceipt)
 *           Prefer App Store Server API + JWS signed transactions for new builds.
 *  - Google: use googleapis Play Developer API with service account creds.
 *  - On success: write entitlement to users/{uid}.flags or seasonPassProgress
 *    inside a transaction so the client cannot replay receipts.
 */
export const validateReceipt = functions.onCall(
  { enforceAppCheck: true },
  async (request) => {
    if (!request.auth) {
      throw new functions.HttpsError("unauthenticated", "Sign in required.");
    }
    const { store, productId, receipt } = request.data as ValidateReceiptRequest;
    if (!store || !productId || !receipt) {
      throw new functions.HttpsError("invalid-argument", "Missing fields.");
    }

    logger.info("validateReceipt called", {
      uid: request.auth.uid,
      store,
      productId,
    });

    // TODO(M7): real validation.
    return { status: "stub", uid: request.auth.uid, productId };
  }
);

// Silence unused-locals for the stub.
void admin;
