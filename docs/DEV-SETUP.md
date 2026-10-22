# Developer setup

Everything here is what *you* have to do once, on your machine. Everything in `Core/` is already written.

---

## 0. The three project files

| File | What it is |
|---|---|
| `FootbalSim.sln` | Solution containing all three CLI projects. Open in VS / Rider / `dotnet build`. |
| `src/FootbalSim.Core/FootbalSim.Core.csproj` | Compiles `unity/Assets/_Project/Core/**/*.cs` into a `netstandard2.1` library. **The sources are shared with Unity — there is no copy.** |
| `src/FootbalSim.Simulator/FootbalSim.Simulator.csproj` | `net8.0` console app that simulates a career. |
| `tests/Core.Tests/Core.Tests.csproj` | `net8.0` NUnit suite referencing the Core project. |

Build output goes to `src/**/bin` and `src/**/obj`, which are git-ignored and outside `unity/Assets`,
so Unity never imports them.

---

## 1. Run the fast tests first (no Unity needed)

```bash
cd FootbalSim
dotnet test
```

~60 assertions in a couple of seconds. Catches every logic regression before Unity opens.

Requirements: .NET SDK 8 (`dotnet --version`). Nothing else.

> Both Core and the tests pin `<LangVersion>9.0</LangVersion>`, which is Unity 6's level, and Core
> targets `netstandard2.1`, which is Unity's API level. If it compiles here, it compiles in Unity.

---

## 1b. Run the simulation (no Unity needed)

```bash
dotnet run --project src/FootbalSim.Simulator -- --seed 42 --seasons 3
```

Prints, from the real Core library:

- the first week's schedule (2 facility + 2 gym + 1 match, 2 off days)
- season-by-season attribute growth through the XP curve
- match record, goals, assists, average rating
- career level and the training multiplier
- market value and wage
- the evening news bulletin
- a transfer approach with the wheel probability and the fan/coach penalty on refusal

That is the whole simulation layer, exercised end to end. If a number looks wrong, it is a `Balance`
question, not a rendering one.

---

## 2. Create the Unity project

1. Unity Hub → **Unity 6 LTS** → New project → **3D (URP)**.
2. Project location: `FootbalSim/unity` — i.e. the folder that already contains `Assets/_Project`.
   *(If Hub refuses to use a non-empty folder, create the project elsewhere and move `Assets/_Project`
   into it.)*
   *Unity will create `Assets/`, `ProjectSettings/`, `Packages/` around the existing `Assets/_Project`.*
3. Open it, then check two settings:
   - **Edit → Project Settings → Player → API Compatibility Level → .NET Standard 2.1** (default). `System.MathF` needs it.
   - **Edit → Project Settings → Editor → Asset Serialization → Force Text**, and **Version Control → Visible Meta Files**.
4. Install packages (Window → Package Manager): Input System, Cinemachine, Timeline, TextMeshPro, Addressables. (`VContainer` and `Newtonsoft`/`System.Text.Json` come later at M1 — Core doesn't need them.)
5. Restart the editor when the Input System asks.

The five assemblies (`Sim.Core`, `Sim.Data`, `Sim.Gameplay`, `Sim.Presentation`, `Sim.Bootstrap`) are already defined as `.asmdef` files, so Unity will compile them on first import.

---

## 3. What you should see after import

| Assembly | Depends on | Contains now |
|---|---|---|
| `FootbalSim.Core` | *nothing* (no UnityEngine) | 32 files: time, schedule, attributes, progression, needs, economy, match, career, news, save |
| `FootbalSim.Data` | Core | empty (M1) |
| `FootbalSim.Gameplay` | Core, Data | empty (M1) |
| `FootbalSim.Presentation` | Core, Data, Gameplay | empty (M2) |
| `FootbalSim.Bootstrap` | all | empty (M1) |

`Core` has `noEngineReferences: true`. If someone ever adds `using UnityEngine` to a Core file, Unity fails the compile — which is the point.

---

## 4. The rule that keeps this maintainable

```
Bootstrap → Presentation → Gameplay → Data → Core
```

- Never let Gameplay reference Presentation.
- Never let anything reference Bootstrap except Bootstrap itself.
- Balance numbers go in `StreamingAssets/Balance/*.csv` (M4), not in code.
- `Core` must stay engine-free so `dotnet test` keeps working in ~2 seconds.

---

## 5. What is implemented right now

**M0 Core (complete):**

| Area | Types |
|---|---|
| Time | `GameTime`, `IClock`, `GameClock` |
| Calendar | `DayType`, `WeekSchedule`, `WeekScheduleGenerator`, `IScheduleService`, `ScheduleService`, `BuildingAccess` |
| Attributes | `AttributeId`, `AttributeCatalog`, `AttributeSet`, `PositionProfile` (ST/W/CM/CB) |
| Progression | `XpCurve`, `AttributePool`, `AttributeProgression`, `LevelCurve`, `PlayerLevel`, `XpTier`, `TrainingRules`, `TrainingSession`, `TrainingTracker`, `MatchXp` |
| Needs | `NeedId`, `NeedsTuning`, `ActivityCosts`, `NeedsModel` |
| Economy | `Wallet`, `LedgerEntry` |
| Match | `RatingTracker`, `MatchResult`, `InstantMatchSimulator`, `MatchSimTuning` |
| Career | `MarketValueCalculator`, `TransferProbability`, `TransferResolver`, `FixtureGenerator`, `LeagueTable` |
| News | `NewsEvent`, `NewsComposer` |
| Save | `SaveGame` + DTOs, `SaveMigration` |
| Common | `GameMath`, `IRandomSource`, `SplitMixRandom`, `Result`, `IEventBus`, `EventBus` |

**Next (M1 Gameplay):** `GameClockDriver`, `PlayerMotor` (first person), `Interactor`/`IInteractable`, `SaveService` (atomic JSON I/O), `ContentDatabase`, `Boot` scene wiring.

---

## 6. Conventions enforced by the code

- `MathF`/`Math`, never `UnityEngine.Mathf` in Core.
- Every random draw goes through `IRandomSource`. Never `UnityEngine.Random` in gameplay logic.
- Saves store **stable string keys** (`attr.finishing`), never enum ordinals — `AttributeCatalog.KeyOf` is the only mapping.
- Derived values (overall, level, cost-to-next) are never persisted.
- Every `Result` failure carries a reason string the UI can localise.
