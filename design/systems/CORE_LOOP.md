# Core Loop Design

## Macro Loop (Mission Arc)
```
Choose Destination → Outfit Ship → Launch → Manage Voyage → Land → Collect Reward → Repeat
```

### 1. Choose Destination
- Map of reachable destinations, each with:
  - Distance (light-years or AU) → determines voyage duration & payout multiplier
  - Hazard profile (asteroid density, radiation, known anomalies)
  - Colonization bonus (fertile world, mineral-rich, strategic relay point)
- Player must contract settlers and secure funding before departure

### 2. Outfit Ship
- Hire/assign crew (specialists: medic, engineer, botanist, commander, etc.)
- Load supplies: food stock, spare parts, seeds, medicine, fuel
- Install/upgrade ship modules: life support, hull plating, grow bays, comms
- Budget is constrained — can't take everything

### 3. Voyage (The Main Game)
See: VOYAGE_EVENTS.md

### 4. Landing
- Arrival condition check: hull %, crew alive %, cargo %
- Payout calculation (base × distance × condition modifiers)
- Unlocks: new destinations, tech, crew types

---

## Micro Loop (Day-to-Day Aboard Ship)
```
Time Advances → Resources Deplete → Events Fire → Player Responds → Consequences Resolve
```

### Resource Depletion (passive, per cycle)
- Food consumed by crew headcount
- Oxygen recycled (depends on plant health + life support status)
- Water reclaimed (depends on recycler integrity)
- Hull integrity can degrade from micro-impacts
- Morale drifts based on living conditions, events, and leadership choices

### Event Types
See: VOYAGE_EVENTS.md

### Player Actions (per event or on demand)
- Assign crew to tasks (repair, grow, tend sick, investigate)
- Make dialogue choices in crew conflict events
- Spend spare parts / medicine / food reserves
- Vent compartments, seal hatches, triage priorities
