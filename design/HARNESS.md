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
| Platform | DECIDED (iOS + Android, Unity) | — |
| Player perspective | DECIDED (god-view mission control) | — |
| Time model | DECIDED (realtime background timers, hours-based) | — |
| Fleet model | DECIDED (F2P = 1 ship; premium = fleet; Priority Mission = F2P taste) | — |
| Monetization model | DECIDED (F2P now, premium layer later) | Validate hook calibration |
| Monetization strategy | DRAFT | Priority Mission event cadence to tune |
| Mobile loop | DRAFT | Session length targets + notification copy |
| Core loop | DRAFT | Update for god-view UX |
| Event system | DRAFT | Define event queue / pending-decision card UX |
| Crew system | DRAFT | Decide O-005 (individuals vs pool) |
| Economy | DRAFT | Validate payout formula with numbers |
| Agency identity | OPEN | Decide O-011 (agency name + branding) |
| UI / UX | NOT STARTED | Mission-control room aesthetic |
| Art direction | NOT STARTED | — |

---

## How to Use This Harness

1. **Add new ideas** → append to the relevant system file, or create a new one under `systems/`
2. **Track decisions** → add rows to `DECISIONS.md` (move from OPEN → DECIDED when locked)
3. **Flag contradictions** → note them in the relevant system file with `> ⚠️ CONFLICT:` prefix
4. **Before coding** → all OPEN decisions in DECISIONS.md must be resolved first

---

## Key Open Questions (needs answers before building)

1. **O-011** — Agency/game name (fictional NASA-equivalent — appears on every screen)?

Everything else is decided. Name this and we can start building.
