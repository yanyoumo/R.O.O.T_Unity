using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class CostLine : MonoBehaviour
    {
        public GameObject CubeLEDTemplate;
        public Transform StartingTEDTrans;
        public Transform EndingTEDTrans;

        public int Income;
        public int Cost;
        private int MaxIncome = 20;

        private List<CubeLED> cubeLEDs=new List<CubeLED>();

        void Awake()
        {
            for (var i = 0; i <= MaxIncome; i++)
            {
                var go = Instantiate(CubeLEDTemplate, transform);
                go.transform.localPosition = Vector3.Lerp(
                    StartingTEDTrans.localPosition, 
                    EndingTEDTrans.localPosition,
                    i / (float) MaxIncome);
                var led = go.GetComponent<CubeLED>();
                led.InitLED();
                led.On = false;
                cubeLEDs.Add(led);
            }
        }
    }
}