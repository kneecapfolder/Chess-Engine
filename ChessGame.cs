using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pieces;

namespace MainProgram;

public class Chess : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    Dictionary<string, SoundEffect> sfx;
    ButtonState oldState;
    Texture2D spriteSheet;
    Texture2D square;
    Piece selected;
    Vector2[] highlight;
    Team currentTeam;

    public Chess()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        _graphics.PreferredBackBufferWidth = 560;
        _graphics.PreferredBackBufferHeight = 560;
        _graphics.ApplyChanges();
        selected = null;
        highlight = Enumerable.Repeat(new Vector2(-1, -1), 3).ToArray();
        currentTeam = Team.White;

        // Create a blank square
        square = new Texture2D(GraphicsDevice, 1, 1);
        square.SetData(new[] { Color.White });

        oldState = ButtonState.Released;
        sfx = new Dictionary<string, SoundEffect>();

        Piece.board = new() {
            // Black
            new Rook(new(0, 0), Team.Black),
            new Knight(new(1, 0), Team.Black),
            new Bishop(new(2, 0), Team.Black),
            new Queen(new(3, 0), Team.Black),
            new King(new(4, 0), Team.Black),
            new Bishop(new(5, 0), Team.Black),
            new Knight(new(6, 0), Team.Black),
            new Rook(new(7, 0), Team.Black),
            new Pawn(new(0, 1), Team.Black),
            new Pawn(new(1, 1), Team.Black),
            new Pawn(new(2, 1), Team.Black),
            new Pawn(new(3, 1), Team.Black),
            new Pawn(new(4, 1), Team.Black),
            new Pawn(new(5, 1), Team.Black),
            new Pawn(new(6, 1), Team.Black),
            new Pawn(new(7, 1), Team.Black),
            
            // White
            new Pawn(new(0, 6), Team.White),
            new Pawn(new(1, 6), Team.White),
            new Pawn(new(2, 6), Team.White),
            new Pawn(new(3, 6), Team.White),
            new Pawn(new(4, 6), Team.White),
            new Pawn(new(5, 6), Team.White),
            new Pawn(new(6, 6), Team.White),
            new Pawn(new(7, 6), Team.White),
            new Rook(new(0, 7), Team.White),
            new Knight(new(1, 7), Team.White),
            new Bishop(new(2, 7), Team.White),
            new Queen(new(3, 7), Team.White),
            new King(new(4, 7), Team.White),
            new Bishop(new(5, 7), Team.White),
            new Knight(new(6, 7), Team.White),
            new Rook(new(7, 7), Team.White),
        };

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
        spriteSheet = Content.Load<Texture2D>("sprites/pieces");
        Piece._spriteBatch = _spriteBatch;
        Piece.spriteSheet = spriteSheet;

        // Load sound effects
        sfx.Add("eat", Content.Load<SoundEffect>("sound-effects/eat"));
        sfx.Add("move", Content.Load<SoundEffect>("sound-effects/move"));
        sfx.Add("select", Content.Load<SoundEffect>("sound-effects/select"));
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        ButtonState newState = Mouse.GetState().LeftButton;
        if (newState == ButtonState.Pressed && oldState == ButtonState.Released) {
            // Get mouse pos
            Vector2 pos = new Vector2(
                (int)Map(Mouse.GetState().X - 20, 0, 520, 0, 8),
                (int)Map(Mouse.GetState().Y - 20, 0, 520, 0, 8)
            );

            if (selected != null) {
                // Deselect the selected piece
                if (selected.pos.Equals(pos))
                    selected = null;
                
                // Move piece
                else if (selected.GetAvailable().Any(p => p.Equals(pos))) {
                    highlight[0] = selected.pos;
                    highlight[1] = pos;

                    // Eat piece
                    bool hasEaten = false;
                    foreach(Piece p in Piece.board) {
                        if (p.pos.Equals(pos)) {
                            Piece.board.Remove(p);
                            sfx["eat"].Play();
                            hasEaten = true;
                            break;
                        }
                    }

                    if (!hasEaten)
                        sfx["move"].Play();

                    // Castling
                    if (selected is King && !selected.hasMoved) {
                        Rook rook = new(new Vector2(-1, -1), Team.White);
                        if (pos.X - selected.pos.X == 2) {
                            rook = (Rook)Piece.board.Find(p => p is Rook && p.pos.X == 7 && p.team == currentTeam);
                            rook.pos.X -= 2;
                            rook.hasMoved = true;
                        }
                        else if (pos.X - selected.pos.X == -2){
                            rook = (Rook)Piece.board.Find(p => p is Rook && p.pos.X == 0 && p.team == currentTeam);
                            rook.pos.X += 3;
                            rook.hasMoved = true;
                        }
                    }
                    
                    // Switch turn
                    if (currentTeam == Team.White)
                        currentTeam = Team.Black;
                    else currentTeam = Team.White;

                    // Updating move checks
                    if (selected is Pawn pawn) {
                        selected.hasMoved = true;
                        pawn.justLeaped = false;
                        if (Math.Abs(pawn.pos.Y - pos.Y) == 2)
                            pawn.justLeaped = true;
                            
                        // En passant
                        if (pawn.pos.X != pos.X && pawn.pos.Y != pos.Y) {
                            Piece eaten = Piece.board.Find(p => p.pos == new Vector2(pos.X, pawn.pos.Y) && p is Pawn p1 && p1.justLeaped);
                            if (eaten != null)
                                Piece.board.Remove(eaten);
                        }
                    }
                    if (selected is Rook or King)
                        selected.hasMoved = true;

                    foreach(Piece p in Piece.board)
                        if (p is Pawn p1 && p != selected)
                            p1.justLeaped = false;

                    selected.pos = pos;
                    selected = null;
                }

                // Select different piece
                else {
                    selected = null;
                    foreach(Piece p in Piece.board)
                        if (p.pos.Equals(pos) && p.team == currentTeam) {
                            selected = p;
                            sfx["select"].Play();
                            break;
                        }
                }
            }

            // Select a new piece
            else foreach(Piece p in Piece.board)
                if (p.pos.Equals(pos) && p.team == currentTeam) {
                    selected = p;
                    sfx["select"].Play();
                    break;
                }
        }

        oldState = newState;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        // Draw board grid
        _spriteBatch.Draw(square, new Rectangle(
            20, 20, 520, 520
        ), Color.MediumSeaGreen);
        for(int y = 0; y < 8; y++)
            for(int x = y%2; x < 8; x += 2)
                _spriteBatch.Draw(
                    square, new Rectangle(
                        x * 65 + 20, y * 65 + 20, 65, 65
                    ), Color.Beige
                );

        // Highlight selected piece
        if (selected != null)
            highlight[2] = selected.pos;
        else highlight[2] = new Vector2(-1, -1);
        
        foreach(Vector2 pos in highlight) {
            if (pos.X != -1)
                _spriteBatch.Draw(square, new Rectangle(
                    (int)pos.X * 65 + 20,
                    (int)pos.Y * 65 + 20,
                    65, 65
                ), Color.GreenYellow * 0.5f);

        }

        // Draw move preview
        if (selected != null)
            foreach(Vector2 v in selected.GetAvailable())
                _spriteBatch.Draw(
                    spriteSheet, new Rectangle((int)v.X*65+20, (int)v.Y*65+20, 65, 65),
                    new Rectangle(Piece.board.Any(p => p.pos.Equals(v)) ? 65 : 0, 130, 65, 65),
                    Color.White
                );

        foreach(Piece p in Piece.board)
            p.Draw();
                
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private float Map(float n, float start1, float end1, float start2, float end2) {
        return (n-start1)/(end1-start1)*(end2-start2)+start2;
    }
}