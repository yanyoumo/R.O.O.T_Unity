using System;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Messages;
using I2.Loc;
using ROOT.SetupAsset;
using UnityEngine;
using static ROOT.TextProcessHelper;


namespace ROOT
{
    using FSMActions = Dictionary<RootFSMStatus, Action>;
    using Trans = RootFSMTransition;
    using FSMTransitions = HashSet<RootFSMTransition>;

    public abstract class FSMTutorialLogic : FSMLevelLogic_Barebone
    {
        protected sealed override string SucceedEndingTerm => ScriptTerms.EndingMessageTutorial;
        protected sealed override string FailedEndingTerm => ScriptTerms.EndingMessageTutorialFailed;
        public override bool IsTutorial => true;
        public override bool CouldHandleSkill => true;
        public override bool CouldHandleBoss => false;
        public override BossStageType HandleBossType => throw new ArgumentException("could not handle Boss");
        
        protected bool AllUnitConnected()
        {
            return LevelAsset.GameBoard.Units.All(u => u.AnyConnection);
        }
        
        #region TutorialRelated

        protected bool ActionEnded { get; private set; } = false;
        protected int ActionIndex { get; private set; } = -1;
        private int LastActionCount { get; set; } = 0;

        protected bool LevelCompleted = false;
        protected bool LevelFailed = false;
        protected bool PlayerRequestedEnd = false;
        protected bool PlayerRequestedQuit = false;

        protected bool OnceFlagA = false;
        protected bool OnceFlagB = false;

        protected abstract string MainGoalEntryContent { get; }
        protected virtual string SecondaryGoalEntryContent { get; } = "";

        private LevelActionAsset LevelActionAsset => LevelAsset.ActionAsset;

        private bool ShowText 
        {
            set => SendHintData(HintEventType.ShowTutorialTextFrame, value);
        }

        private bool ShowCheckList
        {
            set => SendHintData(HintEventType.ShowGoalCheckList, value);
        }

        protected sealed override bool CheckGameOver
        {
            get
            {
                if (LevelCompleted && PlayerRequestedEnd)
                {
                    LevelAsset.TutorialCompleted = true;
                    return true;
                }

                if (LevelFailed && PlayerRequestedQuit)
                {
                    LevelAsset.TutorialCompleted = false;
                    return true;
                }

                return false;
            }
        }
        
        private void CreateUnitOnBoard(TutorialActionData data)
        {
            GameObject go = LevelAsset.GameBoard.InitUnit(Vector2Int.zero, data.Core, data.HardwareType,
                Utils.Shuffle(data.Sides), data.Tier);
            if (data.Pos.x < 0 || data.Pos.y < 0)
            {
                LevelAsset.GameBoard.DeliverUnitRandomPlace(go);
            }
            else
            {
                LevelAsset.GameBoard.DeliverUnitAssignedPlace(go, data.Pos);
            }

            LevelAsset.GameBoard.UpdateBoardUnit();
        }

        private void StepForward()
        {
            ActionIndex++;
        }

        private string ProcessText(string Text)
        {
            Text = Text.Replace("\\n", "\n");
            Text = Text.Replace("单元", "<b>[单元]</b>");
            Text = Text.Replace("方形", "<b>[方形]</b>");
            Text = Text.Replace("圆形", "<b>[圆形]</b>");
            Text = Text.Replace("周期", "<b>[周期]</b>");
            Text = Text.Replace("一般数据", TMPNormalDataCompo());
            Text = Text.Replace("网络数据", TMPNetworkDataCompo());
            Text = Text.Replace("收入/损失",
                TmpBracketAndBold(TmpColorXml("收入", Color.green * 0.4f) + "/" + TmpColorGreenXml("损失")));
            Text = Text.Replace("绿色", TmpBracketAndBold(TmpColorXml("绿色", Color.green * 0.4f)));
            Text = Text.Replace("红色", TmpColorXml("红色", Color.red));
            ColorUtility.TryParseHtmlString("#71003E", out Color col);
            Text = Text.Replace("深紫色", TmpColorXml("深紫色", col));
            return Text;
        }

        private void DisplayText(string text)
        {
            var hintData = new HintEventInfo
            {
                HintEventType = HintEventType.ShowTutorialTextFrame,
                StringData = text,
                BoolData = true,
            };
            MessageDispatcher.SendMessage(hintData);
        }

        protected override void AdditionalReactIO()
        {
            if (LevelCompleted)
            {
                //这段代码想着放到AdditionalRIO里面去。
                PlayerRequestedEnd = CtrlPack.HasFlag(ControllingCommand.Confirm);
            }
        }
        
        protected abstract void AdditionalDealStep(TutorialActionData data);

        /// <summary>
        /// Tutorial父类里面会为通用的动作做一个处理。如果没有会throw
        /// </summary>
        /// <param name="data">输入的TutorialActionData</param>
        private void DealStep(TutorialActionData data)
        {
            switch (data.ActionType)
            {
                case TutorialActionType.Text:
                    if (data.DoppelgangerToggle)
                    {
                        DisplayText(StartGameMgr.UseTouchScreen ? data.DoppelgangerText : data.Text);
                    }
                    else
                    {
                        DisplayText(data.Text);
                    }
                    break;
                case TutorialActionType.CreateUnit:
                    CreateUnitOnBoard(data);
                    break;
                case TutorialActionType.End:
                    ActionEnded = true;
                    break;
                case TutorialActionType.ShowText:
                    TutorialOnHand = false;
                    ShowText = true;
                    break;
                case TutorialActionType.HideText:
                    TutorialOnHand = true;
                    ShowText = false;
                    break;
                case TutorialActionType.ShowCheckList:
                    ShowCheckList = true;
                    break;
                case TutorialActionType.HideCheckList:
                    ShowCheckList = false;
                    break;
                default:
                    AdditionalDealStep(data);
                    break;
            }
        }

        private void DealStepMgr()
        {
            int actionLength = LevelActionAsset.Actions.Length;
            for (int i = LastActionCount; i < actionLength; i++)
            {
                if (LevelActionAsset.Actions[i].ActionIdx > ActionIndex)
                {
                    LastActionCount = i;
                    break;
                }

                //Debug.Log("LevelActionAsset.Actions[i].ActionIdx:"+LevelActionAsset.Actions[i].ActionIdx);
                DealStep(LevelActionAsset.Actions[i]);
            }
        }

        #endregion

        private bool shouldInitTutorial = true;
        private bool TutorialOnHand = false;

        private bool CompletedAndRequestedEnd()
        {
            return LevelCompleted && PlayerRequestedEnd;
        }
        
        private bool CheckTutorialCycle()
        {
            return CtrlPack.HasFlag(ControllingCommand.Confirm);
        }

        private bool CheckNotOnHand()
        {
            return !TutorialOnHand;
        }

        private void TutorialCycle()
        {
            //Debug.Log("TutorialCycle");
            if (!ActionEnded)
            {
                StepForward();
                DealStepMgr();
            }
        }

        protected virtual void TutorialMinorUpkeep()
        {

        }

        private void TutorialInit()
        {
            if (!shouldInitTutorial) return;
            shouldInitTutorial = false;
            //Debug.Log("TutorialInit");
            SendHintData(HintEventType.ShowTutorialTextFrame, false);
            MessageDispatcher.SendMessage(new HintEventInfo {HintEventType = HintEventType.ShowMainGoalContent, StringData = MainGoalEntryContent});
            MessageDispatcher.SendMessage(new HintEventInfo {HintEventType = HintEventType.ShowSecondaryGoalContent, StringData = SecondaryGoalEntryContent});
            StepForward();
            DealStepMgr();
        }

        protected override void AdditionalMajorUpkeep()
        {
            TutorialInit();
        }

        protected sealed override void AdditionalMinorUpkeep()
        {
            TutorialMinorUpkeep();
        }

        protected override void ModifyFSMActions(ref Dictionary<RootFSMStatus, Action> actions)
        {
            base.ModifyFSMActions(ref actions);
            actions.Add(RootFSMStatus.Tutorial_Cycle, TutorialCycle);
        }

        protected override void ModifyRootFSMTransitions(ref HashSet<RootFSMTransition> RootFSMTransitions)
        {
            base.ModifyRootFSMTransitions(ref RootFSMTransitions);
            RootFSMTransitions.Add(new Trans(RootFSMStatus.Tutorial_Cycle, RootFSMStatus.MajorUpKeep, 0, true));
            RootFSMTransitions.Add(new Trans(RootFSMStatus.R_IO, RootFSMStatus.F_Cycle, 5, CompletedAndRequestedEnd));
            RootFSMTransitions.Add(new Trans(RootFSMStatus.R_IO, RootFSMStatus.Tutorial_Cycle, 4, CheckTutorialCycle));
            RootFSMTransitions.Add(new Trans(RootFSMStatus.R_IO, RootFSMStatus.MajorUpKeep, 3, CheckNotOnHand));
        }
    }
}