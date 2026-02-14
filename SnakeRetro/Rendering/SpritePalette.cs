using System.Text.Json;

namespace SnakeRetro.Rendering;

public sealed class SpritePalette
{
    private readonly Dictionary<char, Color?> _colors;

    private SpritePalette(Dictionary<char, Color?> colors)
    {
        _colors = colors;
    }

    public static SpritePalette Load(string path)
    {
        var raw = JsonSerializer.Deserialize<PaletteFile>(File.ReadAllText(path))
                  ?? throw new InvalidOperationException("Nie udało się odczytać palette.json");

        var map = new Dictionary<char, Color?>();
        foreach (var kv in raw.Symbols)
        {
            if (kv.Key.Length != 1)
            {
                throw new InvalidOperationException($"Symbol palety musi mieć 1 znak: '{kv.Key}'");
            }

            var key = kv.Key[0];
            if (string.IsNullOrWhiteSpace(kv.Value))
            {
                map[key] = null;
                continue;
            }

            map[key] = ColorTranslator.FromHtml(kv.Value);
        }

        return new SpritePalette(map);
    }

    public Color? this[char symbol] => _colors.TryGetValue(symbol, out var color) ? color : null;

    private sealed class PaletteFile
    {
        public Dictionary<string, string?> Symbols { get; set; } = new();
    }
}
