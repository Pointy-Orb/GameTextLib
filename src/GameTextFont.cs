using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameTextLib;

public class GameTextFont
{
    public required Texture2D Texture { get; init; }

    ///<summary>
    ///If the is contained in a region of the texture instead of the whole texture, specify that region here
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
    private const string danglingLetters = "Qgjpqy,";

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
        int textureWidth = 0;
        for (int i = 0; i < characterIndiciesString.Length; i++)
        {
            if (TextureDimensions != null)
            {
                textureWidth = TextureDimensions.Value.Width;
                yPos += (DanglingHeight + 1) * (xPos / TextureDimensions.Value.Width);
                if (xPos >= TextureDimensions.Value.Width)
                {
                    xPos = 0;
                }
            }
            else
            {
                textureWidth = Texture.Width;
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
            if (rect.Width + xPos > textureWidth)
            {
                yPos += (DanglingHeight + 1) * ((rect.Width + xPos) / Texture.Width);
                xPos = 0;
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
            if (rect.Width + xPos > textureWidth)
            {
                yPos += (DanglingHeight + 1) * ((rect.Width + xPos) / Texture.Width);
                xPos = 0;
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
    public int Draw(SpriteBatch spriteBatch, char character, Vector2 pos, float scale, Color color, Casing casing = Casing.Normal)
    {
        if (!hasSetUpRectsYet)
        {
            SetUpRects();
        }
        Rectangle rect = UnknownCharRect;
        var casedChar = character;
        switch (casing)
        {
            case Casing.lowercase:
                casedChar = casedChar.ToLower();
                break;
            case Casing.UPPERCASE:
                casedChar = casedChar.ToUpper();
                break;
        }
        if (charRects.TryGetValue(casedChar, out var charRect))
        {
            rect = charRect;
        }
        spriteBatch.Draw(Texture, pos, rect, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        return rect.Width;
    }

    public int CharacterWidth(char character, Casing casing = Casing.Normal)
    {
        var casedChar = character;
        switch (casing)
        {
            case Casing.lowercase:
                casedChar = casedChar.ToLower();
                break;
            case Casing.UPPERCASE:
                casedChar = casedChar.ToUpper();
                break;
        }
        if (charRects.TryGetValue(casedChar, out var charRect))
        {
            return charRect.Width;
        }
        return UnknownCharRect.Width;
    }

    public void DrawInput(SpriteBatch spriteBatch, StringBuilder input, Vector2 pos, Color color, TextAnchor anchor = TextAnchor.Center, float scale = 1f)
    {
        Point size = TextBodySize(input, scale);
        var textPos = pos;
        switch (anchor)
        {
            case TextAnchor.TopLeft:
            case TextAnchor.Top:
            case TextAnchor.TopRight:
                break;
            case TextAnchor.Left:
            case TextAnchor.Center:
            case TextAnchor.Right:
                textPos.Y -= size.Y / 2;
                break;
            case TextAnchor.BottomLeft:
            case TextAnchor.Bottom:
            case TextAnchor.BottomRight:
                textPos.Y -= size.Y;
                break;
        }
        switch (anchor)
        {
            case TextAnchor.TopLeft:
            case TextAnchor.Left:
            case TextAnchor.BottomLeft:
                break;
            case TextAnchor.Top:
            case TextAnchor.Center:
            case TextAnchor.Bottom:
                textPos.X -= size.X / 2;
                break;
            case TextAnchor.TopRight:
            case TextAnchor.Right:
            case TextAnchor.BottomRight:
                textPos.X -= size.X;
                break;
        }
        int textLength = input.Length;
        bool seenALetter = false;
        float startX = textPos.X;
        for (int i = 0; i < textLength; i++)
        {
            seenALetter |= input[i] != ' ';
            if (!seenALetter)
            {
                continue;
            }
            Draw(spriteBatch, input[i], textPos, scale, color);
            textPos.X += CharacterWidth(input[i]) * scale;
            textPos.X += spacing * scale;
        }
    }

    List<int> widths = new();

    public Point TextBodySize(StringBuilder builder, float scale = 1f)
    {
        widths.Clear();
        int width = 0;
        int height = DefaultHeight;
        int textLength = builder.Length;
        bool seenALetter = false;
        for (int i = 0; i < builder.Length; i++)
        {
            seenALetter |= builder[i] != ' ';
            if (!seenALetter)
            {
                continue;
            }
            width += CharacterWidth(builder[i]);
            if (i < textLength - 1)
            {
                width += spacing;
            }
        }
        foreach (int lineWidth in widths)
        {
            if (width < lineWidth)
            {
                width = lineWidth;
            }
        }
        return new Point((int)(width * scale), (int)(height * scale));
    }
}

public enum Casing
{
    Normal,
    lowercase,
    UPPERCASE,
}
