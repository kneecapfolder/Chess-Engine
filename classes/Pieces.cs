using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pieces;

enum Team {
    White, Black
}

// Base piece class
abstract class Piece {
    // Piece size in pixels on the spritesheet
    protected readonly int size = 75;

    // Check that pos is in board
    public Vector2 pos;
    public Team team;
    public List<Vector2> available = new(); // List of available possitions
    public Rectangle source;

    public Piece(Vector2 pos, Team team) {
        this.pos = pos;
        this.team = team;
        source = new Rectangle(0, size*(int)team, size, size);
    }

    public abstract List<Vector2> GetAvailable();
}

sealed class Pawn : Piece {
    bool hasMoved = false;

    public Pawn(Vector2 pos, Team team) : base(pos, team) {
        source.X = 0;
    }

    public override List<Vector2> GetAvailable() {
        available.Clear();
        if (hasMoved)
            available.Add(pos + new Vector2(0, (int)team*-1*-2));
        available.Add(pos + new Vector2(0, (int)team*-1*-1));
        return available;
    }
}