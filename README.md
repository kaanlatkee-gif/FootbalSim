# FootbalSim

A first-person football **career life sim**: you sleep, cook, shower, train and drink your way through a season. Your body is the build — every attribute is earned by physically doing the drill — and the world (the coach, the bartender, the TV news) reacts to what you actually did.

> *Football Manager meets The Sims, in first person, with a controller in your hand instead of a spreadsheet.*

**Status:** pre-production. Design and plan are complete; no code yet.
**Engine:** Unity 6 LTS · **Language:** C# · **Target:** PC / Steam

---

## Documents

| Document | What it is | Read it if you want |
|---|---|---|
| [`docs/GDD.md`](docs/GDD.md) | Game design document — translated, restructured and expanded from the original Turkish note | To know what the game *is* |
| [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) | C# / Unity technical architecture, project layout, coding conventions, testing | To know how it will be *built* |
| [`docs/ROADMAP.md`](docs/ROADMAP.md) | 8-milestone plan (M0–M7, ~37 weeks), scope-control and risk tables | To know *when* and *in what order* |
| [`docs/SOURCE-TRANSLATION.md`](docs/SOURCE-TRANSLATION.md) | Faithful English translation of `Fotbalcareer.txt`, with translator's notes | To check the polish against the original |

---

## The idea in five lines

1. You are a young player at the start of a career. A day is a budget of hours, energy and money.
2. 13 attributes (7 technical, 6 physical) — **no XP, no skill tree** — rise only by doing first-person drills.
3. Your body state feeds those drills: sleep, food, hygiene and alcohol decide whether training actually lands.
4. The city is small and dense: flat, facility, market, bar, car gallery, gym.
5. The television tells the city what you did — a great match, a ten-drink night, a missed session.

---

## Milestones

| | Milestone | Weeks | Proves |
|---|---|---|---|
| M0 | Foundation | 2 | Tooling, CI, clock, tests |
| M1 | Vertical slice | 4 | One full day loop, end to end |
| M2 | Stage 1 — menus, settings, save | 3 | New career → play → reload |
| M3 | Stage 2–3 — city, flat, market, bar | 5 | The life sim works |
| M4 | Stage 4–5 — attributes + 13 minigames | 6 | The game's heart |
| M5 | Career, matches, media | 5 | A full season |
| M6 | Content and polish | 6 | Feature-complete |
| M7 | Beta → v1.0 | 6 | Ship |

Details, cut lists and traps: [`docs/ROADMAP.md`](docs/ROADMAP.md).

---

## Open decisions before M1

These are listed in full in `docs/GDD.md` §14. The ones that block work:

- **D1 — Are matches playable?** Recommended: moment-based hybrid (~10 key moments per match), not an 11v11 match engine.
- **D9 — Shipping title.** "FootbalSim" is a typo in the repo name; decide before the store page exists.
- **D6 — Multi-season in v1.0?** Recommended: no, ship season 1 with a continue hook.

---

## Getting started (once M0 lands)

```bash
git clone <repo>
# Open the Unity project in unity/ with Unity 6 LTS
# Run the fast, engine-free tests:
dotnet test tests/Core.Tests
```
