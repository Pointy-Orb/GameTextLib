using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameTextLib;

public class GameText
{
    private GameTextFont font;
    private TextBank textBank;

    public int messageIndex = 0;

    private string _subBank;
    public string SubBank
    {
        get => _subBank;
        set
        {
            _subBank = value;
            SplitChar = SplitChar;
        }
    }

    public char newLineChar = '\n';
    private char? _splitChar;
    public char? SplitChar
    {
        get => _splitChar;
        set
        {
            _splitChar = value;
            LineCount = 0;
            for (int i = 0; i < CurrentMessage.Length; i++)
            {
                if (CurrentMessage[i] == _splitChar)
                {
                    LineCount++;
                }
            }
        }
    }
    public int lineIndex = 0;
    public int LineCount { get; private set; }
    public int MessageCount => CurrentBank.Length;

    public ReadOnlySpan<char> CurrentLine
    {
        get
        {
            if (SplitChar == null)
            {
                return CurrentMessage.AsSpan().Trim().Trim('\t');
            }
            int currentLine = 0;
            int startIndex = 0;
            int length = 0;
            var currentMessage = CurrentMessage.AsSpan().Trim().Trim('\t');
            currentMessage.Trim(SplitChar.Value);
            for (int i = 0; i < CurrentMessage.Length; i++)
            {
                if (currentMessage[i] == SplitChar)
                {
                    currentLine++;
                    continue;
                }
                if (currentLine == lineIndex && startIndex == 0)
                {
                    startIndex = i;
                }
                if (currentLine == lineIndex)
                {
                    length++;
                }
            }
            return currentMessage.Slice(startIndex, length);
        }
    }

    public GameText(GameTextFont font, TextBank textBank)
    {
        this.font = font;
        this.textBank = textBank;
    }

    public void ChangeTextBank(TextBank newTextBank)
    {
        if (newTextBank != null)
        {
            textBank = newTextBank;
        }
    }

    public string CurrentMessage => CurrentBank[messageIndex];
    private string[] CurrentBank => SubBank == null ? textBank.text : textBank.children[SubBank].text;

    public void Draw(SpriteBatch spriteBatch, Vector2 pos, Color color, TextAnchor anchor = TextAnchor.Center, float scale = 1f)
    {
        Point size = TextBodySize(scale);
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
        int textLength = 0;
        if (SubBank == null)
        {
            textLength = textBank.text[messageIndex].Length;
        }
        else
        {
            textLength = textBank.children[SubBank].text[messageIndex].Length;
        }
        bool seenALetter = false;
        float startX = textPos.X;
        for (int i = 0; i < textLength; i++)
        {
            if (CurrentLine[i] == newLineChar)
            {
                textPos.Y += font.DefaultHeight;
                textPos.X = startX;
                seenALetter = false;
                continue;
            }
            seenALetter |= CurrentLine[i] != ' ';
            if (!seenALetter)
            {
                continue;
            }
            font.Draw(spriteBatch, CurrentLine[i], textPos, scale, color);
            textPos.X += font.CharacterWidth(textBank.text[messageIndex][i]) * scale;
            textPos.X += font.spacing * scale;
        }
    }

    List<int> widths = new();

    public Point TextBodySize(float scale = 1f)
    {
        widths.Clear();
        int width = 0;
        int height = font.DefaultHeight;
        int textLength = CurrentMessage.Length;
        bool seenALetter = false;
        for (int i = 0; i < textLength; i++)
        {
            if (CurrentLine[i] == newLineChar)
            {
                height += font.DefaultHeight;
                widths.Add(width);
                width = 0;
                seenALetter = false;
                continue;
            }
            seenALetter |= CurrentLine[i] != ' ';
            if (!seenALetter)
            {
                continue;
            }
            width += font.CharacterWidth(textBank.text[messageIndex][i]);
            if (i < textLength - 1)
            {
                width += font.spacing;
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

public enum TextAnchor
{
    TopLeft,
    Top,
    TopRight,
    Left,
    Center,
    Right,
    BottomLeft,
    Bottom,
    BottomRight,
}
