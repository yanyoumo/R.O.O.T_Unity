namespace ROOT
{
    public enum HintEventType
    {
        SetGoalContent,
        SetGoalCheckListShow,
        SetTutorialTextContent,
        SetTutorialTextShow,
        GoalComplete,
        GoalFailed,
        SetHelpScreenShow,
        NextIsEnding,
        ToggleHandOnView,
        ToggleAlternateTextPos,
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
    
    public enum PosSetFlag
    {
        NONE,
        Current,
        Next,
        Lerping,
        NextAndLerping,
        CurrentAndLerping,
        CurrentAndNext,
        All,
    }

    public enum RotateCommand
    {
        NOP,
        Clockwise,
        CounterClockwise,
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