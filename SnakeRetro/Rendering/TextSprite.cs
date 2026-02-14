namespace SnakeRetro.Rendering;

public sealed class TextSprite
{
    public TextSprite(string name, char[][] pixels)
    {
        Name = name;
        Pixels = pixels;
        Height = pixels.Length;
        Width = pixels[0].Length;
    }

    public string Name { get; }
    public char[][] Pixels { get; }
    public int Width { get; }
    public int Height { get; }
}
