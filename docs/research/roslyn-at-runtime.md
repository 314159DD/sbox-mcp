# Roslyn at runtime - research brief

**Date:** 2026-04-28
**Source:** Facepunch/sbox-public (local clone of the engine source)
**Confidence:** High (with one critical caveat on version alignment)

## Is Roslyn available at runtime?

**YES, definitively.** Roslyn is **embedded in the production editor runtime** via `Sandbox.Engine`.

- File: `engine/Sandbox.Engine/Sandbox.Engine.csproj` (lines 40-41)
- References:
  - `Microsoft.CodeAnalysis.CSharp` **v5.0.0-2.final**
  - `Microsoft.CodeAnalysis.Analyzers` v3.12.0-beta1.25218.8

Roslyn is actively used at editor runtime for:

- **Syntax tree parsing and incremental compilation** - `engine/Sandbox.Compiling/Compiler/Compiler.SyntaxTree.cs:17-189` calls `CSharpSyntaxTree.ParseText()`, `compilation.RemoveSyntaxTrees()`, `compilation.AddSyntaxTrees()` at runtime.
- **Code generation** - `engine/Sandbox.Compiling/Compiler/Compiler.Generator.cs` processes syntax trees in the live compiler loop.
- **Razor processing** - `engine/Sandbox.Compiling/Compiler/Compiler.Razor.cs` uses `CSharpSyntaxTree.ParseText()` for Razor file compilation.

All these load into `Sandbox.Engine` (net10.0 target), loaded at editor startup via `Bootstrap.PreInit()` → `Mounting.Directory.LoadAssemblies()`.

## Bonus finding: existing `Sandbox.Compiling.Compiler` class

The engine already exposes a runtime C# compiler. Worth investigating in Sprint 1.2 / 1.5 - we may be able to call into `Sandbox.Compiling.Compiler.*` rather than spinning up a parallel Roslyn pipeline. Could materially shrink the codegen tool implementation.

## How addons declare dependencies

**Addons do NOT declare NuGet dependencies via `.csproj` resolution at load time.** The engine uses simple folder-to-DLL convention:

- Addons load from `mount/{name}/{name}.dll` via `Assembly.LoadFile()` (`engine/Sandbox.Engine/Game/Mount/Directory.cs:23-48`, line 38).
- No automatic NuGet resolution; no addon manifest / `addon.json` for deps.
- **Addon dependencies must be vendored** alongside the addon DLL.

## ALC / hot-reload constraints on plugin dependencies

Minimal constraints for plugin DLL loading:

1. **`Assembly.LoadFile()` bypasses the hot-reload ALC entirely.** The hot-reload system (`engine/Sandbox.Hotload/FrameworkSpecific.cs:486-489`) uses `AssemblyLoadContext.GetLoadContext()` for tracking, but the initial mount-load doesn't go through the isolated ALC.
2. **Dependency resolution** for an `Assembly.LoadFile()`-loaded plugin walks: the plugin's folder (probing path), GAC (irrelevant here), and runtime-loaded assemblies. Means a plugin CAN ship its own NuGet dependencies in its `mount/{name}/` folder.
3. **No restriction on what assemblies can be loaded.**

## Recommended approach for sbox-mcp codegen tools

**Use Roslyn directly via `Microsoft.CodeAnalysis.CSharp` v5.0.0-2.final**, matching the engine's version exactly.

```xml
<!-- sbox-mcp.csproj -->
<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="5.0.0-2.final" />
```

At runtime:
```csharp
var tree = CSharpSyntaxTree.ParseText(sourceCode, options: parseOptions);
var compilation = CSharpCompilation.Create(assemblyName)
    .AddSyntaxTrees(tree)
    .AddReferences(/* framework refs */);
```

Two paths:
1. **Reuse engine Roslyn** - let `Assembly.LoadFile` resolve `Microsoft.CodeAnalysis.CSharp` from the already-loaded engine assembly. Simpler if the engine exposes its instance.
2. **Vendor our own copy** - ship `Microsoft.CodeAnalysis.CSharp.dll` + its transitive deps into `mount/sbox-mcp/`. Safer (no version-conflict risk) but bigger DLL footprint (~5MB).

**Recommendation:** start with path 1 (no vendoring). Test in Sprint 1.5 codegen sprint. Fall back to path 2 if version conflicts surface.

## Open questions

1. **Version alignment with MCP SDK transitive deps.** `ModelContextProtocol.AspNetCore` may pull in Roslyn analyzers or a different `Microsoft.CodeAnalysis.*` version. **Verify in Sprint 1.2** by inspecting the resolved package graph after `dotnet restore`. Possible mitigation: explicit `<PackageReference>` to pin versions.
2. **Whether the engine exposes its Roslyn instance or keeps it private.** If private (probable), we vendor our own (path 2 above).
3. **Whether `Sandbox.Compiling.Compiler.*` is callable from a mount/ plugin** or requires editor-internal access. Investigate in Sprint 1.5.

## Conclusion

**For Sprint 1.2 implementation:** reference Roslyn 5.0.0-2.final in the plugin csproj. Codegen is unblocked, no special ALC tricks needed. Existing `Sandbox.Compiling.Compiler` class is a bonus we should explore before reimplementing.
