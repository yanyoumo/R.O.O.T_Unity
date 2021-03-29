using System;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Messages;
using I2.Loc;
using ROOT.SetupAsset;
using ROOT.UI;
using UnityEngine;
using static ROOT.TextProcessHelper;


namespace ROOT
{
    using FSMActions = Dictionary<RootFSMStatus, Action>;
    using Trans = RootFSMTransition;
    using FSMTransitions = HashSet<RootFSMTransition>;
    using CheckingLib = Dictionary<TutorialCheckType, Func<FSMLevelLogic, Board, bool>>;

    //迪公说的想把Tutorial里面判断的逻辑从基于FSM变成这样基于函数代理的。
    //从技术上讲可以、使用Dict存储一个enum-Func对儿；这样在Action里面就可以通过enum配置实际的逻辑。
    //这么搞的确有很多优势、但是也有一些问题。
    //先说优势：
        //1、极端情况下、就都不用每个教程关卡都需要一个新的FSM了。
        //2、判断函数是可以对立拎出来了。
    //再说劣势：
        //1、实现框架需要仔细想；可能会十分复杂。
        //2、判断函数的参数定死了。（FSMLogic和Board可以提供数据源
            //但是一个重要的问题，是具体可配置的参数怎么办？例如判断已有的某个数据是否高于某个阈值、这个阈值怎么传进去？
            //理论上可以传一个Object、但是也有不少问题。
   
    public static class TutorialCheckFunctionList
    {
        public static bool CheckA(FSMLevelLogic fsm,Board board)
        {
            return board.FindUnitUnderBoardPos(Vector2Int.zero) != null;
        }
    }

    public enum TutorialCheckType
    {
        TestA,
    }

    public sealed class FSMTutorialLogic : FSMLevelLogic_Barebone
    {
        private readonly CheckingLib CheckLib = new CheckingLib
        {
            {TutorialCheckType.TestA, TutorialCheckFunctionList.CheckA},
        };
        
        protected override string SucceedEndingTerm => ScriptTerms.EndingMessageTutorial;
        protected override string FailedEndingTerm => ScriptTerms.EndingMessageTutorialFailed;
        public override bool IsTutorial => true;
        public override bool CouldHandleSkill => true;
        public override bool CouldHandleBoss => false;
        public override BossStageType HandleBossType => throw new ArgumentException("could not handle Boss");
        
        protected bool AllUnitConnected()
        {
            return LevelAsset.GameBoard.Units.All(u => u.AnyConnection);
        }
        
        #region TutorialRelated

        protected int ActionIndex { get; private set; } = -1;
        private int LastActionCount { get; set; } = 0;

        /*protected abstract string MainGoalEntryContent { get; }
        protected virtual string SecondaryGoalEntryContent { get; } = "";*/

        private LevelActionAsset LevelActionAsset => LevelAsset.ActionAsset;
        
        private void ShowText(bool val)=>SendHintData(HintEventType.SetTutorialTextShow, val);
        private void ShowCheckList(bool val)=>SendHintData(HintEventType.SetGoalCheckListShow, val);
        

        private bool? PendingEndTutorialData = null;//null不结束、true完成结束、false失败结束。
        //INFO 现在失败还没有需求、有了再补。

        public bool NotEnding => !PendingEndTutorialData.HasValue;
        public bool EndingWSuccess => PendingEndTutorialData.HasValue && PendingEndTutorialData.Value;
        public bool EndingWFailed => PendingEndTutorialData.HasValue && !PendingEndTutorialData.Value;
        
        protected sealed override bool CheckGameOver
        {
            get
            {
                if (!NotEnding)
                {
                    LevelAsset.TutorialCompleted = EndingWSuccess;
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
                HintEventType = HintEventType.SetTutorialTextContent,
                StringData = text,
            };
            MessageDispatcher.SendMessage(hintData);
        }
        
        private bool ProcessdToTutorialCycle;

        protected override void AdditionalReactIO()
        {
            if (_ctrlPack.HasFlag(ControllingCommand.Confirm))
            {
                if (CurrentHandOnCheckMet)
                {
                    UnsetHandOn();
                    return;
                }

                ProcessdToTutorialCycle = true;
                //仔细想了一下、Driver和这个React的流程又要有、一个是吧额外的ActionID转义为ControllingCommand。
                    //顺带、根据这个思路，可能可以把Driver再拆分一下。//现在先不这么做、有空给搞一下。
                //另一个是把ControllingCommand转义为实际的工作。
            }
        }

        //protected abstract void AdditionalDealStep(TutorialActionData data);

        private Func<FSMLevelLogic, Board, bool> PendingHandOnChecking = (a, b) => false;
        
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
                    PendingEndTutorialData = true;
                    break;
                case TutorialActionType.ShowText:
                    ShowText(true);
                    break;
                case TutorialActionType.HideText:
                    ShowText(false);
                    break;
                case TutorialActionType.ShowCheckList:
                    ShowCheckList(true);
                    break;
                case TutorialActionType.HideCheckList:
                    ShowCheckList(false);
                    break;
                case TutorialActionType.HandOn:
                    SetHandOn(data);
                    break;
                case TutorialActionType.CreateCursor:
                    //TODO 要从旧代码里面把这里的逻辑捞出来。
                    break;
                default:
                    throw new NotImplementedException();
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

                DealStep(LevelActionAsset.Actions[i]);
            }
        }

        #endregion

        private bool shouldInitTutorial = true;
        private bool TutorialOnHand = false;

        private void SetHandOn(TutorialActionData data)
        {
            TutorialOnHand = true;
            PendingHandOnChecking = CheckLib[data.HandOnCheckType];
            MessageDispatcher.SendMessage(new HintEventInfo {HintEventType = HintEventType.SetGoalContent, StringData = data.HandOnMission});
            ShowCheckList(true);
            ShowText(false);
            CurrentHandOnCheckMet = false;
        }

        private void UnsetHandOn()
        {
            TutorialOnHand = false;
            PendingHandOnChecking = (a, b) => false;
            ShowCheckList(false);
            CurrentHandOnCheckMet = false;
            StepForward();
            DealStepMgr();
        }

        private bool CompletedAndRequestedEnd()
        {
            return PendingEndTutorialData.HasValue && PendingEndTutorialData.Value;
        }

        private bool CheckTutorialCycle() => ProcessdToTutorialCycle;
        
        private bool CheckNotOnHand() => !TutorialOnHand;

        private void TutorialCycle()
        {
            //Debug.Log("TutorialCycle");
            if (NotEnding && !TutorialOnHand)
            {
                StepForward();
                DealStepMgr();
            }
            ProcessdToTutorialCycle = false; //只要到了这儿这个流程就要改一下。
        }

        private bool CurrentHandOnCheckMet { get; set; }

        private void TutorialMinorUpkeep()
        {
            if (TutorialOnHand)
            {
                CurrentHandOnCheckMet = PendingHandOnChecking(this, LevelAsset.GameBoard);
                //根据现在能识别到需要再接收一下玩家的“回车”来“手动通过”这个判断。
                //那个判断现在具体的执行是：在系统判断到条件满足后、需要玩家手动按动一下确定键（回车）来继续。
                MessageDispatcher.SendMessage(new HintEventInfo {HintEventType = HintEventType.GoalComplete, BoolData = CurrentHandOnCheckMet});
            }
        }

        private void TutorialInit()
        {
            if (!shouldInitTutorial) return;
            shouldInitTutorial = false;
            TutorialOnHand = false;
            LastActionCount = 0;
            ActionIndex = -1;
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