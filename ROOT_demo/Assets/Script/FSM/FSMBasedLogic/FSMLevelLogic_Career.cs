using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using com.ootii.Messages;
using I2.Loc;
using ROOT.Common;
using ROOT.Consts;
using ROOT.Message;
using ROOT.Message.Inquiry;
using ROOT.SetupAsset;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT
{
    public class FSMLevelLogic_Career : FSMLevelLogic_Barebone
    {
        protected virtual string SucceedEndingTerm => ScriptTerms.EndingMessageNoBoss_EarnedMoney;
        protected virtual string FailedEndingTerm => ScriptTerms.EndingMessageNoBoss_NoEarnedMoney;

        protected override IEnumerable<int> GamePlayHintPagesByLevelType
        {
            get
            {
                var res = base.GamePlayHintPagesByLevelType.ToList();
                res.AddRange(new[] {4, 5, 6});
                return res;
            }
        }
        
        protected override float LevelProgress => LevelAsset.StepCount / (float) RoundLibDriver.TotalPlayableCount;
        public override bool CouldHandleTimeLine => true;
        public override BossStageType HandleBossType => throw new ArgumentException("could not handle Boss");
        public override int LEVEL_ART_SCENE_ID => StaticName.SCENE_ID_ADDITIONAL_VISUAL_CAREER;
        protected override bool IsForwardCycle => AutoForward || MovedTile;
        
        public RoundLibDriver RoundLibDriver { get; private set; }
        private bool AutoForward => (AutoDrive.HasValue && AutoDrive.Value);
        private bool IsReverseCycle => (AutoDrive.HasValue && !AutoDrive.Value);
        protected bool IsSkillAllowed => !RoundLibDriver.IsShopRound;
        protected bool BoardCouldIOCurrency => (RoundLibDriver.IsRequireRound || RoundLibDriver.IsDestoryerRound);
        
        private bool CheckIsSkill() => LevelAsset.SkillMgr != null && LevelAsset.SkillMgr.CurrentSkillType.HasValue &&
                                       LevelAsset.SkillMgr.CurrentSkillType.Value == SkillType.Swap;
        private bool CheckAutoF() => AutoDrive.HasValue && AutoDrive.Value;
        private bool CheckAutoR() => IsReverseCycle;

        #region FeatureSet_TutorialOnly.

        protected bool HandlingRound => (!UseTutorialVer || FeatureManager.GetExternalToggleVal(FSMFeatures.Round));
        protected bool HandlingSkill => (!UseTutorialVer || FeatureManager.GetExternalToggleVal(FSMFeatures.Skill));
        #endregion
        

        private StageType? lastStageType = null;
        
        private bool StageAlertSuppressFlag = false;

        private int Cost => LevelAsset.GameBoard.BoardGirdDriver.HeatSinkCost;

        protected virtual void UpdateLevelAsset()
        {
            var lastStage = RoundLibDriver.PreviousRoundGist?.Type ?? StageType.Shop;
            var lastDestoryBool = lastStage == StageType.Destoryer;

            if (RoundLibDriver.IsRequireRound && IsForwardCycle)
            {
                LevelAsset.GameBoard.BoardGirdDriver.UpdatePatternDiminishing();
            }

            if ((lastDestoryBool && !RoundLibDriver.IsDestoryerRound) && !WorldCycler.NeedAutoDriveStep.HasValue)
            {
                LevelAsset.GameBoard.BoardGirdDriver.DestoryHeatsinkOverlappedUnit();
            }
        }

        protected void RoundLockTutorialVerHandler()
        {
            if (UseTutorialVer)
            {
                if (!_roundLocked && TutorialModule.RoundLock)
                {
                    SetUpRoundLock();
                }

                if (!TutorialModule.RoundLock&&_roundLocked)
                {
                    UnSetRoundLock();
                }
            }
        }
        
        protected override void AdditionalMajorUpkeep()
        {
            LevelAsset.GameBoard.BoardGirdDriver.UpkeepHeatSink(RoundLibDriver.CurrentStage.Value);
            LevelAsset.GameBoard.BoardGirdDriver.CheckOverlappedHeatSinkCount(out LevelAsset.occupiedHeatSinkCount);
            if (LevelAsset.SkillEnabled && HandlingSkill)
            {
                LevelAsset.SkillMgr.UpKeepSkill(LevelAsset);
            }
            RoundLockTutorialVerHandler();
        }

        protected override void AdditionalInitLevel()
        {
            base.AdditionalInitLevel();
            WorldExecutor.InitDestoryer(ref LevelAsset);
            LevelAsset.DestroyerEnabled = false;
            WorldExecutor.InitAndStartShop(LevelAsset);
            
            var message = new CurrencyUpdatedInfo()
            {
                CurrencyVal = Mathf.RoundToInt(LevelAsset.GameCurrencyMgr.Currency),
                TotalIncomesVal = 0,
            };
            
            MessageDispatcher.SendMessage(message);
            
            if (LevelAsset.ActionAsset.RoundLib.Count > 0)
            {
                //这个东西放在这里还是怎么着？就先这样吧。
                WorldCycler.InitCycler();
                if (LevelAsset.TimeLine != null)
                {
                    LevelAsset.TimeLine.InitWithAssets(RoundLibDriver);
                }
            }
            MessageDispatcher.SendMessage(new ToggleGameplayUIData {Set = false, SelectAll = false, UITag = UITag.Currency_BareBone});

            LevelAsset.SkillMgr.SkillEnabled = false;
            LevelAsset.Shop.OpenShop(false, 0);
            
            if (UseTutorialVer)
            {
                LevelAsset.TimeLine.CurrentStatus = TimeLineStatus.Disabled;
                for (var i = 0; i < LevelAsset.SkillMgr.SkillPalettes.Count; i++)
                {
                    LevelAsset.SkillMgr.SkillSystemSet(i, false);
                }
            }
        }

        private bool _roundLocked = false;

        private void UseDynamicRoundLib()
        {
            //现在的设计是，用一次动态库之后就不设回去了。
            if (RoundLibDriver.UseStaticLib)
            {
                RoundLibDriver.DynamicRoundLib = new List<RoundData>(LevelAsset.ActionAsset.RoundLib);//用静态Round初始化动态类。
                RoundLibDriver.UseStaticLib = false;
            }
        }
        
        protected virtual void SetUpRoundLock()
        {
            _roundLocked = true;
            UseDynamicRoundLib();
        }
        
        protected virtual void UnSetRoundLock()
        {
            _roundLocked = false;
        }
        
        protected virtual void UpdateRoundData_Stepped()
        {
            if (_roundLocked)
            {
                RoundLibDriver.StretchCurrentRound(LevelAsset.StepCount);
            }
            
            UpdateLevelAsset();//先删除，再清理。
            
            var roundGist = RoundLibDriver.CurrentRoundGist.Value;
            var tCount = RoundLibDriver.GetTruncatedStep(LevelAsset.StepCount);
            if (roundGist.SwitchHeatsink(tCount))
            {
                //Debug.Log("roundGist.SwitchHeatsink(tCount)");
                LevelAsset.GameBoard.BoardGirdDriver.UpdatePatternID();
            }

            var discount = 0;

            if (!LevelAsset.Shop.ShopOpening && RoundLibDriver.IsShopRound && HandlingSkill)
            {
                discount = LevelAsset.SkillMgr.CheckDiscount();
            }

            LevelAsset.Shop.OpenShop(RoundLibDriver.IsShopRound, discount);
            LevelAsset.SkillMgr.SkillEnabled = LevelAsset.SkillEnabled = (IsSkillAllowed && HandlingSkill);
        }

        protected void AddtionalReactIO_Skill()
        {
            if (HandlingSkill && LevelAsset.SkillEnabled && HandlingSkill && LevelAsset.SkillMgr != null)
            {
                LevelAsset.SkillMgr.TriggerSkill(LevelAsset, _ctrlPack);
            }
        }

        protected override void AdditionalReactIO()
        {
            AddtionalReactIO_Skill();
        }

        private void SkillMajorUpkeep()
        {
            if (HandlingSkill)
            {
                LevelAsset.SkillMgr.SwapTick_FSM(LevelAsset, _ctrlPack);
                MovedTile = false;
            }
        }
        
        private void UpdateRoundTimingData_Stepped()
        {
            var currentStageType = RoundLibDriver.CurrentRoundGist.Value.Type;
            if (lastStageType == null || lastStageType.Value != currentStageType)
            {
                //RISK 这个变成每个时刻都改了、想着加一个Guard
                MessageDispatcher.SendMessage(WorldEvent.BoardUpdatedEvent); //为了令使和Round相关的数据强制更新。
                var timingEvent = new TimingEventInfo
                {
                    Type = WorldEvent.InGameStageChangedEvent,
                    CurrentStageType = RoundLibDriver.CurrentRoundGist.Value.Type,
                };
                var timingEvent2 = new TimingEventInfo
                {
                    Type = WorldEvent.CurrencyIOStatusChangedEvent,
                    BoardCouldIOCurrencyData = BoardCouldIOCurrency,
                    UnitCouldGenerateIncomeData = RoundLibDriver.IsRequireRound,
                };
                MessageDispatcher.SendMessage(timingEvent);
                MessageDispatcher.SendMessage(timingEvent2);
                lastStageType = RoundLibDriver.CurrentRoundGist.Value.Type;
            }

            if (RoundLibDriver.PreCheckRoundGist.HasValue)
            {
                var preCheckStageType = RoundLibDriver.PreCheckRoundGist.Value.Type;
                if (!StageAlertSuppressFlag)
                {
                    if (currentStageType != preCheckStageType)
                    {
                        var timingEvent = new TimingEventInfo
                        {
                            Type = WorldEvent.InGameStageWarningEvent,
                            CurrentStageType = currentStageType,
                            NextStageType = preCheckStageType,
                        };
                        MessageDispatcher.SendMessage(timingEvent);
                        StageAlertSuppressFlag = true;
                    }
                }
                else
                {
                    if (currentStageType == preCheckStageType) StageAlertSuppressFlag = false;
                }
            }
        }

        
        private void CareerCycle()
        {
            if (!HandlingRound) return;
            
            if (LevelAsset.DestroyerEnabled)
            {
                WorldExecutor.UpdateDestoryer(LevelAsset);
                if (LevelAsset.WarningDestoryer != null && !WorldCycler.TelemetryPause)//RISK 这里先弄一下，把遥测暂停时的攻击流程关了。这个本应该下沉的。
                {
                    LevelAsset.WarningDestoryer.Step(out var outCore);
                    LevelAsset.DestoryedCoreType = outCore;
                }
            }

            if (RoundLibDriver.CurrentRoundGist.HasValue && !CheckIsSkill())//RISK 这里放了一个技能操作的guard，但是这个东西不清楚是不是应该直接改FSM框架。
            {
                UpdateRoundData_Stepped();
                UpdateRoundTimingData_Stepped();
            }
        }
        
        protected override void SetUpHandlingCurrency()
        {
            MessageDispatcher.SendMessage(new ToggleGameplayUIData {Set = true, SelectAll = false, UITag = UITag.Currency_BareBone});
            MessageDispatcher.SendMessage(new TimingEventInfo
            {
                Type = WorldEvent.CurrencyIOStatusChangedEvent,
                BoardCouldIOCurrencyData = true,
                UnitCouldGenerateIncomeData = true,
            });
            UpdateBoardData_Instantly();
            LevelAsset.BaseDeltaCurrency = !HandlingRound || RoundLibDriver.IsRequireRound ? GetBaseInCome() : 0;
            LevelAsset.BonusDeltaCurrency = 0;
            SendCurrencyMessage(LevelAsset);
        }

        protected override bool NormalCheckGameOver
        {
            get
            {
                var res=RoundLibDriver.HasEnded(LevelAsset.StepCount);
                if (res) WorldCycler.Reset();
                return res;
            }
        }

        protected virtual float GetBonusInCome() => (TypeASignalScore + TypeBSignalScore) * (LevelAsset.CurrencyRebate - 1.0f);//BUG 这个数据有问题？没有实质的加上去？
        
        protected override void BoardUpdatedHandler(IMessage rMessage)
        {
            base.BoardUpdatedHandler(rMessage);
            UpdateBoardData_Instantly_Career();
        }

        private void UpdateBoardData_Instantly_Career()
        {
            if (!HandlingRound) return;
            //在这里更新DeltaCurrency并没有错、严格来说是更新DeltaCurrency的Cache；
            //这个DeltaCurrency只有在Stepped的时刻才会计算到Currency里面。
            LevelAsset.BaseDeltaCurrency = BoardCouldIOCurrency ? (RoundLibDriver.IsRequireRound ? GetBaseInCome() : 0) - Cost : 0;
            LevelAsset.BonusDeltaCurrency = BoardCouldIOCurrency && RoundLibDriver.IsRequireRound ? GetBonusInCome() : 0;
            SendCurrencyMessage(LevelAsset);
        }

        private void ReverseCycle()
        {
            WorldCycler.StepDown();
            LevelAsset.TimeLine.Reverse();
        }

        protected virtual void populateGameOverAsset(ref GameOverAsset _gameOverAsset)
        {
            _gameOverAsset = new GameOverAsset
            {
                SuccessTerm = SucceedEndingTerm,
                FailedTerm = FailedEndingTerm,
                ValueFloat = LevelAsset.GameCurrencyMgr.CurrencyDiffFromStartToNow,
            };
        }

        protected sealed override void GameEnding_Internal()
        {
            PendingCleanUp = true;
            if (UseTutorialVer)
            {
                LevelAsset.GameOverAsset = new GameOverAsset
                {
                    SuccessTerm = ScriptTerms.EndingMessageTutorial,
                    FailedTerm = ScriptTerms.EndingMessageTutorialFailed
                };
            }
            else
            {
                populateGameOverAsset(ref LevelAsset.GameOverAsset);
            }
            LevelMasterManager.Instance.LevelFinished(LevelAsset);
        }

        private void InitCareer()
        {
            CareerCycle();
            _mainFSM.currentStatus = RootFSMStatus.MajorUpKeep;
            _mainFSM.waitForNextFrame = false;
        }
        
        protected override void ModifyFSMActions(ref Dictionary<RootFSMStatus, Action> actions)
        {
            base.ModifyFSMActions(ref actions);
            actions.Add(RootFSMStatus.R_Cycle, ReverseCycle);
            actions.Add(RootFSMStatus.Career_Cycle, CareerCycle);
            actions.Add(RootFSMStatus.Skill, SkillMajorUpkeep);
        }

        protected override void AdditionalArtLevelReference(ref GameAssets LevelAsset)
        {
            base.AdditionalArtLevelReference(ref LevelAsset);
            LevelAsset.TimeLine = FindObjectOfType<TimeLine>();
            LevelAsset.SkillMgr = FindObjectOfType<SkillMgr>();
            LevelAsset.CineCam = FindObjectOfType<CinemachineFreeLook>();
        }

        protected override void ModifyRootFSMTransitions(ref RootFSMTranstionLib RootFSMTransitions)
        {
            base.ModifyRootFSMTransitions(ref RootFSMTransitions);
            RootFSMTransitions.Remove(new RootFSMTransition(RootFSMStatus.F_Cycle, RootFSMStatus.Animate, 1, CheckStartAnimate, TriggerAnimation));
            RootFSMTransitions.Remove(new RootFSMTransition(RootFSMStatus.F_Cycle, RootFSMStatus.MinorUpKeep));

            #region ADD Consequence

            RootFSMTransitions.Remove(new RootFSMTransition(RootFSMStatus.PreInit, RootFSMStatus.MajorUpKeep, 1, CheckInited));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.PreInit, RootFSMStatus.MajorUpKeep, 1, CheckInited, InitCareer));

            #endregion

            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.Skill, RootFSMStatus.Career_Cycle));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.F_Cycle, RootFSMStatus.Career_Cycle));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.R_IO, RootFSMStatus.Skill, 3, CheckIsSkill));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.Career_Cycle, RootFSMStatus.Animate, 1, CheckStartAnimate, TriggerAnimation));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.Career_Cycle, RootFSMStatus.MinorUpKeep));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.MajorUpKeep, RootFSMStatus.R_Cycle, 3, CheckAutoR));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.MajorUpKeep, RootFSMStatus.F_Cycle, 2, CheckAutoF));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.R_Cycle, RootFSMStatus.Career_Cycle));
        }

        protected override void createDriver()
        {
            _actionDriver = new CareerControlActionDriver(this, _mainFSM);
        }

        protected override void FeaturesChangedHandler()
        {
            base.FeaturesChangedHandler();
            if (HandlingRound)
            {
                LevelAsset.TimeLine.CurrentStatus = TimeLineStatus.Normal;
            }
        }

        private void CurrentSignalTypeInquiryHandler(IMessage rMessage)
        {
            if (rMessage is CurrentSignalTypeInquiryData data)
            {
                var asset = LevelAsset.ActionAsset.AdditionalGameSetup;
                data.CurrentSignalCallBack(asset.PlayingSignalTypeA, asset.PlayingSignalTypeB);
            }
        }
        
        protected override void Awake()
        {
            base.Awake();
            RoundLibDriver = new RoundLibDriver {owner = this};
            MessageDispatcher.AddListener(WorldEvent.CurrentSignalTypeInquiry,CurrentSignalTypeInquiryHandler);
            if (UseTutorialVer)
            {
                FeatureManager.RegistFSMFeature(FSMFeatures.Round,new []{FSMFeatures.Shop,FSMFeatures.Currency}, false);
                FeatureManager.RegistFSMFeature(FSMFeatures.Skill,new []{FSMFeatures.Round,FSMFeatures.Shop,FSMFeatures.Currency}, false);
            }
        }

        protected override void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.CurrentSignalTypeInquiry,CurrentSignalTypeInquiryHandler);
            base.OnDestroy();
        }
    }
}