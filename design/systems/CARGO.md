# Cargo System

Cargo is what pays. Every mission carries one or both cargo types. The player develops two parallel progression trees — Passengers and Packages — each with its own upgrades, specialist contractors, events, and failure modes.

Ships can specialize fully in one type or run mixed manifests. Mixed = more revenue potential, more complexity, more things that can go wrong.

---

## The Two Trees

### Passengers
Human colonists, workers, refugees, or VIPs traveling to distant destinations.

**Revenue model:** Per-head payout × distance × condition on arrival (alive, healthy, morale intact).

**Upgrade tree areas:**
- Habitation (berths, privacy, comfort tier)
- Life support redundancy (per-passenger O2, water, food systems)
- Medical bay (illness treatment capacity, quarantine capability)
- Entertainment / recreation (morale maintenance on long voyages)
- Cryo systems (premium: suspend passengers to reduce resource consumption and morale decay)
- Security (conflict containment, lockdown protocols)

**Specialist contractors:**
- Passenger Liaison (morale management, conflict de-escalation)
- Ship's Doctor (illness and injury)
- Counselor (long-voyage psychological support)
- Security Officer (violence, theft, mutiny prevention)
- Cryo Technician (required for cryo-equipped ships)

**Events unique to Passengers:**
- Medical emergency
- Passenger conflict / assault
- Psychological breakdown
- Organized protest / hunger strike
- Stowaway discovered
- VIP demands special treatment
- Passenger death (massive payout penalty + reputation hit)

**Failure mode:** Passengers die, riot, or arrive traumatized → reduced payout, reputation damage, possible legal/regulatory consequence on return.

---

### Packages
Physical freight: equipment, medicine, food stocks, scientific instruments, contraband (black market missions), raw materials.

**Revenue model:** Per-unit payout × distance × delivery condition (intact, undamaged, on time).

**Upgrade tree areas:**
- Cargo hold capacity (volume and weight limits)
- Environmental control (temperature, humidity, pressure — for sensitive cargo)
- Structural integrity (shock absorption, securing systems)
- Refrigeration / cryogenic freight (perishables, biologicals)
- Security vault (high-value or contraband cargo)
- Automated inventory (faster loading/unloading, loss prevention)

**Specialist contractors:**
- Cargo Master (manifest management, loss prevention)
- Refrigeration Tech (perishables and biologicals)
- Hazmat Specialist (required for chemical or radioactive freight)
- Security Specialist (vault cargo, contraband runs)

**Events unique to Packages:**
- Cargo shift / unsecured load (hull stress, potential breach)
- Spoilage (refrigeration failure → perishable loss)
- Theft / pilfering (crew stealing from cargo)
- Contraband discovered (if running black market — regulatory risk on return)
- Fragile shipment damaged (precision instruments, glassware)
- Biohazard leak (biological cargo breach → ship-wide quarantine)
- Cargo fire

**Failure mode:** Cargo arrives damaged, spoiled, or missing → partial or zero payout. Contraband discovery → legal trouble, mission flagged.

---

## Mixed Manifest

A ship can carry both passengers and packages simultaneously.

| Configuration | Revenue ceiling | Complexity | Required contractors |
|---------------|----------------|------------|---------------------|
| Passengers only | Medium | Medium | Liaison, Doctor |
| Packages only | Medium | Low–Medium | Cargo Master |
| Mixed | High | High | Both sets |
| Mixed + VIP passengers + high-value cargo | Very high | Very high | Full specialist manifest |

Mixed missions are harder to crew, harder to manage during events, and harder to balance — but they pay more. This is the endgame loop for experienced players.

**Conflict events:** Packages-only voyages are calmer (no human drama). Passenger-only voyages have no cargo damage events. Mixed creates emergent friction — a passenger who breaks into the cargo hold, a biohazard leak that threatens the colonists, etc.

---

## Progression & Monetization Hooks

Both trees are deep — years to fully upgrade a ship in both directions.

| Hook | Passenger tree | Package tree |
|------|---------------|-------------|
| Timer speed-ups | Habitation upgrade (48h) | Hold expansion (36h) |
| Instant recruit | Rare cryo tech | Rare hazmat specialist |
| Unlock | Cryo capability (premium feel) | Vault / contraband missions |
| Capacity expand | More berths = more passengers = more payout | Larger hold = bigger contracts |

A ship starts with minimal upgrades in both trees. The player chooses which to invest in first — this is a meaningful strategic choice that shapes the type of missions they can take.

---

## Ship Naming

Ships are named by the player on commissioning. The name appears in:
- Mission dashboard header ("*ISV Horizon* — En route to Kepler-452b, Day 3")
- Event reports ("Hull breach detected on *ISV Horizon*")
- Comms logs ("*ISV Horizon*, this is mission control...")
- Mission history / ledger
- Fleet view (if premium)

Players name their first ship during onboarding. Additional ships (premium) are named on purchase. Names can be changed between missions for a small hard currency cost.
