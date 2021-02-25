using System;
using System.Collections.Generic;
using System.Linq;
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
        protected bool AllUnitConnected()
        {
            return LevelAsset.GameBoard.Units.All(u => u.AnyConnection);
        }
        
        #region TutorialRelated

        protected bool ActionEnded { get; private set; } = false;
        protected int ActionIndex { get; private set; } = -1;
        protected int LastActionCount { get; private set; } = 0;

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
            set => LevelAsset.HintMaster.RequestedShowTutorialContent = value;
        }

        private bool ShowCheckList
        {
            set => LevelAsset.HintMaster.ShouldShowCheckList = value;
        }

        protected sealed override void UpdateGameOverStatus()
        {
            if (LevelCompleted && PlayerRequestedEnd)
            {
                PendingCleanUp = true;
                LevelAsset.TutorialCompleted = true;
                LevelMasterManager.Instance.LevelFinished(LevelAsset);
            }

            if (LevelFailed && PlayerRequestedQuit)
            {
                PendingCleanUp = true;
                LevelAsset.TutorialCompleted = false;
                LevelMasterManager.Instance.LevelFinished(LevelAsset);
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

            LevelAsset.GameBoard.UpdateBoardInit();
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
            //Debug.Log(text);
            LevelAsset.HintMaster.TutorialContent = ProcessText(text);
        }

        protected override void AddtionalRecatIO()
        {
            if (LevelCompleted)
            {
                //这段代码想着放到AdditionalRIO里面去。
                PlayerRequestedEnd = CtrlPack.HasFlag(ControllingCommand.Confirm);
            }
        }
        
        protected abstract void AddtionalDealStep(TutorialActionData data);

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
                    AddtionalDealStep(data);
                    break;
            }
        }

        protected void DealStepMgr()
        {
            int actionLength = LevelActionAsset.Actions.Length;
            for (int i = LastActionCount; i < actionLength; i++)
            {
                if (LevelActionAsset.Actions[i].ActionIdx > ActionIndex)
                {
                    LastActionCount = i;
                    break;
                }

                DealStep(LevelActionAsset.Actions[i]);
            }
        }

        /*protected void InitShop()
        {
            LevelAsset.Shop.ShopInit(LevelAsset);
            LevelAsset.Shop.CurrentGameStateMgr = LevelAsset.GameStateMgr;
            LevelAsset.Shop.GameBoard = LevelAsset.GameBoard;
            if (LevelActionAsset.ExcludedShop)
            {
                LevelAsset.Shop.excludedTypes = LevelActionAsset.ShopExcludedType;
            }
        }

        protected void ForceSetWarningDestoryer(Vector2Int nextIncome)
        {
            MeteoriteBomber obj = LevelAsset.WarningDestoryer as MeteoriteBomber;
            obj?.ForceSetDestoryer(nextIncome);
        }*/
        
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
            LevelAsset.HintMaster.HideTutorialFrame = true;
            LevelAsset.HintMaster.TutorialCheckList.SetupEntryContent(MainGoalEntryContent, SecondaryGoalEntryContent);
            StepForward();
            DealStepMgr();
        }

        protected override void AddtionalMajorUpkeep()
        {
            TutorialInit();
        }

        protected sealed override void AddtionalMinorUpkeep()
        {
            TutorialMinorUpkeep();
        }

        protected abstract void AdditionalFSMActionsOperating(ref FSMActions actions);
        protected abstract void AdditionalFSMTransitionOperating(ref FSMTransitions transitions);

        protected override void ModifyFSMActions(ref Dictionary<RootFSMStatus, Action> actions)
        {
            base.ModifyFSMActions(ref actions);
            actions.Add(RootFSMStatus.Tutorial_Cycle, TutorialCycle);
        }

        /*protected sealed override HashSet<RootFSMTransition> RootFSMTransitions
        {
            get
            {
                var transitions = new FSMTransitions
                {
                    new Trans(RootFSMStatus.PreInit, RootFSMStatus.MajorUpKeep, 1, CheckInited),
                    new Trans(RootFSMStatus.PreInit),
                    new Trans(RootFSMStatus.F_Cycle, RootFSMStatus.Animate, 1, CheckStartAnimate, TriggerAnimation),
                    new Trans(RootFSMStatus.F_Cycle, RootFSMStatus.MinorUpKeep),
                    new Trans(RootFSMStatus.Tutorial_Cycle, RootFSMStatus.MajorUpKeep, 0, true),
                    new Trans(RootFSMStatus.Animate, RootFSMStatus.MinorUpKeep),
                    new Trans(RootFSMStatus.MajorUpKeep, RootFSMStatus.R_IO, 1, CheckCtrlPackAny),
                    new Trans(RootFSMStatus.MajorUpKeep),
                    new Trans(RootFSMStatus.MinorUpKeep, RootFSMStatus.Animate, 1, true, CheckLoopAnimate),
                    new Trans(RootFSMStatus.MinorUpKeep, RootFSMStatus.CleanUp),
                    new Trans(RootFSMStatus.R_IO, RootFSMStatus.F_Cycle, 5, CompletedAndRequestedEnd),
                    new Trans(RootFSMStatus.R_IO, RootFSMStatus.Tutorial_Cycle, 4, CheckTutorialCycle),
                    new Trans(RootFSMStatus.R_IO, RootFSMStatus.MajorUpKeep, 3, CheckNotOnHand),
                    new Trans(RootFSMStatus.R_IO, RootFSMStatus.F_Cycle, 2, CheckFCycle),
                    new Trans(RootFSMStatus.R_IO, RootFSMStatus.Animate, 1, CheckStartAnimate, TriggerAnimation),
                    new Trans(RootFSMStatus.R_IO, RootFSMStatus.MajorUpKeep, 0, true),
                    new Trans(RootFSMStatus.CleanUp, RootFSMStatus.MajorUpKeep, 0, true),
                };
                AdditionalFSMTransitionOperating(ref transitions);
                return transitions;
            }
        }*/
    }
}