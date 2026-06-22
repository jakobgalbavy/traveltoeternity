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
| `systems/CREW_AND_SETTLERS.md` | Crew roles, morale system, individual settler traits |
| `systems/ECONOMY.md` | Payout formula, progression, upgrades, risk levers |

---

## Status

| Area | Status | Next step |
|------|--------|-----------|
| Concept | DONE | — |
| Core loop | DRAFT | Decide O-003 (realtime vs turn-based) |
| Event system | DRAFT | Write event resolution prototypes |
| Crew system | DRAFT | Decide O-005 (individuals vs pool) |
| Economy | DRAFT | Validate payout formula with numbers |
| Platform / tech | NOT STARTED | Decide O-007 before any coding |
| UI / UX | NOT STARTED | Depends on O-001 (player perspective) |
| Art direction | NOT STARTED | — |

---

## How to Use This Harness

1. **Add new ideas** → append to the relevant system file, or create a new one under `systems/`
2. **Track decisions** → add rows to `DECISIONS.md` (move from OPEN → DECIDED when locked)
3. **Flag contradictions** → note them in the relevant system file with `> ⚠️ CONFLICT:` prefix
4. **Before coding** → all OPEN decisions in DECISIONS.md must be resolved first

---

## Key Open Questions (needs answers before building)

1. **O-001** — Player perspective (god-view / first-person / hybrid)?
2. **O-003** — Realtime with pause, day-cycle, or turn-based?
3. **O-007** — What engine/platform are we building in?

These three shape everything downstream.
