using System;

namespace ROOT
{
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

    public enum LevelStatus
    {
        Locked = 0,
        Unlocked = 1,
        Played = 2,
        Passed = 3,
    }
}