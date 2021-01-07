using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace ROOT
{
    using FSMActions = Dictionary<RootFSMStatus, Action>;
    using FSMTransitions = List<RootFSMTransition>;

    //���治ͬ�����Ϳ���ʹ��partial�ؼ��ֲ𿪹���
    public abstract class FSMLevelLogic : LevelLogic //LEVEL-LOGIC/ÿһ�ض���һ������ࡣ
    {
        [ReadOnly] bool shouldCycle = false;
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
        }

        //����һ���ṩInfo�ļ����ǣ�Boss�׶�*BossInfoSprayCount*SprayCountPerAnimateInterval;
        private const int SprayCountPerAnimateInterval = 4;
        private const float BossInfoSprayTimerIntervalOffsetRange = 0.5f;

        private float _bossInfoSprayTimerIntervalBase =>
            AnimationDuration / SprayCountPerAnimateInterval;
        private float _bossInfoSprayTimerInterval =>
            _bossInfoSprayTimerIntervalBase + _bossInfoSprayTimerIntervalOffset; 

        private float _bossInfoSprayTimerIntervalOffset = 0.0f;
        private float _bossInfoSprayTimer = 0.0f;
        private Coroutine ManualListenBossPauseKeyCoroutine;

        private int[] SprayCountArray;
        private int SprayCounter = 0;

        protected void BossInit()
        {
            var bossStageCount = LevelAsset.ActionAsset.BossStageCount;
            var totalSprayCount = bossStageCount * SprayCountPerAnimateInterval;
            //������ݻ��ô���ȥ��
            var targetInfoCount =
                Mathf.RoundToInt(LevelAsset.ActionAsset.InfoCount * LevelAsset.ActionAsset.InfoTargetRatio);
            LevelAsset.SignalPanel.SignalTarget = targetInfoCount;

            SprayCountArray = Utils.SpreadOutLayingWRandomization(totalSprayCount, LevelAsset.ActionAsset.InfoCount,
                LevelAsset.ActionAsset.InfoVariantRatio);

            LevelAsset.DestroyerEnabled = true;
            LevelAsset.SignalPanel.IsBossStage = true;
            ManualListenBossPauseKeyCoroutine = StartCoroutine(ManualPollingBossPauseKey());
            WorldCycler.BossStage = true;
        }
        protected void BossUpdate()
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

        protected void PreInit()
        {

        }

        protected void UpKeepAction()
        {
            roundGist = LevelAsset.ActionAsset.GetRoundGistByStep(LevelAsset.StepCount);
            _ctrlPack = WorldController.UpdateInputScheme(LevelAsset, out movedTile, out movedCursor, ref LevelAsset._boughtOnce);
            RootDebug.Watch(_ctrlPack.CtrlCMD.ToString(), WatchID.YanYoumo_ExampleB);
            //RISK ��ʱ��������һ����ش���
            if (Input.GetKeyDown(KeyCode.K))
            {
                WorldCycler.ExpectedStepIncrement(5);
            }
        }

        private void CycleKeepUp()
        {
            WorldLogic.UpkeepLogic(LevelAsset, stage, false); //RISK ���ҲҪŪ��
        }

        protected void ReactIO()
        {
            CycleKeepUp();

            //RISK �������Ҳ�Ȳ�Ҫ�ã����Ū�Ĳ����Ū��
            WorldExecutor_Dispatcher.Root_Executor_Compound_Ordered(
                new[] {LogicCommand.UpdateUnitCursor, LogicCommand.RotateUnit},
                ref LevelAsset, in _ctrlPack, out var res);
            var tRes = (bool[]) res[LogicCommand.UpdateUnitCursor];
            movedTile = tRes[0];
            movedCursor = tRes[1];

            LevelAsset.GameBoard.UpdateBoardRotate(); //TODO ��ת���ڻ������ֵġ���������ż�����

            //RISK �����ù���ԪҲ���ǿ���ƶ�һ����
            WorldExecutor_Dispatcher.Root_Executor(LogicCommand.UpdateShop, ref LevelAsset, in _ctrlPack, out var pRes);
            movedTile |= (bool) pRes;
            movedTile |= _ctrlPack.HasFlag(ControllingCommand.CycleNext); //���flag��ʵ�ʺ���������г�ͻ��

            LevelAsset.SkillMgr.SkillEnabled = LevelAsset.SkillEnabled;
            LevelAsset.SkillMgr.TriggerSkill(LevelAsset, _ctrlPack);
        }

        public RoundGist? roundGist { get; private set; }
        public StageType stage => roundGist?.Type ?? StageType.Shop;

        protected void ForwardCycle()
        {
            var forwardCycle = false;

            var AutoDrive = WorldCycler.NeedAutoDriveStep;
            forwardCycle = (AutoDrive.HasValue && AutoDrive.Value) || movedTile;

            if (forwardCycle)
            {
                WorldLogic.UpdateCycle(LevelAsset, stage);
            }

            //RISK 
            //���Anykeydown����ͬһ֡�ˣ����Բ������ˡ�
            shouldCycle = (AutoDrive.HasValue) || WorldLogic.ShouldCycle(in _ctrlPack, true, in movedTile, in movedCursor);

            if (roundGist.HasValue)
            {
                UpdateRoundStatus(LevelAsset, roundGist.Value);
            }

            LightUpBoard();

            if (LevelAsset.GameOverEnabled)
            {
                UpdateGameOverStatus(LevelAsset);
            }

            if (((AutoDrive.HasValue && AutoDrive.Value || shouldCycle && movedTile)) && (!_noRequirement))
            {
                if (LevelAsset.TimeLine.RequirementSatisfied)
                {
                    LevelAsset.ReqOkCount++;
                }
            }

            Animating = shouldCycle;

            if (Animating)
            {
                //��������̺Ͷ�̬���������ر���ݣ�����˻���Ҫ����һ�¡�
                //RISK Skill�Ǹ�״̬������FF���ܺ�ʹ��ԭ������Ϊ�Ǹ�ʱ�򣬹������룬����Ҳ���˶�Ӧ�¼����ȵĶ�����
                //FFǰ��N��ʱ�̣�����N����������������ֻ��ǡ��������ʱ����ʱ���ᶯ��ʱ��ƥ�䣻
                //�������ʱ���ᶯ����ƥ���������ġ����󡱡�
                //��FSM�����У���ȥ�ܴ���Ŀն����ˣ���ƥ�䲻���ˡ�
                //��Ҳ����˵ʱ������⣻ֻ��Animating�ļ����߼�ԭ��������AutoDrive��֮ǰΪ�˼�ɾ�ˣ�����ԭʼ���߼��������ͺ��ˣ�
                //�����Ǹ��α겻�α��ķ������о������бȡ��ն������ġ����⡱�������Ӹ������㷨��
                //SOLVED-�����Ȱѡ��ն�����������Ū�����ˣ��ȴ�������һ����Ū��
                AnimationTimerOrigin = Time.timeSinceLevelLoad;
                LevelAsset.MovedTileAni = movedTile;
                LevelAsset.MovedCursorAni = movedCursor;
                animate_Co = StartCoroutine(Animate()); //������ɺ���Animating�������
            }
        }

        protected void ReverseCycle()
        {
            CycleKeepUp();
        }

        protected void AnimateAction()
        {
            //Ŀǰ��������յģ���ʱ����ܰ�Animate��CoRoutine����Ķ���Ū������
            Debug.Assert(animate_Co != null);
            WorldLogic.UpkeepLogic(LevelAsset, stage, Animating);
        }

        protected void CleanUp()
        {
            shouldCycle = false;
            movedTile = false;
            movedCursor = false;
            animate_Co = null;
            LevelAsset.AnimationPendingObj = new List<MoveableBase>();
            LevelAsset.DeltaCurrency = 0.0f;
        }

        private RootFSM MainFSM;

        private void InitDestoryer()
        {
            LevelAsset.WarningDestoryer = new MeteoriteBomber {GameBoard = LevelAsset.GameBoard};
            LevelAsset.WarningDestoryer.Init(4, 1);
        }

        private void InitShop()
        {
            LevelAsset.Shop.ShopInit(LevelAsset);
            LevelAsset.Shop.CurrentGameStateMgr = LevelAsset.GameStateMgr;
            LevelAsset.Shop.GameBoard = LevelAsset.GameBoard;
            if (LevelAsset.ActionAsset.ExcludedShop)
            {
                LevelAsset.Shop.excludedTypes = LevelAsset.ActionAsset.ShopExcludedType;
            }
        }

        private void InitCursor(Vector2Int pos)
        {
            LevelAsset.GameCursor = Instantiate(LevelAsset.CursorTemplate);
            Cursor cursor = LevelAsset.GameCursor.GetComponent<Cursor>();
            cursor.InitPosWithAnimation(pos);
            cursor.UpdateTransform(LevelAsset.GameBoard.GetFloatTransformAnimation(cursor.LerpingBoardPosition));
        }

        public sealed override void InitLevel()
        {
            Debug.Assert(ReferenceOk); //�������ȷ��Reference�ġ������С���
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

            LevelAsset.HintMaster.ShouldShowCheckList = false;
        }

        private void LightUpBoard()
        {
            if (_ctrlPack.HasFlag(ControllingCommand.FloatingOnGrid) ||
                _ctrlPack.HasFlag(ControllingCommand.ClickOnGrid))
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

        protected abstract FSMActions fsmActions { get; }
        protected abstract FSMTransitions RootFSMTransitions { get; }

        protected sealed override void StartShop()
        {
            base.StartShop();
        }
        protected sealed override bool UpdateGameOverStatus(GameAssets currentLevelAsset)
        {
            return base.UpdateGameOverStatus(currentLevelAsset);
        }
        protected sealed override void UpdateLogicLevelReference()
        {
            base.UpdateLogicLevelReference();
        }

        #region Transitions

        protected bool AutoTrans()
        {
            return true;
        }

        #endregion

        void Awake()
        {
            LevelAsset = new GameAssets();
            MainFSM = new RootFSM {owner = this};

            UpdateLogicLevelReference();

            MainFSM.ReplaceActions(fsmActions);
            MainFSM.ReplaceTransition(RootFSMTransitions);
        }
        //���ڲ�����΢����ӳ٣�����ΪIO�Ŀ���״̬��Idle����ʵ�ʵĶ�������������Cycle��֮�仹����һ֡��
        //�����ϻ���Ҫ��IO����¼������Խ�FSM����ĳ��״̬�ϣ�Ҫ��Ȼ����Ū��
        void Update()
        {
            //�����и��ܺõĵط���״̬ת�Ƶ��Ǻ�Update��ȫ�����ˡ�
            //��������ø���״̬�������¼�������١����Ǿ��Բ��ǿ��׵Ľⷨ������Ҫ���¼���
            var transitPerFrame = 3;
            for (var i = 0; i < transitPerFrame; i++)
            {
                MainFSM.Execute();
                MainFSM.Transit();
                RootDebug.Watch("FSM:" + MainFSM.currentStatus, WatchID.YanYoumo_ExampleB);
            }
        }
    }
}

