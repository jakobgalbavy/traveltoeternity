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

---

## OPEN

| # | Decision | Options | Notes |
|---|----------|---------|-------|
| O-001 | Player perspective | (a) God-view mission planner, (b) First-person onboard character, (c) Hybrid: base-management + onboard events | Affects entire UX |
| O-002 | Simultaneous missions | One ship at a time vs. fleet management | Fleet = more monetization surface (more timers, more slots to unlock); single ship = cleaner UX |
| O-004 | Ship customization | Pre-built ship classes vs. modular build-your-own | Classes = easier to balance monetization; modular = more depth and upsell surface |
| O-005 | Crew as individuals | Named settlers with stats/needs vs. abstract crew pool | Individual = more attachment, gacha/collection potential; pool = more scalable |
| O-006 | Destination variety | Random procedural planets vs. fixed solar system map | Procedural = replayability; fixed = content update cadence and seasonal events |
| O-008 | Engine | Unity vs. Godot | Both export iOS + Android. Unity has better mobile tooling and ad SDK ecosystem. Godot is free/open. |
| O-009 | Monetization model | F2P (IAP + ads) vs. premium + IAP vs. subscription | See MONETIZATION.md |
| O-010 | Real-time voyage duration | Hours-per-hop (short sessions) vs. days-per-voyage (long burn) | Longer = more urgency for speed-ups; shorter = higher daily engagement |

---

## DEFERRED

| # | Decision | Why deferred |
|---|----------|--------------|
| DEF-001 | Multiplayer co-op | Out of scope for v1 |
| DEF-002 | Earth base management (pre-launch) | Possible expansion layer |
| DEF-003 | Alien life encounters | Adds too many unknowns early on |
