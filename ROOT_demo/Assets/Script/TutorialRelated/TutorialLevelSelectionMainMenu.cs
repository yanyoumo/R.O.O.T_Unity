﻿using ROOT.Consts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ROOT
{
    public class TutorialLevelSelectionMainMenu : MonoBehaviour
    {
        private Vector2Int posZero = new Vector2Int(210, -295);
        private int displaceX = 200;
        private int displaceY = -320;
        private int lineCount = 9;
        public GameObject TutorialQuadTemplate;
        public RectTransform TutorialQuadRoot;

        private RectTransform[] TutorialQuadPosS;
        private LevelSelectionQuad[] TutorialQuadS;

        private int QuadCount = 18;

        void Awake()
        {
            TutorialQuadPosS = new RectTransform[QuadCount];
            for (int i = 0; i < QuadCount; i++)
            {
                var GO = new GameObject("TutorialUI" + (i + 1), typeof(RectTransform));
                GO.transform.SetParent(TutorialQuadRoot);
                GO.transform.localPosition = new Vector3(posZero.x + i%lineCount * displaceX, posZero.y + (i / lineCount) * displaceY, 0);
                GO.transform.localScale = Vector3.one;
                GO.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                GO.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                TutorialQuadPosS[i] = GO.GetComponent<RectTransform>();
            }
        }

        /*public Button[] InitTutorialLevelSelectionMainMenu(LevelQuadDataPack[] data)
        {
            //Debug.Assert(data.Length < 19);
            Button[] res = new Button[data.Length];
            TutorialQuadS = new LevelSelectionQuad[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                TutorialQuadS[i]=Instantiate(TutorialQuadTemplate, TutorialQuadPosS[i]).GetComponentInChildren<LevelSelectionQuad>();
                res[i] = TutorialQuadS[i].InitLevelSelectionQuad(data[i]);
            }
            return res;
        }*/

        public void BackToMenu()
        {
            SceneManager.LoadSceneAsync(StaticName.SCENE_ID_START, LoadSceneMode.Single);
        }
    }
}