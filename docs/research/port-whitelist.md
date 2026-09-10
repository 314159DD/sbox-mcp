# Port whitelist direction - research brief

**Date:** 2026-04-28
**Source:** Facepunch/sbox-public (local clone of the engine source)
**Confidence:** High

## What `engine/Sandbox.Engine/Utility/Web/Http.cs` actually does

This is an **outbound HTTP client wrapper** - not a server. The class is `Http` (static partial class, lines 14–125). Primary code path:

1. **`Http.IsAllowed(Uri)`** (lines 50–84) - public URI validator. Enforces scheme (`http`/`https`/`ws`/`wss`), checks loopback ports, blocks private IP ranges.
2. **`SboxHttpHandler`** (lines 127–245) - `DelegatingHandler` that wraps outbound requests. Calls `Http.IsAllowed()` at line 136 in `HandleRequest()` before allowing any request through `SendAsync()` or `Send()`.
3. **`Http.Requests.cs`** (companion) - public API (`RequestStringAsync`, `RequestBytesAsync`, etc.). All call `CreateRequest()` (line 115), which invokes `Http.IsAllowed()` at line 118 to validate the target URI.

**The 80/443/8080/8443 whitelist is at lines 58–61:**

```csharp
if ( uri.IsLoopback )
    return uri.IsDefaultPort || uri.Port is 80 or 443 or 8080 or 8443;
```

## Is the 80/443/8080/8443 whitelist inbound, outbound, or both?

**Outbound only.** High confidence.

Evidence:

1. **`Http.IsAllowed()` is called exclusively on outbound request paths.**
   - `Http.Requests.cs:118` - `Http.CreateRequest()` validates before `HttpClient.SendAsync()` (lines 102–104).
   - `WebSocket.cs:200` - WebSocket client validates before `_socket.ConnectAsync()` (line 223).
   - Both are CLIENT connections to remote services, not server BIND operations.

2. **WebSurface (embedded browser) has NO port whitelist** - different code path entirely.
   - `WebSurface.cs:181-220` - `CheckUrlIsAllowed()` blocks **all** localhost by default (line 204–205): `if ( uri.IsLoopback ) throw new InvalidOperationException( "Loopback urls are not allowed" );`
   - WebSurface requires `Application.IsEditor && CommandLine.HasSwitch( "-allowlocalhttp" )` to allow localhost at all (lines 201–202).
   - WebSurface does NOT check ports - either allows all localhost (with the flag) or none.
   - Unit tests confirm the asymmetry:
     - `HttpTests.cs:22-27` - `Http.IsAllowed()` accepts `localhost:80/443/8080/8443` ✓
     - `WebSurfaceTests.cs:19-24` - `CheckUrlIsAllowed()` rejects all `localhost:*` ✗ (without the flag)

3. **No inbound server pattern found.**
   - Codebase search for `HttpListener`, `Kestrel`, `WebApplication.CreateBuilder`, `TcpListener.Bind`, `IPAddress.Loopback`, `socket.Bind` returned **no server implementations** in editor code.
   - The only TCP networking is `TcpSocket.cs` (game networking, not localhost HTTP).
   - The whitelist is **never called during any server initialization**.

## Are there separate restrictions on server bind?

**No.** No evidence that the editor forbids binding localhost listeners on arbitrary ports. The whitelist is purely for **outbound** HTTP/WebSocket client connections initiated from inside the editor.

## Recommended port for sbox-mcp

**Bind `127.0.0.1:8080`** for these reasons:

1. **No inbound port restriction found** - we could bind any port (including a random high port like 49152+).
2. **Port 8080 is whitelisted for outbound** - if the editor itself ever needs to call our MCP server (debug overlay, telemetry forwarder, or anything via `Http.cs`), it works on 8080 without the `-allowlocalhttp` flag.
3. **No port-conflict risk identified.** s&box does not appear to bind any localhost port itself; no built-in service claims 8080.
4. **Lowest-friction default** - matches the whitelist as a courtesy.

If 8080 is occupied at startup (another dev tool running), fall back to 8443 (also whitelisted), then a random high port (works inbound, just no editor-outbound access).

## Open questions

- **Static analysis only** - recommend a quick smoke test in Sprint 1.2 that binds two listeners (one on 8080, one on 9000) to confirm no runtime block on either.
- **Native code layer** - if Facepunch wraps any C# binding APIs in native code, an additional check could exist below the C# layer. Unlikely (would have shown up in Networking.cs), but possible.

## Conclusion

For Sprint 1.2: bind to **`127.0.0.1:8080`**. Document `8443` as the documented fallback. Random high port is the third-tier fallback.
