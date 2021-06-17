using System;

namespace ROOT
{
    [Flags]
    public enum EdgeStatus
    {
        //这个东西有个隐含的需要优先级（队列）的设计。怎么搞？
        //队列还是分层？可能要分层。有了分层还要有顺序的概念。
        //目前这个顺序干脆就设计成这个enum从下往上的逻辑、或者得弄一个数列。
        Off = 0,
        InfoZone = 1 << 0,
        SingleInfoZone = 1 << 1,
    }
    
    public enum HintEventType
    {
        SetGoalContent = 0,
        SetGoalCheckListShow = 1,
        SetTutorialTextContent = 2,
        SetTutorialTextShow = 3,
        GoalComplete = 4,
        GoalFailed = 5,
        SetHelpScreenShow = 6,
        NextIsEnding = 7,
        ToggleHandOnView = 8,
        ToggleAlternateTextPos = 9,
        ToggleAlternateCheckGoal = 11,
        ControllerBlockedAlert = 10,
    }

    public enum SupportedScreenRatio
    {
        XGA, //4:3/iPadAir2/iPadPro4Gen
        HD, //16:9/StandAlone/iPhone7Plus/iPhone6
        AppleHD, //2.1645:1/iPhoneX/iPhoneXR/iPhone11Pro/iPhoneXSMax
    }

    public enum InputScheme
    {
        Keyboard,
        Mouse,
        TouchScreen,
    }

    public interface IClickable
    {
        void Clicked();
    }

    public enum RotationDirection
    {
        //即所为当前状态的表示，也作为旋转动作的标识
        //顺时针为正：±0，+90，-90，±180
        North,
        East,
        West,
        South
    }
}