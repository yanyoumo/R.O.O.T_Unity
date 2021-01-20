﻿using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
// https://shimo.im/docs/Dd86KXTqHJpqxwYX
// ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable
namespace ROOT
{
    //这一支儿是在FSM逻辑下好了之后随时整枝剪掉。
    [Obsolete]
    public abstract class BranchingLevelLogic : MonoBehaviour //LEVEL-LOGIC/每一关都有一个这个类。
    {
        /*private float animationTimer => Time.timeSinceLevelLoad - AnimationTimerOrigin;
        private float AnimationLerper
        {
            get
            {
                float res = animationTimer / AnimationDuration;
                return Mathf.Min(res, 1.0f);
            }
        }

        private Coroutine animate_Co;
        

        protected virtual void Awake()
        {
            LevelAsset = new GameAssets();
            UpdateLogicLevelReference();
        }

        IEnumerator Animate()
        {
            while (AnimationLerper < 1.0f)
            {
                yield return 0;
                if (LevelAsset.AnimationPendingObj.Count > 0)
                {
                    foreach (var moveableBase in LevelAsset.AnimationPendingObj)
                    {
                        if (moveableBase.NextBoardPosition == moveableBase.CurrentBoardPosition)
                        {
                            moveableBase.SetPosWithAnimation(moveableBase.NextBoardPosition,
                                PosSetFlag.CurrentAndLerping);
                        }
                        else
                        {
                            moveableBase.LerpingBoardPosition = moveableBase.LerpBoardPos(AnimationLerper);
                        }
                    }
                }

                //加上允许手动步进后，这个逻辑就应该独立出来了。
                if (LevelAsset.MovedTileAni)
                {
                    if (LevelAsset.Shop)
                    {
                        if (LevelAsset.Shop is IAnimatableShop shop)
                        {
                            shop.ShopUpdateAnimation(AnimationLerper);
                        }
                    }
                }

                LevelAsset.GameBoard.UpdateBoardAnimation();
                cursor.UpdateTransform(LevelAsset.GameBoard.GetFloatTransformAnimation(cursor.LerpingBoardPosition));
            }

            foreach (var moveableBase in LevelAsset.AnimationPendingObj)
            {
                //完成后的pingpong
                moveableBase.SetPosWithAnimation(moveableBase.NextBoardPosition, PosSetFlag.All);
            }

            if (LevelAsset.MovedTileAni)
            {
                if (LevelAsset.GameBoard != null)
                {
                    LevelAsset.GameBoard.UpdateBoardPostAnimation();
                }

                if (LevelAsset.Shop)
                {
                    if (LevelAsset.Shop is IAnimatableShop shop)
                    {
                        shop.ShopPostAnimationUpdate();
                    }
                }
            }

            Animating = false;
            yield break;
        }

        //现在一共提供Info的计数是：Boss阶段*BossInfoSprayCount*SprayCountPerAnimateInterval;
        private const int SprayCountPerAnimateInterval = 4;
        //private const int BossInfoSprayCount = 3;
        private const float BossInfoSprayTimerIntervalOffsetRange = 0.5f;

        private float _bossInfoSprayTimerIntervalBase => AnimationDuration / SprayCountPerAnimateInterval;//TODO 这个可能要做成和Anime时长相关的随机数。
        private float _bossInfoSprayTimerInterval => _bossInfoSprayTimerIntervalBase+ _bossInfoSprayTimerIntervalOffset;//TODO 这个可能要做成和Anime时长相关的随机数。
        private float _bossInfoSprayTimerIntervalOffset = 0.0f;
        private float _bossInfoSprayTimer=0.0f;
        private Coroutine ManualListenBossPauseKeyCoroutine;

        private int[] SprayCountArray;
        private int SprayCounter = 0;

        private void BossInit()
        {
            var bossStageCount = LevelAsset.ActionAsset.BossStageCount;
            var totalSprayCount = bossStageCount * SprayCountPerAnimateInterval;
            //这个数据还得传过去。
            var targetInfoCount = Mathf.RoundToInt(LevelAsset.ActionAsset.InfoCount * LevelAsset.ActionAsset.InfoTargetRatio);
            LevelAsset.SignalPanel.SignalTarget = targetInfoCount;

            SprayCountArray = Utils.SpreadOutLayingWRandomization(totalSprayCount, LevelAsset.ActionAsset.InfoCount,
                LevelAsset.ActionAsset.InfoVariantRatio);
            
            LevelAsset.DestroyerEnabled = true;
            LevelAsset.SignalPanel.IsBossStage = true;
            ManualListenBossPauseKeyCoroutine = StartCoroutine(ManualPollingBossPauseKey());
            WorldCycler.BossStage = true;
        }

        private void BossUpdate()
        {
            //Spray的逻辑可以再做一些花活。
            if (!WorldCycler.BossStagePause)
            {
                _bossInfoSprayTimer += Time.deltaTime;
                if (_bossInfoSprayTimer >= _bossInfoSprayTimerInterval)
                {
                    try
                    {
                        LevelAsset.AirDrop.SprayInfo(SprayCountArray[SprayCounter]);
                    }
                    catch(IndexOutOfRangeException)
                    {
                        LevelAsset.AirDrop.SprayInfo(3);
                    }
                    _bossInfoSprayTimerIntervalOffset = Random.Range(
                        -BossInfoSprayTimerIntervalOffsetRange,
                        BossInfoSprayTimerIntervalOffsetRange);
                    _bossInfoSprayTimer = 0.0f;
                    SprayCounter++;
                }
            }
        }


        //原则上这个不让被重载。
        //TODO Digong需要了解一些主干的Update流程。
        //未来需要将动画部分移动至随机位置。
        protected virtual void Update()
        {
            if ((!ReadyToGo) || (PendingCleanUp))
            {
                Playing = false;
                return;
            }

            if (!Playing) Playing = true;
            _ctrlPack = new ControllingPack {CtrlCMD = ControllingCommand.Nop};

            bool? AutoDrive = null;
            bool shouldCycle = false, movedTile = false;
            var roundGist = LevelAsset.ActionAsset.GetRoundGistByStep(LevelAsset.StepCount);
            var stage = roundGist?.Type ?? StageType.Shop;

            if (stage == StageType.Boss)
            {
                //TODO 之后Boss部分就在这儿搞。
                if (!WorldCycler.BossStage)
                {
                    BossInit();
                }
                BossUpdate();
            }
            
            if (!Animating)//很可能动画要改成准轮询的，换句话说程序要有能打断动画的“能力”。
            {
                //Solid_Logic
                animate_Co = null;
                LevelAsset.AnimationPendingObj = new List<MoveableBase>();

                // ShouldCycle这个放到WorldLogic里面去了。
                WorldLogic.UpdateLogic(LevelAsset, in stage, out _ctrlPack, out movedTile, out var movedCursor,out shouldCycle,out AutoDrive);

                if (roundGist.HasValue)
                {
                    UpdateRoundStatus(LevelAsset, roundGist.Value);
                }

                if (_ctrlPack.HasFlag(ControllingCommand.FloatingOnGrid)|| _ctrlPack.HasFlag(ControllingCommand.ClickOnGrid))
                {
                    if (_ctrlPack.HasFlag(ControllingCommand.FloatingOnGrid))
                    {
                        LevelAsset.GameBoard.LightUpBoardGird(_ctrlPack.CurrentPos);
                    }

                    if (_ctrlPack.HasFlag(ControllingCommand.ClickOnGrid))
                    {
                        LevelAsset.GameBoard.LightUpBoardGird(_ctrlPack.CurrentPos,
                            LightUpBoardGirdMode.REPLACE,
                            LightUpBoardColor.Clicked);
                    }
                }
                else
                {
                    LevelAsset.GameBoard.LightUpBoardGird(Vector2Int.zero, LightUpBoardGirdMode.CLEAR);
                }

                if (LevelAsset.GameOverEnabled)
                {
                    UpdateGameOverStatus(LevelAsset);
                }

                Animating = shouldCycle;

                if ((AutoDrive.HasValue && AutoDrive.Value || shouldCycle && movedTile) && (!_noRequirement))
                {
                    if (LevelAsset.TimeLine.RequirementSatisfied)
                    {
                        LevelAsset.ReqOkCount++;
                    }
                }

                if (Animating)
                { 
                    AnimationTimerOrigin = Time.timeSinceLevelLoad;
                    LevelAsset.MovedTileAni = movedTile;
                    LevelAsset.MovedCursorAni = movedCursor;
                    animate_Co=StartCoroutine(Animate()); //这里完成后会把Animating设回来。
                }
            }
            else
            {
                //Animation_Logic_Upkeep
                Debug.Assert(animate_Co != null);
                WorldLogic.UpkeepLogic(LevelAsset, stage, Animating);
            }

            if (LevelAsset.HintEnabled)
            {
                LevelAsset.HintMaster.UpdateHintMaster(_ctrlPack);
            }

            if ((shouldCycle) && (movedTile || AutoDrive.HasValue))
            {
                if (roundGist.HasValue && roundGist.Value.Type == StageType.Require)
                {
                    LevelAsset.GameBoard.UpdatePatternDiminishing();
                }
            }

            LevelAsset.LevelProgress = LevelAsset.StepCount / (float)LevelAsset.ActionAsset.PlayableCount;

            if (_ctrlPack.HasFlag(ControllingCommand.CameraMov))
            {
                var cam = LevelAsset.CineCam;
                cam.m_XAxis.Value += _ctrlPack.CameraMovement.x;
                cam.m_YAxis.Value += _ctrlPack.CameraMovement.y*0.05f;
            }
        }

        protected bool UpdateCareerGameOverStatus(GameAssets currentLevelAsset)
        {
            if (LevelAsset.ActionAsset.HasEnded(LevelAsset.StepCount))
            {
                if (!IsTutorialLevel)
                {
                    PendingCleanUp = true;
                    LevelMasterManager.Instance.LevelFinished(LevelAsset);
                }
                return true;
            }
            return false;
        }

        protected virtual void OnDestroy()
        {
            if (ManualListenBossPauseKeyCoroutine != null)
            {
                StopCoroutine(ManualListenBossPauseKeyCoroutine);
            }
        }*/
    }

    [Obsolete]
    public class DefaultLevelLogic : BranchingLevelLogic //LEVEL-LOGIC/每一关都有一个这个类。
    {
        /*public override void InitLevel()
        {
            Debug.Assert(ReferenceOk); //意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));

            InitCurrencyIoMgr();
            LevelAsset.DeltaCurrency = 0.0f;

            LevelAsset.GameStateMgr = new GameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(LevelAsset.ActionAsset.GameModeAsset);
            LevelAsset.CostChart.CurrencyVal = Mathf.RoundToInt(LevelAsset.GameStateMgr.GetCurrency());

            InitShop();
            StartShop();
            InitDestoryer();
            WorldExecutor.InitCursor(ref LevelAsset,new Vector2Int(2, 3));
            LevelAsset.EnableAllCoreFunctionAndFeature();
            LevelAsset.GameBoard.UpdateBoardAnimation();
            LevelAsset.ActionAsset = null;

            ReadyToGo = true;
        }

        protected void InitDestoryer()
        {
            LevelAsset.WarningDestoryer = new MeteoriteBomber { GameBoard = LevelAsset.GameBoard };
            LevelAsset.WarningDestoryer.Init(5, 2);
        }
        protected void InitShop()
        {
            LevelAsset.Shop.ShopInit(LevelAsset);
            LevelAsset.Shop.CurrentGameStateMgr = LevelAsset.GameStateMgr;
            LevelAsset.Shop.GameBoard = LevelAsset.GameBoard;
        }
        protected void InitCurrencyIoMgr()
        {
            //LevelAsset.BoardDataCollector = gameObject.AddComponent<BoardDataCollector>();
            //LevelAsset.BoardDataCollector.m_Board = LevelAsset.GameBoard;
        }*/
    }
}