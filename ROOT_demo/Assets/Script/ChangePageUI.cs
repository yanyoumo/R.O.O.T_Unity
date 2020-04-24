using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ROOT
{
    public class ChangePageUI : MonoBehaviour
    {
        public int CurrentPage { private set; get; }

        private int _maxPage;//这个是包含的，就是base1计数。

        private Button _nextButton;
        private Button _perviousButton;
        private TextMeshProUGUI _pageNumberText;


        private const string PageTextName = "PageCount";
        private const string InitPageTextContent = "00/00";
        private const string NextButtonName = "NextButton";
        private const string PerviousButtonName = "PerviousButton";

        public void InitChangePageUi(int maxPage, int currentPage = 1)
        {
            this._maxPage = maxPage;
            CurrentPage = currentPage;
            _pageNumberText.text = getDisplayText();

            _perviousButton.interactable = (CurrentPage != 1);
            _nextButton.interactable = (CurrentPage != _maxPage);
        }

        private string getDisplayText()
        {
            return Utils.PaddingNum2Digit(CurrentPage) + "/" + Utils.PaddingNum2Digit(_maxPage);
        }

        private void updateCurrentPageAndText()
        {
            CurrentPage = Mathf.Clamp(CurrentPage, 1, _maxPage);
            _pageNumberText.text = getDisplayText();
        }

        private void ToNextPage()
        {
            CurrentPage++;
            updateCurrentPageAndText();
            _nextButton.interactable = (CurrentPage != _maxPage);
        }

        private void ToPerviousPage()
        {
            CurrentPage--;
            updateCurrentPageAndText();
            _perviousButton.interactable = (CurrentPage != 1);
        }

        void Awake()
        {
            var tmpT = transform.gameObject.GetComponentsInChildren<TextMeshProUGUI>();
            var tmpB = transform.gameObject.GetComponentsInChildren<Button>();

            foreach (var text in tmpT)
            {
                if (text.name == PageTextName)
                {
                    _pageNumberText = text;
                }
            }

            foreach (var button in tmpB)
            {
                if (button.name == NextButtonName)
                {
                    _nextButton = button;
                }
                else if (button.name == PerviousButtonName)
                {
                    _perviousButton = button;
                }
            }

            Debug.Assert(_pageNumberText);
            Debug.Assert(_nextButton);
            Debug.Assert(_perviousButton);

            _pageNumberText.text = InitPageTextContent;
            _nextButton.onClick.AddListener(ToNextPage);
            _perviousButton.onClick.AddListener(ToPerviousPage);

            InitChangePageUi(1);
        }
    }
}