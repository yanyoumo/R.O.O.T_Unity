using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ROOT.UI
{
    [ExecuteInEditMode]
    public class LoadingText : MonoBehaviour
    {
        public TextMeshProUGUI textGUI;
        private int counter = 0;
        private const int maxCount = 5;

        private int timerCounter = 0;
        private int timerCounterMax = 10;

        void Update()
        {
            timerCounter++;
            if (timerCounter >= timerCounterMax)
            {
                timerCounter = 0;
                var dots = "";
                for (int i = 0; i < counter; i++)
                {
                    dots += ".";
                }

                counter++;
                counter %= maxCount;
                textGUI.text = "Loading" + dots;
            }
        }
    }
}