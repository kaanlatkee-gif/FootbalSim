# FootbalSim — Game Design Document

**Working title:** FootbalSim *(repository name; the shipping title is an open decision — see `D9`)*
**Version:** 0.1 — translated, restructured and expanded from the original Turkish design note `Fotbalcareer.txt`
**Date:** 2026-10-22
**Status:** Pre-production / concept locked, numbers unverified

---

## How to read this document

The original note was a stream-of-consciousness feature list written in Turkish. This document does three things:

1. **Translates** every idea faithfully — nothing in the source is dropped.
2. **Polishes** it into a coherent design: the loose ideas are grouped into systems, the missing connective tissue (time, economy, progression, failure states) is filled in, and contradictions are resolved.
3. **Flags** every place where the original was ambiguous, plus every number that is a placeholder.

Conventions used below:

| Marker | Meaning |
|---|---|
| **Source** | Present in the original note, translated as-is |
| **Proposal** | Added by this document to make the idea playable; must be approved or rejected |
| **`D#`** | An open decision listed in §14 |
| **`[TBD]`** | Number to be tuned in playtest, not a design commitment |

Every number in this document is a **first pass**. Balance lives in data files (see `ARCHITECTURE.md` §9), never in code, so tuning does not require a rebuild.

---

## 1. High concept

### 1.1 One-paragraph pitch

**FootbalSim** is a first-person life-and-career simulation. You are a young football player at the start of your professional career: you sleep in a rented flat, you shop and cook, you take the bus or drive to the training facility, you grind drills in first person, you drink too much at the bar on a Saturday, and on Sunday a television in your own living room tells the city what it thinks of you. Everything you do between matches — how you sleep, what you eat, whether you shower, which drills you repeat — feeds a 13-attribute player model that decides how good you actually are when the whistle blows.

### 1.2 Elevator pitch

> *"Football Manager meets The Sims, in first person, with a controller in your hand instead of a spreadsheet."*

### 1.3 Facts

| Field | Value |
|---|---|
| Genre | Life sim / sports career RPG |
| Perspective | First person, mouse + keyboard, gamepad supported |
| Platform | PC (Windows first), Steam; console later `D8` |
| Engine | Unity 6 LTS + C# (see `ROADMAP.md` §1) |
| Session length | 20–40 min (≈ 1–2 in-game days) |
| Target audience | 16–35, football fans who play career modes and life sims |
| References | *Football Manager* (career depth), *The Sims* (needs & objects), *Yakuza* (dense small city + minigames), *Minecraft* villager trading (shop UI), *Persona* (calendar + daily routine) |
| Content scale (v1.0) | 1 city district, 6 building types, 8 interiors, 13 drills, ~40 purchasable items, 1 league season |

### 1.4 What makes it different

Career modes are menus. Life sims have no sport. **FootbalSim makes the boring parts of a footballer's life the gameplay**: the sleep you skipped, the kebab you cooked, the shower you didn't take, the ten drinks you shouldn't have had — all of it is legible on the pitch and in the press.

---

## 2. Design pillars

| # | Pillar | What it means in practice | Test question |
|---|---|---|---|
| P1 | **The body is the build** | No skill trees, no perks. Attributes rise only through physical, in-world action. | *Can the player raise a stat without physically doing the thing?* If yes, cut it. |
| P2 | **Routine is the loop** | A day is a budget of hours. Every activity spends time, energy or money. | *Does this feature create a trade-off between two good options?* |
| P3 | **The world talks back** | The TV, the bartender, the coach and the tabloids reflect what you actually did. | *Does the world acknowledge the player's choices within one in-game day?* |
| P4 | **Small but solid** | One city block done properly beats an open world done badly. | *Can we finish this to shipping quality in the milestone?* |

---

## 3. Core loop

### 3.1 Moment-to-moment (seconds)

Walk → look at an interactable → prompt appears (`E`) → use it → world state changes (energy, money, attribute, time).

### 3.2 Day loop (20–30 min real time)

```
   wake up ──► needs check ──► plan the day ──► travel ──► activity ──► travel ──► …
      ▲                            │                        │
      │                            │  training            │  match day
      │                            ▼                        ▼
      └──────── sleep ◄──── return home ◄──── evening (bar / TV / rest)
```

The player answers three questions every day:
1. **What do I train?** (which attribute, which drill, and how much energy can I afford to spend)
2. **What do I spend?** (money and hours are both scarce)
3. **How do I recover?** (sleep quality, food, shower, rest — all feed tomorrow's training efficiency)

### 3.3 Week loop

| Day | Content |
|---|---|
| Mon–Fri | 2 training slots/day (morning, afternoon), optional gym slot, club-mandated session on Wed |
| Sat | Match day `D1` |
| Sun | Rest day — coach unavailable, bar busy, media digest |

### 3.4 Season loop (v1.0 = one season, ~30 matches)

Match performance → rating → reputation, wage, and selection → higher-level drills and clubs unlocked → end-of-season offer screen → **either** roll into season 2 with carried attributes **or** end the career with a summary. `D6` decides whether v1.0 ships with more than one season.

---

## 4. Player progression model

### 4.1 Attributes — 13 total, 1–99 scale

**Source:** the original lists seven technical and six physical attributes, each tied to a training activity.

| Group | Attribute | ID | Trained by | Source label |
|---|---|---|---|---|
| Technical | Finishing | `FIN` | One-on-one shooting drill | BİTİRİCİLİK |
| Technical | Dribbling | `DRI` | Cone slalom | DRİBLİNG |
| Technical | Heading | `HEA` | Cross-and-head | KAFA VURUŞU |
| Technical | Crossing | `CRO` | Wide-area delivery | ORTA YAPMA |
| Technical | Passing | `PAS` | Rondo / wall passes | PAS |
| Technical | Tackling | `TAC` | Shield-and-steal | TOP KAPMA |
| Technical | Long shots | `LON` | Long-range shooting | UZAKTAN ŞUT |
| Physical | Sprint speed | `SPD` | Treadmill intervals | HIZ |
| Physical | Stamina | `STA` | Treadmill endurance | DAYANIKLILIK |
| Physical | Balance | `BAL` | Pull-up bar holds | DENGE |
| Physical | Strength | `STR` | Dumbbell circuit | GÜÇ |
| Physical | Jumping | `JMP` | Jump Squad | ZIPLAMA |
| Physical | Acceleration | `ACC` | Weighted sled push | HIZLANMA |

**Proposal:** a 14th conceptual block, **Goalkeeping** (`GK`), is intentionally out of scope for v1.0. The player character is always an outfield player. `D5` covers whether GK reflexes are ever added.

**Proposal:** attributes are the *only* progression currency. There is no XP bar, no level, no perk tree. Training awards attribute "training points" directly (§4.4).

### 4.2 Overall rating

**Proposal** (the original asks for an "overall system" but does not define it). Overall is a **position-weighted average** of the 13 attributes, displayed 1–99.

```
Overall = round( Σ(attribute_i × weight_i) / Σ(weight_i) )
```

Weights are authored per position profile as data, not code. Starting profiles:

| Attribute | ST | W (wing) | CM | CB |
|---|---:|---:|---:|---:|
| Finishing | 0.22 | 0.10 | 0.05 | 0.02 |
| Dribbling | 0.12 | 0.20 | 0.08 | 0.03 |
| Heading | 0.12 | 0.03 | 0.05 | 0.14 |
| Crossing | 0.05 | 0.18 | 0.08 | 0.03 |
| Passing | 0.08 | 0.10 | 0.20 | 0.10 |
| Tackling | 0.03 | 0.03 | 0.10 | 0.22 |
| Long shots | 0.10 | 0.08 | 0.08 | 0.02 |
| Sprint speed | 0.10 | 0.15 | 0.08 | 0.12 |
| Stamina | 0.05 | 0.05 | 0.12 | 0.10 |
| Balance | 0.05 | 0.05 | 0.06 | 0.06 |
| Strength | 0.05 | 0.02 | 0.04 | 0.12 |
| Jumping | 0.08 | 0.01 | 0.03 | 0.09 |
| Acceleration | 0.05 | 0.10 | 0.08 | 0.09 |

Rules:
- Weights **need not sum to 1.0**; they are normalised on load. This lets designers edit one number without rebalancing the row.
- The player picks a position at career creation. The profile is **fixed** in v1.0 (no re-training) — `D3`.
- Overall is a *derived* value. Never saved; always recomputed. This prevents save/version drift.

### 4.3 Display

The pause menu shows overall, both group averages, and the full 13-bar attribute sheet. The training facility shows the same sheet so the player can plan before walking to a drill.

### 4.4 Training economy

**Proposal.** Every drill returns a **performance score** (0.00–1.00) from the minigame. That score converts to attribute progress:

```
gain = (baseGain + scoreBonus × score)     // raw progress points
     × headroom(attribute)                   // diminishing returns near 99
     × fatigueMultiplier                     // 1.00 rested → 0.35 exhausted
     × coachBonus                            // 1.00 solo, 1.15 with coach session
     × weeklyMultiplier(drill)               // 1.00, 0.70, 0.40, 0.20 for 1st–4th use of that drill this week
```

| Term | Formula | Notes |
|---|---|---|
| `baseGain` | 8 | `[TBD]` |
| `scoreBonus` | 32 | A perfect drill is 5× a lazy one |
| `headroom(a)` | `(1 − a/99)^1.5` | A 20-rated player climbs fast; a 90-rated player barely moves |
| `fatigueMultiplier` | `lerp(1.0, 0.35, fatigue01)` | `fatigue01` = 1 − energy/100 after the drill |
| `weeklyMultiplier` | authored per drill | Anti-grind: repeating one drill is punished |

An attribute point costs **100 progress points** at all levels; the headroom term already handles the difficulty curve. A well-rested 1.0-score session is worth ≈ 1.4 attribute points at 40 overall, ≈ 0.5 at 75.

**Failure states:** a score below 0.25 awards **no** progress and costs double energy. This is what makes sleeping and eating matter.

---

## 5. Simulation systems

### 5.1 Time & calendar

**Proposal** (the original implies a clock via "sleep to skip the day" and "bladder appears at a certain hour" but never defines one).

- One in-game day = 24 in-game hours.
- Default clock speed: **1 real second = 2 in-game minutes** (a day ≈ 12 real minutes). Configurable 1×/2×/4× in Settings.
- Time **pauses** during: menus, dialogues, cutscenes, minigames, interior transitions.
- Time **does not pause** while the player stands still (prevents idling to farm needs).
- Calendar: 7-day week, 30-match season, seasons are `[TBD]` in length.
- Sleeping fast-forwards the clock — the only sanctioned time skip.

### 5.2 Needs

**Source:** the note asks for an energy bar, a dirtiness bar, a toilet bar, and drunk state. **Proposal:** these are unified as four needs on a 0–100 scale.

| Need | Range | Rises / falls | Effects at the bad end |
|---|---|---|---|
| **Energy** | 0 = collapse, 100 = fresh | −1.5/h awake, −18 per training drill, −25 per match | < 25: drills capped at 0.6 score. 0: forced sleep, lose the rest of the day. |
| **Hygiene** | 0 = filthy, 100 = clean | −2/h, −12 per training, −20 per match | < 30: NPC dialogue changes, coach bonus lost, sleep restores 20% less. |
| **Bladder** | 0 = empty, 100 = urgent | +6/h, +12 per drink, +20 per energy drink | At 100: forced 90-second "find a toilet" event. In public → reputation hit `[TBD]`. |
| **Intoxication** | 0 = sober, 100 = wasted | +12 per drink, −8/h | > 60: screen sway, movement penalty, all scores ×0.6. 10 drinks in one session → news event (§5.7). |

**Deliberate scope choice:** there is **no hunger bar**. Food exists to restore **Energy** (the original describes food as "extra energy"), which keeps the HUD to four bars and avoids a fifth system to balance. `D4` records the alternative.

HUD: four thin bars in the lower-left, hidden when full and untouched for a while.

### 5.3 Sleep and rest

**Source:** *"Bed → skips the day and refills energy, or the sleep duration can be chosen and energy depends on the hour you slept. `E — SLEEP` / `G — REST`."*

At the bed the prompt shows both verbs:

| Input | Action | Behaviour |
|---|---|---|
| `E` | **Sleep** | Duration picker: 4 / 6 / 8 / 10 h. Fast-forwards the clock. Restores energy at 12.5/h × quality. |
| `G` | **Rest** | 30 / 60 / 120 min nap. Restores 40% of the sleep rate, costs no "next day" state. |

**Sleep quality** (0.5–1.2 multiplier) = f(hygiene, intoxication, whether the player slept within ±1 h of the same time as yesterday). Sleeping at 04:00 before a 09:00 session is a real penalty — this is the system that teaches routine.

**Proposal:** sleeping past 10:00 on a training day triggers a missed-session notice from the coach and a small reputation hit.

### 5.4 Food, cooking and spoilage

**Source:** three separate ideas — a **wardrobe/dolap** that stores energy drinks, food and ingredients and "prevents spoilage like a chest"; a **stove** that cooks ingredients into higher-energy meals (*"Kelle Paça: 130% energy"*); and a market to buy the ingredients.

**Proposal — the item pipeline:**

```
market purchase ──► inventory (decays, spoils) ──► storage (dolap/fridge, frozen decay)
                                                         │
                                                    stove ──► cooked meal ──► energy + buff
```

| Item class | Spoils? | Decay | Notes |
|---|---|---|---|
| Ingredients (raw) | Yes | 100 → 0 over 3 in-game days | Bread, meat, vegetables, offal |
| Cooked meals | Yes, faster | 100 → 0 over 1 day | Cannot be eaten below 20 freshness |
| Energy drinks / canned | No | — | Cheap, instant, small energy, adds bladder |
| Alcohol | No | — | See bar (§6.2) |

Storage in the flat **halts decay** while stored (the "chest" behaviour from the source). Cooking consumes 1–3 ingredients and 30 in-game minutes.

**Meal examples** (`[TBD]` values):

| Meal | Ingredients | Energy | Buff | Cost |
|---|---|---:|---|---:|
| Toast | Bread ×1 | +12 | — | 15 |
| Pasta | Pasta ×1, Sauce ×1 | +28 | — | 40 |
| **Kelle Paça** (source example) | Offal ×1, Broth ×1, Bread ×1 | **+45 (≈130% of a standard meal)** | +10% strength gain for 2 h | 120 |
| Recovery shake | Powder ×1, Milk ×1 | +18 | −40 fatigue for next drill | 70 |

> Source note: the original wrote "Kelle Paça: 130% energy". Read literally as a fraction of a full bar that would be broken (100 is the cap), so it is interpreted here as **130% of a standard meal's energy value**, plus a buff. Confirm in `D2`.

### 5.5 Economy

**Proposal** — the source has prices, wages and purchases but no ledger. Currency symbol is display-only (€ by default, localisable).

**Income**

| Source | Amount (per week) | Condition |
|---|---:|---|
| Base wage | 2 500 | Set by club tier; rises with reputation |
| Appearance fee | 1 500 / match | Played ≥ 45 min |
| Win bonus | 2 000 / match | Team won |
| Goal bonus | 500 / goal | — |
| Man of the Match | 1 000 | Rating ≥ 8.5 |

**Outgoings**

| Source | Amount |
|---|---:|
| Rent — cheap / mid / expensive flat | 400 / 900 / 2 000 per week |
| Groceries | 30–120 per trip |
| Bar | 60 per drink, 200 per round |
| Gym membership | 150 / week (facility drills are free) |
| Car upkeep | 80 / week + fuel |
| Club fines | 500 (late), 2 000 (missed session) |

**Proposal:** the player starts with **3 000** and one week of rent prepaid. Going negative triggers a bank notice and, at −5 000, eviction to the cheap flat. This gives money a failure state without a game-over screen.

### 5.6 Matches and career

**Source:** matches exist only as a source of training/performance news (*"outstanding performance in today's match"*), plus a hint that matches raise hygiene and cost energy.

**This is the single largest scope risk in the project** (`D1`). Three options:

| Option | Description | Cost | Risk |
|---|---|---|---|
| **A. Full match sim** | 11v11 real-time football with player control | 6+ months alone | Kills the project |
| **B. Moment-based hybrid** (recommended) | Match is simulated as a timeline of ~8–12 **key moments**. The camera cuts to the player; you get one action per moment (finish, pass, tackle, header) using the drill mechanics you already learned. Everything else is resolved by the sim. | 4–6 weeks | Medium |
| **C. Text sim only** | Result and rating computed from attributes + form; no match gameplay | 1 week | Low, but the game has no "football" in it |

**Recommendation: B.** It reuses the 13 drill mechanics (a one-on-one is literally the Finishing drill), it keeps the game about the body you built, and it fits a 20-minute session. The roadmap is built on B.

Match rating (1.0–10.0) is derived from moment outcomes weighted by position, and feeds reputation, news and selection.

### 5.7 Media, news and reputation

**Source:** *"Couch and TV → you can sit. The TV can run news about what you did that day, e.g. 'great performance in the match: (Player name) put in an outstanding performance in today's match'."* Plus the bar rule: **10 drinks → appears in the TV news.**

**Proposal — a templated news generator.** Any notable event writes a `NewsEvent` (tag, severity, subject, optional number). At 21:00 the TV composes a bulletin from the day's events, highest severity first:

| Tag | Trigger | Headline example |
|---|---|---|
| `match.rating.high` | Rating ≥ 8.0 | *"<Name> put in an outstanding performance in today's match."* |
| `match.rating.low` | Rating ≤ 4.0 | *"<Name> struggled as <Club> fell short."* |
| `bar.binge` | 10 drinks in one session | *"<Name> spotted leaving <Bar> at 3 a.m."* |
| `club.missed` | Missed a mandatory session | *"Discipline questions at <Club> after <Name> no-show."* |
| `purchase.car` | Bought a car | *"<Name> arrives in style — a new <Car>."* |
| `training.milestone` | Attribute crosses 80 | *"<Name>'s <attribute> is drawing attention."* |

The same bulletin appears as a **newspaper** on the coffee table and as a short **phone feed**, so the player cannot miss it. Reputation (0–100) shifts by severity; reputation gates club offers, car financing and bar treatment.

**Why this matters technically:** it is the cheapest system with the highest "the world noticed me" payoff, and it is fully data-driven — see `ARCHITECTURE.md` §6.7.

### 5.8 Reputation (proposal, minimal)

One number, 0–100, starting at 20. Rises with ratings and milestones, falls with scandals. Effects: wage negotiation, news tone, NPC greeting lines, and (v1.1) sponsorship offers. No separate fame/fan/agent simulation in v1.0 — `D7`.

---

## 6. World and locations

### 6.1 City

**Proposal:** one compact district (≈ 200 × 200 m) with six destinations inside a 2-minute walk of the flat, plus a bus stop that acts as a fast-travel node. Scale is deliberately small: it keeps the art budget sane and makes the "commute" a real but non-punishing cost.

```
        ┌──────────────────────────────┐
        │            STADIUM           │  ← match days only (v1.0 exterior + tunnel)
        └──────────────┬───────────────┘
                       │
  ┌────────┐    ┌──────┴──────┐    ┌────────┐
  │  BAR   │────│   PLAZA     │────│ MARKET │
  └────────┘    │  + BUS STOP │    └────────┘
                └──────┬──────┘
        ┌────────┐     │      ┌────────┐
        │  GYM   │─────┼──────│GALLERY │
        └────────┘     │      └────────┘
                ┌──────┴──────┐
                │  FACILITY   │
                └──────┬──────┘
                  ┌────┴─────┐
                  │  FLATS   │  ← cheap / mid / expensive, one entrance each
                  └──────────┘
```

Loading: each interior is its own scene, loaded additively behind a fade through a door trigger. No open-world streaming in v1.0 (`ARCHITECTURE.md` §7.2).

### 6.2 Building and interaction inventory

#### Flat (three tiers)

**Source:** *"three houses — cheap / mid / expensive — differing only in design, shape and size."* Confirmed: **no gameplay difference in v1.0**; the expensive flat is a status purchase and news trigger. `D10` records the option to add a home gym or balcony later.

| Object | Interaction | Source |
|---|---|---|
| **Bed** | `E` Sleep (duration picker) / `G` Rest | ✔ |
| **Storage (dolap)** | Open inventory; food stored here does not spoil | ✔ |
| **Stove** | Cook from ingredients; 30 min | ✔ |
| **Shower** | Cleans hygiene; 15 min; blocked while the bladder need is 100 | ✔ |
| **Toilet** | Empties bladder; 5 min | ✔ |
| **Couch + TV** | Sit; 21:00 bulletin, or a channel menu (news / highlights / off) | ✔ |
| Mirror | Change appearance (cosmetic, v1.1) | Proposal |
| Wardrobe | Change kit / outfit (cosmetic, v1.1) | Proposal |

#### Market

| Object | Interaction | Source |
|---|---|---|
| **Shelves** | **Non-interactive but visibly stocked** — pure set dressing | ✔ |
| **Cashier (NPC)** | Trade panel: buy/sell, item grid, prices, quantity — explicitly modelled on Minecraft villager trading | ✔ |
| Fridge section | Rotating stock, seasonal items | Proposal |

#### Bar

| Object | Interaction | Source |
|---|---|---|
| **Bar stools** | Sittable | ✔ |
| **Bartender (NPC)** | Buy drinks; dialogue reacts to intoxication and reputation | ✔ |
| **Drinks** | Raise intoxication, lower energy; 10 in one session → news event | ✔ |
| Other patrons (NPCs) | Ambient chatter, occasional rumour that reveals a training tip | Proposal |
| Dartboard / pool table | Time-waster that raises a small social stat `D7` | Proposal, cuttable |

#### Gallery (car dealership)

| Object | Interaction | Source |
|---|---|---|
| **Cars** | Inspect → purchase → **handshake cutscene** | ✔ |
| **Salesman (NPC)** | Buy directly from the NPC instead of the car → same cutscene | ✔ |
| Test drive | Short scripted drive, raises the shop's price tolerance | Proposal, cuttable |

#### Training facility

| Object | Interaction | Source |
|---|---|---|
| **Head coach (NPC)** | Dialogue → choose today's training programme → opens the drill selector | ✔ |
| **Teammates (NPCs)** | Idle chatter, rivalry lines, hints about the next opponent | ✔ (listed, undefined) |
| 7 drill stations | Launch the technical drills (§9) | ✔ |
| Physio room | Heal injury / clear fatigue (v1.1) | Proposal |

#### Gym

**Source:** *"Interior design [TBD] — let's save this for later; first let's add the player overall system."*

**Honoured literally.** The gym exists in the city from M3 but its interior is a **grey-box shell** until M4 is complete. The six physical drills are trainable at the facility's gym corner first; the full gym interior, machines and ambient NPCs are a **v1.1** content drop. Movement is never blocked by this — the drills are playable from day one.

#### Stadium

Exterior and tunnel exist in M6 so match days have a destination. No crowd simulation in v1.0.

---

## 7. Controls and interaction

### 7.1 Input map (default, fully rebindable)

| Action | Keyboard / Mouse | Gamepad |
|---|---|---|
| Move | `W A S D` | Left stick |
| Look | Mouse | Right stick |
| Sprint | `Shift` (hold) | Left stick click |
| Interact / Confirm | `E` | `A` / Cross |
| Secondary interact | `G` | `X` / Square |
| Cancel / Back | `Esc` | `B` / Circle |
| Pause | `Esc` | `Start` |
| Journal / Phone | `Tab` | `D-pad up` |
| Map | `M` | `D-pad down` |
| Quick save / load (dev builds only) | `F5` / `F9` | — |

**Source:** the original specifies `E — SLEEP` and `G — REST` on the bed. That maps cleanly onto the global "primary / secondary interact" pair above, so the bed is not a special case.

### 7.2 Interaction UX

- Crosshair dot; when an interactable is within 2.5 m and in a 45° cone it expands and a prompt card fades in: `[E] Sleep  [G] Rest` under the object name.
- Objects with unmet requirements show the requirement instead of the verb (`[E] Locked — needs 3 000`).
- One interactable wins per frame, chosen by dot-product × inverse distance. No precision aiming required.
- All prompts are localised strings from a table, never hard-coded (`GDD` §12).

### 7.3 Player control states

`FreeRoam → Interacting → Dialogue → Minigame → Cutscene`, driven by an explicit state machine. Only `FreeRoam` consumes movement input and advances the clock; every other state pauses time. See `ARCHITECTURE.md` §7.1.

---

## 8. UI / UX

### 8.1 Flow

```
BOOT
 └─► Main Menu ──────────────────────────────────────────────┐
      ├─ New Career ─► modal (career name, position, club)     │
      │                 └─ Confirm ─► intro ─► flat (spawn)    │
      ├─ Continue ─► load most recent save ────────────────────┤
      ├─ Load Game ─► slot list (3 slots + autosave)            │
      ├─ Settings ─► Gameplay / Audio / Video / Controls        │
      └─ Quit ─► confirm dialog                                │
```

### 8.2 Main menu (Stage 1)

**Source:** *"a nice intro screen; then key settings; an Exit tab; a New Game tab → pressing it opens a box where you name your career and confirm; after confirming, you spawn into the game."*

Polished interpretation:

- Cinematic background (slow camera over the stadium at dusk) with a vertical menu: **New Career / Continue / Load / Settings / Quit**.
- **New Career modal**: centred card, title "Name your career", a text field (3–24 chars, validated), a position picker, a club picker (3 starter clubs), and **Confirm / Cancel** buttons. Confirm is disabled until the name is valid. This is the *only* modal in the main menu — everything else is a full screen.
- **Settings → Controls** is the "key settings tab": a rebinding table with conflict detection, reset-to-default, and a gamepad/keyboard toggle. Rebinds persist to disk independently of saves.
- **Quit** asks for confirmation only if a career is loaded.

### 8.3 HUD

| Element | Placement | Visibility |
|---|---|---|
| Needs bars (4) | Lower left | Fade out when full and idle 5 s |
| Clock + day | Upper right | Always |
| Money | Upper right, under clock | On change, then fades |
| Prompt card | Centre, below crosshair | On focus |
| Objective ("Be at the facility by 09:00") | Upper left | Until complete |
| Attribute toast ("Finishing +1") | Right, stacking | 3 s |

No minimap on the HUD in v1.0 — the map is a `M` overlay. Keeps the screen clean and the city learnable.

### 8.4 Settings

Gameplay (clock speed, difficulty, subtitles), Audio (master/music/SFX/voice), Video (resolution, quality preset, FOV, motion blur), Controls (rebind, sensitivity, invert Y). Accessibility minimum for v1.0: subtitles, FOV, sensitivity, colour-blind-safe need colours, unskippable-cutscene toggle.

---

## 9. Training minigames (Stage 5)

**Source:** Stage 5 is titled "MINI GAME" and left **empty**. Stage 4 names 13 drills but specifies none of them. This section defines the framework and all 13.

### 9.1 Shared framework

Every drill is a self-contained scene implementing one contract and returning one result:

```
DrillContext  (difficulty, player attributes relevant to the drill, energy, fatigue, coach present, seed)
DrillResult   (score 0..1, reps, durationSeconds, failureReason?)
```

Rules that apply to all 13:

- **Duration:** 45–90 s of play, plus a 3-2-1 countdown.
- **Input:** must work on both keyboard/mouse and gamepad.
- **Readable feedback:** a live score meter, a visible "perfect" band, and an end card showing the converted attribute gain.
- **Determinism:** seeded RNG, so the same seed and inputs reproduce the same result (needed for tests and replays).
- **Attribute influence:** the player's current attribute sets the difficulty floor — a high Finishing attribute widens the target window, it does not auto-win the drill.
- **Skippable:** a "skip drill" option exists for accessibility and awards a floor score of 0.2.
- **No fail-out:** a bad drill ends normally with a low score; it never ejects the player.

### 9.2 Technical drills (facility)

| # | Drill | Attribute | Core mechanic | Score is | Source hint |
|---|---|---|---|---|---|
| T1 | **One-on-One** | Finishing | Ball rolls to you at the edge of the box; a keeper closes the angle. Aim with the mouse, charge power, release. Targets: 6 shots. | Goals × placement bonus | "close-range shot" |
| T2 | **Cone Slalom** | Dribbling | Steer the ball through 8 gates with a "close control" radius; touching a cone costs time. | Time vs par, gates hit | "obstacle course" |
| T3 | **Cross & Header** | Heading | Three crosses arrive at set intervals; position with `WASD`, time the jump, steer the header into a 3-zone goal. | Target zone accuracy | "head it in from the cross" |
| T4 | **Cross Fire** | Crossing | From the wing, deliver 5 crosses into moving target zones; hold to charge, mouse to aim curl. | Zone hit rate | "you get the idea" |
| T5 | **Rondo** | Passing | Wall passes with two teammates; a moving gate shows the pass window. Wrong-weight passes are intercepted. | Completed passes / 12 | "passing back and forth" |
| T6 | **Shield & Steal** | Tackling | An opponent shields the ball with his back to you. Read his lean, press at the right moment; too early = foul. | Clean steals − fouls | "take the ball off a player with his back turned" |
| T7 | **Top Corner** | Long shots | 25–30 m out, wind and swerve, keeper dives. 5 shots. | Goals × distance bonus | "long shot" |

### 9.3 Physical drills (gym corner at the facility in v1.0, full gym in v1.1)

| # | Drill | Attribute | Core mechanic | Score is | Source hint |
|---|---|---|---|---|---|
| P1 | **Treadmill Intervals** | Sprint speed | Hit a cadence with alternating input at rising speed; miss three beats and the belt slows. | Peak speed reached | "treadmill → max speed" |
| P2 | **Endurance Run** | Stamina | Keep a marker inside a drifting "zone" for 60 s; the zone narrows as fatigue rises. | Time in zone | "treadmill" |
| P3 | **Bar Holds** | Balance | Hold the pull-up bar in a balance window against a wobble simulation; grip meter drains. | Seconds held | "pull-up bar" |
| P4 | **Dumbbell Circuit** | Strength | Rep timing against a form meter; too fast breaks form, too slow wastes the set. | Clean reps × form | "dumbbell" |
| P5 | **Jump Squad** | Jumping | Rhythm-based vertical jump; time the launch on the beat and land in the green. | Best jump height | "Jump Squad" |
| P6 | **Sled Push** | Acceleration | From a standing start, mash/rhythm to accelerate a weighted sled over 10 m; measure 0→10 m time. | Time to 10 m | "weight push → time to max speed" |

> **Note on the source:** "UZAKTAN ŞUT (JUMP SQUAD)" and "ZIPLAMA (JUMP SQUAD)" name the same module twice. Interpreted here as: *Jump Squad* is the jumping minigame; the long-shot drill reuses its **timing ring** UI but is a shooting drill. If the author meant a single combined drill, only T7 changes — `D11`.

### 9.4 Difficulty and variety

**Proposal.** Each drill has 3 difficulty tiers (Amateur / Pro / Elite), unlocked by attribute thresholds (40 / 60 / 75). Higher tiers raise `baseGain` by 20% and 40%. A weekly rotating "coach's focus" gives one drill a ×1.25 bonus — this is what nudges the player to vary training without a nagging UI.

---

## 10. Content tables (first pass)

### 10.1 Drinks (bar)

| Drink | Price | Intoxication | Energy | Notes |
|---|---:|---:|---:|---|
| Beer | 60 | +12 | −4 | Default |
| Raki | 90 | +20 | −8 | Stronger; local flavour |
| Wine | 80 | +15 | −3 | — |
| Soft drink | 25 | 0 | +6 | The sober option |
| Water | 10 | 0 | +2 | — |

**Rule from source:** 10 drinks in one session → `bar.binge` news event. Intoxication caps at 100; at 100 the player passes out and wakes at home at 12:00 with 30 energy.

### 10.2 Cars (gallery)

| Car | Price | Upkeep/wk | Effect |
|---|---:|---:|---|
| Second-hand hatchback | 8 000 | 80 | Travel time −40% |
| Saloon | 24 000 | 120 | Travel time −50%, small reputation gain |
| Sports coupé | 75 000 | 300 | Travel time −60%, reputation +5, `purchase.car` news |
| Supercar | 150 000 | 600 | Travel time −60%, reputation +10, guaranteed news |

**Proposal:** cars exist to compress travel time — the actual gameplay payoff — plus status. No driving simulation beyond a simple vehicle controller on the city roads.

### 10.3 Flats

| Tier | Rent/wk | Size | Contents |
|---|---:|---|---|
| Cheap | 400 | 1 room + bath | Bed, stove, small storage, no couch (TV on a crate) |
| Mid | 900 | 1 bed + living room | Full set, couch, larger storage |
| Expensive | 2 000 | 2 bed + balcony | Full set, balcony, larger storage, better sleep quality (+10%) |

---

## 11. Art, audio, tone

- **Art:** stylised semi-realistic (think *Two Point*/*Yakuza*-lite rather than photoreal). Modular kit-based interiors so six buildings share furniture; 3 flat variants reuse the same pieces at different quality tiers.
- **Audio:** diegetic-first. TV murmur, bar ambience, boot studs on concrete, treadmill whine. Music only in menus and match moments.
- **Tone:** grounded and slightly funny. The TV news is the comedic voice of the game; it should never mock the player cruelly.

---

## 12. Localisation

- Source language is Turkish; **English is the primary development language**, Turkish ships at launch.
- All player-facing strings in a table keyed by ID (`ui.menu.new_career`), never literal.
- Turkish needs ~30% more UI width — every layout is tested with a pseudo-locale.
- Culture: Turkish-specific flavour (kelle paça, rakı) is a feature, not a translation problem. Keep it, gloss it once.

---

## 13. Scope: what v1.0 is

| Priority | Content |
|---|---|
| **Must** | Main menu + career creation; flat with 6 interactions; facility with coach + 7 drills; gym corner with 6 drills; market with cashier trading; bar with bartender and drinks; gallery with car purchase + cutscene; 13 attributes + overall; needs + clock + sleep; save/load; TV news; 1 season |
| **Should** | Reputation, match-day moment gameplay, 3 flat tiers, car travel-time effect, newspapers, coach's weekly focus |
| **Could** | Test drive, dartboard, physio, phone feed, appearance customisation |
| **Won't (v1.0)** | Multiplayer, full 11v11 football, open world, GK career, transfers/agents, women's league, mod support, console ports, voice acting, injuries |

Everything in "Won't" is a deliberate cut to protect the roadmap, not a missing idea.

---

## 14. Open decisions

| ID | Decision | Recommendation |
|---|---|---|
| **D1** | Are matches playable? | **Option B, moment-based hybrid.** Full 11v11 is a different project. |
| **D2** | Is "130% energy" a multiplier on a meal or on the bar? | Treat as **1.3× a standard meal**, plus a buff. |
| **D3** | Can the player change position mid-career? | No in v1.0. Attributes still all trainable; only the overall weighting is fixed. |
| **D4** | Separate hunger bar? | No. Fold food into Energy, as the source implies. |
| **D5** | Goalkeeper career? | Out of scope; revisit only if v1.0 ships well. |
| **D6** | Multi-season career in v1.0? | Ship season 1 + "continue" hook; full multi-season in v1.1. |
| **D7** | Social/morale stat beyond reputation? | No. Reputation covers it. Cut dartboard/social if time is short. |
| **D8** | Console ports? | PC/Steam first. Do not design UI around a controller-only layout that blocks a later port. |
| **D9** | Shipping title | "FootbalSim" is a typo in the repo name. Working title: *FootbalSim*; candidate: **First XI** / **Kickstart** / **Sunday League**. Decide before store page. |
| **D10** | Do flat tiers ever differ mechanically? | Only the +10% sleep quality on the expensive flat. No home gym. |
| **D11** | Long shots vs Jump Squad | Two separate drills sharing one timing-ring component. |

---

## 15. Glossary — Turkish source → English

| Turkish (source) | English used here | Note |
|---|---|---|
| Aşama | Stage / Milestone | 5 stages in the source |
| Giriş ekranı | Main menu / title screen | Stage 1 |
| Atarlar Sekmesi *(typo for Ayarlar)* | Settings tab | Includes key rebinding |
| Yeni oyun sekmesi | New Career tab | Opens naming modal |
| Kariyeri adlandır | Name your career | 3–24 chars |
| Spawn | Spawn | Spawn into the flat after intro |
| Map / şehir | City map / district | 6 destinations |
| Etkileşimli bina | Interactive building | Interiors are separate scenes |
| GYM | Gym | v1.1 interior |
| EV | Flat / house | Cheap / Mid / Expensive |
| BAR | Bar | Drinks, bartender, stools |
| MARKET | Market | Cashier NPC, shelves |
| GALERİ | Car gallery / dealership | Purchase + handshake cutscene |
| TESİS | Training facility | Coach, teammates, 7 drills |
| Dolap | Storage / larder | "Chest" behaviour: stops spoilage |
| Ocak | Stove | Cooking |
| Duş | Shower | Hygiene |
| Tuvalet | Toilet | Bladder |
| Koltuk ve TV | Couch and TV | News bulletin |
| Kirlilik barı | Hygiene bar | Source says "dirtiness" |
| Sarhoş | Drunk / intoxicated | Intoxication need |
| Ara sahne | Cutscene | Handshake on car purchase |
| Teknik Direktör | Head coach / manager | Chooses the training programme |
| Takım arkadaşları | Teammates | Ambient NPCs |
| Bitiş | Finish | — |
| Bitiricilik | Finishing | T1 |
| Dribling | Dribbling | T2 |
| Kafa vuruşu | Heading | T3 |
| Orta yapma | Crossing | T4 |
| Pas | Passing | T5 |
| Top kapma | Tackling | T6 |
| Uzaktan şut | Long shots | T7 |
| Hız | Sprint speed | P1 |
| Dayanıklılık | Stamina | P2 |
| Denge | Balance | P3 |
| Güç | Strength | P4 |
| Zıplama | Jumping | P5 |
| Hızlanma | Acceleration | P6 |
| E-UYU / G-DİNLEN | `E` — Sleep / `G` — Rest | Mapped to primary/secondary interact |
| Kelle paça | Kelle paça (sheep's-head soup) | Untranslated on purpose; a real dish |
| Mini game | Minigame | Stage 5 was empty; defined in §9 |

---

## 16. What changed from the source, and why

| Source issue | Resolution |
|---|---|
| Stage 5 ("minigames") was an empty heading | Full 13-drill framework defined (§9) |
| "Overall system" requested but never specified | Weighted-average formula + 4 position profiles (§4.2) |
| No time system, but sleep skips days | Clock, calendar, configurable speed (§5.1) |
| Needs implied (energy, dirt, bladder) but unscoped | Four needs, decay rates and failure states (§5.2) |
| No economy ledger | Income/outgoings, starting money, bankruptcy branch (§5.5) |
| Matches referenced but never designed | Three options; moment-based hybrid recommended (§5.6) |
| "10 drinks → TV news" had no mechanism | Tagged news-event bus + bulletin composer (§5.7) |
| Gym deferred by the author | Honoured: shell in M3, drills at facility, full gym in v1.1 (§6.2) |
| Buildings listed without interactions | Per-object interaction inventory (§6.2) |
| "Atarlar" (typo), inconsistent keybinds | Unified input map, primary/secondary interact (§7.1) |
| No save system mentioned | 3 slots + autosave, versioned schema (`ARCHITECTURE.md` §8) |
| No failure states anywhere | Collapse, eviction, scandals, missed-session fines (§5) |
