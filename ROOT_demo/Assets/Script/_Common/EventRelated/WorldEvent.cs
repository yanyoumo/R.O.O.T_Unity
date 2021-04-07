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

        public static string BalancingSignalSetupInquiry = "BalancingSignalSetupInquiry";
        public static string AcquiringCostTargetInquiry = "AcquiringCostTargetInquiry";
        public static string BoardGridThermoZoneInquiry = "BoardGridThermoZoneInquiry";

        public static string MainCameraReadyEvent = "MainCameraReadyEvent";
        public static string InGameOverlayToggleEvent = "InGameOverlayToggleEvent";
        
        /// <summary>
        /// 主要是UI方面的事件、主要是可以向核心逻辑调查一些数据；可以放一个回调函数。
        /// </summary>
        /*public static class Visual_Inquiry_Event
        {
            //还是优先使用Cache流程吧、Cache搞不定的情况再Inquiry。
            //public static string CurrencyInquiryEvent = "CurrencyInquiryEvent";
        }*/
        
        public static string BoardShouldUpdateEvent = "BoardShouldUpdateEvent";
        public static string BoardUpdatedEvent = "BoardUpdatedEvent";
        public static string BoardReadyEvent = "BoardReadyEvent";

        public static string ControllingEvent = "ControllingEvent";
        public static string ShopTierOffsetChangedEvent = "ShopTierOffsetChangedEvent";
        public static string HighLightingUIChangedEvent = "TutorialHighLightingStatusChangedEvent";
    }

}