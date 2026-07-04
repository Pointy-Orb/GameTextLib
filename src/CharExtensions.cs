using System;
using System.Collections.Generic;
using System.Text;

namespace GameTextLib;

public static class CharExtensions
{
    public static char ToUpper(this char character)
    {
        for (int i = 0; i < lowerMap.Length; i++)
        {
            if (lowerMap[i] == character)
            {
                return upperMap[i];
            }
        }
        return character;
    }

    public static char ToLower(this char character)
    {
        for (int i = 0; i < lowerMap.Length; i++)
        {
            if (upperMap[i] == character)
            {
                return lowerMap[i];
            }
        }
        return character;
    }

    public static int GetNum(this char character)
    {
        for (int i = 0; i < numMap.Length; i++)
        {
            if (character == numMap[i])
            {
                return i;
            }
        }
        return -1;
    }

    private const string numMap = "0123456789";
    private const string lowerMap = "abcdefghijklmnopqrstuvwxyz";
    private const string upperMap = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
}
