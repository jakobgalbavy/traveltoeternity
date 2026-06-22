# Voyage Events

Events are the heartbeat of the gameplay loop. They fire based on time elapsed, distance traveled, random chance, and current ship state. Events can chain into crises if ignored.

---

## Event Categories

### 1. Ship Systems
| Event | Trigger | Consequence if ignored |
|-------|---------|----------------------|
| Hull micro-leak | Random / meteorite | Pressure loss → crew injury → breach |
| Power fluctuation | Aging reactor / solar flare | System shutdowns, life support at risk |
| Coolant failure | High stress on systems | Reactor overheat → core damage |
| Engine misfire | Long burn / worn parts | Speed loss, extended voyage time |
| Comms array damage | Meteorite / debris field | No Earth contact, crew morale hit |

### 2. Life Support / Biology
| Event | Trigger | Consequence if ignored |
|-------|---------|----------------------|
| Crop blight | High humidity / infection | Food shortage |
| Grow bay water imbalance | Neglected monitoring | Crop failure |
| Oxygen scrubber fouled | Time + high CO2 | Hypoxia events, crew impaired |
| Water recycler clog | Time | Rationing, morale drop |
| Outbreak / illness | Crowding, mutation, stress | Crew death, quarantine needed |

### 3. Space Hazards
| Event | Trigger | Consequence if ignored |
|-------|---------|----------------------|
| Meteorite shower | Random / near asteroid field | Hull damage, module destruction |
| Radiation storm | Solar activity, distance | Crew health damage, equipment failure |
| Gravity well anomaly | Near dense body | Course deviation, fuel burn |
| Debris field | Old collision site | Hull scrapes, navigation required |

### 4. Human Events
| Event | Trigger | Consequence if ignored |
|-------|---------|----------------------|
| Settler conflict | Low morale, crowding, ideology | Escalates to violence, factions form |
| Insubordination | Low morale + weak leadership | Work stoppage, chain reaction |
| Theft / hoarding | Food anxiety | Resource imbalance, crew anger |
| Depression / breakdown | Isolation, loss | Work capacity drops, contagion |
| Romance complication | Long voyage, close quarters | Distraction, jealousy, drama |
| Leadership challenge | Player decisions questioned | Potential mutiny vote |
| Skill emergency | Specialist incapacitated | No one can repair X — improvise or die |

### 5. Discovery / Opportunity
| Event | Trigger | Near certain objects |
|-------|---------|---------------------|
| Derelict ship signal | Random | Salvage opportunity (risk vs. reward) |
| Uncharted body | Off-course | Survey bonus, possible resource cache |
| Distress beacon | Random | Rescue = extra settlers or crew (crowding risk) |
| Black market contact | Illegal colony ships nearby | Trade off-the-books goods |

---

## Event Resolution Mechanics

### Triage System
Every active event has:
- **Urgency** (immediate / hours / days)
- **Required resource** (crew time, spare parts, medicine, food)
- **Success chance** based on assigned crew skill
- **Escalation path** if not resolved in time

### Chaining
Events can chain:
```
Meteorite hit → Hull breach → Oxygen loss → Crew passes out → Crop bay unsupervised → Blight
```
Player must prioritize — they cannot fix everything simultaneously.

### Partial Resolution
Most events can be partially resolved (e.g., patch a leak vs. full repair). Partial resolution buys time but doesn't eliminate the root problem.
