# FootbalSim — Game Design Document

**Version:** 0.2 — incorporates the **second revision** of the source note (`Footbalcareer.txt`)
**Date:** 2026-10-22
**Status:** Pre-production · concept locked · balance unverified
**Companion documents:** `ARCHITECTURE.md` (how it is built), `ROADMAP.md` (when), `SOURCE-TRANSLATION.md` (literal source)

---

## Revision history

| Version | Change |
|---|---|
| 0.1 | First translation and polish of the original note (Stages 1–5) |
| **0.2** | **Second source revision ingested.** Everything is 3D and first-person; matches are *played*, not simulated; progression is XP-gated with a player level; the week has 5–6 fixed active days; transfers, 10 leagues, countries and relationships are in scope; the target is a Steam release. |

What changed in 0.2, in one line each:

| v0.2 change | Where |
|---|---|
| Full first-person **match gameplay** (FC-style controls, dribble combos) replaces the moment-based hybrid | §4.5, §6.2, §8.2 |
| Facility training is **real 3D drills**, not minigames; gym keeps station minigames | §9 |
| Attributes rise through **XP thresholds**, not direct gains | §4.3–4.5 |
| **Player level** with a +4%/level training XP multiplier | §3.5 |
| **Weekly schedule**: 5–6 active days, off-days are skipped | §4.1, §5.1 |
| **Transfers**, 10 leagues, countries, market value, negotiation wheel | §5.9 |
| **Relationships**: fans, coach, girlfriend | §5.8 |
| Target: **Steam release** | §12, roadmap §13 |

---

## How to read this document

| Marker | Meaning |
|---|---|
| **Source** | Present in the original note |
| **v0.2** | Added by the second revision of the note |
| **Proposal** | Added here to make the idea playable; approve or reject |
| **`D#`** | Open decision in §13 |
| **`[TBD]`** | Placeholder number to tune in playtest |

Every number is a first pass. Balance lives in data files, never in code (`ARCHITECTURE.md` §4), so tuning never requires a rebuild.

---

## 1. High concept

### 1.1 Pitch

**FootbalSim** is a first-person football career sim. You live a young player's week: you sleep, cook, shower, commute, train at the facility, lift in the gym, drink at the bar, and then — on match day — you actually walk out and play, in first person, with the ball at your feet. Nothing is a menu: training is a drill you perform, a match is 90 minutes you play, and everything you did between matches decides what your body can do when the whistle blows.

### 1.2 Elevator pitch

> *"Football Manager's career depth, The Sims' daily routine, and a first-person match you actually play — one player's life, one season at a time."*

### 1.3 Facts

| Field | Value |
|---|---|
| Genre | First-person football career / life sim |
| Perspective | **First person — on foot and on the pitch** (v0.2) |
| Platform | PC (Windows), Steam; gamepad supported |
| Engine | Unity 6 LTS + C# |
| Session | 25–45 min (≈ 1 in-game day or 1 match) |
| Audience | 16–35, football career-mode players and life-sim players |
| References | *FC/FIFA* (match controls), *Football Manager* (career), *The Sims* (needs), *Yakuza* (dense city), *Persona* (calendar) |
| Content scale v1.0 | 10 leagues (data), 4 country city kits, 8 interiors per city, 13 attributes, 13 drills, 11v11 matches, 34-match season |

### 1.4 What makes it different

Career modes are menus; life sims have no sport. FootbalSim connects them physically: the sleep you skipped shows up as a worse first touch, the extra gym session shows up as a faster sprint, the ten drinks show up on the news, and the match itself is played from inside the player's head rather than watched from a spreadsheet.

### 1.5 Design pillars

| # | Pillar | In practice | Test question |
|---|---|---|---|
| P1 | **The body is the build** | No perks, no skill trees. Attributes rise only from XP earned by physically doing the drill. | *Can a stat rise without doing the activity?* If yes, cut it. |
| P2 | **Routine is the loop** | A week has 5–6 scheduled days; each day spends hours, energy and money. | *Does this create a trade-off between two good options?* |
| P3 | **You play, not watch** | Matches and facility drills are played in first person. | *Could this be a menu instead?* Then it should not be. |
| P4 | **The world talks back** | TV, coach, fans and tabloids react to what you actually did. | *Does the world acknowledge a choice within one day?* |
| P5 | **Small but solid** | One city block done properly beats an open world done badly. | *Can we ship this at quality in this milestone?* |

---

## 2. Core loop

### 2.1 Moment-to-moment

Walk → focus an object → prompt (`E`) → interact → world state changes. On the pitch: receive → dribble → pass/shoot → react.

### 2.2 Day loop

```
 active day
   wake ──► needs check ──► (optional) roam / NPCs / shop ──► session start
                                                             │
                        training day ────────────────────────┤
                        match day ───────────────────────────┤
                                                             ▼
                      3 drill slots          OR        90-minute match
                            │                              │
                            └────────► return home ◄────────┘
                                          │
                                    TV / news / sleep
```

### 2.3 Week loop (v0.2)

A week has **5 or 6 active days**. The rest are off-days: the UI offers "skip to next scheduled day", and if the player insists on entering the city anyway, **only the market and the flat are open**.

| Day | Activity | Content |
|---|---|---|
| 1 | **Gym session** | 3 drill slots (physical) |
| 2 | **Facility session** | 3 drill slots (technical) |
| 3 | Off day | Skip, or market only |
| 4 | **Gym session** | 3 drill slots |
| 5 | **Facility session** | 3 drill slots |
| 6 | **Match day** | Play or skip |
| 7 | Off day — **or a second match** in a European week | Skip / market |

**European weeks** (club has a continental fixture) have **6 active days**: two matches instead of one rest day.

> **Source reconciliation.** Stage 6 says "trainings can be done 2 times a week, 3 trainings each"; Stage 7 says a week contains 2 gym + 2 facility sessions. Read together, the consistent interpretation is **2 sessions of each type per week, 3 drills per session** — 12 drill slots weekly. If the author meant 2 sessions *in total*, the progression numbers in §4 halve; see `D12`.

### 2.4 Season and career loop

34-match league season → end-of-season review → transfer window (offers in and out) → new season in the same or a new country → map changes with the country (§4.8).

---

## 3. Player progression model (v0.2 — rewritten)

### 3.1 The 13 attributes

**Source:** seven technical and six physical attributes, each tied to a training activity. Unchanged from v0.1.

| Group | Attribute | ID | Trained at | Drill |
|---|---|---|---|---|
| Technical | Finishing | `FIN` | Facility | 3D one-on-one |
| Technical | Dribbling | `DRI` | Facility | 3D slalom |
| Technical | Heading | `HEA` | Facility | 3D crossing + header |
| Technical | Crossing | `CRO` | Facility | 3D wide delivery |
| Technical | Passing | `PAS` | Facility | 3D rondo |
| Technical | Tackling | `TAC` | Facility | 3D shield-and-steal |
| Technical | Long shots | `LON` | Facility | 3D shooting range |
| Physical | Sprint speed | `SPD` | Gym | Treadmill intervals (station) |
| Physical | Stamina | `STA` | Gym | Endurance run (station) |
| Physical | Balance | `BAL` | Gym | Bar hold (station) |
| Physical | Strength | `STR` | Gym | Dumbbell circuit (station) |
| Physical | Jumping | `JMP` | Gym | Jump Squad (station) |
| Physical | Acceleration | `ACC` | Gym | Sled push (station) |

Goalkeeping remains out of scope (`D5`).

### 3.2 Overall rating

Unchanged: a **position-weighted average**, 1–99, computed on demand, never saved.

```
Overall = round( Σ(attribute_i × weight_i) / Σ(weight_i) )
```

| Attribute | ST | W | CM | CB |
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

Weights are normalised on load and are authored per position as data.

### 3.3 Attribute XP — the only way a point is earned (v0.2)

**Source:** *"rather than an attribute increasing in a single training, the player must collect a certain amount of XP for +1: 65→66 needs 1000, 66→67 needs 1250, each time +250."*

Each attribute has its own XP pool and current value. A point costs:

```
cost(a → a+1) = max(250, 1000 + 250 × (a − 65))
```

| Current | Cost to next | Current | Cost to next |
|---:|---:|---:|---:|
| 60–62 | 250 | 80 | 4 750 |
| 63 | 500 | 85 | 6 000 |
| 64 | 750 | 90 | 7 250 |
| 65 | 1 000 | 95 | 8 500 |
| 70 | 2 250 | 98 | 9 250 |
| 75 | 3 500 | 99 | — (max) |

XP is **per attribute**, not a global pool. Earning finishing XP never moves passing.

### 3.4 Training XP awards (v0.2)

**Source:** *"success will be 50/100/150/200/250 XP; each time the same training is done on a different day it gives a 1.02× multiplier — the second time 255 instead of 250, assuming a top-tier performance."*

A drill produces a performance score (0–1). Five tiers:

| Performance | Grade | XP |
|---|---|---:|
| 0.00 – 0.39 | Poor | 50 |
| 0.40 – 0.59 | Average | 100 |
| 0.60 – 0.79 | Good | 150 |
| 0.80 – 0.94 | Great | 200 |
| 0.95 – 1.00 | Perfect | 250 |

**Repeat multiplier:**

```
multiplier(drill) = min(1.50, 1.02 ^ priorSessionsOnDifferentDays(drill))
```

The first time gives ×1.00; the second ×1.02; the third ×1.0404. It rewards *spreading* a drill across weeks, not grinding it in one day. Each session counts only once per day.

**Weekly constraints (both from the source and required for the maths to work):**

| Rule | Value |
|---|---|
| Sessions per week | 2 facility + 2 gym |
| Drill slots per session | 3 |
| Same drill in one session | maximum 2 |
| Same drill per week (one type) | therefore maximum 4 |
| Repeat multiplier window | across different days only |

**Worked example — why the numbers hold together.** A player at 65 trains finishing in the two facility sessions, using both allowed slots each time:

```
4 sessions × 250 XP × (1.00 + 1.02 + 1.0404 + 1.0612) ≈ 1 031 XP
Cost of 65 → 66 = 1 000 XP  →  ≈ 1 attribute point per week (focused)
```

At overall 80 the same effort yields 1 031 / 4 750 ≈ **0.22 points/week**; at 90, **0.14**. The cost curve is what makes a career feel earned, and the "max 2 per session" rule is what prevents a single stat from being maxed in one season.

### 3.5 Player level (v0.2)

**Source:** *"the player also gets a football rating in matches… they earn XP at the end of the match based on that score; this XP does not directly affect attributes, but when the player levels up, level 1 gives a 0% training XP multiplier, level 2 gives 4%, level 3 gives 8%, rising like that — but levelling up should be hard."*

**Two separate currencies, deliberately:**

| Currency | Earned from | Spent on |
|---|---|---|
| Attribute XP | Training drills | That attribute's next point |
| **Career XP** | Match ratings | Player **level** |

Career XP per match (proposal):

```
matchXp = 120 × max(0, rating − 5.0) + resultBonus + goalBonus + assistBonus
resultBonus: win 120 / draw 60 / loss 0
goalBonus: 60 per goal, assistBonus: 40 per assist
```

Level curve — deliberately slow:

```
careerXpForLevel(L) = 1000 × (L − 1)² + 500 × (L − 1)
```

| Level | Total XP | Level | Total XP |
|---:|---:|---:|---:|
| 2 | 1 500 | 6 | 27 500 |
| 3 | 5 000 | 7 | 39 000 |
| 4 | 10 500 | 8 | 52 500 |
| 5 | 18 000 | 10 | 85 500 |

A 34-match season at an average 6.8 rating and a mid-table record earns ≈ 9 000 career XP — **level 3 after season one**, and roughly level 5 after season three. That is the intended "hard".

**Level reward — training XP multiplier:**

```
trainingMultiplier = 1 + 0.04 × (level − 1)      // cap 2.00 at level 26
```

So a level-6 player trains 20% faster than a level-1 player, permanently. This is the only global power curve in the game, and it is fed exclusively by performing in matches.

### 3.6 What no longer exists (removed from v0.1)

| v0.1 mechanic | v0.2 replacement |
|---|---|
| `baseGain + scoreBonus × score` per drill | Fixed 50/100/150/200/250 XP tiers (§3.4) |
| `headroom()` diminishing returns | The rising XP cost curve (§3.3) |
| `fatigueMultiplier` on gain | Fatigue **lowers the performance score**, which drops the XP tier |
| `coachBonus` on gain | Coach presence raises the performance ceiling (score clamp), not the XP |
| Weekly drill multiplier 1.0/0.7/0.4/0.2 | 1.02× repeat bonus across days (§3.4) |

Fatigue and coach presence still matter — they just act on **how well you perform**, which is more honest to a first-person game than a hidden multiplier on a number.

---

## 4. Simulation systems

### 4.1 Time, calendar and schedule (v0.2)

- 1 in-game day = 24 h. Default clock: 1 real second = 2 in-game minutes.
- Time pauses in menus, dialogue, cutscenes, drill transitions and match stoppages.
- **Schedule service** owns the week: 5–6 active days, each with a type (`Gym`, `Facility`, `Match`, `Off`).
- **Off days:** a full-screen prompt offers *Skip to next active day* or *Go out*. If the player goes out, only the flat and market are accessible; every other door reads "Closed today".
- **Active days:** the player may go straight to the session/match, or start the morning and roam the city first (shops, NPCs, dating).
- **Skipping:** match days and training days can both be skipped from the schedule UI. Skipping a training day forfeits its 3 drill slots; skipping a match forfeits the rating, XP, bonus and reputation, and angers the coach.

### 4.2 Needs

Unchanged from v0.1: four needs, 0–100.

| Need | Decay / trigger | Bad-end effect |
|---|---|---|
| **Energy** | −1.5/h, −18 per drill, −25 per match | < 25 caps drill performance; 0 = forced sleep |
| **Hygiene** | −2/h, −12 per drill, −20 per match | < 30: coach bonus lost, NPC dialogue changes |
| **Bladder** | +6/h, +12 per drink | 100 = forced toilet event |
| **Intoxication** | +12 per drink, −8/h | > 60: sway, movement penalty, −40% performance; 10 drinks → news |

### 4.3 Sleep, food, cooking, spoilage

Unchanged from v0.1:

- Bed: `E` Sleep (4/6/8/10 h) or `G` Rest (30/60/120 min). Sleep quality 0.5–1.2 from hygiene, intoxication and routine consistency.
- Storage halts spoilage ("like a chest"). Ingredients spoil over 3 days, cooked meals over 1.
- Stove: 1–3 ingredients + 30 min → meal. Kelle paça = 1.3× a standard meal plus a strength buff (`D2`).

### 4.4 Economy

Base loop unchanged (wages, rent, groceries, bar, cars), extended in v0.2 with **contracts and market value**:

| Item | Value |
|---|---|
| Starting balance | 3 000 |
| First contract wage | 2 500 / week (club tier 3) |
| Appearance fee | 1 500 / match played ≥ 45 min |
| Win bonus | 2 000 |
| Goal bonus | 500 / goal, 250 / assist |
| Rent cheap/mid/expensive | 400 / 900 / 2 000 per week |
| Transfer fee | paid by the club, not the player — it moves the player, not the wallet |

Bankruptcy (< −5 000) evicts to the cheap flat, as in v0.1.

### 4.5 Matches — first person, playable (v0.2, replaced)

**Source:** *"the match is played from the character's perspective… WASD and shift are used, like FC 27's open-world mode… shift+shift knocks the ball on and runs, space controls a high ball, ctrl slides, shift sprints… combos for skill moves, e.g. S+D → A+W to drag it right and go past on the left. Shooting and crossing: mouse aim, right click passes, left click shoots."*

#### Format

| Item | Value |
|---|---|
| Teams | 11 v 11 |
| Length | 2 × 6 min real time `[TBD]` |
| Camera | First person, eye height 1.65 m, 90° FOV |
| Control | One player; the other 21 are AI |
| Difficulty | Amateur / Pro / Elite (affects AI reaction time and error) |

#### Controls (on-pitch)

| Input | Action |
|---|---|
| `W A S D` | Move |
| Mouse | Look / aim |
| `Shift` (hold) | Sprint |
| `Shift` `Shift` (double-tap) | **Knock the ball ahead and run** — push the ball 4–8 m and accelerate |
| `Space` | **Aerial control** — bring a high ball down; timed correctly it kills the ball, badly it bounces away |
| `Ctrl` | **Slide tackle** |
| `S+D` → `A+W` and similar | **Skill-move combos** — buffered direction sequences that produce a feint (inside-out, drag-back, step-over) |
| **Left mouse** | **Shoot** — hold for power, mouse steers placement |
| **Right mouse** | **Pass** — hold for weight; context-sensitive (through ball when aiming beyond a defender) |

Skill-move combos are matched against a data table of input sequences, so new moves are data, not code.

#### Match rating (v0.2)

**Source:** *"starts at 6.5; bad play or losing the ball lowers it; goals and assists and good play raise it."*

Every player starts at **6.5**. Events adjust it live:

| Event | Δ rating |
|---|---:|
| Goal | +0.90 |
| Assist | +0.60 |
| Key pass | +0.15 |
| Shot on target | +0.08 |
| Successful dribble | +0.05 |
| Tackle / interception won | +0.10 |
| Possession lost | −0.12 |
| Failed pass in own third | −0.08 |
| Clear chance missed | −0.30 |
| Foul conceded | −0.10 |
| Yellow / red card | −0.25 / −0.90 |
| Own goal | −0.90 |

Clamped 3.0–10.0 and displayed live in the corner so the player can feel the match "grading" them. Final rating drives career XP (§3.5), relationships (§4.7) and the news (§4.6).

#### Match outcomes and the world

- Result affects the table, the coach relationship and the news.
- A rating ≥ 8.5 triggers a "man of the match" news line and a reputation jump.
- A rating ≤ 4.5 triggers criticism and a coach relationship drop.
- Missed matches (skipped or injured) reduce the coach relationship.

### 4.6 Media

Unchanged mechanically: tagged `NewsEvent`s → `NewsComposer` → TV at 21:00, newspaper, phone feed. v0.2 adds tags:

| Tag | Trigger |
|---|---|
| `transfer.offer` | A club bids for you |
| `transfer.accepted` / `transfer.rejected` | Move completed / your approach was refused |
| `relationship.fans` | Fan relationship crosses a threshold |
| `match.rating.high` / `.low` | Rating ≥ 8.0 / ≤ 4.0 |
| `bar.binge` | 10 drinks in a session |
| `club.missed` | Skipped a session or match |

### 4.7 Relationships (v0.2)

**Source:** rejecting the player's own transfer approach must have *"a negative consequence: the relationship with the fans drops and the relationship with the coach drops"*. The source also asks for NPC conversation and *"getting a girlfriend"*.

| Relationship | Range | Start | Raised by | Lowered by | Effect |
|---|---|---|---|---|---|
| **Fans** | 0–100 | 30 | Goals, great ratings, trophies, loyalty | Transfer flirtation, scandals, poor form | News tone, home atmosphere, shirt sales income, transfer welcome |
| **Coach** | 0–100 | 50 | Training attendance, high ratings, fulfilling the coach's drill request | Skipping sessions, bad matches, rejected transfers | Selection probability, position in the team, drill bonus |
| **Board** | 0–100 | 50 | League position, market value growth | Wage demands, fan unrest | Transfer budget, patience, contract renewals |
| **Girlfriend** | 0–100 | — (optional) | Talking, gifts, dates | Neglect, public scandals | Energy/morale recovery bonus, a news line, a life outside football |

The three club relationships are **Must**. The girlfriend system is **Should** (minimal: 3 dateable NPCs, 5 stages, talk/gift/date actions); see `D14`.

### 4.8 Transfers, leagues and countries (v0.2)

**Source:** *"a realistic transfer policy and countries; changing country changes the map; the top 10 active football leagues; contracts, wages, market value from goals, assists and match ratings…"*

#### Leagues and countries

| Tier | Leagues | Country kit |
|---|---|---|
| 1 | England, Spain, Italy, Germany, France | EN, ES, IT, DE |
| 2 | Portugal, Netherlands, Turkey | PT, NL, TR |
| 3 | Brazil, USA | BR, US |

Ten leagues as **data** is cheap. Ten *cities* is not — v1.0 ships **4 country city kits** (TR, EN, ES, DE) and re-skins the remaining six with palette, signage and skyline dressing (`D13`).

#### Market value

```
marketValue = base(overall) × ageFactor × formFactor × leagueFactor
base(overall)   = 1 000 × 1.12^(overall − 50)
ageFactor       = ≤21: 1.30 | 22–25: 1.15 | 26–28: 1.00 | 29–31: 0.75 | 32–34: 0.50 | 35+: 0.25
formFactor      = clamp(0.5, 1.4, 0.8 + 0.3 × (avgRating − 6.5))
leagueFactor    = tier1: 1.00 | tier2: 0.60 | tier3: 0.35
```

A 24-year-old, 78-overall, 7.2-average player in a tier-1 league: 1 000 × 1.12^28 ≈ 22 000 → ×1.15 ×1.01 ×1.00 ≈ **25 600** (thousands). Wages follow the value, roughly `value / 500` per week.

#### The two-way transfer system

| Path | Flow |
|---|---|
| **Club offers you** | Offer arrives → negotiate wage (each demand lowers their acceptance) → accept / decline → if accepted, move; if refused, you may accept their original terms or stay |
| **You offer yourself** | Pick a club → set wage demand → **wheel spin** shows acceptance % → accepted = move; refused = stay, **fans −8, coach −5, reputation −3** |

**Acceptance probability** (shown to the player before the spin, per the source's "the player can see whether it came or not"):

```
fit        = clamp(0.2, 1.5, marketValue / clubExpectedValue)
wageFactor = clamp(0.10, 1.10, 1 − 0.5 × (demandedWage − expectedWage) / expectedWage)
p          = clamp(0.05, 0.90, 0.55 × fit × wageFactor × reputationFactor)
```

The wheel is a pure visualisation of `p` — the RNG is rolled first, the wheel animates to the result. Never the reverse: an animation-driven RNG is untestable and feels cheatable.

#### Contract content

Wage, length (1–4 years), signing bonus, goal/assist bonuses, release clause (v1.1). Contract expiry forces a renewal negotiation or free agency — a second, gentler transfer path.

---

## 5. World and locations

### 5.1 Cities and access (v0.2)

- **Everything is 3D and first-person** — city, interiors, gym, facility, stadium. No 2D screens except menus, the wheel and station minigames.
- One district per city (≈ 200 × 200 m): flat, facility, gym, market, bar, gallery, stadium, plaza with bus stop.
- **Access rules per day type** (§4.1): active days = all buildings; off days = flat + market only.
- Changing country loads a different city kit and club set.

```
        ┌──────────────────────────────┐
        │           STADIUM            │  match days
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
                  │  FLATS   │
                  └──────────┘
```

### 5.2 Building interaction inventory

Unchanged in structure from v0.1, with the v0.2 3D requirement applied:

| Building | Objects | Notes |
|---|---|---|
| **Flat** ×3 tiers | Bed (`E` sleep / `G` rest), storage, stove, shower, toilet, couch + TV | Same as v0.1; all physically modelled |
| **Market** | Stocked shelves (non-interactive), cashier trade panel | Buy ingredients, drinks, gifts |
| **Bar** | Stools, bartender, drinks, patrons | 10 drinks → news |
| **Gallery** | Cars, salesman, handshake cutscene | Purchase → travel-time effect |
| **Facility** | Coach, teammates, 7 **3D drill stations**, physio (v1.1) | Drills are played on the pitch |
| **Gym** | 6 **station minigames**, lockers, ambient lifters | Entered by walking to the machine |
| **Stadium** | Tunnel, pitch, 22 players, crowd (audio + impostors) | Match day only |

**Gym note:** the source still says "save the gym for later, first add the overall system". v0.2 keeps the gym's *interior art* as the last content drop, but the six physical drills must exist from M4 because the weekly schedule depends on gym sessions.

---

## 6. Controls and interaction

### 6.1 On foot

| Action | Keyboard / Mouse | Gamepad |
|---|---|---|
| Move | `W A S D` | Left stick |
| Look | Mouse | Right stick |
| Sprint | `Shift` | L3 |
| Interact | `E` | A / Cross |
| Secondary interact | `G` | X / Square |
| Pause | `Esc` | Start |
| Schedule / phone | `Tab` | D-pad up |
| Map | `M` | D-pad down |

### 6.2 On the pitch

The source's control scheme, formalised in §4.5: `WASD` + mouse, `Shift` sprint, double-tap `Shift` knock-on, `Space` aerial control, `Ctrl` slide, LMB shoot, RMB pass, buffered direction combos for skill moves.

### 6.3 Interaction UX

Unchanged: crosshair, 2.5 m / 45° cone, prompt card with the verb, locked objects show their requirement.

---

## 7. UI / UX

### 7.1 Main menu (Stage 1)

Unchanged from v0.1: cinematic title screen → New Career / Continue / Load / Settings / Quit. New Career opens the **naming modal** (career name, position, club), Confirm/Cancel, then spawns into the flat. Settings holds the **controls tab** with rebinding.

### 7.2 HUD

| Element | Placement | When |
|---|---|---|
| Needs bars (4) | Lower left | Fade when full |
| Clock, day, day type | Upper right | Always |
| Money | Under clock | On change |
| **Attribute XP bar** (current drill's attribute) | Bottom centre during sessions | Session only |
| **Career XP / level** | Character screen + post-match | On change |
| **Match rating** | Upper right during matches | Match only |
| **Score + clock** | Upper centre | Match only |
| Prompt card | Centre | On focus |

### 7.3 New screens (v0.2)

| Screen | Purpose |
|---|---|
| **Schedule** | The week's 5–6 active days, drill slots used, skip buttons |
| **Character** | 13 attributes, overall, level, career XP, relationships |
| **Transfer hub** | Offers received, clubs to approach, wage slider, acceptance % |
| **Wheel** | Full-screen spin animation with the odds visible |
| **Relationships** | Fans / coach / board / girlfriend with recent causes |
| **Post-match** | Rating breakdown, career XP earned, level progress |

---

## 8. Training content (Stage 4–5, rewritten for 3D)

### 8.1 Two kinds of training

| Type | Where | Presentation | Drills |
|---|---|---|---|
| **Technical** | Facility pitch | Full 3D, first person, ball physics | 7 |
| **Physical** | Gym stations | First-person camera locked to the machine + station minigame overlay | 6 |

Both award the same five XP tiers (§3.4). A session = 3 slots, chosen from the coach's menu (facility) or the machine itself (gym).

### 8.2 Technical drills — 3D at the facility

| # | Drill | Attribute | What the player physically does | Score |
|---|---|---|---|---|
| T1 | **One-on-One** | Finishing | Receive at the edge of the box, beat the closing keeper, shoot | Goals × placement |
| T2 | **Cone Slalom** | Dribbling | Dribble through 8 gates, close control, cones cost time | Time vs par |
| T3 | **Cross & Header** | Heading | Three crosses, position, jump, head into a zone | Zone accuracy |
| T4 | **Cross Fire** | Crossing | Five deliveries from the wing into moving zones | Zone hit rate |
| T5 | **Rondo** | Passing | Wall passes through a moving gate; weight matters | Completed / 12 |
| T6 | **Shield & Steal** | Tackling | Read the shield, time the challenge, avoid the foul | Steals − fouls |
| T7 | **Top Corner** | Long shots | 25–30 m, wind, swerve, keeper | Goals × distance |

Each technical drill **uses the same movement, ball and shooting systems as the match** (`ARCHITECTURE.md` §7). That is the core engineering bet of v0.2: one ball, one set of controls, two contexts.

### 8.3 Physical drills — gym stations

| # | Drill | Attribute | Mechanic | Score |
|---|---|---|---|---|
| P1 | **Treadmill Intervals** | Sprint speed | Cadence input at rising speed | Peak speed |
| P2 | **Endurance Run** | Stamina | Keep a marker in a drifting zone for 60 s | Time in zone |
| P3 | **Bar Holds** | Balance | Hold through a wobble sim; grip drains | Seconds |
| P4 | **Dumbbell Circuit** | Strength | Rep timing against a form meter | Clean reps × form |
| P5 | **Jump Squad** | Jumping | Rhythm jump, land in the green | Height |
| P6 | **Sled Push** | Acceleration | Accelerate a weighted sled 10 m | Time |

Station minigames are **UI overlays with a locked first-person camera**, not full-body animation — this is deliberate, and it is what keeps the gym shippable.

### 8.4 Coach programme

The coach's dialogue sets the session's focus and grants a **performance ceiling bonus** (+5% max score) when the player trains the requested drill. The weekly "coach's focus" rotates and is visible on the schedule.

---

## 9. Content tables

### 9.1 XP and level (v0.2)

See §3.3–3.5. All values live in `Balance/attributes.csv` and `Balance/levels.csv`.

### 9.2 Leagues

| League | Tier | Country kit | Clubs (v1.0) |
|---|---:|---|---:|
| England | 1 | EN | 20 (data only) |
| Spain | 1 | ES | 20 |
| Italy | 1 | IT (reskin) | 20 |
| Germany | 1 | DE | 18 |
| France | 1 | FR (reskin) | 18 |
| Portugal | 2 | PT (reskin) | 18 |
| Netherlands | 2 | NL (reskin) | 18 |
| Turkey | 2 | TR | 18 |
| Brazil | 3 | BR (reskin) | 20 |
| USA | 3 | US (reskin) | 20 |

"Reskin" = same layout kit, different palette, signage, skyline and street props. Full bespoke cities for all ten are a v2.0 cost (`D13`).

### 9.3 Drinks, cars, flats

Unchanged from v0.1 (see v0.1 tables, retained in `Balance/*.csv`):

| Drinks | Price | Intox | Energy |
|---|---:|---:|---:|
| Beer | 60 | +12 | −4 |
| Rakı | 90 | +20 | −8 |
| Wine | 80 | +15 | −3 |
| Soft drink | 25 | 0 | +6 |
| Water | 10 | 0 | +2 |

Cars: hatchback 8 000 → supercar 150 000, travel-time reduction 40–60%. Flats: 400 / 900 / 2 000 per week.

---

## 10. Art, audio, tone

- **Art:** stylised semi-realistic; modular kits for six building types; four city palettes at launch.
- **Characters:** 22 on-pitch players per match is the single biggest art/animation cost. Use one base rig, six body variants, per-club kit textures, LOD to impostors beyond 40 m.
- **Audio:** diegetic-first; crowd as an audio bed with three states (calm, tense, roaring); TV murmur; boot studs.
- **Tone:** grounded, slightly funny; the TV is the comedic voice, never cruel.

---

## 11. Localisation

English and Turkish at launch; all strings in tables; pseudo-locale layout test; Turkish needs ~30% more width.

---

## 12. What v1.0 is

| Priority | Content |
|---|---|
| **Must** | Main menu + career creation; 3D city and 8 interiors; needs, sleep, food, hygiene; market, bar, gallery; 13 attributes with XP; 13 drills (7 3D + 6 station); player level; **playable first-person matches**; weekly schedule with 5–6 active days and skip; ratings and career XP; transfers with the wheel; fans/coach/board relationships; 10 leagues as data; 4 city kits; one full season; Steam build |
| **Should** | Girlfriend system; media newspaper + phone; car travel effects; 3 flat tiers; coach's focus |
| **Could** | Test drive, dartboard, physio, appearance customisation, second-season storylines |
| **Won't (v1.0)** | Multiplayer, full 11v11 broadcast presentation, goalkeeper career, bespoke cities for all 10 leagues, women's league, agents, injuries, mod support, console |

> **Scope reality check.** Playable first-person football is the most expensive item in this document by a wide margin — it is comparable to building a small football game on top of the life sim. It is achievable at an **arcade-authentic** level (solid movement, ball, dribbling, shooting, simplified team AI, one stadium per club) with a long milestone. It is **not** achievable at FC/FIFA fidelity by a small team. See `ROADMAP.md` §2 and `D15`.

---

## 13. Open decisions

| ID | Decision | Status / recommendation |
|---|---|---|
| **D1** | Are matches playable? | **Resolved by v0.2: yes, first person.** |
| **D2** | Is "130% energy" a multiplier on a meal or the bar? | 1.3× a standard meal + buff |
| **D3** | Position change mid-career? | No in v1.0 |
| **D4** | Separate hunger bar? | No — food restores Energy |
| **D5** | Goalkeeper career? | Out of scope |
| **D6** | Multi-season career in v1.0? | Yes — transfers require it. Minimum: seasons 1–3 |
| **D7** | Social stat beyond relationships? | No; fans/coach/board/girlfriend cover it |
| **D8** | Console ports? | PC/Steam first |
| **D9** | Shipping title | Decide before the store page; "FootbalSim" is a repo typo |
| **D10** | Flat tiers mechanically different? | Only +10% sleep quality on the expensive flat |
| **D11** | Long shots vs Jump Squad | Separate drills sharing the timing-ring component |
| **D12** | Weekly training volume: 4 sessions (2+2) or 2 total? | **4 sessions (2 gym + 2 facility)** — reconciles Stages 6 and 7. Halving it halves progression speed |
| **D13** | Cities per country at launch | **4 kits** (TR, EN, ES, DE), six reskins; bespoke later |
| **D14** | Girlfriend system depth | Minimal (3 NPCs, 5 stages) in v1.0; no dating sim |
| **D15** | Match fidelity tier | **Arcade-authentic** (see roadmap §2). Full simulation is not v1.0 |
| **D16** | Match length | 2 × 6 min real time |
| **D17** | Does training grant career XP too? | **No** — career XP is match-only (per source) |
| **D18** | Steam: Early Access or 1.0? | Recommend EA once the match + one season works (M7) |

---

## 14. Glossary — Turkish source → English

| Turkish | English | Note |
|---|---|---|
| Aşama | Stage | 7 stages in v0.2 |
| Giriş ekranı | Title screen | Stage 1 |
| Atarlar *(Ayarlar)* | Settings | Typo in source |
| Kariyeri adlandır | Name your career | 3–24 chars |
| Birinci bakış açısı | First-person | New in v0.2 |
| Etkileşimli bina | Interactive building | — |
| Tesis | Training facility | 3D drills |
| GYM | Gym | Station minigames |
| Antrenman | Training (session) | 3 slots |
| Dolap | Storage | Stops spoilage |
| Ocak | Stove | — |
| Duş | Shower | — |
| Kirlilik barı | Hygiene bar | Source says "dirtiness" |
| Kelle paça | Kelle paça | Untranslated dish |
| Ara sahne | Cutscene | — |
| Teknik Direktör | Head coach | Drill selection |
| Bitiricilik | Finishing | — |
| Dribling | Dribbling | — |
| Kafa vuruşu | Heading | — |
| Orta yapma / orta açma | Crossing | — |
| Pas | Passing | — |
| Top kapma | Tackling | — |
| Uzaktan şut | Long shots | — |
| Hız / Dayanıklılık / Denge / Güç / Zıplama / Hızlanma | Sprint speed / Stamina / Balance / Strength / Jumping / Acceleration | — |
| Tecrübe (exp) | XP | New in v0.2 |
| Seviye | Level | Career level |
| Başarı oranı | Success tier | 50–250 XP |
| Reyting | Match rating | Starts 6.5 |
| Gol / Asist | Goal / Assist | — |
| Top kaybı | Possession lost | Rating penalty |
| Çalım | Skill move | Combo input |
| Topu açarak koşma | Knock-on run | Double-tap Shift |
| Kayma | Slide tackle | Ctrl |
| Fikstür | Fixture list | Generated |
| Piyasa değeri | Market value | — |
| Sözleşme / Maaş | Contract / Wage | — |
| Teklif | Offer | Transfer |
| Çark çevirme | Wheel spin | Acceptance visual |
| Kabul yüzdesi | Acceptance % | — |
| Taraftar | Fans | Relationship |
| Kız arkadaş | Girlfriend | Optional system |
| Ülke / Lig | Country / League | Map changes |
| Maç günü | Match day | — |
| Günü sardırma | Day skip | Off days |

---

## 15. Changelog against the source

| Source issue | Resolution |
|---|---|
| Stage 2 said "map" with no dimension | Everything 3D, first person; city access rules by day type (§5.1) |
| Stage 6 introduces full match gameplay | Designed as arcade-authentic 11v11 first person (§4.5) with a fidelity ladder (roadmap) |
| Stage 6's XP thresholds and grades | Formal cost curve and five tiers (§3.3–3.4) |
| Stage 6's level multiplier | Career XP curve and +4%/level cap (§3.5) |
| Stage 6 "2 trainings/week" vs Stage 7 "4 sessions/week" | Resolved as 2 + 2 sessions (`D12`) |
| Stage 7's 5–6 day week | Schedule service, off-day rules, skip UI (§4.1) |
| Stage 7's transfers and countries | Two-way transfer system, market value, wheel (§4.8) |
| Stage 7's girlfriend | Minimal relationship system (`D14`) |
| Final line: publishable on Steam | Roadmap M9 + Steam checklist |
| v0.1's moment-based match | Removed; replaced by playable matches |
| v0.1's progress-point training formula | Removed; replaced by XP thresholds |
