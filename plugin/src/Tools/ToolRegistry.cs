namespace SboxMcp.Tools;

public sealed class ToolRegistry
{
    private readonly Dictionary<string, ITool> _tools = new();

    public IEnumerable<string> Names => _tools.Keys;

    public void Register(ITool tool)
    {
        if (_tools.ContainsKey(tool.Name))
            throw new InvalidOperationException(
                $"Tool '{tool.Name}' is already registered.");

        _tools[tool.Name] = tool;
    }

    public ITool Get(string name) =>
        _tools.TryGetValue(name, out var tool)
            ? tool
            : throw new KeyNotFoundException($"No tool registered with name '{name}'.");
}
