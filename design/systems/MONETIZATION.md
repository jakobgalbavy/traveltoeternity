# Monetization Design

Monetization is a first-class design concern, not an afterthought. Every core system should have a natural money hook — but the hooks must feel fair, not exploitative, to maintain long-term retention.

**Principle:** Paying speeds things up or reduces friction. It never gates progress entirely (no paywalled story, no pay-to-win PvP). Voyages succeed or fail based on decisions, not wallet size.

---

## F2P vs. Premium Tier (phased rollout)

### Phase 1 — F2P Launch
- Single ship slot
- Full core loop accessible
- Rewarded ads + IAP hard currency
- Priority Mission events: limited-time 2nd ship slot (see below)

### Phase 2 — Premium Layer (post-launch)
- Fleet Commander upgrade: permanent 2nd (and 3rd) ship slot
- Exclusive ship classes and crew specialists
- Season pass with monthly content
- Possibly: ad-free mode as standalone IAP

### Priority Mission Event (F2P Fleet Taste)
- Fires every 2–4 weeks (limited time, 48h window)
- Player is given a pre-crewed 2nd ship for a special high-value mission
- Cannot customize the 2nd ship — it's dispatched as-is (taste, not full experience)
- Completing the Priority Mission gives a premium currency bonus
- UI prominently shows "Upgrade to Fleet Commander to always have 2 ships"
- Design goal: create desire for fleet management without giving it away

---

## Revenue Streams

### 1. Hard Currency (Stars / Stardust — name TBD)
Premium currency purchased with real money. Used to:
- Speed up timers (ship repairs, grow bay cycles, training)
- Recruit rare specialist crew immediately (vs. waiting for the hire pool to refresh)
- Unlock additional mission slots (if fleet management — O-002)
- Revive a failed mission (one-time per mission; expensive)
- Cosmetic ship skins, crew portraits

**Soft entry:** Small amounts of hard currency earned through gameplay (mission milestones, daily goals) so F2P players feel the currency without relying on it.

### 2. IAP Tiers
| Pack | Price (est.) | Contents |
|------|-------------|----------|
| Starter Kit | $1.99 | Hard currency + one rare crew member |
| Mission Pack | $4.99 | Hard currency + rare parts bundle + one ship cosmetic |
| Commander's Edition | $9.99 | Large hard currency + exclusive ship skin + permanent +10% payout bonus |
| Season Pass | $4.99/month | Monthly hard currency allowance + exclusive seasonal content + cosmetics |

### 3. Rewarded Ads
- Watch ad → speed up a timer by 30 minutes (capped: 3–5 per day)
- Watch ad → get a second-chance event resolution (re-roll a bad outcome)
- Watch ad → small resource bundle (spare parts, medicine)

Ads are always optional and never forced. Respects the player; builds goodwill.

### 4. Battle Pass / Season Pass
Each "Season" = a new destination cluster with exclusive lore, events, and cosmetics. Pass includes:
- Free tier: everyone gets some rewards for playing
- Paid tier ($4.99/month): exclusive ship skin, specialist crew, hard currency, early access to new destination

Seasons run 6–8 weeks. Drive content update cadence and recurring revenue.

---

## Natural Monetization Hooks (in core systems)

| System | Hook | What player buys |
|--------|------|-----------------|
| Voyage timers | Long voyages = long waits | Speed-ups |
| Crew hiring | Rare specialists have long refresh timers | Instant recruitment |
| Mission failure | Lost mission = lost investment | Revive token |
| Fleet slots | 1 free slot, more locked | Unlock additional ships |
| Event resolution | Failed repair = cascading damage | Re-roll |
| Grow bay cycles | Crops take real hours | Instant harvest |
| Ship upgrades | Long upgrade timers | Speed-up |

### Design rule: Every timer is a potential sale
Any waiting period in the game is a monetization moment. Timer lengths should be calibrated so they are frustrating *just enough* to prompt a purchase — but not so long they cause churn.

---

## Retention Mechanics (monetization enablers)

Retention drives LTV. Systems that keep players coming back create more purchase opportunities.

| Mechanic | Purpose |
|----------|---------|
| Push notifications | "Your ship needs you — a meteorite hit while you were away" |
| Daily login rewards | Hard currency drip; builds habit |
| Daily missions | Short objectives; complete for resources or soft currency |
| Limited-time events | FOMO; drives season pass conversions |
| Crew relationships / story | Emotional investment keeps players from quitting |
| Long voyage arc | Multi-day commitment; player checks in repeatedly |
| Mission streaks | Bonus payout for consecutive successful missions |

---

## Monetization Red Lines (don'ts)

- No energy/stamina system that locks players out entirely
- No loot boxes with randomized core gameplay items (cosmetics only is acceptable)
- No pay-to-win mechanics that let rich players trivially succeed on extreme difficulty
- No advertisements that interrupt active gameplay (only opt-in rewarded ads)
- Crew members are not gacha — they can be recruited with soft currency (hard currency just speeds it up)
