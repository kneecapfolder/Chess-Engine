using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pieces;

namespace MainProgram;

public class Chess : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    Texture2D square;

    List<Piece> board = new() {
        new Pawn(new(0,0), Team.Black)
    };

    public Chess()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        _graphics.PreferredBackBufferWidth = 640;
        _graphics.PreferredBackBufferHeight = 640;
        _graphics.ApplyChanges();

        // Create a blank square
        square = new Texture2D(GraphicsDevice, 1, 1);
        square.SetData(new[] { Color.White });

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        // Draw board grid
        _spriteBatch.Draw(square, new Rectangle(
            20, 20, 600, 600
        ), Color.MediumSeaGreen);
        for(int y = 0; y < 8; y++)
            for(int x = y%2; x < 8; x += 2)
                _spriteBatch.Draw(
                    square, new Rectangle(
                        x * 75 + 20, y * 75 + 20, 75, 75
                    ), Color.Beige
                );
                
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
