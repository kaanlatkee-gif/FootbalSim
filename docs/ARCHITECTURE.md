# FootbalSim — Technical Architecture

**Target:** Unity 6 LTS (6000.x), C# 9 / .NET Standard 2.1
**Companion documents:** `GDD.md` (design), `ROADMAP.md` (schedule)
**Status:** Proposed for M0 review

---

## 1. Why Unity, and what if not Unity

The design is a first-person 3D life sim with heavy UI, cutscenes and small minigames. That combination has the strongest off-the-shelf support in Unity:

| Requirement | Unity | Godot 4 (.NET) |
|---|---|---|
| First-person controller + interaction | Trivial (CharacterController) | Trivial |
| Cutscenes (handshake, intro) | Timeline + Cinemachine | AnimationTree, more manual |
| ScriptableObject data authoring | Best-in-class | Custom Resources, weaker inspector |
| Character animation retargeting | Mecanim + humanoid rigs | Workable, fewer assets |
| Asset store (interiors, characters) | Huge | Small |
| C# depth | Native, mature | Officially supported, smaller ecosystem |
| Console porting later | First-party paths | Harder |

**Decision: Unity 6 LTS + URP**, desktop-targeted.

**Escape hatch:** almost all gameplay logic lives in a **pure C# core library with zero `UnityEngine` references** (§2). If the engine choice ever changes, `Assets/_Project/Core/` and `Tests/Core.Tests/` move unchanged; only presentation layers are rewritten. This is the single most valuable architectural decision in the document and it costs about one day of setup.

---

## 2. Solution layout

```
FootbalSim/                      (repo root)
├─ README.md
├─ docs/
│  ├─ GDD.md
│  ├─ ARCHITECTURE.md
│  └─ ROADMAP.md
├─ unity/                        (Unity project root)
│  ├─ Assets/
│  │  ├─ _Project/
│  │  │  ├─ Core/               ← pure C#, NO UnityEngine
│  │  │  │  ├─ FootbalSim.Core.asmdef
│  │  │  │  ├─ Time/            GameTime, IClock, Calendar
│  │  │  │  ├─ Attributes/      AttributeId, AttributeSet, PositionProfile
│  │  │  │  ├─ Needs/           NeedId, NeedState, NeedsModel
│  │  │  │  ├─ Economy/         Wallet, LedgerEntry
│  │  │  │  ├─ Training/        DrillResult, ProgressMath
│  │  │  │  ├─ News/            NewsEvent, NewsComposer, NewsTemplate
│  │  │  │  ├─ Match/           MatchSimulator, MatchMoment
│  │  │  │  ├─ Save/            SaveGame, SaveMigration
│  │  │  │  └─ Common/          Result, GameMath, IRandomSource, IEventBus
│  │  │  ├─ Data/               ← ScriptableObject definitions (Unity)
│  │  │  │  ├─ FootbalSim.Data.asmdef
│  │  │  │  ├─ Items/ Attributes/ Drills/ News/ Flats/ Cars/ Drinks/
│  │  │  ├─ Gameplay/           ← MonoBehaviours, services (Unity)
│  │  │  │  ├─ FootbalSim.Gameplay.asmdef
│  │  │  │  ├─ Player/ Interaction/ Needs/ Training/ World/ NPC/
│  │  │  ├─ Presentation/       ← UI, audio, cameras, VFX (Unity)
│  │  │  │  ├─ FootbalSim.Presentation.asmdef
│  │  │  ├─ Bootstrap/          ← scene composition root
│  │  │  │  ├─ FootbalSim.Bootstrap.asmdef
│  │  │  ├─ Scenes/  Prefabs/  Art/  Audio/
│  │  ├─ Tests/
│  │  │  ├─ EditMode/  (Unity Test Framework)
│  │  │  └─ PlayMode/
│  │  ├─ Settings/             URP, input actions, quality
│  │  └─ StreamingAssets/      localisation CSVs, balance CSVs
│  ├─ Packages/manifest.json
│  └─ ProjectSettings/
├─ tests/Core.Tests/            (plain .NET NUnit project)
│  └─ Core.Tests.csproj         links ../../unity/Assets/_Project/Core/**/*.cs
├─ .github/workflows/ci.yml
└─ .gitignore / .gitattributes
```

### Assembly dependency rules

```
Bootstrap ──► Presentation ──► Gameplay ──► Data ──► Core
     └──────────────┴──────────────┴──────────┘
```

- **Core** depends on nothing but the BCL. No `UnityEngine`, no `MonoBehaviour`, no `Mathf` (use the internal `GameMath`).
- **Data** depends on Core only; holds `ScriptableObject` definitions and conversion to Core models.
- **Gameplay** depends on Data; all runtime services and `MonoBehaviour` glue.
- **Presentation** depends on Gameplay; nothing in Gameplay may reference Presentation. UI listens through events.
- **Bootstrap** is the only assembly that knows all four; it is the composition root.

Enforce with asmdef references only (no "Auto Referenced" hacks), and a one-line CI check that `Core/` contains no `using UnityEngine`.

---

## 3. Core services

Seven services, all interfaces in Core, all implementations registered in Bootstrap.

| Service | Responsibility | Notes |
|---|---|---|
| `IClock` | Game time, tick, pause, time scale, day/hour events | Single source of truth for time |
| `INeedsService` | Read/modify the four needs, decay tick | Driven by `IClock` |
| `IAttributeService` | Attribute values + overall + change events | Owns `AttributeSet` |
| `IEconomyService` | Money, ledger, affordability checks | Every transaction is a `LedgerEntry` |
| `ITrainingService` | Validates a drill request, applies results, weekly multipliers | Pure math in Core |
| `ISaveService` | Capture/restore game state, slots, migrations | JSON on disk |
| `IEventBus` | Typed pub/sub between systems | Struct events, no allocations per publish |

DI: **VContainer** (fast, Unity-native) or a hand-rolled 80-line service locator. Recommendation: VContainer from M0 — retrofitting DI later is expensive, and VContainer's scene/`LifetimeScope` model maps cleanly onto per-interior scopes.

---

## 4. Data-driven by default

**Rule: no balance number may appear in a `.cs` file except as a default in a `[SerializeField]` for a designer to override.** Every tunable is a ScriptableObject or CSV row.

| Dataset | Asset type | Authoring |
|---|---|---|
| Attributes, display names, group, icon | `AttributeDefinition` SO | Once |
| Position weight profiles | `PositionProfile` SO | 4+ assets |
| Drill definitions (id, attribute, tiers, par times, gains) | `DrillDefinition` SO | 13 assets |
| Items (ingredients, meals, drinks, cars) | `ItemDefinition` SO | ~40 assets |
| News templates per tag/severity | `NewsTemplate` SO | ~25 assets |
| Flats, clubs, fixtures | SO | ~10 assets |
| Balance numbers for mass tuning | CSV in `StreamingAssets/Balance/` | Designers |

A `ContentDatabase` in Data loads all definitions at boot into dictionaries keyed by stable string ID (`"drill.finishing.one_on_one"`). Stable IDs, never enum ordinals, are what make saves survive content edits.

---

## 5. Time

```csharp
namespace FootbalSim.Core.Time
{
    public readonly record struct GameTime(int Day, int Hour, int Minute)
    {
        public const int MinutesPerHour = 60;
        public const int MinutesPerDay  = 24 * MinutesPerHour;

        public int TotalMinutes => Day * MinutesPerDay + Hour * MinutesPerHour + Minute;

        public static GameTime FromTotalMinutes(int total)
        {
            int day = total / MinutesPerDay;
            int rem = total % MinutesPerDay;
            return new GameTime(day, rem / MinutesPerHour, rem % MinutesPerHour);
        }

        public GameTime AddMinutes(int minutes) => FromTotalMinutes(TotalMinutes + minutes);
        public override string ToString() => $"Day {Day} {Hour:00}:{Minute:00}";
    }

    public interface IClock
    {
        GameTime Now { get; }
        float TimeScale { get; set; }   // 0.5 = slow, 2 = fast; 0 while paused
        bool IsPaused { get; set; }

        void Advance(int gameMinutes);  // instant skip (sleep)

        event Action<GameTime> MinuteChanged;
        event Action<GameTime> HourChanged;
        event Action<GameTime> DayChanged;
    }
}
```

Implementation maps `Time.deltaTime × TimeScale × minutesPerSecond` into whole minutes, fires events, and **never advances while `IsPaused`** or outside `FreeRoam`. Sleep calls `Advance()`, not a coroutine, so a 10-hour skip is instantaneous and testable.

---

## 6. Key systems

### 6.1 Needs

Pure model in Core; a thin `NeedsService` MonoBehaviour ticks it.

```csharp
public enum NeedId { Energy, Hygiene, Bladder, Intoxication }

public sealed class NeedState
{
    public float Value { get; private set; }
    public float Max   { get; }
    public NeedState(float max, float initial) { Max = max; Value = initial; }

    public void Add(float delta) => Value = GameMath.Clamp(Value + delta, 0f, Max);
    public float Normalized => Value / Max;
}

public sealed class NeedsModel
{
    private readonly Dictionary<NeedId, NeedState> _needs;
    private readonly NeedsTuning _tuning;   // per-hour decay, per-activity costs

    public void Tick(int gameMinutes)
    {
        float hours = gameMinutes / 60f;
        foreach (var (id, state) in _needs)
            state.Add(_tuning.DecayPerHour[id] * hours);
    }

    public void ApplyActivity(ActivityCost cost) { /* training, match, drink, meal */ }
}
```

Balance lives in `NeedsTuning` (a Core POCO populated from an SO). The tick is **idempotent for a given number of minutes**, which makes it trivially testable: 60 ticks of 1 minute == 1 tick of 60 minutes (within float tolerance; use a tolerance of 0.01 in tests).

### 6.2 Interaction

```csharp
public readonly record struct InteractionPrompt(string Key, string Verb, bool Enabled);

public interface IInteractable
{
    string DisplayName { get; }
    bool CanInteract(PlayerContext ctx);
    InteractionPrompt GetPrompt(PlayerContext ctx);   // localisation key + verb
    void Interact(PlayerContext ctx);                 // primary (E)
    void SecondaryInteract(PlayerContext ctx);        // optional (G); default no-op
}

public interface IInteractor
{
    IInteractable Current { get; }
    void Tick(IReadOnlyList<IInteractable> candidates);
}
```

Selection: filter by `CanInteract`, then by distance ≤ 2.5 m and dot ≥ cos(45°), then rank by `dot² / distance`. The player controller holds one `IInteractor`; the HUD only reads `Current` and renders its prompt. Adding a new interactive object never touches the controller or the HUD.

`SecondaryInteract` is a default interface method returning nothing, so the bed is the only object that overrides it (Sleep vs Rest) — exactly the source's `E` / `G`.

### 6.3 Training pipeline

```csharp
public readonly record struct DrillResult(
    DrillId Drill,
    float Score01,          // 0..1
    int Reps,
    float DurationSeconds,
    string FailureReason = null);

public static class ProgressMath
{
    public const float BaseGain   = 8f;
    public const float ScoreBonus = 32f;
    public const float PointsPerAttribute = 100f;

    public static float Headroom(float attribute) =>
        MathF.Pow(Math.Clamp(1f - attribute / 99f, 0f, 1f), 1.5f);

    public static float Gain(
        float currentAttribute, float score01, float fatigue01,
        float coachBonus, float weeklyMultiplier)
    {
        float fatigue = 1f - 0.65f * Math.Clamp(fatigue01, 0f, 1f);  // 1.00 → 0.35
        float raw = (BaseGain + ScoreBonus * Math.Clamp(score01, 0f, 1f))
                    * Headroom(currentAttribute) * fatigue
                    * coachBonus * weeklyMultiplier;
        return score01 < 0.25f ? 0f : raw;
    }
}
```

The minigame scene **never** touches attributes. It produces a `DrillResult`; `ITrainingService` validates (does the player have the energy? is the coach present? has the weekly cap been hit?), applies it, raises `AttributeChanged`, and logs a `LedgerEntry`-style training record for the weekly report. This keeps 13 minigames from each re-implementing progression math.

### 6.4 Economy

```csharp
public readonly record struct LedgerEntry(GameTime When, int Amount, string ReasonKey, string SourceId);

public interface IEconomyService
{
    int Balance { get; }
    bool CanAfford(int amount);
    bool TrySpend(int amount, string reasonKey, string sourceId = null);
    void Earn(int amount, string reasonKey, string sourceId = null);
    IReadOnlyList<LedgerEntry> Today { get; }
    event Action<int> BalanceChanged;
}
```

Every transaction is journaled — the journal feeds the TV bulletin ("spotted leaving the bar", "arrives in style") and a debug screen. No `PlayerPrefs`, no direct `Money -= x` anywhere.

### 6.5 Save system

```csharp
[Serializable]
public sealed class SaveGame
{
    public int SchemaVersion = 1;
    public string CareerName;
    public long CreatedUtcTicks;
    public int PlaytimeSeconds;

    public GameTimeSave Time;
    public NeedsSave Needs;                       // need id → value
    public Dictionary<string, float> Attributes;  // stable attribute id → value
    public Dictionary<string, int> Inventory;     // item id → qty + freshness list
    public int Balance;
    public string FlatId, ClubId;
    public List<string> OwnedCarIds;
    public List<string> WorldFlags;               // "met_coach", "tutorial_done"
    public List<NewsSave> NewsLog;
    public int Reputation;
}
```

Rules:

1. **Versioned + migrated.** `SaveMigration.Migrate(json, fromVersion, toVersion)`. A save that cannot be migrated loads nothing and shows a clear message — it never half-loads.
2. **Atomic writes.** Write to `save_1.tmp`, flush, then replace. Never truncate the live file.
3. **Three slots + one autosave**, autosave on sleep and on returning home.
4. **Stable string IDs everywhere** (`"attr.finishing"`, `"item.kelle_paca"`). Renaming an enum member must not break a save.
5. **Never persist derived values** (overall, needs bar colours, computed prompts).
6. Serializer: `System.Text.Json` with a source-generated context (AOT-safe, fast) over `JsonUtility`, which cannot handle dictionaries.
7. Save path: `Application.persistentDataPath/Saves/`; dev builds also write a human-readable `debug_save.json`.

### 6.6 Match simulation (Option B from GDD §5.6)

```csharp
public sealed class MatchSimulator
{
    public MatchResult Simulate(MatchSetup setup, IRandomSource rng)
    {
        // 1. Team strength from squad ratings + form
        // 2. Generate a timeline of ~10 MatchMoment (minute, type, context)
        // 3. Resolve each moment: player-controlled ones are returned as Pending
        // 4. Fill non-player moments from attribute-weighted rolls
    }
}

public enum MomentType { OneOnOne, Header, LongShot, ThroughPass, Tackle, Cross }
```

The simulator is pure Core: given a seed and a list of resolved player moments, it returns a full result. Determinism means a match can be replayed, tested, and unit-tested without a scene — and minigames supply the player's moments by reusing the drill components.

### 6.7 News composer

```csharp
public readonly record struct NewsEvent(
    string Tag, int Severity, string SubjectPlayer, string Club, float? Number);

public sealed class NewsComposer
{
    public IReadOnlyList<NewsLine> Compose(IReadOnlyList<NewsEvent> events, int maxLines = 4);
}
```

Templates are data (`{player}`, `{club}`, `{value}`, `{drink}` token substitution). The composer sorts by severity, drops duplicates, and guarantees at least one line if anything happened. Presentation layer renders it to TV, newspaper and phone from the same list — one source, three surfaces.

### 6.8 Randomness

`IRandomSource` wraps a seeded PRNG (xoshiro or `System.Random` with a stable algorithm). The seed is stored in the save. `UnityEngine.Random` is banned outside presentation-only VFX. Determinism is a testability feature, not a nicety.

---

## 7. Scene flow and player state

### 7.1 Control state machine

```
FreeRoam ──focus interactable──► Interacting ──► Dialogue ──► Minigame ──► Cutscene
   ▲              │                   │             │              │            │
   └──────────────┴───────────────────┴─────────────┴──────────────┴────────────┘
                          (all non-FreeRoam states: clock paused)
```

Implemented as an explicit `PlayerStateMachine` with `Enter/Exit/Tick` per state — not a boolean soup of `if (isMenu)`. Only `FreeRoam` reads movement input and lets `IClock` advance. Every state is a separate small class; adding the phone menu later is a new state, not a new `if`.

### 7.2 Scene composition

| Scene | Contents | Loaded |
|---|---|---|
| `Boot` | Bootstrap, services, `ContentDatabase`, save check | Always |
| `MainMenu` | Menu UI, cinematic camera | Single |
| `City` | Terrain, buildings' exteriors, doors, NPC paths | Additive |
| `Interior_<Name>` | One per interior: flat tiers ×3, market, bar, gallery, facility, gym | Additive |

Doors call `SceneFlowService.Enter(interiorId)` → fade → unload previous interior, load next, place the player at a named spawn point, restore the state machine. **No seamless open world in v1.0.** Target: interior transition under 1.5 s on the min-spec machine.

The player rig, HUD and services persist across transitions (in `Boot`, or via `DontDestroyOnLoad` on a single `Persistent` root). Only environment content swaps.

---

## 8. Project conventions

### 8.1 Code style

| Item | Rule |
|---|---|
| Language version | C# 9 (Unity 6 constraint), nullable annotations **enabled** in Core |
| Naming | `PascalCase` types/members, `_camelCase` private fields, `camelCase` locals/params |
| File name | Must equal the type name for MonoBehaviours/SOs; grouping allowed in Core |
| Namespaces | `FootbalSim.Core.*`, `FootbalSim.Data.*`, `FootbalSim.Gameplay.*`, `FootbalSim.Presentation.*` |
| Unity API use | No `FindObjectOfType`, no `GetComponent` in `Update`, no string-based tags/layers (use layer constants) |
| `Update` | Only on the controller, clock driver, and the interactor. Everything else is event-driven or ticked by a single `GameLoop` |
| Public API | `sealed` by default; interfaces where a test double is needed and nowhere else |
| Errors | No `try/catch` swallowing. Use `Result<T>` for expected failures (insufficient funds, locked door) |
| Comments | Explain *why*, not *what*. Every non-obvious balance formula cites its design section (`// GDD §4.4`) |

### 8.2 Quality gates

- Roslyn analyzers + `TreatWarningsAsErrors` in the Core test project.
- StyleCop/`.editorconfig` shared; CI fails on new warnings in Core.
- A `Core` compile must not reference `UnityEngine`: verified by a CI grep.
- Every new Core system ships with tests in the same PR.

### 8.3 Git

- `main` protected; work on feature branches; squash-merge with a descriptive title.
- Conventional commits: `feat:`, `fix:`, `data:`, `docs:`, `test:`, `chore:`.
- Unity: force **text** serialisation (YAML), visible meta files, `.gitattributes` with LFS for `*.fbx`, `*.png`, `*.wav`, `*.psd`.
- Never commit `Library/`, `Temp/`, `Logs/`, `obj/`, `Build*/`.

---

## 9. Testing strategy

| Layer | Framework | What it covers | Runtime |
|---|---|---|---|
| Core (fast) | NUnit via `dotnet test` | Progression math, needs tick, economy, save roundtrip + migration, news composer, match determinism | < 5 s, runs on every push |
| Unity EditMode | Unity Test Framework | SO conversion, content database IDs unique, prefab wiring | ~30 s |
| Unity PlayMode | UTF | One smoke test per scene: load, spawn, interact with the primary object, exit | ~3 min |
| Manual | Test plan per milestone | Feel, reading of prompts, tuning | Milestone gates |

The `tests/Core.Tests/Core.Tests.csproj` trick — referencing Core sources directly by glob — means gameplay logic is tested in seconds without launching Unity. This is why Core must stay engine-free.

```xml
<ItemGroup>
  <Compile Include="../../unity/Assets/_Project/Core/**/*.cs" />
</ItemGroup>
```

---

## 10. Performance budgets (min spec: GTX 1650 / 16 GB / R5 3600)

| Budget | Target |
|---|---|
| Frame time | ≤ 16.6 ms (60 fps) at 1080p |
| Interior draw calls | ≤ 300 |
| City block draw calls | ≤ 800 (occlusion + baked lightmaps) |
| Load to main menu | ≤ 8 s |
| Interior transition | ≤ 1.5 s |
| Save write | ≤ 100 ms (off the main thread) |
| HUD allocations | 0 B/frame after warm-up |
| GC spikes | ≤ 5 ms |

Techniques: baked lightmaps indoors, single-directional light outdoors, GPU instancing for repeated props, object pooling for prompts and toasts, Addressables for interiors and audio, `IL2CPP` + `Master` build for release, no per-frame LINQ/`string.Format` in gameplay code.

---

## 11. Third-party packages

| Package | Purpose | Risk |
|---|---|---|
| Input System | Rebinding, gamepad parity | Low |
| Cinemachine | Cutscene and menu cameras | Low |
| Timeline | Handshake/intro cutscenes | Low |
| DOTween (or PrimeTween) | UI and prompt animation | Low |
| TextMeshPro | All text, localisation | Low |
| Addressables | Interior/audio loading | Medium — adds build complexity |
| VContainer | DI | Low |
| System.Text.Json | Saves | Low |
| NUnit | Tests | Low |
| Odin Inspector (optional) | Designer tooling | Paid, optional |

**Deliberately not used:** any physics-heavy character controller asset, any dialogue framework (dialogues are 1–2 nodes), any ECS framework. The game is small; complexity must be earned.

---

## 12. CI/CD

`.github/workflows/ci.yml`:

1. `dotnet test tests/Core.Tests` — every push and PR (no Unity licence needed).
2. `game-ci/unity-test-runner` — EditMode + PlayMode, on PRs to `main`.
3. `game-ci/unity-builder` — Windows x64 IL2CPP build, uploaded as an artifact, nightly and on tags.
4. Cache `Library/` between runs; keep build minutes sane.
5. Release tags `v*` → draft GitHub release with the build zip.

A Unity licence must be stored as repository secrets (`UNITY_LICENSE`) — set up in M0, not at release time when it becomes an emergency.

---

## 13. Known technical risks

| Risk | Mitigation |
|---|---|
| 13 minigames = 13 bespoke input systems | One shared `DrillScene` template + reusable components (timing ring, aim reticle, cadence meter, balance wobble). Every drill is a composition, not a rewrite. |
| Interior count growth (8 interiors in v1.0) | Interiors are prefabs in Addressables loaded into one `Interior` scene; only `City` and `Boot` are real scenes. |
| Save corruption during rapid autosaves | Atomic write + single-flight lock + autosave only on safe transitions |
| Balance drift between CSV and SO | Single source: CSV imported to SO at build; SO edited only for one-offs |
| Determinism breaking on engine upgrade | Core RNG is our own, never `UnityEngine.Random` |
| Localisation strings hard-coded under deadline pressure | An editor validation test fails the build on any literal UI string outside the table |
| Solo-dev bus factor | Documents are the source of truth; every system has a section number referenced from code |

---

## 14. Build order for the first vertical slice (M1)

Proves the whole stack end-to-end in ~4 weeks:

```
Boot scene → services registered → MainMenu → name career modal → save created
  → flat interior loads → walk → look at bed → [E] Sleep 8h → clock advances 480 min
  → energy restored → walk to door → City loads → walk to facility
  → coach dialogue → drill select → Finishing minigame (T1) → DrillResult
  → attribute +1 toast → money ledger entry → walk home
  → couch → TV composes a bulletin from today's events
  → sleep → autosave → quit → Continue → state restored exactly
```

If that path works, every system in the design has been proven once. Everything after M1 is depth, not new architecture.

---

## 15. Folder-level definition of done

A system is "done" when: Core logic exists and is unit-tested, data is authored (not hard-coded), presentation is wired, save state is versioned, strings are localised, it survives a save/load roundtrip, and it appears in `docs/` with its section number. Anything less is "implemented", not "done".
