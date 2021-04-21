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
        Text,
        CreateUnit,
        CreateCursor,
        ShowText,
        HideText,
        End,
        ShowCheckList,
        HideCheckList,
        HandOn,
        SetUnitStationary,
        ShowStorePanel,
        ToggleAlternateTextPos,
        HighLightUI,
        MoveCursorToPos,
        MoveCursorToUnitByTag,
        SetTimeline,
        ResetStep,//实际是重置“视在Step”、RawStep目前也不让重设。
        HighLightGrid,
        ToggleGameplayUI,
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
