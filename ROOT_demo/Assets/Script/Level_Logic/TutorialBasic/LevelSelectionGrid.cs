using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ROOT.UI
{
    //TODO
    public class LevelSelectionGrid : MonoBehaviour
    {
        [HideInInspector] public GameObject LevelQuadTemplate;

        private RectTransform[] TutorialQuadPosS;
        private TutorialLevelSelectionQuad[] TutorialQuadS;

        private int QuadCount = 20;

        void Awake()
        {
            TutorialQuadPosS = new RectTransform[QuadCount];
            for (int i = 0; i < QuadCount; i++)
            {
                var GO = new GameObject("TutorialUI" + (i + 1), typeof(RectTransform));
                GO.transform.SetParent(transform);
                GO.transform.localScale = Vector3.one;
                TutorialQuadPosS[i] = GO.GetComponent<RectTransform>();
            }
        }

        public Button[] InitTutorialLevelSelectionMainMenu(TutorialQuadDataPack[] data)
        {
            Button[] res = new Button[data.Length];
            TutorialQuadS = new TutorialLevelSelectionQuad[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                TutorialQuadS[i]=Instantiate(LevelQuadTemplate, TutorialQuadPosS[i]).GetComponentInChildren<TutorialLevelSelectionQuad>();
                res[i] = TutorialQuadS[i].InitTutorialLevelSelectionQuad(data[i]);
            }
            return res;
        }
    }
}