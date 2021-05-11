namespace ROOT
{
    public enum LevelType
    {
        Tutorial,
        Career,
        //Classic
    }

    public enum TutorialActionType
    {
        //这个的顺序不能变！
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
        AutoProceedRound = 22
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
