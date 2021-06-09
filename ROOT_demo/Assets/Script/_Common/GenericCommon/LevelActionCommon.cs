using System;

namespace ROOT
{
    public enum LevelType
    {
        Tutorial,
        Career,
        //Classic
    }

    [Flags]
    public enum LevelFeature //这个认为是纯表现的内容，不和任何实际逻辑相关。现在先凑八个。
    {
        Tutorial = 1 << 0,
        BasicPlayable = 1 << 1,
        RoundBase = 1 << 2,
        SelectableUnit = 1 << 3,
        Boss = 1 << 4,
        Acquire = 1 << 5,
        Telemetry = 1 << 6,
        PlaceHolderA = 1 << 7,
    }

    public enum TutorialActionType
    {
        //这个的顺序不能变！也不完全是了、现在后面放一个int，这个对应就好了。
        Text = 0,
        CreateUnit = 1,
        CreateCursor = 2,
        ShowText = 3,
        HideText = 4,
        End = 5,
        ShowCheckList = 6,
        HideCheckList = 7,
        HandOn = 8,
        SetUnitStationary = 9,
        ToggleFSMCoreFeat = 10,
        ToggleAlternateTextPos = 11,
        HighLightUI = 12,
        MoveCursorToPos = 13,
        MoveCursorToUnitByTag = 14,
        SetTimeline = 15,
        ResetStep = 16, //实际是重置“视在Step”、RawStep目前也不让重设。
        HighLightGrid = 17,
        ToggleGameplayUI = 18,
        DeleteUnit = 19,
        ToggleTutorialHintPage = 20,
        ToggleAlternateHandsOnGoal = 21,
        AutoProceedToNextStage = 22,
        SetSkillEnabling = 23
    }

    public enum BossStageType
    {
        Telemetry,
        Acquiring,
    }

    public enum StageType{
        Shop,
        Require,
        Destoryer,
        Boss,
        Ending,
    }
    
    public enum RoundType
    {
        Normal,
        Boss,//Telemetry...So on.
    }
}
