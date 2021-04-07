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
        ShowStorePanel,//TODO not tested
        ToggleAlternateTextPos,
        HighLightUI,
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
