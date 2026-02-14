using System.Reflection;
using SnakeRetro.Core;

namespace SnakeRetro.Rendering;

public sealed class TextSpriteLibrary
{
    private readonly string _basePath;
    private readonly Dictionary<string, TextSprite> _cache = new();

    public TextSpriteLibrary()
    {
        _basePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppContext.BaseDirectory, "AssetsSrc");
        Palette = SpritePalette.Load(Path.Combine(_basePath, "palette.json"));
    }

    public SpritePalette Palette { get; }

    public TextSprite GetTile(string name) => GetSprite(Path.Combine("Tiles", $"{name}.txt"));
    public TextSprite GetItem(string name) => GetSprite(Path.Combine("Items", $"{name}.txt"));
    public TextSprite GetUi(string name) => GetSprite(Path.Combine("UI", $"{name}.txt"));

    public TextSprite GetSnake(string part, Direction direction)
    {
        var dir = direction.ToString().ToLowerInvariant();
        return GetSprite(Path.Combine("Snake", $"{part}_{dir}.txt"));
    }

    private TextSprite GetSprite(string relativePath)
    {
        if (_cache.TryGetValue(relativePath, out var sprite))
        {
            return sprite;
        }

        var fullPath = Path.Combine(_basePath, relativePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Brak sprite TXT: {fullPath}");
        }

        var lines = File.ReadAllLines(fullPath)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        if (lines.Length == 0)
        {
            throw new InvalidOperationException($"Pusty sprite: {relativePath}");
        }

        var width = lines[0].Length;
        if (lines.Any(line => line.Length != width))
        {
            throw new InvalidOperationException($"Nierówna szerokość sprite: {relativePath}");
        }

        var pixels = lines.Select(l => l.ToCharArray()).ToArray();
        sprite = new TextSprite(relativePath, pixels);
        _cache[relativePath] = sprite;
        return sprite;
    }
}
