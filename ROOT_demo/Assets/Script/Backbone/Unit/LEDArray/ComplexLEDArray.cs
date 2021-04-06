using Sirenix.Utilities;
using UnityEngine;

namespace ROOT
{
    public class ComplexLEDArray : LEDArray
    {
        private const float baseStarting = -3.85f;
        private const float ledWidth = 0.5f;
        private const float ledSpacing = 0.3f;
        
        public GameObject CubeLEDTemplate;
        public Transform TemplateCube;
        public int val = -1;

        private const int BaseMaxIncome = 10;
        private int BaseMaxStep = 10;
        //private const int BaseLumpIncome = BaseMaxIncome;
        private int MaxIncome = BaseMaxIncome;
        private int OldMaxIncome = -1;
        //private int _lumpedValUnit = 0;
        //private int _lumpedValLengthUnit => _lumpedValUnit + 1;
        //private int LumpedVal => _lumpedValUnit * BaseLumpIncome;

        public GameObject LumpedLED;

        private void InitLEDArray()
        {
            LumpedLED.GetComponent<SimplerCubeLED>().TurnOnColor = LEDColor;
            _LEDArray = new SingleLED[BaseMaxIncome];
            for (int i = 0; i < BaseMaxIncome; i++)
            {
                var GO = Instantiate(CubeLEDTemplate, transform);
                GO.transform.localPosition = new Vector3(baseStarting + i * (ledWidth + ledSpacing)+ ledWidth/2.0f, 0.0f, 0.0f);
                //GO.GetComponent<SimplerCubeLED>().InitLED();
                _LEDArray[i] = GO.GetComponent<SimplerCubeLED>();
                _LEDArray[i].TurnOff();
                _LEDArray[i].TurnOnColor = LEDColor;
            }
        }

        private void UpdateMaxVal()
        {
            if (val<=BaseMaxIncome)
            {
                MaxIncome = BaseMaxIncome;
            }
            else
            {
                var reminder = val - BaseMaxIncome;
                var reminderUnit = Mathf.CeilToInt(reminder / (float)BaseMaxStep);
                MaxIncome = BaseMaxIncome + reminderUnit * BaseMaxStep;
            }
        }

        private void CleanSingalLED(SingleLED LED)
        {
            Destroy(LED.gameObject);
            LED = null;
        }

        private void CleanLED()
        {
            _LEDArray.ForEach(CleanSingalLED);
        }

        private void UpdateLEDArray()
        {
            CleanLED();
            _LEDArray = new SingleLED[MaxIncome];
            float currentLEDWidth = (7.7f)/((8.0f/5.0f)*MaxIncome-(3.0f/5.0f));
            float currentLEDSpacing = (3.0f / 5.0f) * currentLEDWidth;
            float relativeLEDScale = currentLEDWidth / ledWidth;

            for (int i = 0; i < MaxIncome; i++)
            {
                var GO = Instantiate(CubeLEDTemplate, transform);
                GO.transform.localPosition = new Vector3(baseStarting + i * (currentLEDWidth + currentLEDSpacing) + currentLEDWidth / 2.0f, 0.0f, 0.0f);
                _LEDArray[i] = GO.GetComponent<SimplerCubeLED>();
                _LEDArray[i].TurnOff();
                _LEDArray[i].TurnOnColor = LEDColor;
                _LEDArray[i].transform.localScale = new Vector3(relativeLEDScale, 1.0f, 1.0f);
            }
        }

        /*private void UpdateLEDArray()
        {
            if (_lumpedValUnit > 0)
            {
                LumpedLED.gameObject.SetActive(true);
                float currentLEDWidth = (7.7f) / ((77.0f / 5.0f) + (8.0f / 5.0f) * _lumpedValLengthUnit);
                float currentLEDSpacing = (3.0f / 5.0f) * currentLEDWidth;
                float currentSP = baseStarting + _lumpedValLengthUnit * (currentLEDWidth + currentLEDSpacing);
                float relativeLEDScale = currentLEDWidth / ledWidth;
                float LumpedLEDWidth = _lumpedValLengthUnit * currentLEDWidth+(_lumpedValLengthUnit - 1)* currentLEDSpacing;
                float LumpedLEDScale = LumpedLEDWidth / ledWidth;
                float LumpedLEDPos = baseStarting + LumpedLEDWidth / 2.0f;
                for (var i = 0; i < _LEDArray.Length; i++)
                {
                    _LEDArray[i].transform.localPosition = new Vector3(currentSP + i * (currentLEDWidth + currentLEDSpacing) + currentLEDWidth / 2.0f, 0.0f, 0.0f);
                    _LEDArray[i].transform.localScale = new Vector3(relativeLEDScale, 1.0f, 1.0f);
                }
                LumpedLED.transform.localPosition = new Vector3(LumpedLEDPos, 0.0f, 0.0f);
                LumpedLED.GetComponent<SimplerCubeLED>().LEDSize = LumpedLEDScale;
                //LumpedLED.GetComponent<SimplerCubeLED>().InitLED(LumpedVal);
                LumpedLED.GetComponent<SimplerCubeLED>().InitLED(-1);
                LumpedLED.GetComponent<SimplerCubeLED>().TurnOn();
            }
            else
            {
                LumpedLED.gameObject.SetActive(false);
                for (var i = 0; i < _LEDArray.Length; i++)
                {
                    _LEDArray[i].transform.localPosition = new Vector3(baseStarting + i * (ledWidth + ledSpacing) + ledWidth / 2.0f, 0.0f, 0.0f);
                    _LEDArray[i].transform.localScale = Vector3.one;
                }
            }
        }*/

        public override int Val
        {
            set
            {
                if (val != value)
                {
                    val = value;

                    UpdateMaxVal();
                    if (OldMaxIncome != MaxIncome)
                    {
                        UpdateLEDArray();
                    }
                    OldMaxIncome = MaxIncome;
                    for (var i = 0; i < _LEDArray.Length; i++)
                    {
                        if (i < val)
                        {
                            _LEDArray[i].TurnOn();
                        }
                        else
                        {
                            _LEDArray[i].TurnOff();
                        }
                    }
                }
            }
        }

        public void OnEnable()
        {
            //TemplateCube.gameObject.SetActive(false);
        }

        public void Awake()
        {
            InitLEDArray();
            Val = 0;
            Destroy(TemplateCube.gameObject);
        }
        public void Update()
        {
            /*if (Time.frameCount%30==0)
            {
                Val = Random.Range(0, 50);
            }*/

            //Val = (Time.frameCount / 30);
        }
    }
}