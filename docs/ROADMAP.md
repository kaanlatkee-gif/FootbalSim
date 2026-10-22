# FootbalSim — Development Roadmap

**Version:** 0.2 (matches GDD v0.2)
**Target:** **Steam release** (PC), Unity 6 LTS / C#, first-person 3D
**Assumption:** one full-time developer. Every milestone includes a 20% reserve. **Cut scope before you cut the reserve.**

> **The honest headline:** v0.2 adds a playable first-person football match. That single feature is roughly as large as the rest of the game. The plan below gives it **10 weeks** and a hard fidelity cap — and even so it is the only milestone where the schedule can genuinely break. Read §4 before committing to a date.

---

## 1. Stack

| Decision | Choice | Why |
|---|---|---|
| Engine | **Unity 6 LTS** | Best first-person + ball physics + 22-character AI + cutscene + asset coverage |
| Language | **C# 9** | Unity's ceiling; records, pattern matching, nullable in Core |
| Renderer | URP | Baked lightmaps indoors, cheap outdoor lighting |
| Input | New Input System | Rebinding is a Stage-1 requirement; do it once, correctly |
| Physics | Custom kinematic ball + `CharacterController` players | Predictable, tunable, testable |
| Data | ScriptableObjects + CSV | Designers tune without recompiling |
| Alternative | Godot 4 + C# | Viable; weaker stadium/character assets. Core stays portable either way |

---

## 2. Match fidelity ladder — read this first

"First-person football" means very different things at different budgets. Pick the tier in writing before M5 starts.

| Tier | What it is | Time | Verdict |
|---|---|---|---|
| **A. Full simulation** | FC/FIFA-level ball control, 5-star skill system, tactical AI, broadcast presentation | 12–18 months | **Not possible** for this team |
| **B. Arcade-authentic** (planned) | Solid first-person movement, dribbling, knock-on, aerial control, slide, 6 skill combos, shooting/passing with aim, 10v10 simple AI, one stadium | **10 weeks** | **This is v1.0** |
| **C. Reduced** | Same as B but no skill combos, 8v8, simplified AI, match as 2 × 4 min | 6 weeks | Fallback if B slips |
| **D. Text/moment match** | Rating from a simulation | 1 week | Safety net if B fails entirely |

**Gate:** at the end of M5a (week 4), if the ball + player + one AI team are not playable and fun, drop to tier **C** immediately. Not in week 8.

---

## 3. Milestone overview

| M | Name | Weeks | Ends when… |
|---|---|---|---|
| **M0** | Foundation | 2 | A built exe runs, `dotnet test` is green, CI works |
| **M1** | Vertical slice (life loop) | 4 | One full day works end to end and autosaves |
| **M2** | Stage 1 — menus, settings, save | 3 | Create → play → quit → continue, with rebinds |
| **M3** | Stage 2–3 — city & interiors | 5 | Shop, cook, shower, drink, read the news in a loop |
| **M4** | Stage 4 — XP, attributes, 13 drills | 6 | All 13 attributes train through XP; 13 drills playable |
| **M5a** | Match core | 4 | You can run, dribble, shoot and score in a test arena |
| **M5b** | Match AI & flow | 4 | 11v11 with rules, halves, goals, restarts |
| **M5c** | Ratings & match integration | 2 | A played match produces a rating, XP, news and a table result |
| **M6** | Career — fixtures, transfers, leagues, relationships | 5 | A full season, a transfer window, a country change |
| **M7** | Schedule & day rules | 2 | The 5–6 day week drives everything |
| **M8** | Content & polish | 6 | Feature- and content-complete, localised |
| **M9** | Steam beta → launch | 6 | Ship |

**Total: 49 weeks (≈ 11.5 months).** Tier C would bring it to ≈ 45 weeks; tier D to ≈ 40.

---

## 4. M0 — Foundation (2 weeks)

**Goal:** decide everything boring once, so nothing blocks anybody later.

- [ ] Unity 6 LTS + URP project at `unity/`, folders exactly as `ARCHITECTURE.md` §2
- [ ] Four asmdefs with the one-way dependency chain enforced
- [ ] `tests/Core.Tests` (NUnit) compiling against `Core/**/*.cs`; `dotnet test` green
- [ ] CI: `dotnet test` on push, game-ci on PR, `UNITY_LICENSE` secret stored
- [ ] `.gitignore` / `.gitattributes` / LFS / text serialisation / visible meta files
- [ ] `GameTime`, `IClock`, `IEventBus`, `IRandomSource`, `GameMath`, `Result<T>`
- [ ] `XpCurve`, `LevelCurve` + tests (these are pure maths and prove the harness)
- [ ] VContainer wired in a `Boot` scene, clock running in a debug overlay
- [ ] `ContentDatabase` + one CSV imported to an SO
- [ ] `docs/` committed; README points at it

**Exit:** build launches, clock ticks on screen, CI green.
**Trap:** do not start with the player controller. It is the most fun task and the least load-bearing.

---

## 5. M1 — Vertical slice, life loop (4 weeks)

**Goal:** prove the whole life-sim stack in one day.

- [ ] First-person motor (walk/look/sprint), footstep audio, gamepad parity
- [ ] `IInteractable` / `IInteractor`, prompt card, 2.5 m cone
- [ ] `PlayerStateMachine`: `FreeRoam`, `Interacting`
- [ ] Grey-box flat: bed (`E` sleep 4/6/8/10 h, `G` rest), shower
- [ ] `NeedsModel` ticking off `IClock` (energy, hygiene)
- [ ] Two doors + `SceneFlowService` additive interior loading with a fade
- [ ] Facility grey-box: coach dialogue → one 3D drill stub (T1) → `DrillResult`
- [ ] `AttributeXpService` + `XpTier` + attribute toast
- [ ] `IEconomyService` with a ledger
- [ ] TV composing one news line from today's events
- [ ] Save/load: time, needs, attributes+xp, money; autosave on sleep

**Exit:** cold start → day → sleep → quit → continue restores state exactly.
**Trap:** exactly one drill. Twelve more is M4.

---

## 6. M2 — Stage 1: menus, settings, save (3 weeks)

- [ ] `MainMenu` scene with cinematic camera and animated menu
- [ ] **New Career modal**: name validation, position, club; Confirm/Cancel
- [ ] Intro cutscene (Timeline, ~20 s) → spawn in flat
- [ ] Settings: Gameplay / Audio / Video / Controls
- [ ] **Controls**: full rebinding, conflict detection, reset, persisted separately from saves
- [ ] 3 save slots + autosave, with thumbnail, career name, playtime
- [ ] Localisation skeleton (EN + TR) and pseudo-locale test
- [ ] `settings.json` pipeline

**Exit:** fresh install → named career → quit → relaunch → continue, rebinds intact.
**Trap:** the rebinding UI is ~1 week of these 3. Budget it explicitly.

---

## 7. M3 — Stage 2–3: city and interiors (5 weeks)

- [ ] City block art pass: plaza, bus stop, six façades, street dressing
- [ ] **Flat** ×3 tiers from one modular kit
- [ ] Bed, storage (spoilage halted), stove, shower, toilet, couch/TV all wired
- [ ] Food pipeline: ingredient → inventory → storage → cook → meal → energy + buff
- [ ] **Market**: stocked shelves, cashier trade panel (Minecraft-villager style)
- [ ] **Bar**: stools, bartender, drink table, intoxication, 10-drink news
- [ ] Hygiene + bladder live; forced toilet at 100
- [ ] Bus stop fast travel
- [ ] News composer → TV, newspaper, phone
- [ ] Relationship service v1 wired to news tone

**Exit:** a full in-game week of shopping, cooking, showering, sleeping and drinking works.
**Trap:** spoilage is the buggiest system here — test it with time-skip tests in Core.

---

## 8. M4 — Stage 4: XP, attributes, 13 drills (6 weeks)

### 4a. Progression (1.5 wk)
- [ ] 13 `AttributePool`s, cost curve, level-ups, save/load
- [ ] Overall from position weights; attribute sheet UI
- [ ] `TrainingRules`: 2+2 sessions, 3 slots, max 2 same drill, 1.02× repeat
- [ ] Post-session XP screen (tier, multiplier, progress bar)
- [ ] Player level: career XP curve, +4%/level multiplier

### 4b. Technical drills, 3D at the facility (3 wk)
- [ ] T1 One-on-One, T2 Cone Slalom, T3 Cross & Header, T4 Cross Fire, T5 Rondo, T6 Shield & Steal, T7 Top Corner
- [ ] **Reuses the M1 motor** — no new control scheme
- [ ] Coach programme selector; weekly coach's focus

### 4c. Gym stations (1.5 wk)
- [ ] P1–P6 as first-person station minigames with locked camera
- [ ] Shared components: timing ring, cadence meter, wobble sim, form meter
- [ ] Gym interior grey-box with six stations

**Exit:** a focused, rested week at level 1 gains ≈1 point in a trained attribute at 65, and the same effort visibly stalls at 80+.
**Trap:** build the shared components *first*. If drill 3 is written from scratch, drills 4–13 each cost a week.

---

## 9. M5 — The match (10 weeks, split into three gates)

### M5a — Match core (4 weeks)
- [ ] Test arena scene, ball, goal, pitch dimensions
- [ ] `BallController`: kinematic integration, bounce, roll, curve, possession states
- [ ] First-person motor in match context; three gaits; match stamina
- [ ] `DribbleController` with combo buffer + 6 data-driven skill moves
- [ ] `ShootingController` / `PassingController` (LMB/RMB, aim, power, weight)
- [ ] `Space` aerial control, `Ctrl` slide, double-`Shift` knock-on
- [ ] One AI team of 5 for testing

**Gate 5a:** running at a defender, beating him with a combo, and scoring must already feel good. **If not → tier C now.**

### M5b — AI and match flow (4 weeks)
- [ ] `TeamAi`: shape/press/support/mark/recover, 10 outfield + keeper per side
- [ ] `MatchRules`: halves, out of play, throw-ins, corners, goal kicks, offside (simplified), cards
- [ ] `MatchDirector` state machine; restarts; substitutions (optional)
- [ ] Crowd audio, HUD scoreboard, commentary stings
- [ ] 3 difficulty tiers by reaction delay / positioning error / tackle accuracy
- [ ] Performance pass: 22 characters within budget (§`ARCHITECTURE.md` 11)

### M5c — Ratings and integration (2 weeks)
- [ ] `RatingTracker` with live rating, 6.5 start, event deltas
- [ ] Post-match screen: rating, career XP, level progress
- [ ] `MatchResult` feeds table, news, reputation, coach relationship
- [ ] Skip-match path → `SimulateInstant()` producing the same `MatchResult`

**Exit:** a played match changes your rating, your career XP, the league table, the news and your coach's opinion of you — and a skipped match does the same through the sim path.

---

## 10. M6 — Career (5 weeks)

- [ ] League and club data: 10 leagues, 4 city kits, fixtures via seeded round-robin
- [ ] 34-match season, table, European midweek fixtures
- [ ] Contracts: wage, length, bonuses, expiry and renewal
- [ ] Market value formula and display
- [ ] **Transfer UI**: offers received, clubs to approach, wage slider, acceptance %
- [ ] **Wheel**: RNG rolled first, animation second, odds shown
- [ ] Transfer consequences: fans −8, coach −5 on a rejected approach
- [ ] Country change → new city kit loaded, new club, new league
- [ ] Relationships screen with causes (fans / coach / board / girlfriend)
- [ ] Girlfriend system (minimal: 3 NPCs, 5 stages) — cut first if behind

**Exit:** play a season, move clubs in the summer, and the world is visibly different afterwards.

---

## 11. M7 — Schedule and day rules (2 weeks)

- [ ] `WeekSchedule` generator: 2 gym + 2 facility + 1–2 match + off days
- [ ] Day-type gating on every building ("Closed today")
- [ ] Off-day skip prompt; skip from the schedule UI
- [ ] Skip a training day → forfeits 3 slots; skip a match → coach/fans penalty
- [ ] Start-the-morning-or-go-straight-there flow for active days
- [ ] Schedule screen with slots used and the weekly coach's focus

**Exit:** the week plays itself correctly for a full season with no manual bookkeeping.

---

## 12. M8 — Content and polish (6 weeks)

- [ ] Gym interior art pass; facility art pass (pitch, physio, changing room, teammates)
- [ ] Stadium art per country kit; crowd impostors
- [ ] Three cars modelled; gallery art pass; three flat tiers furnished
- [ ] Cutscenes: intro, handshake, season end, transfer reveal
- [ ] Animation pass: locomotion blend tree, match animations (pass, shoot, tackle, celebrate)
- [ ] Full audio pass: crowd states, studs, ambience, music beds
- [ ] Localisation complete EN + TR; layout QA
- [ ] Accessibility: subtitles, FOV, sensitivity, colour-blind need colours, cutscene skip
- [ ] **Balance lock**: XP costs, level curve, transfer odds, match difficulty
- [ ] Performance: budgets met on min spec
- [ ] Save migration tests from every historical schema

**Exit:** feature- and content-complete; no placeholder art or text a player can reach.

---

## 13. M9 — Steam beta → launch (6 weeks)

| Week | Focus |
|---|---|
| 1–2 | Closed beta, 30–60 testers; instrument where players quit and which drill they repeat |
| 3 | Triage: blockers + top-10 friction only |
| 4 | Steam page, trailer, capsule art, achievements, cloud saves, depot config |
| 5 | Release candidate; localisation QA; press/creator keys; wishlist push |
| 6 | Launch, hotfix window, day-1 patch |

**Steam checklist (start the paperwork in M7, not M9):**

- [ ] Steamworks partner account + app fee paid
- [ ] Store page with 5+ screenshots, trailer, short + long description, tags
- [ ] Age/content questionnaire, EULA, privacy policy
- [ ] Achievements (~20), Steam Cloud, Steam Input action manifest
- [ ] Build pipeline: `steamcmd` upload behind manual approval
- [ ] Review build ≥ 2 weeks before launch; store page live ≥ 4 weeks before
- [ ] Pricing, regional pricing, launch discount plan

**Exit:** crash-free sessions > 99%, median session > 30 min, no blocker bugs, page live.

---

## 14. Dependency graph

```
M0 ─► M1 ─► M2 ───────────────► M3 ─► M4 ─┬─► M5a ─► M5b ─► M5c ─► M6 ─► M7 ─► M8 ─► M9
                        ▲                  │
                        └── Core services ─┘
M4's 3D drills share the M1 motor ────────┘
M5's ball/motor/shooting reused by M4 drills if M4 runs after M5a  ← see note
```

**Sequencing note:** technical drills (M4b) and match gameplay (M5) share one motor, one ball and one shooting component. If you can tolerate a later first playable, **run M5a before M4b** — the drills then inherit a proven ball instead of inventing a second one. That reordering saves roughly a week and removes the biggest architectural risk in the project.

---

## 15. Scope control

| If behind at… | Cut | Never cut |
|---|---|---|
| M2 | Cinematic menu camera | Rebinding, save slots |
| M3 | Third flat tier | Food/spoilage pipeline |
| M4 | Drill difficulty tiers | All 13 attributes trainable |
| M4 | 13 drills → 9 (drop Cross Fire, Bar Holds, Dumbbell Circuit, Sled Push; remap) | The shared component library |
| **M5a gate** | **Drop to match tier C: 8v8, 4 skill moves, 2 × 4 min** | Ball feel and shooting |
| M5b | Offside (arcade-fudge it), substitutions, commentary | 11v11, halves, goals, restarts |
| M6 | Girlfriend system; 10 leagues → 6 | Transfers + the wheel |
| M6 | 4 city kits → 2 (TR, EN) | Country change changing the map |
| M8 | Crowd animation | Performance budgets |
| Anytime | Newspapers, phone feed, test drive, dartboard, physio | Save integrity, Core tests, Steam build pipeline |

**Hard floor for v1.0:** menus + naming → flat with sleep/cook/shower/TV → 3D drills at the facility and gym stations → market → bar → 13 attributes with XP → level → **a playable first-person match** → a season → transfers → news → save/load → Steam build.

---

## 16. Risk register (v0.2)

| # | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| R1 | Match gameplay overruns 10 weeks | **High** | **Fatal** | Fidelity ladder + week-4 gate; tier C fallback pre-approved |
| R2 | Match AI never feels like football | High | High | 5 states, 1 formation, 2 weeks of pure tuning in M5b |
| R3 | 22 characters miss performance budget | High | High | LOD/impostor decision in M5a; 11v11 only |
| R4 | Solo developer burnout over 11 months | High | High | 20% reserve, a playable build every milestone, cut lists written in advance |
| R5 | 13 drills overrun | Medium | High | Shared components; 9-drill floor |
| R6 | Steam launch paperwork surprises | Medium | Medium | Checklist starts M7 |
| R7 | Ten leagues of content | High | Medium | Data-only leagues + 4 city kits |
| R8 | Balance (XP economy) feels grindy or trivial | Medium | High | Data-driven tuning, weekly playtests from M4 |
| R9 | Save corruption in the wild | Low | Fatal | Atomic writes, migrations, beta soak |
| R10 | Scope creep | Certain | High | New ideas go to `v1.1.md`, never into the current milestone |

---

## 17. Post-launch (v1.1+), explicitly unscheduled

Bespoke cities for the remaining six leagues · goalkeeper career · women's league · injuries and physio · agents and release clauses · console ports · online friendlies · appearance customisation · photo mode · mod support.

---

## 18. Definition of done, per milestone

- [ ] Tasks complete and merged
- [ ] Core logic unit-tested; `dotnet test` green
- [ ] Content authored as data
- [ ] Save/load roundtrip verified with the new state
- [ ] Strings localised (EN + TR), pseudo-locale checked
- [ ] Performance within budget on min spec
- [ ] A build artefact exists and has been played by someone who is not the developer
- [ ] `docs/` updated; open decisions in `GDD.md` §13 resolved or deferred in writing
