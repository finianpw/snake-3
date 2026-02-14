namespace SnakeRetro.Core;

public enum Direction
{
    Up,
    Right,
    Down,
    Left
}

public enum WallMode
{
    Classic,
    Wrap
}

public readonly record struct GridPoint(int X, int Y);
