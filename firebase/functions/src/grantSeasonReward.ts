import * as functions from "firebase-functions/v2/https";
import * as admin from "firebase-admin";

interface GrantSeasonRewardRequest {
  seasonId: string;
  tier: number;
  track: "free" | "premium";
}

/**
 * Grants a season pass tier reward into users/{uid}/ownedItems.
 *
 * M7 TODO:
 *  - Look up reward item ids from catalog/seasons/{seasonId}.tiers[tier][track]
 *  - Verify caller has reached `tier` in users/{uid}/seasonPassProgress
 *  - Verify premium track requires isPremium = true
 *  - Verify reward not already claimed (claimedFreeTiers / claimedPremiumTiers)
 *  - Run all writes in a single transaction
 */
export const grantSeasonReward = functions.onCall(
  { enforceAppCheck: true },
  async (request) => {
    if (!request.auth) {
      throw new functions.HttpsError("unauthenticated", "Sign in required.");
    }
    const { seasonId, tier, track } = request.data as GrantSeasonRewardRequest;
    if (!seasonId || tier == null || !track) {
      throw new functions.HttpsError("invalid-argument", "Missing fields.");
    }
    return { status: "stub", uid: request.auth.uid, seasonId, tier, track };
  }
);

void admin;
