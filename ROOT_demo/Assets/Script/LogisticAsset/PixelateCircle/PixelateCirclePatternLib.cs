using System;
using System.Collections.Generic;
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