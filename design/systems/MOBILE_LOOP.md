# Mobile Game Loop Design

Mobile realtime is different from desktop realtime. Players interact in short sessions (3–10 minutes) and the game runs in the background between sessions. Design must accommodate both.

---

## Session Structure

### Active Session (player has app open)
- Review ship status dashboard
- Resolve any queued events (presented as notification cards)
- Make decisions: assign crew, spend resources, set next task
- Check on crops, repairs in progress
- Explore map / plan next mission
- Average session: 5–10 minutes

### Background Phase (app closed, timers running)
- Voyage progresses (distance covered per real hour)
- Resources deplete passively
- Events queue up (do not auto-resolve — player decides)
- Crop cycles tick
- Repairs/upgrades count down

**Key design rule:** Nothing catastrophic happens while the player is away without warning. Events are queued as "pending decisions," not resolved badly in the background. This keeps absence feel safe — but creates urgency to return.

---

## Push Notification Strategy

Notifications are the re-engagement hook. They must be compelling, not spammy.

| Trigger | Message example | Priority |
|---------|----------------|----------|
| Critical event queued | "Hull breach detected on the Meridian — crew at risk" | HIGH |
| Crop ready to harvest | "Grow Bay B is ready — harvest before spoilage" | MEDIUM |
| Timer complete | "Engine repair finished. Ready for next burn." | MEDIUM |
| Daily mission available | "New mission available: 48h window" | LOW |
| Voyage milestone | "50% of the journey to Kepler-452b complete" | LOW |
| Long absence (12h+) | "Your crew is waiting, Commander." | LOW |

Player controls notification levels in settings. Don't push LOW priority by default — respect notification fatigue.

---

## Voyage Duration Calibration

Voyage length in real time determines the game's tempo (see O-010):

### Option A: Hours-based (recommended for early game)
- Short-range missions: 4–12 hours real time
- Mid-range: 1–3 days
- Long-range: 5–10 days
- Extreme: 2–4 weeks

### Option B: Day-based (endgame / prestige)
- Reserved for legendary destinations
- Weeks-long voyages become a prestige status symbol
- Massive payout and reputation reward

**Recommendation:** Start with hours-based. Long voyages in real time feel endless unless the player is deeply invested. Gate day-scale voyages behind high reputation.

---

## Session Engagement Tactics

| Tactic | Implementation |
|--------|---------------|
| Cliffhanger exit | When player closes app during an active event, show "Your crew needs a decision before you go" |
| Return hook | First thing on open: compelling event card, not a menu |
| Idle gains | Small resources trickle in while away (feel rewarded for returning) |
| Streak bonus | Log in 7 days in a row → bonus payout on next mission |
| "While you were away" summary | Recap of voyage progress since last session |

---

## UI Implications (mobile-first)

- **One-thumb reachable primary actions** — critical decisions must not require precise taps
- **Dashboard-first layout** — ship status glanceable in 3 seconds
- **Notification cards** — events presented as swipeable cards (approve / investigate / defer)
- **Portrait and landscape** — portrait for dashboard/events; landscape for map/navigation
- **No tiny text** — accessibility: readable at arm's length
