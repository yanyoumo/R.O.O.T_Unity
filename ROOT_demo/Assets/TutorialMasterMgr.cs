using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    //TODO 这货要变成由GameMasterManager直接管理的一环。
    public partial class TutorialMasterMgr : MonoBehaviour
    {
        public GameObject TutorialCanvas;

        private ScoreSet NextGameScoreSet = new ScoreSet(1000.0f, 3);
        private PerMoveData NextGamePerMoveData = new PerMoveData();

        private GameMasterManager gameMasterManager;
        /*public static event RootEVENT.GameMajorEvent RequestTutorialStart;
        public static event RootEVENT.GameStartEvent RequestTutorialStartWParam;
        public static event RootEVENT.TutorialStartEvent TutorialStart;*/
        private bool gameMasterManagerReady = false;
        TutorialQuadDataPack[] dataS;

        /*public void GameStarted()
        {
            DefaultLevelMgr.GameStarted -= GameStarted;
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_TUTORIAL);//干脆自己Unload掉，到时候再Reload也方便。
        }

        void Awake()
        {
            InitTutorialActions();
            DefaultLevelMgr.GameStarted += GameStarted;
        }

        void Start()
        {
            dataS = new TutorialQuadDataPack[tutorialActions.Length];
            for (var i = 0; i < tutorialActions.Length; i++)
            {
                dataS[i] = tutorialActions[i].GetTutorialQuadDataPack;
            }

            var buttons = TutorialCanvas.GetComponentInChildren<TutorialLevelSelectionMainMenu>()
                .InitTutorialLevelSelectionMainMenu(dataS);

            for (var i = 0; i < buttons.Length; i++)
            {
                var buttonId = i;
                buttons[i].onClick.AddListener(() => { ButtonsListener(buttonId); });
            }
        }

        void Update()
        {
            if (gameMasterManager != null)
            {
                if (!gameMasterManagerReady)
                {
                    gameMasterManagerReady = true;
                    RequestTutorialStart += gameMasterManager.LoadGamePlay_Full;
                    RequestTutorialStartWParam += gameMasterManager.LoadGamePlay_Full_Param;
                    TutorialStart += gameMasterManager.LoadTutorial_Full;
                }
            }
            else
            {
                gameMasterManager = FindObjectOfType<GameMasterManager>();
            }
        }

        public void StartTutorial()
        {
            TutorialCanvas.gameObject.SetActive(false);
            //现在终于可以通过Tutorial程控的控制游戏的运行逻辑了。
            RequestTutorialStartWParam?.Invoke(NextGameScoreSet, NextGamePerMoveData, typeof(InfiniteGameStateMgr));
        }

        public void ButtonsListener(int buttonId)
        {
            TutorialStart?.Invoke(tutorialActions[buttonId]);
        }*/
    }
}