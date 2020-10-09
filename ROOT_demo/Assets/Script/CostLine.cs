using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using Random = System.Random;

namespace ROOT
{
    public class CostLine : MonoBehaviour
    {
        public Transform HigherCostArrowRoot;
        public Transform LowerCostArrowRoot;

        public Transform LumpedLEDRoot;
        public GameObject CubeLEDTemplate;
        public Transform StartingLEDTrans;
        public Transform EndingLEDTrans;
        private int income;//这里有一个隐形的最大值，就是LumpedLED也比整个时间线长的时候，这个需要处理一下，就是到某个值后LumpedLED物理尺寸不再增长，现在这个最大值是200左右
        private float LEDscaleRatio=> BaseMaxIncome / (float)MaxIncome;
        //private float LEDLumpedscaleRatio=> BaseMaxIncome / (float)MaxIncome;
        private int _lumpedValUnit = 0;
        private const float LumpedValUnitLength = 0.6f;//实际的物理距离
        private float LumpedValUnitLengthRatio => LumpedValUnitLength/(Vector3.Distance(EndingLEDTrans.position, StartingLEDTrans.position));//实际的物理距离
        private int LumpedVal => _lumpedValUnit * BaseLumpIncome;
        private Vector3 startingLP => StartingLEDTrans.localPosition + new Vector3(_lumpedValUnit * LumpedValUnitLength, 0, 0);
        private Vector3 LumpLEDEnding => StartingLEDTrans.localPosition + new Vector3((_lumpedValUnit-1) * LumpedValUnitLength, 0, 0);
        private int ReminderIncome => income - LumpedVal;
        private int ReminderMaxIncome => MaxIncome - LumpedVal;
        private CubeLED LumpedLED = null;

        private float LumpedShrink
        {
            get
            {
                if (_lumpedValUnit==0)
                {
                    return 1.0f;
                }
                else
                {
                    return (1 - LumpedValUnitLengthRatio * _lumpedValUnit);
                }
            }
        }

        private int cost;
        private const int BaseMaxIncome = 10;
        private const int BaseLumpIncome = BaseMaxIncome;
        private int MaxIncome = BaseMaxIncome;
        private List<CubeLED> cubeLEDs = new List<CubeLED>();

        /// <summary>
        /// 根据具体Income更新MaxIncome，MaxIncome不会小于BaseMaxIncome
        /// </summary>
        private void UpdateMaxIncome()
        {
            if (income != 0)
            {
                var reminder = income % BaseMaxIncome;
                if (reminder == 0)
                {
                    MaxIncome = ((income / BaseMaxIncome)) * BaseMaxIncome;
                }
                else
                {
                    MaxIncome = ((income / BaseMaxIncome) + 1) * BaseMaxIncome;
                }
            }
            else
            {
                MaxIncome = BaseMaxIncome;
            }

            var tmpIncome = income;
            _lumpedValUnit = 0;
            while (tmpIncome> BaseMaxIncome)
            {
                tmpIncome -= BaseLumpIncome;
                _lumpedValUnit++;
            }
        }

        /// <summary>
        /// 专门处理LED数量的问题，LED点亮的问题不在这里处理。
        /// </summary>
        private void UpdateLEDArray()
        {
            var DesiredLength = ReminderMaxIncome;
            var maxIndex = Math.Max(DesiredLength, cubeLEDs.Count);
            for (var i = 0; i < maxIndex; i++)
            {
                if (i < cubeLEDs.Count)
                {
                    if (i >= DesiredLength)
                    {
                        Destroy(cubeLEDs[i].gameObject);
                        cubeLEDs[i] = null;
                        continue;
                    }
                }
                else
                {
                    var go = Instantiate(CubeLEDTemplate, transform);
                    var led = go.GetComponent<CubeLED>();
                    led.InitLED();
                    cubeLEDs.Add(led);
                }

                cubeLEDs[i].RepresentingVal = LumpedVal + (i + 1);
                cubeLEDs[i].RepText.gameObject.SetActive(false);
                if (i==0)
                {
                    cubeLEDs[i].RepText.gameObject.SetActive(true);
                }
                if (i == BaseMaxIncome-1)
                {
                    cubeLEDs[i].RepText.gameObject.SetActive(true);
                }

                cubeLEDs[i].LEDSize = LumpedShrink;
                cubeLEDs[i].transform.localPosition = Vector3.Lerp(
                    startingLP, 
                    EndingLEDTrans.localPosition,
                    i / (float) (DesiredLength - 1));
            }
            //这里需要缩短cubeLEDs Array的长度。
            cubeLEDs.RemoveAll(led => led == null);
        }

        /// <summary>
        /// 更新集总LED的内容。
        /// </summary>
        void updateLumpedLED()
        {
            if (LumpedVal > 0)
            {
                if (LumpedLED == null)
                {
                    var go = Instantiate(CubeLEDTemplate, LumpedLEDRoot);
                    LumpedLED = go.GetComponent<CubeLED>();
                    LumpedLED.TurnOn();
                }

                LumpedLED.InitLED(LumpedVal);
                var LumpedLEDX = (LumpLEDEnding.x + StartingLEDTrans.localPosition.x) / 2.0f;
                LumpedLED.transform.localPosition = new Vector3(LumpedLEDX, 0.0f, -0.13f);
                var LumpedLEDLength = LumpedValUnitLength * _lumpedValUnit;
                LumpedLEDLength /= (Vector3.Distance(StartingLEDTrans.position, EndingLEDTrans.position) / BaseMaxIncome);
                LumpedLED.LEDSize = LumpedLEDLength*1.4f;
                LumpedLED.RepresentingVal = LumpedVal;
            }
            else
            {
                if (LumpedLED != null)
                {
                    Destroy(LumpedLED.gameObject);
                    LumpedLED = null;
                }
            }
        }

        [HideInInspector]
        public int Income
        {
            set
            {
                income = value;
                UpdateMaxIncome();
                UpdateLEDArray();

                cubeLEDs.ForEach(led => led.TurnOff());
                for (var i = 0; i < cubeLEDs.Count; i++)
                {
                    if (i < ReminderIncome)
                    {
                        cubeLEDs[i].TurnOn();
                    }
                }

                updateLumpedLED();
            }
            get => income;
        }

        [HideInInspector]
        public int Cost
        {
            set
            {
                cost = value;
                HigherCostArrowRoot.gameObject.SetActive(false);
                LowerCostArrowRoot.gameObject.SetActive(false);
                if (cost >= LumpedVal && cost <= LumpedVal + BaseMaxIncome)
                {
                    cubeLEDs.ForEach(LED => LED.SetArrow = false);
                    cubeLEDs.Where(LED => LED.RepresentingVal == cost).ForEach(LED => LED.SetArrow = true);
                    if (LumpedLED != null)
                    {
                        LumpedLED.SetArrow = (cost == LumpedLED.RepresentingVal);
                    }
                }
                else
                {
                    if (cost> (LumpedVal+BaseMaxIncome/2.0f))
                    {
                        HigherCostArrowRoot.gameObject.SetActive(true);
                        HigherCostArrowRoot.GetComponentInChildren<TextMeshPro>().text = cost + "";
                    }
                    else
                    {
                        LowerCostArrowRoot.gameObject.SetActive(true);
                        LowerCostArrowRoot.GetComponentInChildren<TextMeshPro>().text = cost + "";
                    }
                }
            }
            get => cost;
        }


        void Awake()
        {
            HigherCostArrowRoot.gameObject.SetActive(false);
            LowerCostArrowRoot.gameObject.SetActive(false);
        }
    }
}