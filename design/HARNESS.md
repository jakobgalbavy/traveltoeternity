# Travel to Eternity — Design Harness

This is the master index for all design documentation. Every significant decision, system, and open question is tracked here or in a linked file.

---

## Document Map

| File | Contents |
|------|----------|
| `OVERVIEW.md` | Core concept, pillars, win/lose conditions, tone |
| `DECISIONS.md` | All design decisions: open, decided, deferred |
| `systems/CORE_LOOP.md` | Macro loop (mission arc) + micro loop (day-to-day) |
| `systems/VOYAGE_EVENTS.md` | Full event taxonomy, chaining, resolution mechanics |
| `systems/CREW_AND_SETTLERS.md` | Crew roles, morale system, individual settler traits, distance/specialization curve |
| `systems/ECONOMY.md` | Payout formula, progression, upgrades, risk levers |
| `systems/MONETIZATION.md` | Revenue streams, IAP tiers, ad strategy, season pass, hooks, red lines |
| `systems/MOBILE_LOOP.md` | Session design, background timers, push notifications, voyage duration, UI principles |

---

## Status

| Area | Status | Next step |
|------|--------|-----------|
| Concept | DONE | — |
| Platform | DECIDED (iOS + Android) | Decide O-008 (Unity vs Godot) |
| Time model | DECIDED (realtime + background timers) | Decide O-010 (voyage duration calibration) |
| Monetization strategy | DRAFT | Decide O-009 (F2P model) + validate hooks |
| Mobile loop | DRAFT | Refine session length targets |
| Core loop | DRAFT | Update for mobile session structure |
| Event system | DRAFT | Define event queue / pending-decision UX |
| Crew system | DRAFT | Decide O-005 (individuals vs pool) |
| Economy | DRAFT | Validate payout formula with numbers |
| UI / UX | NOT STARTED | Decide O-001 (player perspective) first |
| Art direction | NOT STARTED | — |

---

## How to Use This Harness

1. **Add new ideas** → append to the relevant system file, or create a new one under `systems/`
2. **Track decisions** → add rows to `DECISIONS.md` (move from OPEN → DECIDED when locked)
3. **Flag contradictions** → note them in the relevant system file with `> ⚠️ CONFLICT:` prefix
4. **Before coding** → all OPEN decisions in DECISIONS.md must be resolved first

---

## Key Open Questions (needs answers before building)

1. **O-001** — Player perspective (god-view commander / first-person / hybrid)?
2. **O-008** — Engine: Unity vs. Godot? (Both export to iOS + Android)
3. **O-009** — Monetization model: pure F2P, premium + IAP, or subscription-first?
4. **O-010** — Voyage duration: hours-based or days-based for base tier?
5. **O-002** — Fleet management (multiple ships) or single-ship focus?

These shape architecture, UX, and monetization depth downstream.
