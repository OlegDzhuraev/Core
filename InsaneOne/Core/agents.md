# agents.md — InsaneOne.Core

> Language convention: this file is always written in English, regardless of the language used
> in the conversation that edits it.

## What this repository is

`InsaneOne.Core` is a Unity mini-framework/tooling library — a set of reusable utilities and
architectural primitives that speed up game development: DI/service locator, a scene context
system, localization, an audio wrapper, UI widgets, effects, a teams mechanic, extension
methods (`Extensions`), and editor tooling such as a code template generator and a project
setup window.

This is **not a standalone application** — the repository is meant to be dropped in as source
inside a Unity project's `Assets/` folder (see `CoreData.GitPackages` — a list of sibling
`InsaneOne.*` packages installed the same way). There's no root `package.json` — the build
depends entirely on the Unity Editor and its `.asmdef` files.

## Structure

```
Sources/
  Architect/           Context<T>, ServiceLocator, IInitService — architectural scaffolding
    Injection/          separate asmdef with no dependencies — lightweight DI container
    ServiceLocator/      static service locator + [Locate] field injection
  Audio/                Audio (static player), AudioData/AudioGroupData, SoundMixer
  Components/           ready-made MonoBehaviour components (Interactable, TeamBehaviour, ...)
  Data/                 CoreData — ScriptableObject holding package settings
  Editor/               EditorWindow tools, custom drawers, template generator
  Effects/              sprite animation, camera shake, line renderer, etc.
  Extensions/           static extension methods (Transform, Color, Physics, Random...)
  Goals/                achievements/objectives: GoalsConfig asset, GoalsService (json saves), GoalNotificationPopup
  LevelDesign/           gizmo handles + an Editor/ subfolder with its own asmdef
  Localization/         CSV-based localization + Localization/UI (LocalizedTMPText)
  Teams/                team mechanic, enabled via the INSANE_TEAMS_EXTENSION flag
  UI/                   custom UI widgets (Panel, Fader, TabControl, LayeredSelectable/...)
  Utility/              Timer, PauseUtility, GridUtility, logger, etc.
Tests/                  NUnit tests, separate asmdef, compiled only with UNITY_INCLUDE_TESTS
Templates/              .txt code templates for TemplatesGenerator (new script scaffolding)
Resources/, Materials/, Shaders/   package assets
```

## Assembly Definitions

Several separate `.asmdef` files — when adding new files, make sure they land in the right
assembly and don't grow its dependencies unnecessarily:

- `InsaneOne.Core` (root) — the main assembly, references `Unity.TextMeshPro`,
  `InsaneOne.PerseidsPooling`, `DOTween.Modules`.
- `InsaneOne.Core.Injection` (`Sources/Architect/Injection`) — **intentionally has no
  references**, pure reflection-based DI, doesn't even depend on `InsaneOne.Core`. Don't add
  Unity-specific code here unless strictly necessary (see the `#if UNITY_5_3_OR_NEWER` guards
  already used carefully in `Container`/`UnityContainer` and their neighbors).
- `InsaneOne.Core.Editor` (`Sources/Editor`) — `includePlatforms: ["Editor"]`, references
  `InsaneOne.Core`.
- `InsaneOne.Core.LD.Editor` (`Sources/LevelDesign/Editor`) — separate editor assembly for
  level design tooling.
- `InsaneOne.Core.Tests` (`Tests/`) — `NUnit`, `defineConstraints: ["UNITY_INCLUDE_TESTS"]`,
  `autoReferenced: false`.

## Code conventions

- **Namespace ≠ file path.** For example, `Sources/Teams/*` lives in `InsaneOne.Core` (not
  `InsaneOne.Core.Teams`), `Sources/Localization/*` is in `InsaneOne.Core.Locales`, and
  `Sources/Architect/ServiceLocator/*` is in `InsaneOne.Core.Architect`. Before creating a new
  file, check the `namespace` of neighboring files in the same folder rather than deriving it
  from the path.
- **Allman brace style** (opening `{` on its own line), tab indentation (some older files use
  spaces/inconsistent indentation — don't carry that into new code, use tabs as the baseline
  style).
- Public members of static utilities and extension methods almost always carry a one-line
  `/// <summary>...</summary>`; multi-line XML doc blocks aren't used.
- Heavy use of expression-bodied methods and properties (`=>`) where the body is a single
  statement.
- Private fields — `camelCase` without an underscore prefix; constants — `PascalCase`
  (e.g. `const int MaxSourcesInLayer`).
- Member ordering within a class: constants, then static members, then events, then public
  properties, and only after those — internal/private fields and `[SerializeField]` fields.
- Static singleton-style classes (`Audio`, `Localization`, `PauseUtility`, `ServiceLocator`,
  `Context<T>`) are the main architectural pattern for "global" systems; each one has an
  explicit `Init`/`Initialize` and often a `Reset`/`Dispose`/`Unload` to clear state between
  game runs or tests.
- Static systems holding runtime state (`Audio`, `Localization`, `PauseUtility`,
  `ServiceLocator`) also reset themselves on play-mode start via
  `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`, so they
  stay correct when the consuming project uses Enter Play Mode Options with domain reload
  disabled — follow the same pattern when adding a new static system.
- Runtime components use the legacy `Input` class — consuming projects on the new Input
  System package need Active Input Handling set to "Both" (or "Input Manager (Old)").
- Runtime logs go through `CoreUnityLogger` (`CoreUnityLogger.I.Log(msg, LogLevel.Warning)`),
  which prefixes messages with `[InsaneOne.Core]` and respects the CoreData `SuppressLogs`
  setting; pass a context object (`..., LogLevel.Warning, this`) to ping it from the Console.
  Exceptions: `ServiceLocator` (must compile without Unity — uses `UnityEngine.Debug` inside
  `#if UNITY_5_3_OR_NEWER`) and editor tooling under `Sources/Editor` / `Sources/LevelDesign/Editor`.
- Optional features are compiled via `#if INSANE_<FEATURE>` scripting define symbols — that's
  how the entire `Teams` module is gated (`INSANE_TEAMS_EXTENSION`). When adding a new optional
  subsystem, follow the same pattern rather than a runtime flag.
- Some code (mostly in `Injection`) supports running outside Unity via
  `#if UNITY_5_3_OR_NEWER` — don't break this conditional compilation when touching those files.
- `// todo InsaneOne.Core: ...` comments are used as explicit tech-debt markers — don't
  touch/remove them in passing without the user asking.

## SerializeReference Migrator AI bridge

`Sources/Editor/SerializeReferenceMigratorAiBridge.cs` lets an AI agent drive the
SerializeReference Migrator while the Unity Editor has the project open — no batch mode and
no editor restart needed. The bridge (auto-loaded via `[InitializeOnLoad]`) polls a command
file and answers with a result file, both under `Library/InsaneOne/SerializeReferenceMigrator/`
(never imported, never committed):

```
Library/InsaneOne/SerializeReferenceMigrator/command.json   <- agent writes a command here
Library/InsaneOne/SerializeReferenceMigrator/result.json    <- editor writes the answer here
```

Flow for the agent: write a command with a unique `id` into `command.json`, then poll
`result.json` until its `id` matches — `command.json` is picked up within ~0.5s and deleted
once processed, so it never re-runs. Supported commands:

- `{"id":"1","command":"ping"}` — health check, replies with `editorVersion`.
- `{"id":"2","command":"scan"}` — lists every broken SerializeReference type found in the project
  (ScriptableObject assets, prefab components and scene components):
  `entries: [{assetPath, referenceId, oldAssemblyName, oldNamespaceName, oldClassName, holder}]`,
  where `holder` is "ComponentName on 'GameObjectName'" for prefab/scene entries, empty for assets.
- `{"id":"3","command":"apply","fixes":[{"assetPath":"Assets/X.asset","referenceId":1000,"className":"New","namespace":"New.Ns","assembly":"New.Asm"}]}`
  — remaps the listed references to their new types and returns `appliedCount`, `failedCount`,
  `errors`, and `entries` = the remaining broken references after a re-scan (use it to verify).
  Scenes that are currently open in the editor are reported in `errors` instead of being patched
  (close the scene first, otherwise the next scene save would overwrite the fix).

`referenceId` values come from a previous scan result; assets must be text-serialized (Force
Binary projects get an error in `errors`). The scan/apply logic itself lives in the headless
static core `SerializeReferenceMigrator` (unit-tested in `Tests/Editor`) — the EditorWindow and
the AI bridge are both thin wrappers around it, so they always stay in sync.

Operational notes: if `result.json` never picks up your `id`, the editor is most likely not
running, is unfocused (it refreshes/compiles only when focused — new scripts, including bridge
changes, need one focus to load), or has compile errors in the Console. Ask the user to focus
the Unity window and retry. A `ping` first is the cheapest way to check the bridge is alive
(also use it after any script change to confirm the editor recompiled successfully). The
bridge works on the main thread inside the editor, so scans of large projects can take a few
seconds to answer. Scenes located inside `Packages/` can't be opened by EditorSceneManager
and are skipped with an Info log; `apply` also refuses assets inside packages (patching them
in the consuming project would be lost on the next package update).

## Tests

- `Tests/*.cs` — NUnit (`[TestFixture]`/`[Test]`), only runnable inside the Unity Test Runner
  (there's no `dotnet test`/CLI runner in the repo). There's no headless CI script — if tests
  need to run, do it through the Unity Editor (Window → General → Test Runner) or `-runTests`
  batch mode, if that's available in the user's environment.
- Tests touching static singletons (`ServiceLocator`, `Localization`) call `Reset()` or the
  equivalent at the start — don't forget this in new tests, or state will leak between tests.

## Verifying changes

There's no `.sln`/`.csproj` and no CI config in the repo — the project can't be built outside
Unity. After making changes:
- Verify correctness by manual review (there's no build available at hand).
- If you change a `.asmdef` — make sure dependencies don't create cycles and editor-only code
  doesn't leak into a runtime assembly.
- Don't delete `.meta` files or rename files without accounting for their `.meta` counterparts
  — Unity ties GUIDs to `.meta` files, and losing/desyncing them breaks references in scenes
  and prefabs.

## Commits

History uses bracketed prefixes: `[+]` new feature, `[*]` change/improvement to existing code,
`[!]` bug fix, `[-]` removed feature or old code. Follow this format when writing commit messages in this repository.
