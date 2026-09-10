# EditorEvent vocabulary - research brief

**Date:** 2026-04-28
**Source:** Facepunch/sbox-public (local clone)
**Confidence:** High overall; per-event mappings: high; subscription mechanics: high.

## EditorEvent class

**Location:** `engine/Sandbox.Tools/Events/EditorEvent.cs` (156 lines)

Public API:

- `static void Init()` (line 18) - initialize event manager, called once at startup
- `internal static void RegisterAssembly(Assembly incoming)` (line 38) - register a loaded assembly for event discovery
- `internal static void UnregisterAssembly(Assembly outgoing)` (line 29) - unregister; orphaned instances cached for re-registration
- `static void Register(object obj)` (line 46) - manually register an instance for events
- `static void Unregister(object obj)` (line 51)
- `static void Run(string name)` / `Run<T>(string, T)` / `Run<T,U>(...)` / `Run<T,U,V>(...)` (lines 56–100) - fire named events
- `static void RunInterface<T>(Action<T> action)` (line 85) - interface-based dispatch

Built-in attribute shortcuts:

- `EditorEvent.FrameAttribute` → event name `"tool.frame"` (line 11)
- `EditorEvent.HotloadAttribute` → event name `"hotloaded"` (line 13)

Marker interfaces (line 105+):

- `EditorEvent.IEventListener` - base
- `EditorEvent.ISceneEdited` - `GameObjectPreEdited`, `GameObjectEdited`, `ComponentPreEdited`, `ComponentEdited`
- `EditorEvent.ISceneView` - `DrawGizmos(Scene)`, `ShowContextMenu(...)`

## Subscription mechanism

**Two patterns coexist:**

### 1. Attribute-based (static + instance methods)

Methods decorated with `[EventAttribute]` (or `[EditorEvent.Frame]`, `[EditorEvent.Hotload]`, etc.) are auto-discovered when the assembly is registered.

- `EventSystem.AddEventsForType(Type, Type rootType)` (line 164) recursively inspects type + base types for `[EventAttribute]`-marked methods.
- Multiple `[Event("name")]` attributes on one method are supported.
- `Priority` property controls order (lower priority runs first; default 0).
- Static + instance methods both work (instance subscribers are tracked per-type).
- **Per-assembly registration:** when an assembly unloads (hot-reload), instances are orphaned but retained; on re-register, they re-attach automatically (`EventSystem.cs:318–342`).

### 2. Interface-based (`RunInterface<T>`)

Classes implementing marker interfaces are registered via:

- `EditorEvent.Register(object obj)` adds the instance to a `WeakHashSet<AllTargets>` (line 46).
- `RunInterface<T>(Action<T>)` finds all `AllTargets` implementing `T` and invokes the action (line 85).
- No attribute needed; subscribed by interface implementation.

## Event vocabulary

### Lifecycle / editor system

| Event key | When fires | Payload | Source |
|---|---|---|---|
| `editor.created` | Editor window spawn | `EditorMainWindow` | `engine/Sandbox.Tools/Editor/EditorMainWindow.cs` |
| `hotloaded` | After hot-reload assembly swap completes | none | `engine/Sandbox.Tools/ToolsDll.cs:126` (plus game-side at `:123`) |
| `app.exit` | Editor shutdown | none | `engine/Sandbox.Tools/ToolsDll.cs:46` |

### Scene / play mode

| Event key | When fires | Payload | Source |
|---|---|---|---|
| `scene.startplay` | Just before scene enters play mode | none | `engine/Sandbox.Tools/Scene/EditorScene.cs` |
| `scene.play` | Scene now in play mode | none | `engine/Sandbox.Tools/Scene/EditorScene.cs` |
| `scene.stop` | Scene exits play mode | none | `engine/Sandbox.Tools/Scene/EditorScene.cs` |
| `scene.session.save` | User pressed Save on active scene | none | `engine/Sandbox.Tools/Scene/Session/SceneEditorSession.cs` |
| `scene.beforesave` | Just before save commit | `Scene` | `engine/Sandbox.Tools/Scene/Session/SceneEditorSession.cs` |
| `scene.saved` | After save complete | `Scene` | `engine/Sandbox.Tools/Scene/Session/SceneEditorSession.cs` |

### Assets / files

| Event key | When fires | Payload | Source |
|---|---|---|---|
| `content.changed` | A file on disk changed | `string` (filename) | `engine/Sandbox.Tools/FileSystem.cs` |
| `asset.contextmenu` | Right-click asset menu | `Menu` + asset context | game-side |
| `asset.nativecontextmenu` | Native asset context menu | `Menu`, `NativeAsset` | `engine/Sandbox.Tools/Assets/AssetSystem.cs` |
| `package.changed` | Package install/remove/modify | `Package` | `engine/Sandbox.Tools/Extensions/PackageExtensions.cs` |
| `package.changed.installed` | Package installed | `Package` | `engine/Sandbox.Tools/Assets/AssetSystem.Cloud.cs` |
| `package.changed.favourite` | Favorite toggled | `Package` | `engine/Sandbox.Tools/Extensions/PackageExtensions.cs` |
| `package.changed.rating` | Rating changed | `Package` | `engine/Sandbox.Tools/Extensions/PackageExtensions.cs` |
| `localaddons.changed` | Local addons changed | none | `engine/Sandbox.Tools/Utility/Utility.Projects.cs` |

### Map editor (Hammer)

| Event key | When fires | Payload | Source |
|---|---|---|---|
| `hammer.initialized` | Map editor fully initialized | none | `engine/Sandbox.Tools/MapEditor/Hammer.cs` |
| `hammer.selection.changed` | Selection in map | none | `engine/Sandbox.Tools/MapEditor/MapDoc/Selection.cs` |
| `hammer.mapview.contextmenu` | Right-click in map viewport | `Menu`, `MapView` | `engine/Sandbox.Tools/MapEditor/Hammer.cs` |
| `hammer.rendermapview` | Map viewport render pass | `MapView` | `engine/Sandbox.Tools/MapEditor/MapView.cs` |
| `hammer.rendermapviewhud` | Map HUD render pass | none | `engine/Sandbox.Tools/MapEditor/Hammer.cs` |

### UI / viewport

| Event key | When fires | Payload | Source |
|---|---|---|---|
| `tool.frame` | Every frame (via `[EditorEvent.Frame]`) | none | `engine/Sandbox.Tools/ManagedTools.cs` |
| `sceneview.paintoverlay` | Scene viewport overlay paint | none | game-side |
| `editor.titlebar.buttons.build` | Title bar build button | `TitleBarButtons` | `engine/Sandbox.Tools/Qt/Window/TitleBar.cs` |
| `editor.preferences` | Preferences window opened | container | game-side |
| `keybinds.update` | Keybinds changed | none | game-side |

### Asset compilation / actions

| Event key | When fires | Payload | Source |
|---|---|---|---|
| `compile.shader` | Compile shader | path | `engine/Sandbox.Tools/Assets/NativeAsset/NativeAsset.cs` |
| `open.shader` | Open shader editor | absolute path | `engine/Sandbox.Tools/Assets/NativeAsset/NativeAsset.cs` |
| `assetsystem.openpicker` | Asset picker dialog open | parameters | `engine/Sandbox.Tools/Assets/AssetSystem.cs` |
| `assetsystem.newfolder` | New asset folder | none | game-side |
| `tools.gamedata.refresh` | Refresh game data | none | `engine/Sandbox.Tools/GameData/GameData.Assembly.cs` |

### Menus & misc

| Event key | When fires | Payload | Source |
|---|---|---|---|
| `folder.contextmenu` | Right-click asset folder | `Menu` | game-side |
| `qt.mousepressed` | Qt mouse press | none | `engine/Sandbox.Tools/ManagedTools.cs` |
| `command <name>` | Console command | varies | `engine/Sandbox.Tools/ManagedTools.cs` |
| `refresh` | Generic refresh | none | `engine/Sandbox.Tools/ManagedTools.cs` |

### ActionGraph (visual scripting)

`actiongraph.saving`, `actiongraph.saved`, plus custom event classes (FindGraphTargetEvent, QueryNodeTypesEvent, PopulateCreateSubGraphMenuEvent, etc.) using static `EventName` properties.

## Interface-based subscriptions (additional)

| Interface | Methods | Source |
|---|---|---|
| `AssetSystem.IEventListener` | `OnAssetChanged(Asset)`, `OnAssetThumbGenerated(Asset)`, `OnAssetTagsChanged()` | `engine/Sandbox.Tools/Assets/AssetSystem.cs` |
| `EditorEvent.ISceneEdited` | `GameObjectPreEdited`, `GameObjectEdited`, `ComponentPreEdited`, `ComponentEdited` | `engine/Sandbox.Tools/Events/EditorEvent.cs:107` |
| `EditorEvent.ISceneView` | `DrawGizmos(Scene)`, `ShowContextMenu(...)` | `engine/Sandbox.Tools/Events/EditorEvent.cs:142` |
| `NavMesh.IEventListener` | `OnAreaDefinitionChanged()` | `engine/Sandbox.Engine/Game/Navigation/NavMesh/NavMesh.EditorEvents.cs` |
| `ResourceLibrary.IEventListener` | `OnRegister(GameResource)`, `OnUnregister(GameResource)` | `engine/Sandbox.Engine/Resources/ResourceLibrary.cs` |

Pattern:

```csharp
public class MyTool : EditorEvent.ISceneEdited
{
    public MyTool() => EditorEvent.Register(this);
    public void GameObjectEdited(GameObject go, string property) { /* ... */ }
}
```

## Hot-reload survival

Both subscription patterns survive ALC swaps **automatically**:

- **Attribute-based:** assembly unload → `EditorEvent.UnregisterAssembly()` orphans instances; reload → `RegisterAssembly()` re-attaches them (`EventSystem.cs:318–342`).
- **Interface-based:** instances live in `WeakHashSet<AllTargets>` - survive as long as the instance object survives the ALC swap.

**Caveat:** if a plugin's static reference to its instance gets GC'd during unload, manual `EditorEvent.Register()` is required post-reload.

## Recommended subscriptions for sbox-mcp Sprint 1.2

### Runtime status / play-mode awareness
- `[Event("scene.play")]` - notify MCP client / update internal "is playing" flag
- `[Event("scene.stop")]` - same on exit
- `[Event("scene.startplay")]` - pre-play hook if we need to capture scene snapshot

### Hot-reload coordination
- `[Event("hotloaded")]` - refresh reflective tool schemas after engine code reloads (works in tandem with our own `Enroller`'s `OnAssemblyAdded`)

### File / scene change feed (optional Phase 1 polish)
- `[Event("content.changed")]` - forward file changes to MCP client as event (only if MCP client subscribed)
- `[Event("scene.saved")]` - confirm scene save back to LLM

### Lifecycle
- `[Event("editor.created")]` - initialize MCP server connection state once editor is fully ready (server may already be up by static-ctor time, but this is the "I see UI" hook)
- `[Event("app.exit")]` - graceful MCP server shutdown

## Open questions

1. **Event-name case sensitivity** - `EventSystem.cs:249` lowercases names. All fired events appear lowercase already. Test with mixed case to confirm.
2. **Argument-type matching** - strict arity + type checking, no marshalling (`EventSystem.BuildDelegate:209`). Mismatched signature throws `ArgumentException` at dispatch.
3. **Priority order across event groups** - unclear whether priorities are global or per-event-name.
4. **Editor vs game-side `hotloaded`** - both `EditorEvent.Run("hotloaded")` and `Event.Run("hotloaded")` are called; possibly fire at slightly different times. Sprint 1.2 should subscribe to the editor side specifically.

## Conclusion

EditorEvent is a static, assembly-aware, hot-reload-resilient event system. **Use the attribute-based pattern (`[Event("...")]`)** for sbox-mcp's lifecycle and runtime-status hooks; subscriptions auto-survive hot-reload. The vocabulary above covers everything we need for Phase 1 + 2; richer subscriptions (asset-change forwarding, scene-edit hooks) are optional polish.
