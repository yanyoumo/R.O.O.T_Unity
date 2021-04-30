using com.ootii.Messages;
using ROOT.Message;
using static ROOT.WorldEvent;

namespace ROOT.UI
{
    /*public interface RoundRelatedUI
    {
        public StageType StageType => StageType.Shop;
    }*/
    
    public abstract class RoundRelatedUIBase : HideableUI
    {
        protected StageType StageType = StageType.Shop;
        
        protected virtual void RoundTypeChangedHandler(IMessage rmessage)
        {
            if (rmessage is TimingEventInfo info)
            {
                StageType = info.CurrentStageType;
            }
        }
        
        protected override void Awake()
        {
            base.Awake();
            MessageDispatcher.AddListener(InGameStageChangedEvent, RoundTypeChangedHandler);
        }

        protected override void OnDestroy()
        {
            MessageDispatcher.RemoveListener(InGameStageChangedEvent, RoundTypeChangedHandler);
            base.OnDestroy();
        }
    }

}