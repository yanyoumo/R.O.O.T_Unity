using com.ootii.Messages;
using ROOT.Common;
using ROOT.Message;
using ROOT.Message.Inquiry;
using UnityEngine;
using static ROOT.WorldEvent;

namespace ROOT.UI
{
    public abstract class HideableUI : MonoBehaviour
    {
        protected abstract UITag UITag { get; }

        private void HidingEventHandler(IMessage rMessage)
        {
            if (rMessage is ToggleGameplayUIData info)
            {
                if (info.Set)
                {
                    if (UITag == info.UITag)
                    {
                        gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (info.SelectAll || UITag == info.UITag)
                    {
                        gameObject.SetActive(false);
                    }
                }
            }
        }

        protected virtual void Awake()
        {
            MessageDispatcher.AddListener(ToggleGamePlayUIEvent, HidingEventHandler);
        }

        protected virtual void OnDestroy()
        {
            MessageDispatcher.RemoveListener(ToggleGamePlayUIEvent, HidingEventHandler);
        }
    }
}