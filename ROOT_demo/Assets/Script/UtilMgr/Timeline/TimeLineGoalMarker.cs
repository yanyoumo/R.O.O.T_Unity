using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ROOT
{
    public class TimeLineGoalMarker : MonoBehaviour
    {
        public MeshRenderer CheckQuad;
        public Material tick;
        public Material cross;

        public TextMeshPro CurrentLabel;
        public TextMeshPro TargetLabel;


        private int _targetCount;
        public int TargetCount
        {
            set
            {
                _targetCount = value;
                //TargetLabel.text = PadCount(_targetCount);
                UpdateCheck();
            }
            get => _targetCount;
        }

        private int _currentCount;
        public int CurrentCount
        {
            set
            {
                _currentCount = value;
                //CurrentLabel.text = PadCount(_currentCount);
                UpdateCheck();
            }
            get => _currentCount;
        }

        public void UpdateCheck()
        {
            //CheckQuad.material = _currentCount >= _targetCount ? tick : cross;
        }

        public string PadCount(int count)
        {
            if (count < 10)
            {
                return "0" + count;
            }
            else
            {
                return count.ToString();
            }
        }
    }
}