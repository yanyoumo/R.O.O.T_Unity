using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ROOT.UI
{
    public class LevelSelectionRow : MonoBehaviour
    {
        public LevelSelectionGrid SelectionGrid;
        public TextMeshProUGUI TitleTextTMP;
        public LayoutElement ElementProxy;
        public RectTransform RectTransform;

        public string TitleText
        {
            set
            {
                TitleTextTMP.text = value;
            }
        }

        private void UpdateMinSize()
        {
            var rect = RectTransform.rect;
            ElementProxy.minHeight = rect.height;
            ElementProxy.minWidth = rect.width;
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateMinSize();
        }
    }
}