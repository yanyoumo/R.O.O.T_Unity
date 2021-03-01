using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using com.ootii.Messages;
using ROOT.Signal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public class FSMLevelLogic_Career : FSMLevelLogic_Barebone
    {
        public override int LEVEL_ART_SCENE_ID => StaticName.SCENE_ID_ADDITIONAL_VISUAL_CAREER;

        protected RoundLibDriver RoundLibDriver;

        public override bool IsTutorial => false;
        public override bool CouldHandleSkill => true;
        public override bool CouldHandleBoss => false;
        public override BossStageType HandleBossType => throw new ArgumentException("could not handle Boss");
        
        private bool IsSkillAllowed => !RoundLibDriver.IsShopRound;
        private bool BoardCouldIOCurrency => (RoundLibDriver.IsRequireRound || RoundLibDriver.IsDestoryerRound);
        
        private bool CheckIsSkill() => LevelAsset.SkillMgr != null && LevelAsset.SkillMgr.CurrentSkillType.HasValue &&
                                       LevelAsset.SkillMgr.CurrentSkillType.Value == SkillType.Swap;

        private bool CheckAutoF() => AutoDrive.HasValue && AutoDrive.Value;
        private bool CheckAutoR() => IsReverseCycle;

        private void UpdateLevelAsset()
        {
            var lastStage = RoundLibDriver.PreviousRoundGist?.Type ?? StageType.Shop;
            var lastDestoryBool = lastStage == StageType.Destoryer;

            if (RoundLibDriver.IsRequireRound && IsForwardCycle)
            {
                LevelAsset.GameBoard.BoardGirdDriver.UpdatePatternDiminishing();
            }

            if ((lastDestoryBool && !RoundLibDriver.IsDestoryerRound) &&
                !WorldCycler.NeedAutoDriveStep.HasValue)
            {
                LevelAsset.GameBoard.BoardGirdDriver.DestoryHeatsinkOverlappedUnit();
            }

            if ((LevelAsset.DestroyerEnabled && !RoundLibDriver.IsDestoryerRound) &&
                !WorldCycler.TelemetryStage)
            {
                LevelAsset.WarningDestoryer.ForceReset();
            }
        }

        protected override void AdditionalArtLevelReference(ref GameAssets LevelAsset)
        {
            LevelAsset.TimeLine = FindObjectOfType<TimeLine>();
            LevelAsset.SkillMgr = FindObjectOfType<SkillMgr>();
            LevelAsset.CineCam = FindObjectOfType<CinemachineFreeLook>();
        }
        
        protected override void AdditionalInitLevel()
        {
            base.AdditionalInitLevel();
            WorldExecutor.InitDestoryer(ref LevelAsset);
            WorldExecutor.InitShop(ref LevelAsset);
            WorldExecutor.StartShop(ref LevelAsset);
            
            var message = new CurrencyUpdatedInfo()
            {
                CurrencyVal = Mathf.RoundToInt(LevelAsset.GameCurrencyMgr.Currency),
                IncomesVal = 0,
            };
            
            MessageDispatcher.SendMessage(message);
            
            if (LevelAsset.ActionAsset.RoundLib.Count > 0)
            {
                //这个东西放在这里还是怎么着？就先这样吧。
                WorldCycler.InitCycler();
                if (LevelAsset.TimeLine != null)
                {
                    LevelAsset.TimeLine.InitWithAssets(LevelAsset);
                }
            }
        }

        private void UpdateRoundData_Stepped()
        {
            var roundGist = RoundLibDriver.CurrentRoundGist.Value;
            var tCount = LevelAsset.ActionAsset.GetTruncatedStep(LevelAsset.StepCount);
            if (roundGist.SwitchHeatsink(tCount))
            {
                LevelAsset.GameBoard.BoardGirdDriver.UpdatePatternID();
            }

            UpdateLevelAsset();

            LevelAsset.DestroyerEnabled = WorldCycler.TelemetryStage;

            if (RoundLibDriver.IsRequireRound || RoundLibDriver.IsShopRound)
            {
                var normalRval = roundGist.normalReq;
                var networkRval = roundGist.networkReq;
                var noRequirement = (normalRval == 0 && networkRval == 0);
                if (noRequirement)
                {
                   LevelAsset.TimeLine.RequirementSatisfied = true;
                }
                else
                {
                    var signalInfo = new BoardSignalUpdatedInfo
                    {
                        SignalData = new BoardSignalUpdatedData()
                        {
                            TgtTypeASignal = normalRval,
                            TgtTypeBSignal = networkRval,
                        },
                    };
                    MessageDispatcher.SendMessage(signalInfo);

                    if (LevelAsset.TimeLine.RequirementSatisfied)
                    {
                        if (roundGist.Type == StageType.Require)
                        {
                            LevelAsset.ReqOkCount++;
                        }
                    }

                }
            }

            var discount = 0;

            if (!LevelAsset.Shop.ShopOpening && RoundLibDriver.IsShopRound)
            {
                discount = LevelAsset.SkillMgr.CheckDiscount();
            }

            LevelAsset.Shop.OpenShop(RoundLibDriver.IsShopRound, discount);
            LevelAsset.SkillMgr.SkillEnabled = LevelAsset.SkillEnabled = IsSkillAllowed;
        }
        
        private void AddtionalRecatIO_Skill()
        {
            if (LevelAsset.SkillEnabled)
            {
                if (LevelAsset.SkillMgr != null)
                {
                    LevelAsset.SkillMgr.TriggerSkill(LevelAsset, _ctrlPack);
                }
            }
        }

        protected override void AddtionalRecatIO() => AddtionalRecatIO_Skill();

        private void CareerCycle()
        {
            if (LevelAsset.DestroyerEnabled)
            {
                WorldExecutor.UpdateDestoryer(LevelAsset);
                if (LevelAsset.WarningDestoryer != null)
                {
                    LevelAsset.WarningDestoryer.Step(out var outCore);
                    LevelAsset.DestoryedCoreType = outCore;
                }
            }

            if (RoundLibDriver.CurrentRoundGist.HasValue)
            {
                UpdateRoundData_Stepped();
                var timingEvent = new TimingEventInfo
                {
                    Type = WorldEvent.InGameStatusChangedEvent,
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
            }
        }

        protected override bool CheckGameOver
        {
            get
            {
                var res=LevelAsset.ActionAsset.HasEnded(LevelAsset.StepCount);
                if (res) WorldCycler.Reset();
                return res;
            }
        }

        protected override void UpdateBoardData_Instantly()
        {
            base.UpdateBoardData_Instantly();
            var inCome = 0;
            var cost = 0;

            var tmpInComeM = TypeASignalScore + TypeBSignalScore;
            if (BoardCouldIOCurrency) //这个现在和Round完全绑定了。
            {
                inCome += Mathf.FloorToInt(tmpInComeM);
                inCome = Mathf.RoundToInt(inCome * LevelAsset.CurrencyRebate);
                if (!RoundLibDriver.IsRequireRound) inCome = 0;
                cost = LevelAsset.GameBoard.BoardGirdDriver.heatSinkCost;
                LevelAsset.DeltaCurrency = inCome - cost;
            }
            else
            {
                LevelAsset.DeltaCurrency = 0;
            }


            var message = new CurrencyUpdatedInfo()
            {
                CurrencyVal = Mathf.RoundToInt(LevelAsset.GameCurrencyMgr.Currency),
                IncomesVal = Mathf.RoundToInt(LevelAsset.DeltaCurrency),
            };
            MessageDispatcher.SendMessage(message);
        }

        private void ReverseCycle()
        {
            WorldCycler.StepDown();
            LevelAsset.TimeLine.Reverse();
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

        public override IEnumerator UpdateArtLevelReference(AsyncOperation aOP,AsyncOperation aOP2)
        {
            while (!aOP.isDone||!aOP2.isDone)
            {
                yield return 0;
            }
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVEVISUAL));
            LevelAsset.ItemPriceRoot = GameObject.Find("PlayUI");
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(LEVEL_ART_SCENE_ID));
            LevelAsset.Shop = FindObjectOfType<ShopBase>();
            LevelAsset.DataScreen = FindObjectOfType<DataScreen>();
            LevelAsset.HintMaster = FindObjectOfType<HintMaster>();
            AdditionalArtLevelReference(ref LevelAsset);
            LevelAsset.HintMaster.HideTutorialFrame = false;
            PopulateArtLevelReference();
        }
        
        protected override void ModifiyRootFSMTransitions(ref HashSet<RootFSMTransition> RootFSMTransitions)
        {
            base.ModifiyRootFSMTransitions(ref RootFSMTransitions);
            RootFSMTransitions.Remove(new RootFSMTransition(RootFSMStatus.F_Cycle, RootFSMStatus.Animate, 1, CheckStartAnimate, TriggerAnimation));
            RootFSMTransitions.Remove(new RootFSMTransition(RootFSMStatus.F_Cycle, RootFSMStatus.MinorUpKeep));

            #region ADD Consequence

            RootFSMTransitions.Remove(new RootFSMTransition(RootFSMStatus.PreInit, RootFSMStatus.MajorUpKeep, 1,
                CheckInited));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.PreInit, RootFSMStatus.MajorUpKeep, 1,
                CheckInited, InitCareer));

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

        protected override void Awake()
        {
            base.Awake();
            RoundLibDriver = new RoundLibDriver {owner = this};
        }
    }
}