---
name: level-design-editor-tools
description: Build or extend Unity Editor tools for level-design object placement/painting in InsaneOne.Core (Object Placer, Transform Randomizer and similar Scene-view scatter/brush tools). Use when adding a new placement/scatter/painting EditorWindow, wiring a Scene-view click-to-place interaction, or extending Sources/LevelDesign/Editor tooling.
---

# Level Design Editor Tools (InsaneOne.Core)

Playbook for building/extending Scene-view interactive placement tools in this repo — distilled
from `ObjectPlacerWindow` and its section classes under `Sources/LevelDesign/Editor/`. Read
[agents.md](../../../agents.md) first for repo-wide conventions (Allman braces, tabs, namespace ≠
path, no build available — verify by careful manual review).

## Where code lives

- `Sources/LevelDesign/` — runtime-safe pieces (e.g. `GizmoHandle.cs`), compiled into the root
  `InsaneOne.Core` assembly.
- `Sources/LevelDesign/Editor/` — everything Editor-only, compiled into
  `InsaneOne.Core.LD.Editor` (`includePlatforms: ["Editor"]`, no references by default — only add
  a reference if you actually need runtime `InsaneOne.Core` types).
- Namespace `InsaneOne.Core.LevelDesign` for new Editor-only LD tooling (check neighboring files
  before assuming — namespace is not derived from the folder path in this repo).
- Menu items under `Tools/InsaneOne/Level Design/<Tool Name>...`.
- A one-paragraph mention belongs in the git repo's root `README.md` under `## Level Design
  Tools`, matching the terse existing entries. This `Sources/` tree lives a few folders *inside*
  the actual repo root (confirm with `git rev-parse --show-toplevel` rather than assuming a fixed
  `../` count — the nesting depth is a local checkout quirk, not a stable convention).

## UI architecture: EditorWindow composed of section VisualElements

Don't build one monolithic `CreateGUI()`. Split the window into small `VisualElement` subclasses,
one per logical UI block (see `ObjectPlacerGeneralSection`, `ObjectPlacerPaletteSection`,
`ObjectPlacerPlacementSettingsSection`):

- Each section builds its own controls in its constructor, adds the shared `group-box` USS class
  (from `Resources/InsaneOne/ToolsStyles.uss`) to itself, and exposes only read-only public
  properties (`IsToolActive`, `LayerMask`, `SelectedEntry`, ...) — never raw control references.
- The `EditorWindow` (`CreateGUI`) just loads the shared stylesheet, instantiates each section,
  and composes them. It owns cross-cutting logic (Scene-view interaction, placement) that reads
  from section properties, not the other way around.
- Conditional sub-fields (e.g. a range slider that only matters when its toggle is on) use a
  `RegisterValueChangedCallback` on the toggle to flip `style.display` between
  `StyleKeyword.Auto`/`StyleKeyword.None` via a small `GetDisplay(bool)` helper — see any
  `Randomize *` toggle pair for the pattern.
- Wrap the composed sections in a `ScrollView(ScrollViewMode.Vertical)` so the window content
  scrolls instead of clipping when the window is short.

## Session persistence (survive domain reload, not Editor restart)

Every configurable field should persist across domain reload within the same Editor session —
use `SessionState`, not plain fields or `EditorPrefs` (the latter survives Editor restarts and is
global, not per-session):

- Primitives: `SessionState.Get/SetBool|Int|Float|String(key, default)`. Key convention:
  `"InsaneOne.<ToolName>.<FieldName>"`, one `const string ...Key` per field, declared next to the
  section's other consts.
- Project asset references (e.g. an `ObjectPalette` SO): can't go into `SessionState` directly —
  persist the asset's GUID (`AssetDatabase.AssetPathToGUID`/`GUIDToAssetPath` +
  `LoadAssetAtPath<T>`), not the object itself.
- Scene object references (e.g. an auto-parent `Transform`): assets don't have a GUID for scene
  objects — use `GlobalObjectId.GetGlobalObjectIdSlow`/`GlobalObjectIdentifierToObjectSlow`
  instead (see `ObjectPlacerGeneralSection.SaveParent`/`LoadPersistedParent`).
- Cross-window shared state (e.g. a tool's on/off flag also driven by a Scene-view overlay
  toggle) belongs in its own small static class with the state backed directly by
  `SessionState` and a `static event Action<bool> Changed` for two-way sync — see
  `ObjectPlacerToolState`. Don't duplicate the flag as a plain field in more than one place.
- On load, always restore via `SetValueWithoutNotify` so restoring state doesn't re-trigger the
  same save/side-effect callback.

## Scene-view click interaction (place-on-click without stealing the camera or Selection)

This is the fiddly IMGUI part — copy the shape of `ObjectPlacerWindow.OnSceneGUI`:

1. Subscribe in `OnEnable`/unsubscribe in `OnDisable`: `SceneView.duringSceneGui += OnSceneGUI;`.
   The Scene-view interaction only runs while the tool's window is open.
2. Each call: `var controlId = GUIUtility.GetControlID(FocusType.Passive);` then, only on
   `EventType.Layout`, `HandleUtility.AddDefaultControl(controlId);` — this makes your control the
   fallback pick when no other control claims the event, without out-competing real handles.
3. Raycast every event: `HandleUtility.GUIPointToWorldRay(e.mousePosition)` +
   `Physics.Raycast(...)` against the tool's layer mask.
4. Compute `eventType = e.GetTypeForControl(controlId);` and branch on it (not `e.type` directly)
   for Mouse events, so you only react when your control is actually the eligible one.
5. On `MouseDown` with `e.button == 0 && !e.alt && !e.control && !e.command`: claim
   `GUIUtility.hotControl = controlId;`, do the placement, `e.Use();`. The modifier checks matter:
   plain left-click is the only thing you intercept, so alt-drag (orbit), right-drag (look),
   middle-drag (pan) and scroll (zoom) all pass through untouched.
6. On `MouseDrag` with `GUIUtility.hotControl == controlId` (for drag-scatter/brush tools): repeat
   placement gated by a minimum spacing distance, `e.Use();`.
7. On `MouseUp` with `GUIUtility.hotControl == controlId`: release `GUIUtility.hotControl = 0;`,
   `e.Use();`.
8. Claiming `hotControl` and calling `e.Use()` on the matched events is what suppresses Unity's
   default click-to-select — don't try to fight it any other way (no `Selection.activeObject`
   tricks needed).
9. Call `sceneView.Repaint()` on `EventType.MouseMove` so preview visuals (see below) update live
   even though nothing forces a Scene-view repaint by default.

## Preview visuals: `Handles`, not custom IMGUI

Draw all live previews with `UnityEditor.Handles`, sized independent of camera distance via
`HandleUtility.GetHandleSize(point) * k`:

- Point marker: `Handles.color = ...; Handles.SphereHandleCap(0, point, Quaternion.identity, size, EventType.Repaint);`
  — pass literal `EventType.Repaint` (not the real current event) to force-draw regardless of
  which event is currently being processed; pass controlID `0` since this is a pure visual, not
  an interactive handle.
- Direction/normal indicator: `Handles.color = Color.cyan; Handles.DrawLine(point, point + normal * length);`.
- Radius/area preview (e.g. scatter jitter radius): `Handles.color = <low-alpha color>; Handles.DrawSolidDisc(point, normal, radius);`.
- Pick colors that communicate state, e.g. blue when the current action is valid, red when it
  isn't (no palette/entry selected, slope too steep, etc.) — recompute the color from the same
  gating booleans used to decide whether a click would actually place something.

## Tool activation toggle: Scene-view Overlay, not a Handles-drawn button

To add a Scene-view on/off toggle "the way Unity itself does it" (Gizmos, 2D, Camera overlays),
use the `UnityEditor.Overlays`/`UnityEditor.Toolbars` API, not an IMGUI button drawn via
`Handles.BeginGUI`:

```csharp
[Overlay(typeof(SceneView), Id, "Display Name", defaultDisplay: true)]
public class MyToolOverlay : ToolbarOverlay
{
    public const string Id = "InsaneOne.LevelDesign.MyToolOverlay";
    MyToolOverlay() : base(MyToolToggle.Id) { }
}

[EditorToolbarElement(Id, typeof(SceneView))]
class MyToolToggle : EditorToolbarToggle { /* bind to the shared SessionState-backed tool state, see ObjectPlacerToolOverlay.cs */ }
```

Bind the toggle to the same shared static state class the window's own toggle uses (see Session
persistence above), so both stay in sync regardless of which one the user clicks.

## Undo grouping

- Every spawned object: `Undo.RegisterCreatedObjectUndo(instance, "Place Prefab");` right after
  `PrefabUtility.InstantiatePrefab`.
- For a multi-object gesture (drag-scatter, brush stroke): wrap the whole gesture so one gesture
  = one undo step — `Undo.IncrementCurrentGroup(); Undo.SetCurrentGroupName("...");` at gesture
  start (`MouseDown`), `Undo.CollapseUndoOperations(groupIndex)` at gesture end (`MouseUp`). Do
  this unconditionally (even for a plain single click) — collapsing a group with one operation is
  harmless and still gives it a clean, consistent name in the Undo History window.

## Reference files

`Sources/LevelDesign/Editor/ObjectPlacerWindow.cs`, `ObjectPlacerGeneralSection.cs`,
`ObjectPlacerPaletteSection.cs`, `ObjectPlacerPlacementSettingsSection.cs`,
`ObjectPlacerToolState.cs`, `ObjectPlacerToolOverlay.cs`, `ObjectPalette.cs` are the canonical
example implementing every pattern above end to end.
