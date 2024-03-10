using System.IO;

namespace ExtraLandscapingTools;

class ELT_UI
{
    internal static string GetStringFromEmbbededJSFile(string path) {
        return new StreamReader(ELT.GetEmbedded("UI."+path)).ReadToEnd();
    }
}