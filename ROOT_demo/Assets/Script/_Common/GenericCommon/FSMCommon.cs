using UnityEngine;
using static ROOT.TextProcessHelper;

namespace ROOT.Common
{
    public enum RootFSMStatus
    {
        //这里写全部的、Root系列中、全部可以使用的潜在状态。
        PreInit, //FSM逻辑在初始化完成之前“阻塞性”逻辑、原则上里面不写实际逻辑。
        MajorUpKeep, //查询玩家的输入事件、并且进行基础的清理、更新逻辑。
        MinorUpKeep, //即使在Animate流程也会执行的逻辑部分、主要是查询是否有打断输入。
        R_Cycle, //倒行逻辑的部分。
        F_Cycle, //整形逻辑的核心逻辑、主要是执行具体的主干更新、数据更新等等。
        Career_Cycle, //现有“职业”模式需要的逻辑、包含但不限于对时间轴数据的更新、等等。
        Tutorial_Cycle, //教程相关流程的演进。
        R_IO, //ReactToIO、对从Driver获得的CtrlPack转换成实际执行的逻辑。
        Skill, //这个是在使用某些技能的时候需要进行Upkeep的代码。
        TelemetryPause, //在Boss暂停的时候执行的代码。
        Animate, //将动画向前执行一帧、但是Root的动画流程时绑定时间而不是绑定帧数的。
        CleanUp, //将所有FSM的类数据重置、并且是FSM流程等待一帧的充分条件。
        COUNT, //搁在最后、计数的。
    }

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

}
