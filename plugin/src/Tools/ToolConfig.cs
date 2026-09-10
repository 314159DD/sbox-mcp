using System.Text.Json;
using System.Text.Json.Nodes;

namespace SboxMcp.Tools;

public sealed record ToolConfig(int Version, IReadOnlyList<ToolEntry> Tools)
{
    public static ToolConfig LoadFromJson(string json)
    {
        var doc = JsonNode.Parse(json)
            ?? throw new JsonException("Tool config root is null.");

        var version = doc["version"]?.GetValue<int>() ?? 0;
        var toolsNode = doc["tools"]?.AsArray();

        var tools = toolsNode is null
            ? Array.Empty<ToolEntry>()
            : toolsNode
                .Where(n => n is not null)
                .Select(n => new ToolEntry(
                    Name: n!["name"]?.GetValue<string>() ?? "",
                    Description: n["description"]?.GetValue<string>() ?? "",
                    Schema: n["schema"]?.ToJsonString() ?? "{}"))
                .ToArray();

        return new ToolConfig(version, tools);
    }

    public static ToolConfig LoadFromFile(string path) =>
        LoadFromJson(File.ReadAllText(path));
}

public sealed record ToolEntry(string Name, string Description, string Schema);
