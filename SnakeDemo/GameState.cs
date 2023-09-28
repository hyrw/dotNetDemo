using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Xml;

namespace SnakeDemo;

public class GameState
{
    public int Rows { get; }
    public int Cols { get; }
    public GridValue[,] Grid { get; }
    public Direction Direction { get; private set; }
    public int Score { get; private set; }
    public bool GameOver { get; private set; }

    private readonly LinkedList<Position> snakePositions = new();
    private Random random = new ();

    public GameState(int rows, int cols)
    {
        Rows = rows;
        Cols = cols;
        Grid = new GridValue[rows, cols];
        Direction = Direction.Right;

        AddSnake();
        AddFood();
    }

    private void AddSnake()
    {
        int row = Rows / 2;
        for (int col = 0; col < 3; col++)
        {
            Grid[row, col] = GridValue.Snake;
            snakePositions.AddFirst(new Position(row, col));
        }
    }

    private IEnumerable<Position> EmptyPosition()
    {
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                if (Grid[r, c] == GridValue.Empty)
                {
                    yield return new Position(r, c);
                }
            }
        }
    }

    private void AddFood()
    {
        List<Position> empty = new List<Position>(EmptyPosition());
        if (empty.Count == 0)
        {
            return;
        }

        Position position = empty[random.Next(empty.Count)];
        Grid![position.Row, position.Col] = GridValue.Food;
    }

    public Position HeadPosition()
    {
        return snakePositions.First!.Value;
    }

    public Position TailPosition()
    {
        return snakePositions.Last!.Value;
    }

    public IEnumerable<Position> SnakePositions()
    {
        return snakePositions;
    }

    private void AddHead(Position position)
    {
        snakePositions.AddFirst(position);
        Grid[position.Row, position.Col] = GridValue.Snake;
    }

    private void RemoveTail()
    {
        Position tail = snakePositions.Last!.Value;
        Grid[tail.Row, tail.Col] = GridValue.Empty;
        snakePositions.RemoveLast();
    }

    public void ChangeDirecation(Direction direction)
    {
        Direction = direction;
    }

    private bool OutsideGrid(Position position)
    {
        return position.Row < 0 || position.Row >= Rows ||
               position.Col < 0 || position.Col >= Cols;
    }

    private GridValue WillHit(Position position)
    {
        if (OutsideGrid(position))
        {
            return GridValue.Outside;
        }

        if (position == TailPosition())
        {
            return GridValue.Empty;
        }

        return Grid[position.Row, position.Col];
    }

    public void Move()
    {
        Position newHeadPosition = HeadPosition().Translate(Direction);
        GridValue hit = WillHit(newHeadPosition);

        if (hit == GridValue.Outside || hit == GridValue.Snake)
        {
            GameOver = true;
        }else if (hit == GridValue.Empty)
        {
            RemoveTail();
            AddHead(newHeadPosition);
        }else if (hit == GridValue.Food)
        {
            AddHead(newHeadPosition);
            Score++;
            AddFood();
        }
    }
}
