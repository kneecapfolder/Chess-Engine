using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pieces;

enum Team {
    White, Black
}

abstract class Piece {
    // Piece size in pixels on the spritesheet
    protected readonly static int size = 65;
    public static Texture2D spriteSheet;
    public static SpriteBatch _spriteBatch;
    public static List<Piece> board;
    
    public bool hasMoved = false;
    public Vector2 pos;
    public Team team;
    public List<Vector2> available = new(); // List of available possitions
    public Rectangle source;

    public Piece(Vector2 pos, Team team) {
        this.pos = pos;
        this.team = team;
        source = new Rectangle(0, size * (int)team, size, size);
    }

    protected bool IsAvailable(Vector2 _pos, bool otherTeam = false) {
        if (otherTeam)
            return !board.Any(p => p.pos.Equals(_pos) && p.team != team);
        else return !board.Any(p => p.pos.Equals(_pos) && p.team == team);
    }

    protected List<Vector2> CleanAvailable() {
        available.Remove(pos);
        List<Vector2> toRemove = new();
        foreach(Vector2 v in available)
            if (v.X < 0 || v.X >= 8 || v.Y < 0 || v.Y >= 8)
                toRemove.Add(v);
        available.RemoveAll(v => toRemove.Contains(v));
        return available;
    }

    protected List<Vector2> GetLines(Vector2[] transforms) {
        available.Clear();
        bool[] finishedPaths = new bool[transforms.Length];
        for(int i = 1; i < 8; i++) {
            for(int j = 0; j < transforms.Length; j++) {
                if (!finishedPaths[j]) {
                    Vector2 _pos = pos + Vector2.Multiply(transforms[j], i);
                    bool stopped = false;
                    foreach(Piece p in board) {
                        if (p.pos.Equals(_pos)) {
                            if (p.team != team)
                                available.Add(_pos);
                            finishedPaths[j] = true;
                            stopped = true;
                            break;
                        }
                    }
                    if (!stopped && IsAvailable(_pos))
                        available.Add(_pos);
                }
            }
        }
        return CleanAvailable();
    }

    public void Draw() {
        _spriteBatch.Draw(
            spriteSheet, new Rectangle(
                (int)pos.X * 65 + 20, (int)pos.Y * 65 + 20, 65, 65
            ), source, Color.White
        );
    }

    public abstract List<Vector2> GetAvailable();
}

sealed class King : Piece {
    
    public King(Vector2 pos, Team team) : base(pos, team) {
        source.X = 0;
    }

    public override List<Vector2> GetAvailable() {
        available.Clear();
        for(int y = -1; y <= 1; y++)
            for(int x = -1; x <= 1; x++)
                if (IsAvailable(pos + new Vector2(x, y)))
                    available.Add(pos + new Vector2(x, y));

        // Castling
        if (!hasMoved) {
            if (
                board.Any(p => p.pos == pos - new Vector2(4, 0) && !p.hasMoved && p is Rook) &&
                !board.Any(p => p.pos == pos - new Vector2(3, 0) || p.pos == pos - new Vector2(2, 0) || p.pos == pos - new Vector2(1, 0))
            ) available.Add(pos - new Vector2(2, 0)); // left castle

            if (
                board.Any(p => p.pos == pos + new Vector2(3, 0) && !p.hasMoved && p is Rook) &&
                !board.Any(p => p.pos == pos + new Vector2(2, 0) || p.pos == pos + new Vector2(1, 0))
            ) available.Add(pos + new Vector2(2, 0)); // Left castle
        }
        return CleanAvailable();
    }
}

sealed class Queen : Piece {
    public Queen(Vector2 pos, Team team) : base(pos, team) {
        source.X = size;
    }

    public override List<Vector2> GetAvailable() {
        return GetLines(new Vector2[]{
            new Vector2(1, 0),
            new Vector2(-1, 0),
            new Vector2(0, 1),
            new Vector2(0, -1),
            new Vector2(1, 1),
            new Vector2(-1, 1),
            new Vector2(1, -1),
            new Vector2(-1, -1)
        });
    }
}

sealed class Bishop : Piece {
    public Bishop(Vector2 pos, Team team) : base(pos, team) {
        source.X = 2*size;
    }

    public override List<Vector2> GetAvailable() {
        return GetLines(new Vector2[]{
            new Vector2(1, 1),
            new Vector2(-1, 1),
            new Vector2(1, -1),
            new Vector2(-1, -1)
        });
    }
}

sealed class Knight : Piece {
    public Knight(Vector2 pos, Team team) : base(pos, team) {
        source.X = 3*size;
    }

    public override List<Vector2> GetAvailable() {
        available.Clear();
        Vector2[] positions = {
            new Vector2(1, 2),
            new Vector2(2, 1),
            new Vector2(2, -1),
            new Vector2(1, -2),
            new Vector2(-1, -2),
            new Vector2(-2, -1),
            new Vector2(-2, 1),
            new Vector2(-1, 2),
        };
        foreach(Vector2 v in positions)
            if (IsAvailable(pos + v))
                available.Add(pos + v);
        return CleanAvailable();
    }
}

sealed class Rook : Piece {


    public Rook(Vector2 pos, Team team) : base(pos, team) {
        source.X = 4*size;
    }

    public override List<Vector2> GetAvailable() {
        return GetLines(new Vector2[]{
            new Vector2(1, 0),
            new Vector2(-1, 0),
            new Vector2(0, 1),
            new Vector2(0, -1)
        });
    }
}

sealed class Pawn : Piece {
    public bool justLeaped = false;

    public Pawn(Vector2 pos, Team team) : base(pos, team) {
        source.X = 5*size;
    }

    public override List<Vector2> GetAvailable() {
        available.Clear();
        Vector2 offset = new Vector2(0, team == Team.White? -1 : 1);
        if (!board.Any(p => p.pos.Equals(pos + offset)))
            available.Add(pos + offset);
        if (!hasMoved && available.Count > 0 && !board.Any(p => p.pos.Equals(pos + Vector2.Multiply(offset, 2))))
            available.Add(pos + Vector2.Multiply(offset, 2));

        for(int i = -1; i <= 1; i += 2)
            if (!IsAvailable(pos + offset + new Vector2(i, 0), true) || (board.Any(p => p.pos == pos + new Vector2(i, 0) && p is Pawn pawn && pawn.justLeaped && IsAvailable(pos + offset + new Vector2(i, 0)))))
                available.Add(pos + offset + new Vector2(i, 0));


        
        return CleanAvailable();
    }
}