# FootbalSim

A **first-person football career sim**. You live a young player's week — sleep, cook, shower, train, lift, drink — and then you walk out and **play the match yourself**, in first person, with the ball at your feet. Every attribute is earned by doing the drill, and the world (coach, fans, TV news) reacts to what you actually did.

> *Football Manager's career depth, The Sims' daily routine, and a first-person match you actually play.*

**Engine:** Unity 6 LTS · **Language:** C# · **Platform:** PC / Steam
**Status:** M0 in progress — design, architecture and plan complete; **Core library implemented** (32 files + ~60 unit tests).

---

## Documents

| Document | What it is |
|---|---|
| [`docs/GDD.md`](docs/GDD.md) | Game design — **v0.2**, covering Stages 1–7 of the source note |
| [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) | C#/Unity architecture: Core-first layout, match layer, progression, saves, performance |
| [`docs/ROADMAP.md`](docs/ROADMAP.md) | 12 milestones, M0–M9, **≈ 49 weeks** to a Steam launch |
| [`docs/SOURCE-TRANSLATION.md`](docs/SOURCE-TRANSLATION.md) | Literal English translation of both revisions of the Turkish note |
| [`docs/DEV-SETUP.md`](docs/DEV-SETUP.md) | How to open the project, run the tests, and what is already coded |

---

## The idea in six lines

1. Everything is **3D and first-person** — the city, the interiors, the pitch.
2. A week has **5 or 6 active days**: 2 gym sessions, 2 facility sessions, 1–2 matches. Off days are skipped or limited to the market.
3. **13 attributes** (7 technical, 6 physical) rise only through XP earned by physically performing drills — never from a menu.
4. **Matches are played**: WASD + mouse, sprint, knock-on, aerials, slide tackles, skill-move combos, left-click shoot, right-click pass.
5. Your match rating (starting at 6.5) earns career XP; levels give a permanent **+4% training XP** each.
6. Transfers run both ways — clubs offer you, or you approach them and spin the **acceptance wheel**; a rejected approach costs fan and coach relationships.

---

## Milestones at a glance

| | Milestone | Weeks |
|---|---|---|
| M0 | Foundation — Core library, CI, tests | 2 |
| M1 | Vertical slice — one full day | 4 |
| M2 | Stage 1 — menus, settings, save | 3 |
| M3 | Stage 2–3 — city and interiors | 5 |
| M4 | Stage 4 — XP, attributes, 13 drills | 6 |
| M5a–c | **The playable match** | 10 |
| M6 | Career — season, transfers, leagues | 5 |
| M7 | Schedule and day rules | 2 |
| M8 | Content and polish | 6 |
| M9 | Steam beta → launch | 6 |

Full detail, cut lists, the match fidelity ladder and the risk register: [`docs/ROADMAP.md`](docs/ROADMAP.md).

---

## The one risk that matters

A playable first-person football match is roughly as large as the whole life-sim half of the game. The plan gives it 10 weeks and a **hard fidelity cap** ("arcade-authentic", not FC/FIFA), with a written fallback at week 4. Read `ROADMAP.md` §2 before promising a date.

---

## Decide before M1

From `GDD.md` §13:

- **D12 — weekly training volume:** 2 sessions of *each* type (2 gym + 2 facility) — the only reading that reconciles Stages 6 and 7.
- **D13 — cities at launch:** 4 city kits (TR, EN, ES, DE) and six reskins; ten bespoke cities is out of scope.
- **D18 — Steam:** Early Access once the match and one season work (M7), or a single 1.0 launch.
- **D9 — title:** "FootbalSim" is a typo in the repo name; decide before the store page exists.

---

## Run it now (no Unity required)

Requires the **.NET 8 SDK** only.

```bash
git clone <repo> && cd FootbalSim

# 1. Play three seasons headlessly and watch the systems work
dotnet run --project src/FootbalSim.Simulator

# 2. Run the unit tests (~60 assertions, ~5 seconds)
dotnet test

# 3. Or build everything
dotnet build FootbalSim.sln
```

The simulator prints the weekly schedule, per-season training gains, match record, career level,
market value, the evening news bulletin and a transfer negotiation — all from the same Core library
the Unity build uses.

Options: `dotnet run --project src/FootbalSim.Simulator -- --seed 42 --seasons 5`

## Solution layout

| Project | Target | Purpose |
|---|---|---|
| `src/FootbalSim.Core` | netstandard2.1 | Engine-free gameplay core (32 files). Compiles the same sources Unity uses. |
| `src/FootbalSim.Simulator` | net8.0 | Console app: headless career simulation. |
| `tests/Core.Tests` | net8.0 | NUnit suite. `dotnet test` |

Open `FootbalSim.sln` in Visual Studio, Rider or VS Code. The Unity editor project lives in `unity/`
and is opened separately — see [`docs/DEV-SETUP.md`](docs/DEV-SETUP.md).
