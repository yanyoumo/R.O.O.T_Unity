using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace ROOT
{
    public class CareerFSMLevelLogic : LevelLogic //LEVEL-LOGIC/ÿһ�ض���һ������ࡣ
    {
        [ReadOnly]bool shouldCycle = false;
        [ReadOnly] bool movedTile = false;
        [ReadOnly] bool movedCursor = false;

        #region BossStage

        private static float BossStagePauseCostTimer = 0.0f;
        private const float BossStagePauseCostInterval = 1.0f;
        private const int BossStagePricePerInterval = 1;
        private IEnumerator BossStagePauseCost()
        {
            yield return 0;
            while (true)
            {
                yield return 0;
                BossStagePauseCostTimer += Time.deltaTime;

                if (!(BossStagePauseCostTimer >= BossStagePauseCostInterval)) continue;
                BossStagePauseCostTimer = 0.0f;
                LevelAsset.ReqOkCount -= BossStagePricePerInterval;

                if (LevelAsset.ReqOkCount > 0) continue;
                WorldExecutor_Dispatcher.Root_Executor_void_PUBLIC(LogicCommand.BossUnpaused, ref LevelAsset);
                yield break;
            }
        }
        private bool _noRequirement;

        #endregion

        //ASSET
        private float animationTimer => Time.timeSinceLevelLoad - AnimationTimerOrigin;
        private float AnimationLerper
        {
            get
            {
                float res = animationTimer / AnimationDuration;
                return Mathf.Min(res, 1.0f);
            }
        }

        private Coroutine animate_Co;

        protected override void UpdateLogicLevelReference()
        {
            LevelAsset.CursorTemplate = Resources.Load<GameObject>("Cursor/Prefab/Cursor");
            LevelAsset.GameBoard = FindObjectOfType<Board>();
            LevelAsset.AirDrop = LevelAsset.GameBoard.AirDrop;
            LevelAsset.AirDrop.GameAsset = LevelAsset;
            LevelAsset.Owner = this;
        }

        protected override void Awake()
        {
            Debug.Log("FSM Awake");
            LevelAsset = new GameAssets();
            UpdateLogicLevelReference();
            MainFSM = gameObject.AddComponent<CareerDefaultFSM>();
            MainFSM.owner = this;
        }

        /// <summary>
        /// ��Ҫ�������Levelȥ�Զ������Link��
        /// </summary>
        /// <param name="aOP">��һ��Loading�����߼��������첽����ʵ��</param>
        /// <returns></returns>
        public override IEnumerator UpdateArtLevelReference(AsyncOperation aOP)
        {
            while (!aOP.isDone)
            {
                yield return 0;
            }

            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVEVISUAL));
            LevelAsset.ItemPriceRoot = GameObject.Find("PlayUI");
            LevelAsset.DataScreen = FindObjectOfType<DataScreen>();
            LevelAsset.HintMaster = FindObjectOfType<HintMaster>();
            LevelAsset.TimeLine = FindObjectOfType<TimeLine>();
            LevelAsset.CostLine = FindObjectOfType<CostLine>();
            LevelAsset.Shop = FindObjectOfType<ShopBase>();
            LevelAsset.SkillMgr = FindObjectOfType<SkillMgr>();
            LevelAsset.CostChart = FindObjectOfType<CostChart>();
            LevelAsset.SignalPanel = FindObjectOfType<SignalPanel>();
            LevelAsset.CineCam = FindObjectOfType<CinemachineFreeLook>();
            LevelAsset.HintMaster.HideTutorialFrame = false;
            PopulateArtLevelReference();
        }

        public override bool CheckReference()
        {
            bool res = true;
            res &= (LevelAsset.DataScreen != null);
            return res;
        }

        public override void PopulateArtLevelReference()
        {
            ReferenceOk = CheckReference();
        }

        protected override void StartShop()
        {
            LevelAsset.Shop.ShopStart();
        }

        protected override bool UpdateGameOverStatus(GameAssets currentLevelAsset)
        {
            //��������ͺܽӽ�����Ҫ�����¶��ˡ�
            if (!currentLevelAsset.GameStateMgr.EndGameCheck()) return false;
            PendingCleanUp = true;
            LevelMasterManager.Instance.LevelFinished(LevelAsset);
            return true;
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

                //���������ֶ�����������߼���Ӧ�ö��������ˡ�
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
                //��ɺ��pingpong
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

        //����һ���ṩInfo�ļ����ǣ�Boss�׶�*BossInfoSprayCount*SprayCountPerAnimateInterval;
        private const int SprayCountPerAnimateInterval = 4;
        //private const int BossInfoSprayCount = 3;
        private const float BossInfoSprayTimerIntervalOffsetRange = 0.5f;

        private float _bossInfoSprayTimerIntervalBase => AnimationDuration / SprayCountPerAnimateInterval;//TODO �������Ҫ���ɺ�Animeʱ����ص��������
        private float _bossInfoSprayTimerInterval => _bossInfoSprayTimerIntervalBase + _bossInfoSprayTimerIntervalOffset;//TODO �������Ҫ���ɺ�Animeʱ����ص��������
        private float _bossInfoSprayTimerIntervalOffset = 0.0f;
        private float _bossInfoSprayTimer = 0.0f;
        private Coroutine ManualListenBossPauseKeyCoroutine;

        private int[] SprayCountArray;
        private int SprayCounter = 0;

        private void BossInit()
        {
            var bossStageCount = LevelAsset.ActionAsset.BossStageCount;
            var totalSprayCount = bossStageCount * SprayCountPerAnimateInterval;
            //������ݻ��ô���ȥ��
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
            //Spray���߼���������һЩ���
            if (!WorldCycler.BossStagePause)
            {
                _bossInfoSprayTimer += Time.deltaTime;
                if (_bossInfoSprayTimer >= _bossInfoSprayTimerInterval)
                {
                    try
                    {
                        LevelAsset.AirDrop.SprayInfo(SprayCountArray[SprayCounter]);
                    }
                    catch (IndexOutOfRangeException)
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

        int _obselateStepID = -1;
        bool lastDestoryBool = false;

        protected override void OnDestroy()
        {
            if (ManualListenBossPauseKeyCoroutine != null)
            {
                StopCoroutine(ManualListenBossPauseKeyCoroutine);
            }
        }
        public void PreInit()
        {

        }

        public void IdleAction()
        {
            _ctrlPack = WorldController.UpdateInputScheme(LevelAsset, out movedTile, out movedCursor, ref LevelAsset._boughtOnce);
            RootDebug.Watch(_ctrlPack.CtrlCMD.ToString(), WatchID.YanYoumo_ExampleB);
        }

        public void UpkeepAction()
        {
            var roundGist = LevelAsset.ActionAsset.GetRoundGistByStep(LevelAsset.StepCount);
            var stage = roundGist?.Type ?? StageType.Shop;
            WorldLogic.UpkeepLogic(LevelAsset, in stage, false);//RISK ���ҲҪŪ��
        }

        public void MajorCycle()
        {
            var roundGist = LevelAsset.ActionAsset.GetRoundGistByStep(LevelAsset.StepCount);
            var stage = roundGist?.Type ?? StageType.Shop;
            animate_Co = null;
            LevelAsset.AnimationPendingObj = new List<MoveableBase>();
            LevelAsset.DeltaCurrency = 0.0f;
            var forwardCycle = false;
            
            //RISK �������Ҳ�Ȳ�Ҫ�ã����Ū�Ĳ����Ū��
            WorldExecutor_Dispatcher.Root_Executor_Compound_Ordered(
                new[] { LogicCommand.UpdateUnitCursor, LogicCommand.RotateUnit },
                ref LevelAsset, in _ctrlPack, out var res);
            var tRes = (bool[])res[LogicCommand.UpdateUnitCursor];
            movedTile = tRes[0];
            movedCursor = tRes[1];

            LevelAsset.GameBoard.UpdateBoardRotate(); //TODO ��ת���ڻ������ֵġ���������ż�����

            //RISK �����ù���ԪҲ���ǿ���ƶ�һ����
            WorldExecutor_Dispatcher.Root_Executor(LogicCommand.UpdateShop, ref LevelAsset, in _ctrlPack, out var pRes);
            movedTile |= (bool)pRes;
            movedTile |= _ctrlPack.HasFlag(ControllingCommand.CycleNext); //���flag��ʵ�ʺ���������г�ͻ��

            LevelAsset.SkillMgr.SkillEnabled = LevelAsset.SkillEnabled;
            LevelAsset.SkillMgr.TriggerSkill(LevelAsset, _ctrlPack);

            forwardCycle = movedTile;
            
            if (forwardCycle)
            {
                WorldLogic.UpdateCycle(LevelAsset, stage);
            }

            //RISK 
            //���Anykeydown����ͬһ֡�ˣ����Բ������ˡ�
            shouldCycle = WorldLogic.ShouldCycle(in _ctrlPack, true, in movedTile, in movedCursor);

            if (roundGist.HasValue)
            {
                UpdateRoundStatus(LevelAsset, roundGist.Value);
            }

            LightUpBoard();

            if (LevelAsset.GameOverEnabled)
            {
                UpdateGameOverStatus(LevelAsset);
            }

            if (((shouldCycle && movedTile)) && (!_noRequirement))
            {
                if (LevelAsset.TimeLine.RequirementSatisfied)
                {
                    LevelAsset.ReqOkCount++;
                }
            }

            Animating = shouldCycle;

            if (Animating)
            {
                AnimationTimerOrigin = Time.timeSinceLevelLoad;
                LevelAsset.MovedTileAni = movedTile;
                LevelAsset.MovedCursorAni = movedCursor;
                animate_Co = StartCoroutine(Animate()); //������ɺ���Animating�������
            }
        }

        public void AnimateAction()
        {
            //Ŀǰ��������յģ���ʱ����ܰ�Animate��CoRoutine����Ķ���Ū������
            var roundGist = LevelAsset.ActionAsset.GetRoundGistByStep(LevelAsset.StepCount);
            var stage = roundGist?.Type ?? StageType.Shop;
            Debug.Assert(animate_Co != null);
            WorldLogic.UpkeepLogic(LevelAsset, in stage, Animating);
        }

        public void CleanUp()
        {
            shouldCycle = false;
            movedTile = false;
            movedCursor = false;
        }

        public void Execute()
        {
            switch (MainFSM.currentStatus)
            {
                case RootFSMStatus.PreInit:
                    PreInit();
                    break;
                case RootFSMStatus.Idle:
                    IdleAction();
                    break;
                case RootFSMStatus.Upkeep:
                    UpkeepAction();
                    break;
                case RootFSMStatus.Cycle:
                    MajorCycle();
                    break;
                case RootFSMStatus.CleanUp:
                    CleanUp();
                    break;
                case RootFSMStatus.BossInit:
                    BossInit();
                    break;
                case RootFSMStatus.Boss:
                    BossUpdate();
                    break;
                case RootFSMStatus.Animate:
                    AnimateAction();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private RootFSMBase MainFSM;

        protected void InitDestoryer()
        {
            LevelAsset.WarningDestoryer = new MeteoriteBomber { GameBoard = LevelAsset.GameBoard };
            LevelAsset.WarningDestoryer.Init(4, 1);
        }
        protected void InitShop()
        {
            LevelAsset.Shop.ShopInit(LevelAsset);
            LevelAsset.Shop.CurrentGameStateMgr = LevelAsset.GameStateMgr;
            LevelAsset.Shop.GameBoard = LevelAsset.GameBoard;
            if (LevelAsset.ActionAsset.ExcludedShop)
            {
                LevelAsset.Shop.excludedTypes = LevelAsset.ActionAsset.ShopExcludedType;
            }
        }
        protected void InitCurrencyIoMgr()
        {
            /*LevelAsset.BoardDataCollector = gameObject.AddComponent<BoardDataCollector>();
            LevelAsset.BoardDataCollector.m_Board = LevelAsset.GameBoard;*/
        }
        protected void InitCursor(Vector2Int pos)
        {
            LevelAsset.GameCursor = Instantiate(LevelAsset.CursorTemplate);
            Cursor cursor = LevelAsset.GameCursor.GetComponent<Cursor>();
            cursor.InitPosWithAnimation(pos);
            cursor.UpdateTransform(LevelAsset.GameBoard.GetFloatTransformAnimation(cursor.LerpingBoardPosition));
        }

        public override void InitLevel()
        {
            Debug.Assert(ReferenceOk);//�������ȷ��Reference�ġ������С���
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));

            LevelAsset.DeltaCurrency = 0.0f;

            LevelAsset.GameStateMgr = new GameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(LevelAsset.ActionAsset.GameModeAsset);
            LevelAsset.CostChart.CurrencyVal = Mathf.RoundToInt(LevelAsset.GameStateMgr.GetCurrency());

            InitShop();
            InitDestoryer();
            InitCursor(new Vector2Int(2, 3));
            LevelAsset.EnableAllCoreFunctionAndFeature();
            LevelAsset.GameBoard.InitBoardWAsset(LevelAsset.ActionAsset);
            LevelAsset.GameBoard.UpdateBoardAnimation();
            StartShop();

            ReadyToGo = true;
            if (LevelAsset.ActionAsset.RoundDatas.Length > 0)
            {
                //��������������ﻹ����ô�ţ����������ɡ�
                WorldCycler.InitCycler();
                LevelAsset.TimeLine.InitWithAssets(LevelAsset);
            }
            //LevelAsset.TimeLine.SetGoalCount = LevelAsset.ActionAsset.TargetCount;
            //LevelAsset.SignalPanel.TgTtMission= LevelAsset.ActionAsset.TargetCount;
            LevelAsset.HintMaster.ShouldShowCheckList = false;
        }

        private void LightUpBoard()
        {
            if (_ctrlPack.HasFlag(ControllingCommand.FloatingOnGrid) || _ctrlPack.HasFlag(ControllingCommand.ClickOnGrid))
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
        }

        protected override void Update()
        {
            Execute();
            MainFSM.Transit();
            //���ڲ�����΢����ӳ٣�����ΪIO�Ŀ���״̬��Idle����ʵ�ʵĶ�������������Cycle��֮�仹����һ֡��
            //�����ϻ���Ҫ��IO����¼������Խ�FSM����ĳ��״̬�ϣ�Ҫ��Ȼ����Ū��
            //RootDebug.Watch("FSM:" + MainFSM.currentStatus, WatchID.YanYoumo_ExampleA);
        }
    }
}