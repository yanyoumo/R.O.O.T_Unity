using UnityEngine;

namespace ROOT
{
    public static class TextProcessHelper
    {
        public static string TmpColorBlueXml(string content)
        {
            return TmpColorXml(content, Color.blue);
        }
        public static string TmpColorGreenXml(string content)
        {
            return TmpColorXml(content, Color.green * 0.35f);
        }
        public static string TmpColorXml(string content, Color col)
        {
            var hexCol = ColorUtility.ToHtmlStringRGB(col);
            return "<color=#" + hexCol + ">" + content + "</color>";
        }
        public static string TmpColorBold(string content)
        {
            return "<b>" + content + "</b>";
        }
        public static string TmpBracket(string content)
        {
            return "[" + content + "]";
        }
        public static string TmpBracketAndBold(string content) 
        {
            return TmpColorBold("[" + content + "]");
        }
        public static string TMPNormalDataCompo()
        {
            return TmpBracketAndBold(TmpColorGreenXml("一般数据"));
        }
        public static string TMPNetworkDataCompo()
        {
            return TmpBracketAndBold(TmpColorBlueXml("网络数据"));
        }
    }
}