using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace ROOT
{
    //TutorialManager全权由这个东西管理，TutorialMasterManager只传一个ActionBase进来。
    //这里负责把Visual和TutorialManager的引用建立起来。然后就TutorialManager和GameManager接手了。
    public class GameMasterManager : MonoBehaviour
    {

        private ScoreSet NextGameScoreSet = new ScoreSet(1000.0f, 3);
        private PerMoveData NextGamePerMoveData;
        private Type NextGameState;
        private TutorialActionBase NextActionBase;
        private GameMgr gameMgr;


        private static GameGlobalStatus gameGlobalStatus;
        private TutorialMgr tutorialMgr;

        public static GameGlobalStatus getGameGlobalStatus()
        {
            return gameGlobalStatus;
        }

        public static void UpdateGameGlobalStatuslastEndingIncome(float lastEndingIncome)
        {
            gameGlobalStatus.lastEndingIncome = lastEndingIncome;
        }

        public static void UpdateGameGlobalStatuslastEndingTime(float lastEndingTime)
        {
            gameGlobalStatus.lastEndingTime = lastEndingTime;
        }

        void SetUpNextGameParam(ScoreSet scoreSet=null,PerMoveData _perMoveData=new PerMoveData())
        {
            if (scoreSet != null)
            {
                NextGameScoreSet = scoreSet;
            }
            else
            {
                NextGameScoreSet = new ScoreSet(1000.0f, 60);
            }
            NextGamePerMoveData = _perMoveData;
        }

        public void LoadTutorial_LogicOnly(TutorialActionBase actionBase)
        {
            if (gameGlobalStatus.CurrentGameStatus != GameStatus.Tutorial)
            {
                gameGlobalStatus.CurrentGameStatus = GameStatus.Tutorial;
                NextActionBase = actionBase;
                SceneManager.LoadSceneAsync(StaticName.SCENE_ID_ADDTIVELOGIC, LoadSceneMode.Additive);
            }
        }

        public void LoadTutorial_Full(TutorialActionBase actionBase)
        {
            if (gameGlobalStatus.CurrentGameStatus != GameStatus.Tutorial)
            {
                gameGlobalStatus.CurrentGameStatus = GameStatus.Tutorial;
                NextActionBase = actionBase;
                SceneManager.LoadSceneAsync(StaticName.SCENE_ID_ADDTIVEVISUAL, LoadSceneMode.Additive);
                SceneManager.LoadSceneAsync(StaticName.SCENE_ID_ADDTIVELOGIC, LoadSceneMode.Additive);
            }
        }

        void ReloadGamePlay_LogicOnly()
        {
            if (gameGlobalStatus.CurrentGameStatus != GameStatus.Playing)
            {
                gameGlobalStatus.CurrentGameStatus = GameStatus.Playing;
                GameOverMgr.GameRequestSameRestart -= ReloadGamePlay_LogicOnly;
                SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_GAMEOVER);
                SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_GAMEPLAY));
                SetUpNextGameParam();
                SceneManager.LoadSceneAsync(StaticName.SCENE_ID_ADDTIVELOGIC, LoadSceneMode.Additive);
            }
        }

        public void LoadGamePlay_Full_Param(ScoreSet scoreSet, PerMoveData _perMoveData,Type _nextGameState)
        {
            SetUpNextGameParam(scoreSet, _perMoveData);
            NextGameState = _nextGameState;
            LoadGamePlay_Full();
        }

        public void LoadGamePlay_Full()
        {
            if (gameGlobalStatus.CurrentGameStatus != GameStatus.Playing)
            {
                gameGlobalStatus.CurrentGameStatus = GameStatus.Playing;
                SceneManager.LoadSceneAsync(StaticName.SCENE_ID_ADDTIVEVISUAL,
                    LoadSceneMode.Additive); //VisualScene应该做到仅仅Load进来没有任何报错。//UI放在Visual里面，换句话说，就是不能依赖别人。
                SetUpNextGameParam();
                SceneManager.LoadSceneAsync(StaticName.SCENE_ID_ADDTIVELOGIC,
                    LoadSceneMode.Additive); //需要等Masterlink两边的reference，然后才能运行。
            }
        }

        void Awake()
        {
            Random.InitState(Mathf.FloorToInt(Time.realtimeSinceStartup));
            //先写在这儿。
            gameGlobalStatus = new GameGlobalStatus {CurrentGameStatus = GameStatus.Starting};
        }

        // Start is called before the first frame update
        void Start()
        {
            //LoadGamePlay_Full();
        }

        void CleanUpLogicScene()
        {
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_ADDTIVELOGIC);
        }

        void CleanUpVisualScene()
        {
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_ADDTIVEVISUAL);
        }

        void CurrentGameOver()
        {
            GameMgr.GameOverReached -= CurrentGameOver;
            if (gameGlobalStatus.CurrentGameStatus != GameStatus.Ended)
            {
                gameGlobalStatus.CurrentGameStatus = GameStatus.Ended;
                SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_GAMEPLAY));
                CleanUpLogicScene();
                SceneManager.LoadSceneAsync(StaticName.SCENE_ID_GAMEOVER, LoadSceneMode.Additive);
                GameOverMgr.GameRequestSameRestart += ReloadGamePlay_LogicOnly;
            }
        }

        void CurrentTutorialOver()
        {
            if (gameGlobalStatus.CurrentGameStatus == GameStatus.Tutorial)
            {
                gameGlobalStatus.CurrentGameStatus = GameStatus.Starting;
                TutorialMgr.TutorialCompleteReached -= CurrentTutorialOver;
                SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_GAMEPLAY));
                CleanUpLogicScene();
                TutorialMasterMgr.TutorialStart -= LoadTutorial_Full;
                TutorialMasterMgr.TutorialStart += LoadTutorial_LogicOnly;
                SceneManager.LoadSceneAsync(StaticName.SCENE_ID_TUTORIAL, LoadSceneMode.Additive);
            }
        }

        bool KeepFindObjectOfType<T>(ref T val) where T : UnityEngine.Object
        {
            if (val == null)
            {
                val = FindObjectOfType<T>();
                return ((val == null));
            }
            else
            {
                return true;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (gameMgr != null)
            {
                if (gameMgr.CheckReference())
                {
                    if (!gameMgr.Playing)
                    {
                        if (gameGlobalStatus.CurrentGameStatus == GameStatus.Playing)
                        {
                            gameMgr.SetReady_GamePlay(NextGameScoreSet, NextGamePerMoveData, NextGameState);
                        }
                        else if (gameGlobalStatus.CurrentGameStatus == GameStatus.Tutorial)
                        {
                            gameMgr.SetReady_Tutorial(NextActionBase.GetScoreSet, NextActionBase.GetPerMoveData,
                                NextActionBase.GetGameMove);
                        }
                    }
                }
                else
                {
                    var tmpO = FindObjectsOfType<GameObject>();
                    var dataScreen = FindObjectOfType<DataScreen>();

                    foreach (var go in tmpO)
                    {
                        if (go.name == "PlayUI")
                        {
                            gameMgr.ItemPriceRoot = go;
                        }
                    }

                    gameMgr.dataScreen = dataScreen;
                    gameMgr.UpdateReference();
                }           
            }
            else
            {
                //这是第一次找到GameMgr的时候，只在上面运行一次的东西写在这。
                gameMgr = FindObjectOfType<GameMgr>();
                if (gameMgr != null)
                {
                    GameMgr.GameOverReached += CurrentGameOver;
                    if (gameGlobalStatus.CurrentGameStatus == GameStatus.Tutorial)
                    {
                        if (tutorialMgr == null)
                        {
                            GameObject goO = new GameObject();
                            GameObject go = Instantiate(goO, gameMgr.transform);
                            go.name = "TutorialMgr";
                            tutorialMgr = go.AddComponent<TutorialMgr>();
                            tutorialMgr.tutorialAction = NextActionBase;
                            tutorialMgr.MainGameMgr = gameMgr;
                            TutorialMgr.TutorialCompleteReached += CurrentTutorialOver;
                            Destroy(goO);
                        }
                    }
                }
            }
        }
    }
}