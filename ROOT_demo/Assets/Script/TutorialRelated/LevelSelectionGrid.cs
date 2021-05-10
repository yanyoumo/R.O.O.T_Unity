using UnityEngine;
using UnityEngine.UI;

namespace ROOT.UI
{
    //TODO
    public class LevelSelectionGrid : MonoBehaviour
    {
        [HideInInspector] public GameObject LevelQuadTemplate;

        private RectTransform[] TutorialQuadPosS;
        private LevelSelectionQuad[] TutorialQuadS;

        private int QuadCount = 20;

        public void SetSelectableLevels(int gameProgress)
        {
            foreach (var t in TutorialQuadS)
            {
                t.LevelSelectable = (t.LevelAccessID <= gameProgress);
            }
        }
        
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
            TutorialQuadS = new LevelSelectionQuad[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                TutorialQuadS[i] = Instantiate(LevelQuadTemplate, TutorialQuadPosS[i]).GetComponentInChildren<LevelSelectionQuad>();
                res[i] = TutorialQuadS[i].InitTutorialLevelSelectionQuad(data[i]);
            }

            return res;
        }
    }
}