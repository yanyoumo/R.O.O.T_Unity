using System;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Messages;
using ROOT.Common;
using ROOT.Message;
using ROOT.Message.Inquiry;
using ROOT.SetupAsset;
using Sirenix.Utilities;
using UnityEngine;
using static ROOT.FSMLevelLogic;
using static ROOT.TutorialActionType;

namespace ROOT.FSM
{
    using FSMActions = Dictionary<RootFSMStatus, Action>;
    using Trans = RootFSMTransition;
    using FSMTransitions = HashSet<RootFSMTransition>;
    using CheckingLib = Dictionary<TutorialCheckType, Func<FSMLevelLogic, Board, bool>>;
    
    public partial class TutorialFSMModule
    {
        private FSMLevelLogic owner;
        private GameAssets LevelAsset => owner.LevelAsset;

        #region TutorialRelated

        //private bool _couldHandleShopLocal;

        protected int CurrentActionIndex { get; private set; } = -1;

        private LevelActionAsset LevelActionAsset => LevelAsset.ActionAsset;
        private TutorialActionData[] tutActions => LevelActionAsset.Actions;

        private bool? PendingEndTutorialData = null;//null不结束、true完成结束、false失败结束。
        //INFO 现在失败还没有需求、有了再补。

        private int handOnTimeOutThreadholdSec = -1;
        private float handOnTimeOutTimer = 0.0f;

        private bool NotEnding => !PendingEndTutorialData.HasValue;
        private bool EndingWSuccess => PendingEndTutorialData.HasValue && PendingEndTutorialData.Value;
        public bool EndingWFailed => PendingEndTutorialData.HasValue && !PendingEndTutorialData.Value;

        internal bool TutorialCheckGameOver
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

        private void StepForward() => CurrentActionIndex++;

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

        internal void TutorialReactIO()
        {
            if (owner._ctrlPack.HasFlag(ControllingCommand.Confirm))
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
        
        private Func<FSMLevelLogic, Board, bool> PendingHandOnChecking = (a, b) => false;

        private Dictionary<TutorialActionType, Action<TutorialActionData>> StepActionLib;
        private Dictionary<TutorialActionType, Tuple<Action,Action>> ActionPrepLib;

        private void SetStationaryByTag(TutorialActionData data)
        {
            //TODO 现在还没做unset流程、实际上Unit就没有UnsetStationary流程。
            LevelAsset.GameBoard.FindUnitsByUnitTag(data.TargetTag).ForEach(u => u.SetupStationUnit());
        }

        private void DealStep(TutorialActionData data)
        {
            StepActionLib.Where(v => v.Key == data.ActionType).Select(v => v.Value).ForEach(v => v(data));
        }

        private void DealStepMgr()
        {
            //现在是执行也按照SubIdx升序执行。
            tutActions.Where(a => a.ActionIdx == CurrentActionIndex).OrderBy(d=>d.ActionSubIdx).ForEach(DealStep);
            if (tutActions.Any(a => a.ActionIdx == CurrentActionIndex + 1 && a.ActionType == End))
            {
                MessageDispatcher.SendMessage(new HintEventInfo { HintEventType = HintEventType.NextIsEnding });
            }
        }

        #endregion

        private bool shouldInitTutorial = true;
        private bool TutorialOnHand = false;

        private void SetupHandOnTimer(TutorialActionData data)
        {
            handOnTimeOutThreadholdSec = data.TimeOutSec == 0 ? 60 : data.TimeOutSec;//RISK
            handOnTimeOutTimer = 0.0f;
        }
        
        private void ResetHandOnTimer()
        {
            handOnTimeOutThreadholdSec = -1;
            handOnTimeOutTimer = 0.0f;
        }

        private bool _roundLock = false;
        public bool RoundLock
        {
            get => _roundLock;
            private set
            {
                if (owner.CouldHandleTimeLine)
                {
                    if (!owner.FeatureManager.CheckHasFeature(FSMFeatures.Round))
                    {
                        throw new ApplicationException("Round feature not registered");
                    }
                    _roundLock = value;
                    return;
                }
                Debug.LogWarning("this FSM could not handle Timeline");
            }
        }
        
        private void SetHandOn(TutorialActionData data)
        {
            SetupHandOnTimer(data);
            TutorialOnHand = true;
            PendingHandOnChecking = CheckLib[data.HandOnCheckType];
            CurrentHandOnCheckMet = PendingHandOnChecking(owner, LevelAsset.GameBoard);
            
            //TODO Failed部分逻辑也在这里做。

            RoundLock = data.HandsOnLockRound;
            
            ShowCheckListFunc(true);
            ShowTextFunc(false);
            MessageDispatcher.SendMessage(new HintEventInfo { HintEventType = HintEventType.SetGoalContent, StringData = data.HandOnMission });
            MessageDispatcher.SendMessage(new HintEventInfo { HintEventType = HintEventType.ToggleHandOnView, BoolData = true });
            MessageDispatcher.SendMessage(new HintEventInfo { HintEventType = HintEventType.GoalComplete, BoolData = CurrentHandOnCheckMet });
        }

        private void UnsetHandOn()
        {
            ResetHandOnTimer();
            TutorialOnHand = false;
            PendingHandOnChecking = (a, b) => false;
            CurrentHandOnCheckMet = false;

            RoundLock = false;

            ShowCheckListFunc(false);
            MessageDispatcher.SendMessage(new HintEventInfo {HintEventType = HintEventType.ToggleHandOnView, BoolData = false});
            
            StepForward();
            DealStepMgr();
        }

        private bool CompletedAndRequestedEnd()
        {
            return PendingEndTutorialData.HasValue && PendingEndTutorialData.Value;
        }

        private bool CheckTutorialEnding() => EndingWSuccess;

        private bool CheckTutorialCycle() => ProcessdToTutorialCycle;

        private bool CheckNotOnHandNorForceCycle() => !TutorialOnHand && !_forceCycle;

        private bool _forceCycle = false;
        
        private void TutorialCycle()
        {
            if (NotEnding && !TutorialOnHand)
            {
                StepForward();
                DealStepMgr();
            }
            ProcessdToTutorialCycle = false; //只要到了这儿这个流程就要改一下。
        }

        private bool CurrentHandOnCheckMet { get; set; }

        internal void TutorialInit()
        {
            if (!shouldInitTutorial) return;
            shouldInitTutorial = false;
            TutorialOnHand = false;
            //LastActionCount = 0;
            CurrentActionIndex = -1;
            WorldExecutor.InitAndStartShop(LevelAsset);//shop这个东西还是要留给开关。
            LevelAsset.Shop.OpenShop(false, 0);
            MessageDispatcher.SendMessage(new HintEventInfo { HintEventType = HintEventType.ToggleHandOnView, BoolData = false });
            MessageDispatcher.SendMessage(new ToggleGameplayUIData {Set = false, SelectAll = true,});
        }

        private void ToggleAlternateText(TutorialActionData data)
        {
            //Debug.Log("MessageDispatcher.SendMessage(new HintEventInfo {HintEventType = HintEventType.ToggleAlternateTextPos});");
            MessageDispatcher.SendMessage(new HintEventInfo {HintEventType = HintEventType.ToggleAlternateTextPos});
        }

        private void HighLightUIFunc(TutorialActionData data)
        {
            MessageDispatcher.SendMessage(new HighLightingUIChangedData
            {
                Toggle = data.HLSet,
                uiTag = data.UITag,
                CustomBool = data.HLCustomPos,
                pos = data.HighLightingPos,
                scale = data.HighLightingScale
            });
        }

        private void MoveCursorToPosFunc(TutorialActionData data)
        {
            var actionPack = new ActionPack
            {
                ActionID = RewiredConsts.Action.Composite.ForceFlyUnit,
                OnBoardPos = data.Pos,
                Sender = this,
            };
            MessageDispatcher.SendMessage(actionPack);
            _forceCycle = true;
        }

        private void MoveCursorToUnitByTagFunc(TutorialActionData data)
        {
            var units = owner.LevelAsset.GameBoard.FindUnitsByUnitTag(data.TargetTag);
            if (units.Length==0)
            {
                Debug.LogWarning("not find unit by that tag");
                return;
            }
            var actionPack = new ActionPack
            {
                ActionID = RewiredConsts.Action.Composite.ForceFlyUnit,
                OnBoardPos = units[0].CurrentBoardPosition,
                Sender = this,
            };
            MessageDispatcher.SendMessage(actionPack);
            _forceCycle = true;
        }

        private void SetRoundRelatedData(TutorialActionData data)
        {
            if (owner is FSMLevelLogic_Career logic_career)
            {
                switch (data.TimeLineStatus)
                {
                    case TimeLineStatus.Normal:
                        logic_career.FeatureManager.RequestChangeFeature(FSMFeatures.Round, true, true);
                        break;
                    case TimeLineStatus.NoToken:
                        logic_career.FeatureManager.RequestChangeFeature(FSMFeatures.Round, false, true);
                        logic_career.LevelAsset.TimeLine.CurrentStatus = TimeLineStatus.NoToken;
                        break;
                    case TimeLineStatus.Disabled:
                        logic_career.FeatureManager.RequestChangeFeature(FSMFeatures.Round, false, true);
                        logic_career.LevelAsset.TimeLine.CurrentStatus = TimeLineStatus.Disabled;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            Debug.LogWarning("need use at least career level of fsm or deeper");
        }

        private void ResetApparentStep(TutorialActionData tutorialActionData)
        {
            WorldCycler.ResetApparentStep();
        }

        private void HighLightGridFunc(TutorialActionData tutorialActionData)
        {
            var data = new BoardGridHighLightSetData
            {
                Set = tutorialActionData.Set,
                AllClear = tutorialActionData.AllClear,
                HLType = tutorialActionData.HighLightType,
                Poses = tutorialActionData.poses
            };
            MessageDispatcher.SendMessage(data);
        }

        private void ToggleGameplayUIFunc(TutorialActionData tutorialActionData)
        {
            var data = new ToggleGameplayUIData
            {
                Set = tutorialActionData.Set,
                SelectAll = tutorialActionData.AllClear,
                UITag = tutorialActionData.UITag
            };
            MessageDispatcher.SendMessage(data);
        }

        private void ToggleFSMCoreFeatFunc(TutorialActionData tutorialActionData)
        {
            owner.FeatureManager.RequestChangeFeature(tutorialActionData.FSMCoreFeat, tutorialActionData.Set, true);
        }

        private void DeleteUnitFunc(TutorialActionData tutorialActionData)
        {
            if (tutorialActionData.AllClear)
            {
                owner.LevelAsset.GameBoard.TryDeleteAllUnits();
            }
            else
            {
                owner.LevelAsset.GameBoard.TryDeleteUnitByTags(tutorialActionData.TargetTag);
            }
        }

        private void ToggleTutorialHintPageFunc(TutorialActionData tutorialActionData)
        {
            MessageDispatcher.SendMessage(new HintPageChangedData
            {
                Toggle = tutorialActionData.Set, 
                TutorialOrGameplay = true, 
                PageNum = tutorialActionData.Numpage,
                AdditiveOrReplace = tutorialActionData.Set//这个没写错、是锁死：关掉肯定是Replace，打开是Additive
            });
        }

        private void ToggleAlternateHandsOnGoalFunc(TutorialActionData tutorialActionData)
        {
            MessageDispatcher.SendMessage(new HintEventInfo {HintEventType = HintEventType.ToggleAlternateCheckGoal});
        }

        private void AutoProceedToNextStageFunc(TutorialActionData tutorialActionData)
        {
            if (!owner.CouldHandleTimeLine)
            {
                Debug.Log("current fsm could not handle TimeLine");
                return;
            }
            if (!(owner is FSMLevelLogic_Career owner_Career))
            {
                throw new ApplicationException("FSM should support TimeLine but no correct API");
            }
            var nextStep=owner_Career.RoundLibDriver.GetCurrentStageRemainingStep(owner.LevelAsset.StepCount);
            WorldCycler.ExpectedStepIncrement(nextStep);
        }
        
        public TutorialFSMModule(FSMLevelLogic _fsm)
        {
            owner = _fsm;
            StepActionLib = new Dictionary<TutorialActionType, Action<TutorialActionData>> {
                {Text, data => DisplayText(data.DoppelgangerToggle && StartGameMgr.UseTouchScreen ? data.DoppelgangerText : data.Text)},
                {CreateUnit, data =>CreateUnitOnBoard(data,LevelAsset)},
                {End, data => PendingEndTutorialData = true},
                {ShowText, data => ShowTextFunc(true)},
                {HideText, data =>  ShowTextFunc(false)},
                {ShowCheckList, data => ShowCheckListFunc(true)},
                {HideCheckList, data => ShowCheckListFunc(false)},
                {HandOn, SetHandOn},
                {CreateCursor, data => WorldExecutor.InitCursor(LevelAsset, data.Pos)},
                {SetUnitStationary, SetStationaryByTag},
                {ToggleFSMCoreFeat, ToggleFSMCoreFeatFunc},
                {ToggleAlternateTextPos, ToggleAlternateText},
                {HighLightUI, HighLightUIFunc},
                {MoveCursorToPos, MoveCursorToPosFunc},
                {MoveCursorToUnitByTag, MoveCursorToUnitByTagFunc},
                {SetTimeline, SetRoundRelatedData},
                {ResetStep, ResetApparentStep},
                {HighLightGrid, HighLightGridFunc},
                {ToggleGameplayUI, ToggleGameplayUIFunc},
                {DeleteUnit, DeleteUnitFunc},
                {ToggleTutorialHintPage, ToggleTutorialHintPageFunc},
                {ToggleAlternateHandsOnGoal, ToggleAlternateHandsOnGoalFunc},
                {AutoProceedToNextStage, AutoProceedToNextStageFunc},
            };
        }

        internal void TutorialMajorUpkeep()
        {
            if (TutorialOnHand)
            {
                CurrentHandOnCheckMet = PendingHandOnChecking(owner, LevelAsset.GameBoard);//这边就就地测一下
                MessageDispatcher.SendMessage(new HintEventInfo { HintEventType = HintEventType.GoalComplete, BoolData = CurrentHandOnCheckMet });
                if (handOnTimeOutThreadholdSec > 0)
                {
                    if (CurrentHandOnCheckMet)
                    {
                        handOnTimeOutTimer = 0.0f;
                    }
                    else
                    {
                        //现在的设计是HandOn之后每X秒后震3Sec
                        handOnTimeOutTimer += Time.deltaTime;
                        if (handOnTimeOutTimer >= handOnTimeOutThreadholdSec)
                        {
                            handOnTimeOutTimer = 0.0f;
                            MessageDispatcher.SendMessage(WorldEvent.HelpScreenShouldAlertEvent);
                            MessageDispatcher.SendMessage(WorldEvent.TutorialMissionShouldAlertEvent);
                        }
                    }
                }
            }
            else
            {
                var validKeyInTutorial = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Tab);
                if (Input.anyKeyDown && !validKeyInTutorial)
                {
                    //现在这个执行还稍稍有些半吊子，还是需要优化一下。
                    MessageDispatcher.SendMessage(new HintEventInfo {HintEventType = HintEventType.ControllerBlockedAlert});
                }
            }
            if (CurrentActionIndex == -1)//第一步自动执行。
            {
                StepForward();
                DealStepMgr();
            }
        }

        internal void TutorialMinorUpkeep()
        {
            if (TutorialOnHand)
            {
                //这个流程有个问题、需要再走一周才能判断，但是放在Minor里面的，应该能判明白的？//还得放到major里面用一下。
                //有可能是digong那个函数的具体实现的问题、但是和整体时序都有关系。
                CurrentHandOnCheckMet = PendingHandOnChecking(owner, LevelAsset.GameBoard);
                //根据现在能识别到需要再接收一下玩家的“回车”来“手动通过”这个判断。
                //那个判断现在具体的执行是：在系统判断到条件满足后、需要玩家手动按动一下确定键（回车）来继续。
                MessageDispatcher.SendMessage(new HintEventInfo { HintEventType = HintEventType.GoalComplete, BoolData = CurrentHandOnCheckMet });
            }
            _forceCycle = false;
        }

        internal void InjectTutorialFSMActions(ref Dictionary<RootFSMStatus, Action> actions)
        {
            actions.Add(RootFSMStatus.Tutorial_Cycle, TutorialCycle);
        }

        internal void InjectTutorialFSMTransitions(ref RootFSMTranstionLib RootFSMTransitions)
        {
            RootFSMTransitions.Add(new Trans(RootFSMStatus.Tutorial_Cycle, RootFSMStatus.F_Cycle, 1, CheckTutorialEnding));
            RootFSMTransitions.Add(new Trans(RootFSMStatus.Tutorial_Cycle, RootFSMStatus.MajorUpKeep, 0, true));
            var existingRIOMaxPriority = RootFSMTransitions.GetMaxPriorityByStatus(RootFSMStatus.R_IO);
            RootFSMTransitions.Add(new Trans(RootFSMStatus.R_IO, RootFSMStatus.F_Cycle, existingRIOMaxPriority + 3, CompletedAndRequestedEnd));
            RootFSMTransitions.Add(new Trans(RootFSMStatus.R_IO, RootFSMStatus.Tutorial_Cycle, existingRIOMaxPriority + 2, CheckTutorialCycle));
            RootFSMTransitions.Add(new Trans(RootFSMStatus.R_IO, RootFSMStatus.MajorUpKeep, existingRIOMaxPriority + 1, CheckNotOnHandNorForceCycle));
        }
    }
}