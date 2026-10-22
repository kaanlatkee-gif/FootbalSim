# FootbalSim — Development Roadmap

**Target:** Steam (PC) v1.0 on Unity 6 LTS / C#
**Basis:** `GDD.md` v0.1 and `ARCHITECTURE.md`
**Assumptions:** a solo full-time developer working ~4 weeks per milestone, with **20% time reserve** already included in every milestone. Cut scope before you cut the reserve — the reserve is what keeps the schedule honest.

---

## 1. Why this stack

| Decision | Choice | Reason |
|---|---|---|
| Engine | **Unity 6 LTS** | Best first-person 3D + UI + cutscene + store-asset coverage; C# is the primary scripting language |
| Language | **C# 9** (Unity's ceiling) | Strong typing, records, pattern matching; nullable in Core |
| Renderer | **URP** | Cheap enough for interiors, supports baked lighting well |
| Input | New Input System | Rebinding table is a Stage-1 requirement, so it must be right from the start |
| Data | ScriptableObjects + CSV | Designers tune without recompiling |
| Alternative | Godot 4 + C# | Viable, lighter, weaker cutscene/asset ecosystem. Choose only if you refuse Unity's licensing. |

Everything below assumes Unity. The pure-C# Core layer (§`ARCHITECTURE.md` 2) makes the migration cost survivable if that changes.

---

## 2. Milestone overview

| M | Name | Duration | Ends when… | Stage |
|---|---|---|---|---|
| **M0** | Foundation | 2 wk | A built `.exe` opens a menu and reads a CSV | — |
| **M1** | Vertical slice | 4 wk | The full day loop in §`ARCHITECTURE.md` 14 works and autosaves | 1–3 |
| **M2** | Menus, settings, save | 3 wk | A player can create, rename, save, load and quit a career | **Stage 1** |
| **M3** | City & the three core interiors | 5 wk | You can shop, drink, sleep, cook and shower in one seamless loop | **Stage 2–3** |
| **M4** | Attributes & training | 6 wk | All 13 attributes trainable via 13 minigames, overall shown | **Stage 4–5** |
| **M5** | Career, matches & media | 5 wk | A full season plays out with news, ratings and money | — |
| **M6** | Content & polish | 6 wk | Feature-complete, content-complete, localised | — |
| **M7** | Beta → v1.0 | 6 wk | Ship | — |

**Total: 37 weeks (≈ 8.5 months)** to v1.0 for one developer. Multiply by ~0.6 per additional experienced developer after the second person (coordination cost), with M0–M1 being the only phase that does *not* compress.

---

## 3. M0 — Foundation (2 weeks)

**Goal:** the boring decisions, made once, so they never block anyone later.

### Tasks
- [ ] Unity 6 LTS project, URP, folder layout exactly as `ARCHITECTURE.md` §2
- [ ] Four assembly definitions with the dependency direction enforced
- [ ] `tests/Core.Tests` compiling against Core sources; `dotnet test` green on an empty suite
- [ ] CI: `dotnet test` on push; game-ci test-runner on PR; Unity licence secret stored
- [ ] `.gitignore`, `.gitattributes`, LFS rules, Unity text serialisation + visible meta files
- [ ] `IClock` + `GameTime` + tests (advance, wrap at midnight, pause)
- [ ] `IEventBus`, `IRandomSource`, `GameMath`, `Result<T>`
- [ ] VContainer installed; `Boot` scene with a root `LifetimeScope` registering the clock
- [ ] `ContentDatabase` stub + one CSV parsed into an SO at import
- [ ] `docs/` committed; README points at the three documents

### Exit criteria
A Windows build launches, shows a debug overlay with the running game clock, and the CI badge is green.

### Traps
- Do **not** start with the player controller. It is the most fun task and the least architecturally load-bearing.
- Do **not** skip the test project "for now". It is 30 minutes now and a week later.

---

## 4. M1 — Vertical slice (4 weeks)

**Goal:** one complete day, end to end, with every system proven once. This is the milestone that de-risks the entire project.

### Tasks
- [ ] First-person controller: walk, look, sprint, footstep audio, gamepad parity
- [ ] `IInteractable` / `IInteractor`, prompt card, 2.5 m cone selection
- [ ] `PlayerStateMachine` with `FreeRoam` and `Interacting`
- [ ] One grey-box flat interior: bed (`E` sleep 4/6/8/10 h, `G` rest)
- [ ] `NeedsModel` ticking off `IClock`; energy + hygiene only
- [ ] One grey-box city block with two doors (flat ↔ facility)
- [ ] `SceneFlowService`: additive interior load, fade, spawn point
- [ ] Facility grey-box: coach NPC dialogue → drill selector
- [ ] **One** minigame — T1 One-on-One — producing a real `DrillResult`
- [ ] `ProgressMath` + `IAttributeService`; Finishing attribute rises; toast shown
- [ ] `IEconomyService` with a ledger; drill cost/wage entry
- [ ] TV object that composes one bulletin from today's events
- [ ] Save/load of time, needs, attributes, money — roundtrip tested
- [ ] Autosave on sleep

### Exit criteria
The §`ARCHITECTURE.md` 14 path runs from a cold start to a restored save with no console errors, at 60 fps.

### Traps
- Resist adding drills 2–13 here. One drill proves the contract; the other twelve are M4 content.
- The vertical slice is allowed to be ugly. Art grey-boxes only.

---

## 5. M2 — Stage 1: Menus, settings, save (3 weeks)

**Source:** *Aşama 1* — nice title screen, key settings, exit, new-game tab with career naming, spawn.

### Tasks
- [ ] `MainMenu` scene: cinematic camera, animated vertical menu
- [ ] **New Career modal**: career name validation, position picker, club picker, Confirm/Cancel
- [ ] Intro cutscene (Timeline, ~20 s) → spawn in the flat
- [ ] **Settings**: Gameplay / Audio / Video / Controls
- [ ] **Controls**: full rebinding table, conflict detection, reset-to-default, persistence independent of saves
- [ ] Save slots UI: 3 slots + autosave, with playtime, career name, timestamp, screenshot thumbnail
- [ ] Confirm dialogs for overwrite/delete/quit
- [ ] Localisation skeleton: English + Turkish tables, pseudo-locale test
- [ ] Options pipeline writing to `settings.json`

### Exit criteria
A fresh player can launch, name a career, play, quit, relaunch and continue — with rebinds intact.

### Traps
- The rebinding UI is bigger than it looks: ~1 week of the 3. Budget it explicitly.
- The New Career modal is the *only* modal in the menu. Do not let it become a wizard.

---

## 6. M3 — Stage 2 & 3: City, flats, market, bar (5 weeks)

**Source:** *Aşama 2* — city map, first city design, interactive buildings (gym / home / bar / market / gallery / facility), then *Aşama 3* — home interactions.

### Tasks
- [ ] Real city block art pass: plaza, bus stop, six façades, street dressing
- [ ] **Flat** ×3 tiers (cheap / mid / expensive) from one modular kit
- [ ] Bed, storage (spoilage halted), stove (cooking), shower, toilet, couch/TV — all wired
- [ ] Food pipeline: ingredients → inventory → storage → cook → meal → energy + buff; spoilage visuals
- [ ] **Market** interior: stocked non-interactive shelves, cashier trade panel, buy/sell
- [ ] **Bar** interior: stools, bartender dialogue, drink table, intoxication, 10-drink news trigger
- [ ] Hygiene + bladder needs live; forced toilet event at 100
- [ ] Bus stop fast travel + walking time cost
- [ ] NPC ambient barks (no systemic NPC AI yet)
- [ ] TV bulletin: 3 surfaces (TV, newspaper, phone) from one `NewsComposer`
- [ ] Reputation v1, wired to news tone

### Exit criteria
A player can live a full in-game week: shop, cook, eat, shower, sleep, drink, read the news — and money, needs and news all react correctly.

### Traps
- **Gym interior stays a shell.** The source explicitly deferred it, and honouring that saves 1.5 weeks. Physical drills live in the facility's gym corner until M6.
- Spoilage timing is the most bug-prone part of this milestone — test it with time-skip tests in Core.

---

## 7. M4 — Stage 4 & 5: Attributes and 13 minigames (6 weeks)

**Source:** *Aşama 4* — player overall (technical & physical, trained in first person); *Aşama 5* — minigame (left blank in the source; defined in `GDD.md` §9).

This is the **largest and riskiest milestone**. It is also the heart of the game.

### 4a. Attribute system (1.5 wk)
- [ ] 13 attributes in `AttributeSet`, 1–99, all persistent
- [ ] Position profiles ×4 as data; weighted overall (GDD §4.2)
- [ ] Attribute sheet UI: overall, group averages, bars, delta preview
- [ ] Full `ProgressMath`: headroom, fatigue, coach bonus, weekly multipliers
- [ ] Training report screen after each session

### 4b. Drill framework (1 wk)
- [ ] `DrillScene` template: countdown, live meter, end card, skip
- [ ] Shared components: timing ring, aim reticle, cadence meter, balance wobble, target zones
- [ ] `DrillDefinition` SO + difficulty tiers (Amateur / Pro / Elite)
- [ ] Deterministic seeded results; PlayMode test per drill contract

### 4c. Content — 13 drills (3.5 wk)
- [ ] Technical T1–T7 (facility): One-on-One, Cone Slalom, Cross & Header, Cross Fire, Rondo, Shield & Steal, Top Corner
- [ ] Physical P1–P6 (gym corner): Treadmill Intervals, Endurance Run, Bar Holds, Dumbbell Circuit, Jump Squad, Sled Push
- [ ] Coach programme selector: pick drills for the day, coach bonus when present
- [ ] Weekly "coach's focus" rotation (+25% to one drill)
- [ ] Energy/fatigue integration; score < 0.25 → no gain, double cost
- [ ] Balance pass on `baseGain` / `ScoreBonus` / headroom exponent

### Exit criteria
A new career can raise any attribute by ~10 points in one in-game week of focused, rested training, and grinding a single drill visibly stalls.

### Traps
- **Build the shared components first.** If drill 3 is written from scratch, drills 4–13 will each take a week and none will feel consistent.
- Target **2 drills per week** after the framework, not 13 in a sprint.
- Playtest the *feel* of each drill for one hour with the same tester. A drill that is unfun at 60 overall will still be unfun at 90.

---

## 8. M5 — Career, matches, media (5 weeks)

### Tasks
- [ ] Match-day hybrid (`GDD` §5.6, Option B): ~10 key moments per match, 6 moment types reusing drill components
- [ ] `MatchSimulator` pure-Core, seeded, unit-tested for determinism
- [ ] Rating 1.0–10.0 from moment outcomes, weighted by position
- [ ] Fixture list: 30-match season, calendar integration
- [ ] Wages, appearance fees, win/goal bonuses, club fines
- [ ] Reputation → selection, wage, news tone, bar treatment
- [ ] News templates ×25 across 6 tags and 3 severities
- [ ] Stadium exterior + tunnel; match-day crowd ambience (audio only)
- [ ] Season end: summary screen, offers from 3 clubs, and a "continue" hook for season 2
- [ ] Gallery: cars, handshake cutscene, **travel-time reduction** effect

### Exit criteria
A full season is playable start to finish, with a plausible final table, a wage history and a news archive that reads like it was about *your* season.

### Traps
- Match gameplay is where scope creep kills this project. Ten moments, six types, no exceptions. An 11v11 match is M9-at-the-earliest and probably never.
- Do not build a transfer market. Offers at season end are enough for v1.0.

---

## 9. M6 — Content and polish (6 weeks)

### Tasks
- [ ] **Gym interior** (the deferred item): full room, machines tied to P1–P6, ambient lifters
- [ ] **Gallery** interior art pass, three cars modelled
- [ ] Facility art pass: pitch, physio, changing room, teammates
- [ ] All 3 flat tiers fully furnished
- [ ] Cutscene polish: intro, handshake, season-end
- [ ] Full audio pass: footsteps by surface, interior ambience, UI, TV murmur, music beds
- [ ] Animation pass: player locomotion blend tree, NPC idles, door/hand interactions
- [ ] Localisation pass: Turkish + English complete, pseudo-locale layout check
- [ ] Accessibility: subtitles, FOV, sensitivity, colour-blind need colours, cutscene skip
- [ ] **Balance lock**: attribute growth curves, economy, drink/match effects
- [ ] Performance: hit §`ARCHITECTURE.md` 10 budgets on min spec
- [ ] Save migration tested from every historical test save

### Exit criteria
Feature-complete, content-complete, no placeholder art or text anywhere reachable by a player.

---

## 10. M7 — Beta to v1.0 (6 weeks)

| Week | Focus |
|---|---|
| 1–2 | Closed beta (20–50 testers). Instrument funnels: where do players quit? which drill is repeated most? |
| 3 | Bug triage; fix only blockers and top-10 friction points |
| 4 | Store page, trailer, screenshots, capsule art, Steam depot setup, achievements |
| 5 | Release candidate; wishlist push; press keys; localisation QA on both languages |
| 6 | Launch, hotfix window, day-1 patch |

### Exit criteria
Crash-free session rate > 99%, median session > 25 min, no blocker bugs, store page live.

---

## 11. Dependency graph

```
M0 ──► M1 ──► M2 ─────────────► M3 ──► M4 ──► M5 ──► M6 ──► M7
        │                        ▲        │       │
        └── Core services ───────┘        │       │
                                          │       └── gym interior needs P1–P6 drills (M4)
                                          └────────── matches reuse drill components (M4)
```

M2 and M3 can be reordered if art is the bottleneck: M2 is mostly UI, M3 mostly environment. Nothing after M4 can start early — match moments and the gym interior both depend on the drill component library.

---

## 12. Scope control

| If you are behind at… | Cut this | Never cut this |
|---|---|---|
| M2 | Cinematic camera; ship a static menu | Rebinding + save slots |
| M3 | Flat tiers → 2 instead of 3 | Food/spoilage pipeline (it is the life-sim spine) |
| M4 | Drill difficulty tiers; ship 1 tier | The shared component library |
| M4 | 13 drills → **9** (drop Cross Fire, Bar Holds, Dumbbell Circuit, Sled Push) and re-map those attributes to nearest drills | All 13 attributes must remain trainable |
| M5 | Cars → 2 models; gallery art pass | Match moment gameplay (the game is called FootbalSim) |
| M6 | Gym interior → v1.1 | Performance budgets |
| Anytime | Newspapers, phone feed, test drive, dartboard, physio | Save integrity, Core tests |

**Hard floor for v1.0:** main menu + naming → flat with sleep/cook/shower/TV → facility with coach and ≥ 9 drills → market → bar → 13 attributes + overall → money → one season of matches → news → save/load. Everything else is negotiable.

---

## 13. Risk register

| # | Risk | Likelihood | Impact | Mitigation | Owner decision |
|---|---|---|---|---|---|
| R1 | 13 minigames overrun | High | High | Shared components; 9-drill floor | M4 cut list |
| R2 | Match gameplay explodes in scope | High | Fatal | Option B locked in M0; 10 moments max | GDD D1 |
| R3 | Solo-dev burnout over 8 months | Medium | High | 20% reserve; a playable build after every milestone | — |
| R4 | Art volume (8 interiors) | Medium | High | Modular kit, 3 flat tiers from one set, grey-box longer | M3/M6 |
| R5 | Unity licensing/pricing change | Low | High | Core stays engine-free; budget for Pro if needed | — |
| R6 | Balance never feels right | Medium | Medium | Data-driven tuning + weekly playtests from M1 | M6 balance lock |
| R7 | Turkish/English text overflow | Medium | Low | Pseudo-locale from M2 | M6 |
| R8 | Save corruption in the wild | Low | Fatal | Atomic writes, versioned migration, beta soak | M7 |
| R9 | Scope creep from "we could add…" | Certain | High | This document. New ideas go to a `v1.1.md` backlog, not into M4. | Every review |

---

## 14. Post-launch backlog (v1.1+), explicitly not scheduled

Full gym expansion with machine-specific animations · goalkeeper career · multi-season transfers and agents · women's league · console ports · appearance customisation · home gym / balcony gameplay · social stat and dating · injuries and physio · sponsorship deals · mod support · photo mode.

Every one of these is a good idea. None of them is on the v1.0 critical path.

---

## 15. Definition of done — checklist per milestone

- [ ] All milestone tasks complete and merged to `main`
- [ ] Core logic unit-tested; `dotnet test` green
- [ ] New content authored as data, not hard-coded
- [ ] Save/load roundtrip verified with the new state
- [ ] Strings localised (EN + TR), pseudo-locale layout checked
- [ ] Performance within budget on min spec
- [ ] Playable build uploaded as a CI artifact and played by someone who is not the developer
- [ ] `docs/` updated; open decisions in `GDD.md` §14 resolved or explicitly deferred
