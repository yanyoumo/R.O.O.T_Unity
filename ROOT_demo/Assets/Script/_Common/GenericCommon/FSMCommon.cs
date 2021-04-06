using UnityEngine;
using static ROOT.TextProcessHelper;

public static class FSMLevelLogic_Common
{
    public static string ProcessText(string Text)
    {
        Text = Text.Replace("\\n", "\n");
        Text = Text.Replace("单元", "<b>[单元]</b>");
        Text = Text.Replace("方形", "<b>[方形]</b>");
        Text = Text.Replace("圆形", "<b>[圆形]</b>");
        Text = Text.Replace("周期", "<b>[周期]</b>");
        Text = Text.Replace("一般数据", TMPNormalDataCompo());
        Text = Text.Replace("网络数据", TMPNetworkDataCompo());
        Text = Text.Replace("收入/损失", TmpBracketAndBold(TmpColorXml("收入", Color.green * 0.4f) + "/" + TmpColorGreenXml("损失")));
        Text = Text.Replace("绿色", TmpBracketAndBold(TmpColorXml("绿色", Color.green * 0.4f)));
        Text = Text.Replace("红色", TmpColorXml("红色", Color.red));
        ColorUtility.TryParseHtmlString("#71003E", out Color col);
        Text = Text.Replace("深紫色", TmpColorXml("深紫色", col));
        return Text;
    }
}
