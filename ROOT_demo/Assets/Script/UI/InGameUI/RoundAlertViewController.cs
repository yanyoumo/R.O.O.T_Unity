using System.Collections;
using com.ootii.Messages;
using Doozy.Engine.UI;
using I2.Loc;
using ROOT.Consts;
using ROOT.Message;
using TMPro;
using UnityEngine;


namespace ROOT.UI
{
    public class RoundAlertViewController : MonoBehaviour
    {
        public UIView ViewFrame;
        public TextMeshProUGUI TextContent;

        private StageType currentStageType = StageType.Shop;
        private StageType nextStageType = StageType.Shop;
        private string currentStageString=>LocalizationManager.GetTranslation(StaticName.GetNameTermForStage(currentStageType));
        private string nextStageString=>LocalizationManager.GetTranslation(StaticName.GetNameTermForStage(nextStageType));
        private string nextStageCountString => StaticNumericData.StageWarningThreshold.ToString("D");

        private readonly float RoundAlertPersistTime = 1.5f;

        private IEnumerator AlertViewTimer()
        {
            yield return new WaitForSeconds(RoundAlertPersistTime);
            ViewFrame.Hide();
        } 
        
        private void DisplayRoundAlertOnce()
        {
            TextContent.text = "距离" + currentStageString + "阶段结束还有[" + nextStageCountString + "]时刻";
            ViewFrame.Show();
            StartCoroutine(AlertViewTimer());
        }
        
        private void StageWaringHandler(IMessage rMessage)
        {
            if (rMessage is TimingEventInfo info)
            {
                currentStageType = info.CurrentStageType;
                nextStageType = info.NextStageType;
                DisplayRoundAlertOnce();
            }
        }
        
        private void Awake()
        {
            MessageDispatcher.AddListener(WorldEvent.InGameStageWarningEvent, StageWaringHandler);
        }

        private void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.InGameStageWarningEvent, StageWaringHandler);
        }
    }
}

