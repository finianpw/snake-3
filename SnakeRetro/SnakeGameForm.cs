using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using SnakeRetro.Core;
using SnakeRetro.Rendering;

namespace SnakeRetro;

public sealed class SnakeGameForm : Form
{
    private const int LogicalWidth = 320;
    private const int LogicalHeight = 300;
    private const int TileSize = 16;
    private const int GridWidth = 20;
    private const int GridHeight = 15;
    private const int UiTop = 240;
    private const int ApplesPerSpeedStep = 4;

    private readonly Random _random = new();
    private readonly Timer _gameTimer;
    private readonly Timer _blinkTimer;
    private readonly List<GridPoint> _snake = new();
    private readonly TextSpriteLibrary _sprites = new();
    private readonly TextSpriteRenderer _spriteRenderer = new();
    private readonly RetroTextRenderer _text = new();

    private Direction _direction = Direction.Right;
    private Direction _nextDirection = Direction.Right;
    private GridPoint _apple;
    private int _score;
    private int _bestScore;
    private bool _isPaused;
    private bool _isGameOver;
    private bool _appleBlinkFrame;
    private int _eatenApples;
    private WallMode _wallMode = WallMode.Classic;

    private readonly string _bestScorePath;

    public SnakeGameForm()
    {
        Text = "Snake Retro 90s (TXT sprites)";
        DoubleBuffered = true;
        ClientSize = new Size(LogicalWidth * 3, LogicalHeight * 3);
        MinimumSize = new Size(LogicalWidth * 2 + 16, LogicalHeight * 2 + 39);
        KeyPreview = true;

        _bestScorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SnakeRetro", "best_score.txt");
        _bestScore = LoadBestScore();

        _gameTimer = new Timer { Interval = 170 };
        _gameTimer.Tick += (_, _) => TickGame();

        _blinkTimer = new Timer { Interval = 300 };
        _blinkTimer.Tick += (_, _) => { _appleBlinkFrame = !_appleBlinkFrame; Invalidate(); };

        KeyDown += HandleKeyDown;
        MouseDown += HandleMouseDown;

        StartNewGame();
        _gameTimer.Start();
        _blinkTimer.Start();
    }

    private void StartNewGame()
    {
        _snake.Clear();
        _snake.Add(new GridPoint(8, 7));
        _snake.Add(new GridPoint(7, 7));
        _snake.Add(new GridPoint(6, 7));

        _direction = Direction.Right;
        _nextDirection = Direction.Right;
        _score = 0;
        _eatenApples = 0;
        _isPaused = false;
        _isGameOver = false;
        _gameTimer.Interval = 170;
        SpawnApple();
        Invalidate();
    }

    private void SpawnApple()
    {
        GridPoint candidate;
        do
        {
            candidate = new GridPoint(_random.Next(0, GridWidth), _random.Next(0, GridHeight));
        } while (_snake.Contains(candidate));

        _apple = candidate;
    }

    private void TickGame()
    {
        if (_isPaused || _isGameOver)
        {
            return;
        }

        _direction = _nextDirection;
        var head = _snake[0];
        var next = _direction switch
        {
            Direction.Up => new GridPoint(head.X, head.Y - 1),
            Direction.Right => new GridPoint(head.X + 1, head.Y),
            Direction.Down => new GridPoint(head.X, head.Y + 1),
            Direction.Left => new GridPoint(head.X - 1, head.Y),
            _ => head
        };

        if (_wallMode == WallMode.Wrap)
        {
            next = new GridPoint((next.X + GridWidth) % GridWidth, (next.Y + GridHeight) % GridHeight);
        }
        else if (next.X < 0 || next.X >= GridWidth || next.Y < 0 || next.Y >= GridHeight)
        {
            EndGame();
            return;
        }

        if (_snake.Contains(next))
        {
            EndGame();
            return;
        }

        _snake.Insert(0, next);

        if (next == _apple)
        {
            _score += 10;
            _eatenApples++;
            if (_score > _bestScore)
            {
                _bestScore = _score;
                SaveBestScore();
            }

            if (_eatenApples % ApplesPerSpeedStep == 0)
            {
                _gameTimer.Interval = Math.Max(70, _gameTimer.Interval - 12);
            }

            SpawnApple();
        }
        else
        {
            _snake.RemoveAt(_snake.Count - 1);
        }

        Invalidate();
    }

    private void EndGame()
    {
        _isGameOver = true;
        Invalidate();
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.P)
        {
            _isPaused = !_isPaused;
            Invalidate();
            return;
        }

        if (e.KeyCode == Keys.R)
        {
            StartNewGame();
            return;
        }

        var requested = e.KeyCode switch
        {
            Keys.Up or Keys.W => Direction.Up,
            Keys.Right or Keys.D => Direction.Right,
            Keys.Down or Keys.S => Direction.Down,
            Keys.Left or Keys.A => Direction.Left,
            _ => _nextDirection
        };

        if (!IsOpposite(_direction, requested))
        {
            _nextDirection = requested;
        }
    }

    private void HandleMouseDown(object? sender, MouseEventArgs e)
    {
        var logical = ToLogicalPoint(e.Location);
        var buttonRect = new Rectangle(198, 257, 112, 28);
        if (buttonRect.Contains(logical))
        {
            _wallMode = _wallMode == WallMode.Classic ? WallMode.Wrap : WallMode.Classic;
            Invalidate();
        }
    }

    private Point ToLogicalPoint(Point p)
    {
        var scale = Math.Min(ClientSize.Width / (float)LogicalWidth, ClientSize.Height / (float)LogicalHeight);
        var offsetX = (ClientSize.Width - LogicalWidth * scale) / 2f;
        var offsetY = (ClientSize.Height - LogicalHeight * scale) / 2f;
        return new Point((int)((p.X - offsetX) / scale), (int)((p.Y - offsetY) / scale));
    }

    private static bool IsOpposite(Direction current, Direction requested)
    {
        return (current == Direction.Up && requested == Direction.Down)
               || (current == Direction.Down && requested == Direction.Up)
               || (current == Direction.Left && requested == Direction.Right)
               || (current == Direction.Right && requested == Direction.Left);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using var frame = new Bitmap(LogicalWidth, LogicalHeight, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(frame);
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.Half;

        DrawBackground(g);
        DrawSnake(g);
        DrawApple(g);
        DrawUi(g);

        e.Graphics.Clear(Color.Black);
        e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;

        var scale = Math.Min(ClientSize.Width / (float)LogicalWidth, ClientSize.Height / (float)LogicalHeight);
        var drawW = (int)(LogicalWidth * scale);
        var drawH = (int)(LogicalHeight * scale);
        var drawX = (ClientSize.Width - drawW) / 2;
        var drawY = (ClientSize.Height - drawH) / 2;

        e.Graphics.DrawImage(frame, new Rectangle(drawX, drawY, drawW, drawH), new Rectangle(0, 0, LogicalWidth, LogicalHeight), GraphicsUnit.Pixel);
    }

    private void DrawBackground(Graphics g)
    {
        var tileNames = new[] { "grass_a", "grass_b", "dirt_a", "moss_a", "stone_a" };
        for (var y = 0; y < GridHeight; y++)
        {
            for (var x = 0; x < GridWidth; x++)
            {
                var index = (x * 13 + y * 17) % tileNames.Length;
                var sprite = _sprites.GetTile(tileNames[index]);
                _spriteRenderer.DrawSprite(g, sprite, _sprites.Palette, x * TileSize, y * TileSize, 1);
            }
        }

        using var borderPen = new Pen(Color.FromArgb(18, 17, 17), 2f);
        g.DrawRectangle(borderPen, 0, 0, GridWidth * TileSize - 1, GridHeight * TileSize - 1);
    }

    private void DrawSnake(Graphics g)
    {
        for (var i = 0; i < _snake.Count; i++)
        {
            var p = _snake[i];
            var x = p.X * TileSize;
            var y = p.Y * TileSize;

            if (i == 0)
            {
                _spriteRenderer.DrawSprite(g, _sprites.GetSnake("head", _direction), _sprites.Palette, x, y, 1);
                continue;
            }

            if (i == _snake.Count - 1)
            {
                var tailDir = GetTailDirection(i);
                _spriteRenderer.DrawSprite(g, _sprites.GetSnake("tail", tailDir), _sprites.Palette, x, y, 1);
                continue;
            }

            var prev = _snake[i - 1];
            var next = _snake[i + 1];
            if (prev.X == next.X || prev.Y == next.Y)
            {
                var bodyDir = prev.X == next.X ? Direction.Up : Direction.Right;
                _spriteRenderer.DrawSprite(g, _sprites.GetSnake("body", bodyDir), _sprites.Palette, x, y, 1);
            }
            else
            {
                var turnDir = GetTurnDirection(prev, p, next);
                _spriteRenderer.DrawSprite(g, _sprites.GetSnake("turn", turnDir), _sprites.Palette, x, y, 1);
            }
        }
    }

    private Direction GetTailDirection(int index)
    {
        var tail = _snake[index];
        var beforeTail = _snake[index - 1];
        if (beforeTail.X < tail.X) return Direction.Right;
        if (beforeTail.X > tail.X) return Direction.Left;
        if (beforeTail.Y < tail.Y) return Direction.Down;
        return Direction.Up;
    }

    private static Direction GetTurnDirection(GridPoint prev, GridPoint current, GridPoint next)
    {
        var from = new GridPoint(prev.X - current.X, prev.Y - current.Y);
        var to = new GridPoint(next.X - current.X, next.Y - current.Y);

        if ((from.X == -1 && to.Y == -1) || (to.X == -1 && from.Y == -1)) return Direction.Up;
        if ((from.X == 1 && to.Y == -1) || (to.X == 1 && from.Y == -1)) return Direction.Right;
        if ((from.X == 1 && to.Y == 1) || (to.X == 1 && from.Y == 1)) return Direction.Down;
        return Direction.Left;
    }

    private void DrawApple(Graphics g)
    {
        var name = _appleBlinkFrame ? "apple_blink_a" : "apple_blink_b";
        _spriteRenderer.DrawSprite(g, _sprites.GetItem(name), _sprites.Palette, _apple.X * TileSize, _apple.Y * TileSize, 1);
    }

    private void DrawUi(Graphics g)
    {
        DrawPanelBackground(g);
        _spriteRenderer.DrawSprite(g, _sprites.GetUi("icon_apple"), _sprites.Palette, 8, UiTop + 9, 1);
        _spriteRenderer.DrawSprite(g, _sprites.GetUi("icon_trophy"), _sprites.Palette, 8, UiTop + 33, 1);

        _text.DrawText(g, $"SCORE: {_score}", 28, UiTop + 10, 2, Color.FromArgb(232, 225, 203));
        _text.DrawText(g, $"BEST: {_bestScore}", 28, UiTop + 34, 2, Color.FromArgb(200, 182, 130));

        DrawModeButton(g);

        if (_isPaused)
        {
            _text.DrawText(g, "PAUZA", 126, 108, 3, Color.FromArgb(245, 227, 129));
        }

        if (_isGameOver)
        {
            _text.DrawText(g, "GAME OVER", 90, 108, 3, Color.FromArgb(248, 144, 127));
            _text.DrawText(g, "R - RESTART", 96, 132, 2, Color.FromArgb(232, 225, 203));
        }

        _text.DrawText(g, "P-PAUZA R-RESET", 120, UiTop + 10, 2, Color.FromArgb(164, 157, 142));
    }

    private static void DrawPanelBackground(Graphics g)
    {
        var rect = new Rectangle(0, UiTop, LogicalWidth, 60);
        using var dark = new SolidBrush(Color.FromArgb(86, 80, 66));
        using var mid = new SolidBrush(Color.FromArgb(122, 113, 93));
        using var light = new SolidBrush(Color.FromArgb(165, 152, 123));
        using var shadow = new SolidBrush(Color.FromArgb(18, 17, 17));

        g.FillRectangle(mid, rect);
        for (var y = rect.Top; y < rect.Bottom; y++)
        {
            for (var x = rect.Left; x < rect.Right; x++)
            {
                if ((x + y) % 2 == 0)
                {
                    g.FillRectangle(dark, x, y, 1, 1);
                }
            }
        }

        g.FillRectangle(light, rect.Left, rect.Top, rect.Width, 2);
        g.FillRectangle(light, rect.Left, rect.Top, 2, rect.Height);
        g.FillRectangle(shadow, rect.Left, rect.Bottom - 2, rect.Width, 2);
        g.FillRectangle(shadow, rect.Right - 2, rect.Top, 2, rect.Height);
    }

    private void DrawModeButton(Graphics g)
    {
        var rect = new Rectangle(198, UiTop + 17, 112, 28);
        using var dark = new SolidBrush(Color.FromArgb(54, 49, 41));
        using var light = new SolidBrush(Color.FromArgb(123, 114, 94));
        using var mid = new SolidBrush(Color.FromArgb(87, 79, 66));
        g.FillRectangle(mid, rect);
        g.FillRectangle(light, rect.X, rect.Y, rect.Width, 2);
        g.FillRectangle(light, rect.X, rect.Y, 2, rect.Height);
        g.FillRectangle(dark, rect.X, rect.Bottom - 2, rect.Width, 2);
        g.FillRectangle(dark, rect.Right - 2, rect.Y, 2, rect.Height);

        var label = _wallMode == WallMode.Classic ? "WALL: CLASSIC" : "WALL: WRAP";
        _text.DrawText(g, label, rect.X + 8, rect.Y + 8, 1, Color.FromArgb(235, 219, 185));
    }

    private int LoadBestScore()
    {
        if (!File.Exists(_bestScorePath))
        {
            return 0;
        }

        var text = File.ReadAllText(_bestScorePath).Trim();
        return int.TryParse(text, out var score) ? score : 0;
    }

    private void SaveBestScore()
    {
        var dir = Path.GetDirectoryName(_bestScorePath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(_bestScorePath, _bestScore.ToString());
    }
}
