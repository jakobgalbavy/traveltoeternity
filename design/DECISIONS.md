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

---

## OPEN

| # | Decision | Options | Notes |
|---|----------|---------|-------|
| O-001 | Player perspective | (a) God-view mission planner, (b) First-person onboard character, (c) Hybrid: base-management + onboard events | Affects entire UX |
| O-002 | Simultaneous missions | One ship at a time vs. fleet management | Fleet adds complexity; single ship keeps focus |
| O-003 | Realtime vs. turn-based | Realtime with pause (like FTL) vs. turn/day cycle vs. pure turn-based | Affects event pacing |
| O-004 | Ship customization | Pre-built ship classes vs. modular build-your-own | Build-your-own adds depth, class-based is faster to balance |
| O-005 | Crew as individuals | Named settlers with stats/needs vs. abstract crew pool | Individual = more drama, pool = more scalable |
| O-006 | Destination variety | Random procedural planets vs. fixed solar system map | Procedural = replayability, fixed = lore/progression |
| O-007 | Platform / tech stack | Browser/HTML5 vs. native (Electron, Unity, Godot) vs. Python/pygame | To be decided before coding starts |

---

## DEFERRED

| # | Decision | Why deferred |
|---|----------|--------------|
| DEF-001 | Multiplayer co-op | Out of scope for v1 |
| DEF-002 | Earth base management (pre-launch) | Possible expansion layer |
| DEF-003 | Alien life encounters | Adds too many unknowns early on |
