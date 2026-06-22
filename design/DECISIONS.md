# Travel to Eternity — Open Decisions

This file tracks design decisions: **open** (needs a choice), **decided** (locked in), and **deferred** (acknowledged but not now).

---

## DECIDED

| # | Decision | Choice | Rationale |
|---|----------|--------|-----------|
| D-001 | Game name | "Travel to Eternity" | Established by user |
| D-002 | Core loop | Mission management during voyage | Ships leave Earth, player manages voyage to destination |
| D-003 | Earning mechanic | Distance = payout multiplier | Further destinations = more reward |
| D-004 | Crew cost scaling | Longer distance → more specialized → more expensive crew required | Counterbalances the distance multiplier; specialists are rare and costly |
| D-005 | Platform | iOS + Android (mobile-first) | User confirmed |
| D-006 | Time model | Realtime with background timers | Voyages run in real time; events fire while app is closed; push notifications call player back |
| D-007 | Monetization priority | High — designed in from the start, not bolted on | Core loops must create natural monetization hooks |
| D-008 | Player perspective | God-view command center — no player character aboard | Player is mission control, not a crew member. Think NASA flight director, not astronaut. |
| D-009 | Engine | Unity | User has Unity installed |
| D-010 | Monetization model | F2P base; premium features unlocked via IAP/subscription later | Start lean; premium layer added as game grows |
| D-011 | Voyage timer base | Short-range = hours; mid = ~1 day; long = days; extreme = weeks | See MOBILE_LOOP.md for calibration |
| D-012 | Fleet management | Single ship for F2P; multiple ships = premium | F2P gets a 2nd ship slot during special "Priority Mission" events only — a taste of premium |
| D-013 | Ship customization | Modular with a deep upgrade tree | Years to fully upgrade; starts small. Every module is an upgrade timer = monetization hook. |
| D-014 | Crew model | Individual contractors | Hired/fired freely. Price scales with specialization and experience. May have demands. Long-haul missions need expensive specialists. |
| D-015 | Destination map | Fixed star map, unlocked progressively | New destination clusters added via content updates. Seasonal events tied to specific locations. |

---

## OPEN

| # | Decision | Options | Notes |
|---|----------|---------|-------|
| O-001 | Player perspective | (a) God-view mission planner, (b) First-person onboard character, (c) Hybrid: base-management + onboard events | Affects entire UX |
| O-011 | Organization name | Needs a fictional NASA-equivalent name (can't use NASA) | Shapes UI branding, lore, and player identity |

---

## DEFERRED

| # | Decision | Why deferred |
|---|----------|--------------|
| DEF-001 | Multiplayer co-op | Out of scope for v1 |
| DEF-002 | Earth base management (pre-launch) | Possible expansion layer |
| DEF-003 | Alien life encounters | Adds too many unknowns early on |
