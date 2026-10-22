# FootbalSim — Technical Architecture

**Version:** 0.2 (matches GDD v0.2)
**Target:** Unity 6 LTS (6000.x), C# 9, .NET Standard 2.1
**Companion documents:** `GDD.md`, `ROADMAP.md`

---

## 0. What changed in v0.2

| Area | Change |
|---|---|
| Match | A full first-person match layer is now in scope: ball physics, dribbling, shooting, 21 AI players, rating tracking |
| Progression | `ProgressMath` replaced by `AttributeXpService` + `PlayerLevelService` |
| Calendar | A `ScheduleService` owns the 5–6 day week and the skip flow |
| Career | `TransferService`, `MarketValueCalculator`, `RelationshipService` |
| Scenes | Stadium/arena scenes added; per-country city kits |
| Performance | 22 animated characters per frame is now the dominant budget |
| Save | New schema fields for XP, level, schedule, contracts, relationships |

The v0.1 decisions that survive unchanged: Unity + URP, a **pure C# Core** with no `UnityEngine`, assemblies with a one-way dependency chain, data-driven balance, versioned saves, and a fast `dotnet test` suite.

---

## 1. Why Unity, and the escape hatch

| Requirement | Unity | Godot 4 (.NET) |
|---|---|---|
| First-person + ball physics | Strong | Strong |
| 22-character AI match | Mature NavMesh, animation, LOD | Workable, more manual |
| Cutscenes (handshake, intro) | Timeline + Cinemachine | Manual |
| ScriptableObject authoring | Best-in-class | Weaker inspector |
| Asset store (stadiums, kits, characters) | Huge | Small |
| C# depth | Native | Good, smaller ecosystem |
| Console later | First-party paths | Harder |

**Decision: Unity 6 LTS + URP**, PC-targeted.

**Escape hatch:** gameplay logic lives in `Core/` with **zero** `UnityEngine` references. If the engine ever changes, Core and its tests move unchanged; only presentation is rewritten. This costs a day at M0 and pays for itself the first time a system needs testing without launching the editor.

---

## 2. Solution layout

```
FootbalSim/
├─ docs/                          GDD, ARCHITECTURE, ROADMAP, SOURCE-TRANSLATION
├─ unity/
│  ├─ Assets/_Project/
│  │  ├─ Core/                    pure C#, no UnityEngine
│  │  │  ├─ Time/                 GameTime, IClock, Calendar, ScheduleService
│  │  │  ├─ Attributes/           AttributeId, AttributeSet, PositionProfile
│  │  │  ├─ Progression/          AttributeXpService, XpCurve, PlayerLevelService
│  │  │  ├─ Needs/                NeedId, NeedState, NeedsModel
│  │  │  ├─ Economy/              Wallet, LedgerEntry, Contract, MarketValue
│  │  │  ├─ Training/             DrillResult, XpTier, TrainingRules
│  │  │  ├─ Match/                MatchSetup, MatchState, RatingTracker, MatchSimulator
│  │  │  ├─ Career/               FixtureList, TransferOffer, TransferProbability, Relationships
│  │  │  ├─ News/                 NewsEvent, NewsComposer, NewsTemplate
│  │  │  ├─ Save/                 SaveGame, SaveMigration
│  │  │  └─ Common/               Result, GameMath, IRandomSource, IEventBus
│  │  ├─ Data/                    ScriptableObjects + ContentDatabase
│  │  ├─ Gameplay/
│  │  │  ├─ Player/               FirstPersonMotor, Interactor, PlayerStateMachine
│  │  │  ├─ Football/             BallController, DribbleController, ShootingController, MatchDirector
│  │  │  ├─ Ai/                   TeamAi, PlayerAgent, GoalkeeperAgent
│  │  │  ├─ Training/             DrillScenes, StationMinigames
│  │  │  ├─ World/                SceneFlowService, Doors, Npcs
│  │  │  └─ Services/             Unity implementations of every Core interface
│  │  ├─ Presentation/            HUD, menus, cutscenes, audio, VFX
│  │  ├─ Bootstrap/               composition root
│  │  ├─ Scenes/ Prefabs/ Art/ Audio/
│  │  ├─ Settings/                URP, input actions, quality
│  │  └─ StreamingAssets/         localisation CSVs, balance CSVs
│  ├─ Packages/manifest.json
│  └─ ProjectSettings/
├─ tests/Core.Tests/              NUnit, links Core sources, no Unity
├─ .github/workflows/ci.yml
└─ .gitignore / .gitattributes
```

### Assembly rules

```
Bootstrap ──► Presentation ──► Gameplay ──► Data ──► Core
```

- **Core**: BCL only. No `UnityEngine`, no `Mathf` (use `GameMath`), no `Time.deltaTime`.
- **Data**: SO definitions and conversion into Core models.
- **Gameplay**: MonoBehaviours and Unity service implementations.
- **Presentation**: UI/audio/camera; Gameplay must never reference it.
- **Bootstrap**: the only assembly that sees all four.

CI greps `Core/` for `using UnityEngine` and fails the build if found.

---

## 3. Core services

| Service | Responsibility |
|---|---|
| `IClock` | Game time, tick, pause, scale |
| `IScheduleService` | The week's active days, session slots, skip rules |
| `INeedsService` | Four needs and their decay |
| `IAttributeService` | Attribute values, overall, change events |
| `IAttributeXpService` | Per-attribute XP pools, cost curve, level-ups |
| `IPlayerLevelService` | Career XP, level, training multiplier |
| `IEconomyService` | Money and ledger |
| `ITrainingService` | Session rules, XP awards, repeat multipliers |
| `IMatchService` | Match setup, rating tracking, result finalisation |
| `ICareerService` | Fixtures, contracts, transfers, market value |
| `IRelationshipService` | Fans, coach, board, girlfriend |
| `ISaveService` | Slots, versioned JSON, migrations |
| `IEventBus` | Typed pub/sub |

DI: **VContainer**, with a root `LifetimeScope` in `Boot` and per-interior child scopes.

---

## 4. Data-driven balance

No balance number in a `.cs` file. Everything is a ScriptableObject or CSV under `StreamingAssets/Balance/`:

| Dataset | File |
|---|---|
| Attribute XP costs | `attribute_xp.csv` |
| Level curve | `levels.csv` |
| Drill definitions and XP tiers | `drills.csv` + `DrillDefinition` SO |
| Leagues, clubs, fixtures templates | `leagues.csv` |
| Transfer and wage factors | `transfers.csv` |
| News templates | `NewsTemplate` SO ×~40 |
| Items, drinks, cars, flats | SO + CSV |

Stable string IDs (`"attr.finishing"`, `"drill.t1.one_on_one"`, `"club.tr.galatasaray"`) — never enum ordinals. IDs are what make saves survive content edits.

---

## 5. Time and schedule

```csharp
public readonly record struct GameTime(int Day, int Hour, int Minute)
{
    public const int MinutesPerDay = 1440;
    public int TotalMinutes => Day * MinutesPerDay + Hour * 60 + Minute;
    public static GameTime FromTotalMinutes(int t) =>
        new(t / MinutesPerDay, t % MinutesPerDay / 60, t % 60);
    public GameTime AddMinutes(int m) => FromTotalMinutes(TotalMinutes + m);
}

public enum DayType { Off, Gym, Facility, Match }

public sealed class WeekSchedule
{
    public int WeekIndex { get; }
    public IReadOnlyList<DayType> Days { get; }        // exactly 7
    public bool HasEuropeanFixture { get; }
    public int ActiveDayCount => Days.Count(d => d != DayType.Off);
}

public interface IScheduleService
{
    WeekSchedule Current { get; }
    DayType Today { get; }
    bool IsBuildingOpen(string buildingId, DayType day, int hour);
    bool TrySkipToNextActiveDay(out GameTime wakeAt);
}
```

Rules encoded in Core, so they are testable without a scene:

| Day type | Buildings open |
|---|---|
| `Off` | Flat, market |
| `Gym` | Flat, market, gym, bar |
| `Facility` | Flat, market, facility, bar |
| `Match` | Flat, market, stadium, bar |

A week generator produces 2 gym + 2 facility + 1 match, upgrading to 2 matches when the club has a European fixture — the 5/6-day rule from the source. The generator is pure and seeded, so a fixture list can be asserted in tests.

---

## 6. Progression (v0.2)

### 6.1 Attribute XP

```csharp
public static class XpCurve
{
    /// GDD §3.3 — source: 65->66 = 1000, +250 per point above 65.
    public static int CostToNext(int attribute) =>
        Math.Max(250, 1000 + 250 * (attribute - 65));
}

public sealed class AttributePool
{
    public int Value { get; private set; }      // 1..99
    public int Xp { get; private set; }

    public AttributeGain Add(int xp)
    {
        Xp += xp;
        int gained = 0;
        while (Value < 99 && Xp >= XpCurve.CostToNext(Value))
        {
            Xp -= XpCurve.CostToNext(Value);
            Value++;
            gained++;
        }
        return new AttributeGain(gained, Xp, XpCurve.CostToNext(Value));
    }
}

public static class XpTier
{
    public static int FromScore(float score01) => score01 switch
    {
        < 0.40f => 50,
        < 0.60f => 100,
        < 0.80f => 150,
        < 0.95f => 200,
        _       => 250,
    };
}
```

Repeat multiplier and slot rules live in `TrainingRules` (Core), not in the drill scenes:

```csharp
public sealed class TrainingRules
{
    public const int SessionsPerTypePerWeek = 2;
    public const int SlotsPerSession          = 3;
    public const int MaxSameDrillPerSession   = 2;
    public const float RepeatBonusPerDay      = 1.02f;
    public const float RepeatBonusCap         = 1.50f;

    public float RepeatMultiplier(string drillId, int priorDaysTrained) =>
        MathF.Min(RepeatBonusCap, MathF.Pow(RepeatBonusPerDay, priorDaysTrained));
}
```

The drill scene produces a `DrillResult`; `ITrainingService` validates the slot, applies the multiplier and calls `AttributePool.Add`. **No minigame ever touches an attribute directly.**

### 6.2 Player level

```csharp
public static class LevelCurve
{
    /// GDD §3.5 — deliberately slow: 1000*(L-1)^2 + 500*(L-1)
    public static int TotalXpForLevel(int level) =>
        1000 * (level - 1) * (level - 1) + 500 * (level - 1);

    public static float TrainingMultiplier(int level) =>
        MathF.Min(2.00f, 1f + 0.04f * (level - 1));
}

public static class MatchXp
{
    public static int For(MatchResult r) =>
        (int)MathF.Max(0, 120f * (r.Rating - 5f))
        + (r.Win ? 120 : r.Draw ? 60 : 0)
        + 60 * r.Goals + 40 * r.Assists;
}
```

Career XP is **only** awarded from matches (`D17`), matching the source. Its only sink is the level, whose only effect is the training multiplier — a clean single-loop economy that cannot be exploited.

---

## 7. Match architecture (v0.2 — the big one)

### 7.1 Layers

```
MatchDirector (scene state machine)
 ├─ MatchRules            (time, halves, out-of-play, restarts)   [Core]
 ├─ RatingTracker        (events -> live rating)                 [Core]
 ├─ TeamAi x2            (formation, marking, pressing)          [Gameplay]
 │   ├─ PlayerAgent x10  (state machine + steering)
 │   └─ GoalkeeperAgent
 ├─ PlayerRig            (the human: motor, dribble, shoot, pass)
 └─ BallController       (custom ball physics)
```

`MatchDirector` is the only object that advances match time; `MatchRules` and `RatingTracker` are engine-free and unit-tested.

### 7.2 Ball

Do **not** drive the ball with a plain `Rigidbody` and hope. Use a hybrid:

- A kinematic `BallController` integrates velocity, gravity, Magnus (curve), bounce and roll friction itself in `FixedUpdate`.
- Collision with players is resolved by a sphere-vs-capsule test that also decides **possession** (`BallState.Loose / Controlled / InFlight`).
- A trigger collider reports goals/out-of-play to `MatchRules`.

```csharp
public sealed class BallController : MonoBehaviour
{
    public BallState State { get; private set; }
    public Vector3 Velocity { get; private set; }
    public PlayerAgent Owner { get; private set; }

    public void Kick(Vector3 velocity, float spin) { /* ... */ }
    public void GiveTo(PlayerAgent player)         { /* ... */ }
    public void Release(Vector3 velocity)          { /* ... */ }
}
```

Deterministic integration at a fixed 60 Hz step means replays and tests behave identically.

### 7.3 Dribbling and skill moves

```csharp
public sealed class DribbleController : MonoBehaviour
{
    // Combo buffer: direction presses with timestamps, matched against data
    private readonly Queue<DirectionPress> _buffer = new();

    public SkillMove Match(IReadOnlyList<SkillMoveDefinition> moves, float now);
}
```

Skill moves are data:

```json
{ "id": "move.inside_out", "sequence": ["S+D", "A+W"], "windowMs": 450,
  "durationMs": 420, "ballOffset": 0.9, "beatChance": 0.65 }
```

`S+D → A+W` (drag right, go left) is literally the source's example. New moves ship as data.

### 7.4 Player motor

First person, capsule collider, custom acceleration/braking (not raw `CharacterController.Move` with instant velocity), with three gaits (walk 2.2, run 5.0, sprint 7.6 m/s) and a ball-possession modifier. Sprint drains a **match stamina** pool that is separate from the day-level Energy need — a 90-minute match should not empty tomorrow.

### 7.5 AI

Keep it small and legible. Every outfield AI is a state machine over:

```
HoldShape → Press → Support → Mark → Recover
```

with three inputs: distance to ball, distance to formation slot, and team phase (attack/defence). Utility scores choose the state; steering (seek/arrive/separation) moves the body; the ball controller decides who may take possession.

**Do not build:** behaviour trees with 40 nodes, per-player pathfinding to the centimetre, or squad-wide tactical simulation. 10 agents per team, one formation per club, five states, is enough to *look* like football at 6-minute halves.

| Difficulty | Reaction delay | Positioning error | Tackle accuracy |
|---|---:|---:|---:|
| Amateur | 380 ms | ±2.5 m | 55% |
| Pro | 240 ms | ±1.4 m | 70% |
| Elite | 140 ms | ±0.7 m | 84% |

### 7.6 Rating tracker

```csharp
public sealed class RatingTracker
{
    private float _rating = 6.5f;                       // GDD §4.5
    public float Rating => Math.Clamp(_rating, 3f, 10f);

    public void Apply(MatchEventType type, MatchContext ctx)
    {
        _rating += type switch
        {
            MatchEventType.Goal          =>  0.90f,
            MatchEventType.Assist        =>  0.60f,
            MatchEventType.KeyPass       =>  0.15f,
            MatchEventType.PossessionLost=> -0.12f,
            MatchEventType.BigMiss       => -0.30f,
            MatchEventType.YellowCard    => -0.25f,
            MatchEventType.RedCard       => -0.90f,
            MatchEventType.OwnGoal       => -0.90f,
            _ => 0f
        };
        RatingChanged?.Invoke(Rating);
    }
}
```

Every match event is also a `NewsEvent` candidate, so a hat-trick reaches the TV the same evening.

### 7.7 Match time and skipping

Match days can be skipped (GDD §4.1). Skipping calls `MatchDirector.SimulateInstant()` — a pure-Core, attribute-weighted result with no presentation — which produces the same `MatchResult` shape as a played match. **One result type, two producers**, so downstream systems (rating, XP, news, table, transfers) never branch on "was it played".

---

## 8. Career

### 8.1 Fixtures

`FixtureService` generates a double round-robin per league from a seeded algorithm, inserts European midweek fixtures on a coefficient basis, and exposes `NextFixture`, `Table`, and `MatchDay`.

### 8.2 Transfers

```csharp
public readonly record struct TransferOffer(
    string ClubId, int Wage, int Years, int Fee, int SigningBonus, OfferOrigin Origin);

public enum OfferOrigin { ClubToPlayer, PlayerToClub }

public static class TransferProbability
{
    /// GDD §4.8
    public static float Accept(
        float marketValue, float clubExpectedValue, float expectedWage,
        float demandedWage, float reputationFactor)
    {
        float fit = Math.Clamp(marketValue / clubExpectedValue, 0.2f, 1.5f);
        float wage = Math.Clamp(1f - 0.5f * (demandedWage - expectedWage) / expectedWage, 0.10f, 1.10f);
        return Math.Clamp(0.55f * fit * wage * reputationFactor, 0.05f, 0.90f);
    }
}
```

**Wheel UI contract:** the RNG roll happens in `TransferService` **before** the wheel animates. The wheel receives `(probability, success)` and animates to that outcome. Never let the animation choose the result — that is untestable and feels rigged.

### 8.3 Relationships

```csharp
public enum RelationshipId { Fans, Coach, Board, Girlfriend }

public sealed class RelationshipService
{
    public void Apply(RelationshipId id, float delta, string causeId)
    {
        // clamped 0..100, logs the cause for the UI ("why did my coach dislike me?")
    }
}
```

Every delta stores its cause, so the Relationships screen can show *"−8 fans: your transfer approach to <club> was rejected"*. Unexplained stat drops are the fastest way to make a sim feel arbitrary.

---

## 9. Save system (v0.2 schema)

```csharp
public sealed class SaveGame
{
    public int SchemaVersion = 2;
    public string CareerName;
    public int PlaytimeSeconds;

    public GameTimeSave Time;
    public WeekScheduleSave Schedule;
    public Dictionary<string, float> Needs;
    public Dictionary<string, AttributeSave> Attributes;   // value + xp
    public int CareerXp, Level;
    public Dictionary<string, int> Inventory;
    public int Balance;
    public string FlatId, ClubId, LeagueId, CountryId;
    public List<string> OwnedCarIds;
    public ContractSave Contract;
    public Dictionary<string, float> Relationships;
    public List<TransferOfferSave> PendingOffers;
    public List<NewsSave> NewsLog;
    public List<string> WorldFlags;
    public List<FixtureSave> Fixtures;
}
```

Rules: versioned + migrated; atomic writes via temp file + replace; 3 slots + autosave; stable string IDs; **never persist derived values** (overall, cost-to-next, ratings history beyond what the UI needs). `SchemaVersion` 1 saves from v0.1 do not exist in the wild, so v0.2 starts at 2 and migration is exercised by tests from day one.

---

## 10. Scene flow

| Scene | Contents | Load |
|---|---|---|
| `Boot` | Services, content database, save check | always |
| `MainMenu` | Menu, cinematic camera | single |
| `City_<Country>` | Exteriors, doors, NPC paths | additive |
| `Interior_<Id>` | 8 interiors per city | additive |
| `Arena_<Club>` | Stadium, pitch, tunnel, crowd impostors | additive on match day |

Interiors are prefabs in Addressables loaded into one `Interior` scene — only `Boot`, `MainMenu` and `City` are real Unity scenes. A country change unloads `City_<Country>` and its interiors and loads the new kit.

Player control states:

```
FreeRoam ─► Interacting ─► Dialogue ─► Drill ─► Cutscene
     └────► Match (own state machine) ─────┘
```

Only `FreeRoam` and `Match` advance the clock; inside `Match`, the day clock is paused and match time runs instead.

---

## 11. Performance budgets (min spec GTX 1650 / 16 GB / R5 3600)

| Budget | Target |
|---|---|
| Frame time | ≤ 16.6 ms at 1080p |
| 22 players + ball | ≤ 8 ms total (animation + AI + physics) |
| Crowd | ≤ 1.5 ms (impostors, 1 draw call per stand section) |
| Interior draw calls | ≤ 300 |
| City draw calls | ≤ 800 |
| Interior transition | ≤ 1.5 s |
| Match load | ≤ 6 s |
| Save write | ≤ 100 ms, off main thread |
| GC spikes | ≤ 5 ms |

Techniques: GPU skinning + animation LOD (full rig < 15 m, half-rate 15–35 m, impostor beyond), 3 LODs per character, shared skeleton, Addressables, object pooling for VFX and prompts, no per-frame LINQ in gameplay code, `IL2CPP` release builds.

**Risk note:** 22 skinned characters is the budget item most likely to fail on low-end hardware. Decide the player-count LOD strategy at M5a, not during polish.

---

## 12. Testing strategy

| Layer | Framework | Covers | Time |
|---|---|---|---|
| Core | NUnit (`dotnet test`) | XP curve, level curve, training rules, schedule generation, transfer probability, rating tracker, save roundtrip, fixture generation | < 5 s |
| Unity EditMode | UTF | Content IDs unique, SO → Core mapping, prefab wiring | ~40 s |
| Unity PlayMode | UTF | Scene smoke tests, one drill contract per drill, a scripted 60-second match | ~5 min |
| Manual | Test plan | Match feel, drill fun, balance | per milestone |

The Core suite is the project's safety net: it runs without a Unity licence, on every push, in seconds.

```xml
<ItemGroup>
  <Compile Include="../../unity/Assets/_Project/Core/**/*.cs" />
</ItemGroup>
```

---

## 13. Packages

| Package | Purpose | Risk |
|---|---|---|
| Input System | Rebinding, gamepad | Low |
| Cinemachine | Menus, cutscenes, match camera | Low |
| Timeline | Intro, handshake, season-end | Low |
| DOTween / PrimeTween | UI motion | Low |
| TextMeshPro | All text | Low |
| Addressables | Interiors, arenas, audio | Medium |
| VContainer | DI | Low |
| System.Text.Json | Saves | Low |
| NUnit | Tests | Low |
| Unity Cinemachine + Physics | — | built-in |

**Not used:** ECS/DOTS (team of 22 does not need it), any football framework, any dialogue framework.

---

## 14. CI

1. `dotnet test tests/Core.Tests` on every push.
2. `game-ci/unity-test-runner` (EditMode + PlayMode) on PRs to `main`.
3. `game-ci/unity-builder` Windows IL2CPP artifact nightly and on tags.
4. Cache `Library/`; store `UNITY_LICENSE` as a secret at M0.
5. Steam: `steamcmd` upload step behind a manual approval, from M7.

---

## 15. Technical risks introduced by v0.2

| Risk | Impact | Mitigation |
|---|---|---|
| Match gameplay overruns everything | Fatal | Fidelity ladder, 12-week cap, M5 ends with "good enough", not "perfect" |
| Ball physics feels wrong | High | Custom kinematic ball from day one, 60 Hz fixed step, tuning harness scene |
| 22 characters tank performance | High | LOD/impostor strategy in M5a; cap to 11v11 only, no subs rendering |
| AI looks like headless chickens | High | Five states, one formation, heavy tuning time budgeted (2 weeks) |
| Drill and match diverge into two control systems | High | **One** ball, **one** motor, **one** shooting/passing component, used by both |
| Save schema churn during M5–M6 | Medium | Version bump + migration test per change |
| Ten leagues of content | High | 4 city kits, data-only leagues (`D13`) |
| Steam launch surprises | Medium | Steamworks integration starts M7, not M9 |

---

## 16. Definition of done

Core logic tested, data authored (not hard-coded), presentation wired, save state versioned, strings localised, survives a save/load roundtrip, documented with a section reference. Anything less is "implemented", not "done".
