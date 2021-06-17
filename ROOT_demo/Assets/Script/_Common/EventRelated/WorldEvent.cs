namespace ROOT
{
    public static class WorldEvent
    {
        public static string InGameStageWarningEvent = "InGameStageWarningEvent";
        public static string InGameStageChangedEvent = "InGameStageChangedEvent";
        public static string CurrencyIOStatusChangedEvent = "CurrencyIOStatusChangedEvent";
        public static string CurrencyUpdatedEvent = "CurrencyUpdatedEvent";
        public static string BoardSignalUpdatedEvent = "BoardSignalUpdatedEvent";
        public static string HintRelatedEvent = "HintRelatedEvent";
        public static string BoardGridHighLightSetEvent = "BoardGridHighLightSetEvent";

        public static string CurrentSignalTypeInquiry = "CurrentSignalTypeInquiry";
        public static string BalancingSignalSetupInquiry = "BalancingSignalSetupInquiry";
        public static string AcquiringCostTargetInquiry = "AcquiringCostTargetInquiry";
        public static string BoardGridThermoZoneInquiry = "BoardGridThermoZoneInquiry";

        public static string MainCameraReadyEvent = "MainCameraReadyEvent";
        public static string ApparentStepResetedEvent="ApparentStepResetedEvent";
        
        public static string GamePauseEvent = "GamePauseEvent";
        public static string RequestGamePauseEvent = "RequestGamePauseEvent";
        public static string RequestLevelQuitEvent = "RequestLevelQuitEvent";
        
        public static string InGameOverlayToggleEvent = "InGameOverlayToggleEvent";
        //public static string TelemetryInfoZoneToggleEvent = "TelemetryInfoZoneToggleEvent";
        public static string ToggleGamePlayUIEvent = "ToggleGamePlayUIEvent";
        public static string HintScreenChangedEvent = "HintScreenChangedEvent";
        public static string HelpScreenShouldAlertEvent = "HelpScreenShouldAlertEvent";
        public static string TutorialMissionShouldAlertEvent = "TutorialMissionShouldAlertEvent";
        public static string ScanUnitLockChangedEvent = "ScanUnitLockChangedEvent";

        public static string ToggleHintUIUpEvent = "ToggleHintUIUpEvent";
        public static string ToggleHintUIDownEvent = "ToggleHintUIDownEvent";
        public static string UIShouldMakeWay = "UIShouldMakeWay";
        public static string UICouldResume = "UICouldResume";
        
        public static string BoardShouldUpdateEvent = "BoardShouldUpdateEvent";
        public static string BoardUpdatedEvent = "BoardUpdatedEvent";
        public static string BoardReadyEvent = "BoardReadyEvent";

        public static string ControllingEvent = "ControllingEvent";
        public static string ShopTierOffsetChangedEvent = "ShopTierOffsetChangedEvent";
        public static string HighLightingUIChangedEvent = "TutorialHighLightingStatusChangedEvent";

        public static string CursorMovedEvent = "CursorMovedEvent";
    }
}