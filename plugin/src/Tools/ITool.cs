namespace SboxMcp.Tools;

/// <summary>
/// Common contract for sbox-mcp tools. Tool descriptions and JSON schemas
/// live in config/tools.json - this interface only carries the runtime
/// identity. Tools are dispatched through the MCP SDK's attribute-based
/// registration; ToolRegistry tracks them for introspection and reload.
/// </summary>
public interface ITool
{
    string Name { get; }
}
