using System.Collections.Generic;

namespace GameTextLib;

public class Locale
{
    public Dictionary<string, TextBank> data;
}

public class TextBank
{
    public string[] text = null;
    public Dictionary<string, TextBank> children = null;
}
