using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    public class SkillCountLEDArray : LEDArray
    {
        private bool _inited=false;

        private readonly float offset = -0.1f;

        public GameObject SingleSkillCountTemplate;

        public void InitSkillCountArray(int MaxCount)
        {
            _LEDArray = new SingleLED[MaxCount];
            for (int i = 0; i < MaxCount; i++)
            {
                var obj = Instantiate(SingleSkillCountTemplate, transform);
                var script = obj.GetComponent<SkillCountSingleLED>();
                obj.transform.localPosition = Vector3.right * offset * i;
                script.TurnOnColor = LEDColor;
                _LEDArray[i] = script;
            }

            Val = MaxCount - 1;
        }
        
        public override int Val
        {
            set
            {
                for (var i1 = 0; i1 < _LEDArray.Length; i1++)
                {
                    if (i1 < value)
                    {
                        _LEDArray[i1].TurnOn();
                    }
                    else
                    {
                        _LEDArray[i1].TurnOff();
                    }
                }
            }
        }
    }
}