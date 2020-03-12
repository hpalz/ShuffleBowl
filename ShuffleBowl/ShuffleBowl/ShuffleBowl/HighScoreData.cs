using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using ShuffleBowl;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

public struct HighScoreData
{

    public char[][] PlayerName;
    public int[] Score;
    public int Count;

    public HighScoreData(int count)
    {
        PlayerName = new char[count][];
        Score = new int[count];
        Count = count;

        for (int x = 0; x < Count; x++)
        {
            PlayerName[x] = new char[3];
            PlayerName[x][0] = 'A';
            PlayerName[x][1] = 'A';
            PlayerName[x][2] = 'A';
            Score[x] = 0;
        }
    }
    public void Draw(SpriteBatch spriteBatch, Viewport viewport, HighScoreData hs, SpriteFont font, SpriteFont TitleFont)
    {
        Vector2 position = new Vector2();
        String str = "High Scores";

        for (int x = 0; x < hs.Count; x++)
        {
            position.X = (viewport.Width * .25f);
            position.Y = (viewport.Height * (x*.06f)+235f);
            // Draw the Score in the top-left of screen
            spriteBatch.DrawString(
                font,                          // SpriteFont
                new String(hs.PlayerName[x]),  // Text
                position,                      // Position
                Color.White);                  // Tint
            position.X = (viewport.Width * .55f);
            position.Y = (viewport.Height * (x * .06f) + 235f);
            // Draw the Score in the top-left of screen
            spriteBatch.DrawString(
                font,                          // SpriteFont
                hs.Score[x].ToString(),  // Text
                position,                      // Position
                Color.White);                  // Tint
        }
        position.X = (viewport.Width / 2 - TitleFont.MeasureString(str).X / 2);
        position.Y = (viewport.Height * .08f)+85f;
        spriteBatch.DrawString(TitleFont, str, position, Color.White);
    }
}
