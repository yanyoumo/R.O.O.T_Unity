using System;
using System.Collections;
using System.Collections.Generic;
using ROOT;
using UnityEngine;

namespace ROOT
{
    [CreateAssetMenu(fileName = "NewPixelateCirclePatternLib", menuName = "PixelateCircle/New PixelateCircle PatternLib")]
    [Serializable]
    public class PixelateCirclePatternLib : ScriptableObject
    {
        public List<PixelateCirclePattern> Lib;
    }
}