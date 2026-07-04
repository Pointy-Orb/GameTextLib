using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameTextLib;

public class GameText
{
    private GameTextFont font;
    private TextBank textBank;

    public int messageIndex = 0;

    private string _subBank;

    public object[] Substitutions { get; private set; }

    public bool HasSubBank(string bank)
    {
        return textBank.children.ContainsKey(bank);
    }

    public string SubBank
    {
        get => _subBank;
        set
        {
            _subBank = value;
            SplitChar = SplitChar;
            lineIndex = 0;
            messageIndex = 0;
        }
    }

    public Casing casing = Casing.Normal;

    public char newLineChar = '\n';
    private char? _splitChar;
    public char? SplitChar
    {
        get => _splitChar;
        set
        {
            _splitChar = value;
            if (value == null)
            {
                return;
            }
        }
    }
    public int lineIndex = 0;
    public int LineCount
    {
        get
        {
            if (SplitChar == null)
            {
                return 1;
            }
            int lineCount = 1;
            var currentMessage = CurrentMessage.Trim(SplitChar.Value);
            for (int i = 0; i < currentMessage.Length; i++)
            {
                if (currentMessage[i] == _splitChar)
                {
                    lineCount++;
                }
            }
            return lineCount;
        }
    }
    public int MessageCount => CurrentBank.Length;

    public ReadOnlySpan<char> CurrentLine
    {
        get
        {
            if (SplitChar == null)
            {
                return CurrentMessage;
            }
            int currentLine = 0;
            int startIndex = 0;
            int length = 0;
            var currentMessage = CurrentMessage.Trim(SplitChar.Value);
            for (int i = 0; i < currentMessage.Length; i++)
            {
                if (currentMessage[i] == SplitChar)
                {
                    currentLine++;
                    continue;
                }
                if (currentLine == lineIndex && currentLine != 0 && startIndex == 0)
                {
                    startIndex = i;
                }
                if (currentLine == lineIndex)
                {
                    length++;
                }
            }
            return currentMessage.Slice(startIndex, length).Trim('\t');
        }
    }

    public GameText(GameTextFont font, TextBank textBank)
    {
        this.font = font;
        this.textBank = textBank;
        SetupTextBank(textBank);
    }

    public void ChangeTextBank(TextBank newTextBank)
    {
        if (newTextBank != null)
        {
            textBank = newTextBank;
        }
        SetupTextBank(newTextBank);
    }

    private void SetupTextBank(TextBank newTextBank)
    {
        int substitutionCount = 0;
        for (int i = 0; i < newTextBank.text.Length; i++)
        {
            var message = newTextBank.text[i];
            for (int j = 0; j < message.Length; j++)
            {
                int left = j - 1;
                int right = j + 1;
                if (left < 0 || right >= message.Length)
                {
                    continue;
                }
                if (message[left] != '{' || message[right] != '}')
                {
                    continue;
                }
                int subNum = message[j].GetNum();
                if (subNum < 0)
                {
                    continue;
                }
                if (substitutionCount < subNum + 1)
                {
                    substitutionCount = subNum + 1;
                }
            }
        }
        Substitutions = new object[substitutionCount];
    }

    public void ChangeFont(GameTextFont newFont)
    {
        if (newFont != null)
        {
            font = newFont;
        }
    }

    public ReadOnlySpan<char> CurrentMessage => CurrentBank[messageIndex].AsSpan().Trim().Trim('\t');
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
        int textLength = CurrentLine.Length;
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
            bool isSubstitution = ActOnSubstitution(
                CurrentLine,
                i,
                (letter) =>
                {
                    font.Draw(spriteBatch, letter, textPos, scale, color, casing);
                    textPos.X += font.CharacterWidth(letter, casing) * scale;
                    textPos.X += font.spacing * scale;
                }
            );
            if (isSubstitution)
            {
                i += 2;
                continue;
            }
            font.Draw(spriteBatch, CurrentLine[i], textPos, scale, color, casing);
            textPos.X += font.CharacterWidth(CurrentLine[i], casing) * scale;
            textPos.X += font.spacing * scale;
        }
    }

    List<int> widths = new();

    public Point TextBodySize(float scale = 1f)
    {
        widths.Clear();
        int width = 0;
        int height = font.DefaultHeight;
        int textLength = CurrentLine.Length;
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
            bool isSubstitution = ActOnSubstitution(
                CurrentLine,
                i,
                (letter) =>
                {
                    width += font.CharacterWidth(letter, casing);
                }
            );
            if (isSubstitution)
            {
                i += 2;
                continue;
            }
            width += font.CharacterWidth(CurrentLine[i], casing);
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

    private bool ActOnSubstitution(ReadOnlySpan<char> message, int i, Action<char> act)
    {
        if (message[i] != '{')
        {
            return false;
        }
        if (i + 2 >= message.Length)
        {
            return false;
        }
        if (message[i + 2] != '}')
        {
            return false;
        }
        var subNum = message[i + 1].GetNum();
        if (subNum < 0 || subNum >= Substitutions.Length)
        {
            return false;
        }
        var substitution = Substitutions[subNum];
        if (substitution is string subString)
        {
            for (int j = 0; j < subString.Length; j++)
            {
                bool subSub = ActOnSubstitution(subString.AsSpan(), j, act);
                if (subSub)
                {
                    j += 2;
                    continue;
                }
                act.Invoke(subString[j]);
            }
        }
        return true;
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
