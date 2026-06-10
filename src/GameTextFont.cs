using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameTextLib;

public class GameTextFont
{
    public required Texture2D Texture { get; init; }

    ///<summary>
    ///If the font is contained in a region of the texture instead of the whole texture, specify that region here
    ///</summary>
    public Rectangle? TextureDimensions = null;

    public required int DefaultHeight { get; init; }
    public required int DanglingHeight { get; init; }

    public required int CapitalLetterWidth { get; init; }
    public required int LowercaseLetterWidth { get; init; }
    public required int NumberWidth { get; init; }

    ///<remarks>
    ///This rect is relative to the whole texture, not the section delegated by TextureDimensions
    ///</remarks>
    public required Rectangle UnknownCharRect { get; init; }

    ///<summary>
    ///The width of any character that doesn't match the default width for its group.
    ///</summary>
    public Dictionary<char, int> specialWidths { get; init; } = null;

    ///<summary>
    ///After checking through capital + lowercase letters as well as numbers, the texture is read sequentially for special characters
    ///</summary>
    ///<remarks>
    ///The char designates the character in question, while the int designates its width
    ///</remarks>
    public required (char, int)[] SpecialCharacterOrder { get; init; }

    public int spacing = 1;

    private const string characterIndiciesString = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
    private const string danglingLetters = "gjpqy,";

    private Dictionary<char, Rectangle> charRects = new();

    private bool hasSetUpRectsYet = false;

    public void SetUpRects()
    {
        if (hasSetUpRectsYet)
        {
            return;
        }
        int xPos = 0;
        int yPos = 0;
        int xOffset = 0;
        if (TextureDimensions != null)
        {
            xOffset = TextureDimensions.Value.X;
            yPos = TextureDimensions.Value.Y;
        }
        for (int i = 0; i < characterIndiciesString.Length; i++)
        {
            if (TextureDimensions != null)
            {
                yPos += (DanglingHeight + 1) * (xPos / TextureDimensions.Value.Width);
                if (xPos >= TextureDimensions.Value.Width)
                {
                    xPos = 0;
                }
            }
            else
            {
                yPos += (DanglingHeight + 1) * (xPos / Texture.Width);
                if (xPos >= Texture.Width)
                {
                    xPos = 0;
                }
            }
            var rect = Rectangle.Empty;
            switch (i)
            {
                case var expression when i < 26:
                    rect.Width = CapitalLetterWidth;
                    break;
                case var expression when i > 25 && i < 52:
                    rect.Width = LowercaseLetterWidth;
                    break;
                case var expression when i > 51:
                    rect.Width = NumberWidth;
                    break;
            }
            if (specialWidths.ContainsKey(characterIndiciesString[i]))
            {
                rect.Width = specialWidths[characterIndiciesString[i]];
            }
            if (danglingLetters.Contains(characterIndiciesString[i]))
            {
                rect.Height = DanglingHeight;
            }
            else
            {
                rect.Height = DefaultHeight;
            }
            rect.X = xPos + xOffset;
            rect.Y = yPos;
            charRects.Add(characterIndiciesString[i], rect);
            xPos += rect.Width + 1;
        }
        for (int i = 0; i < SpecialCharacterOrder.Length; i++)
        {
            if (TextureDimensions != null)
            {
                yPos += (DanglingHeight + 1) * (xPos / TextureDimensions.Value.Width);
                if (xPos >= TextureDimensions.Value.Width)
                {
                    xPos = 0;
                }
            }
            else
            {
                yPos += (DanglingHeight + 1) * (xPos / Texture.Width);
                if (xPos >= Texture.Width)
                {
                    xPos = 0;
                }
            }
            var rect = Rectangle.Empty;
            rect.Width = SpecialCharacterOrder[i].Item2;
            if (danglingLetters.Contains(SpecialCharacterOrder[i].Item1))
            {
                rect.Height = DanglingHeight;
            }
            else
            {
                rect.Height = DefaultHeight;
            }
            rect.X = xPos + xOffset;
            rect.Y = yPos;
            charRects.Add(SpecialCharacterOrder[i].Item1, rect);
            xPos += rect.Width + 1;
        }
        hasSetUpRectsYet = true;
    }

    ///<returns>
    ///The width of the character just drawn
    ///</returns>
    public int Draw(SpriteBatch spriteBatch, char character, Vector2 pos, float scale, Color color)
    {
        if (!hasSetUpRectsYet)
        {
            SetUpRects();
        }
        Rectangle rect = UnknownCharRect;
        if (charRects.TryGetValue(character, out var charRect))
        {
            rect = charRect;
        }
        spriteBatch.Draw(Texture, pos, rect, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        return rect.Width;
    }

    public int CharacterWidth(char character)
    {
        if (charRects.TryGetValue(character, out var charRect))
        {
            return charRect.Width;
        }
        return UnknownCharRect.Width;
    }
}
