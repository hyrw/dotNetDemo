using System;
using System.Collections.Generic;

namespace SnakeDemo;
public class Direction
{

    public readonly static Direction Left = new(0, -1);
    public readonly static Direction Right = new(0, 1);
    public readonly static Direction Up = new(-1, 0);
    public readonly static Direction Down = new(1, 0);

    public int RowOffset { get; }
    public int ColOffset { get; }

    private Direction(int rowOffset, int colOffset)
    {
        RowOffset = rowOffset;
        ColOffset = colOffset;
    }

    public Direction Opposite()
    {
        return new Direction(-RowOffset, -ColOffset);
    }

    public override bool Equals(object? obj)
    {
        return obj is Direction direction &&
               RowOffset == direction.RowOffset &&
               ColOffset == direction.ColOffset;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(RowOffset, ColOffset);
    }

    public static bool operator ==(Direction? left, Direction? right)
    {
        return EqualityComparer<Direction>.Default.Equals(left, right);
    }

    public static bool operator !=(Direction? left, Direction? right)
    {
        return !(left == right);
    }
}
