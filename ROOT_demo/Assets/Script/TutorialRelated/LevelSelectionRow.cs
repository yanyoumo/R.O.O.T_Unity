using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ROOT.UI
{
    public class LevelSelectionRow : MonoBehaviour
    {
        public LevelSelectionGrid SelectionGrid;
        public TextMeshProUGUI TitleTextTMP;

        public string TitleText
        {
            set
            {
                TitleTextTMP.text = value;
            }
        }
    }
}