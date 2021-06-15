using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace ROOT.UI
{
    public class LevelFeatureIconArray_UI : MonoBehaviour
    {
        public Material DefaultMat;
        public Material BWMat;

        public Sprite[] IconSprites;
        public Image[] IconImages;

        public LevelFeature TestFeature;

        [Button]
        public void TestLevelFeature()
        {
            SetLevelFeature(TestFeature);
        }

        public void SetLevelFeature(LevelFeature _levelFeature)
        {
            for (var i = 0; i < 8; i++)
            {
                var currentFlag = 1 << i;
                var hasFlag = ((int) _levelFeature & currentFlag) == currentFlag;
                IconImages[i].material = hasFlag ? DefaultMat : BWMat;
            }
        }

        private void Awake()
        {
            Debug.Assert(IconSprites.Length == IconImages.Length, "Target Image and Sprites count not matching.");
            for (var i = 0; i < IconImages.Length; i++)
            {
                IconImages[i].sprite = IconSprites[i];
            }
        }
    }
}
