namespace SnakeRetro.Rendering;

public sealed class TextSpriteRenderer
{
    public void DrawSprite(Graphics g, TextSprite sprite, SpritePalette palette, int x, int y, int pixelSize)
    {
        for (var row = 0; row < sprite.Height; row++)
        {
            for (var col = 0; col < sprite.Width; col++)
            {
                var symbol = sprite.Pixels[row][col];
                var color = palette[symbol];
                if (color is null)
                {
                    continue;
                }

                using var brush = new SolidBrush(color.Value);
                g.FillRectangle(brush, x + col * pixelSize, y + row * pixelSize, pixelSize, pixelSize);
            }
        }
    }
}
