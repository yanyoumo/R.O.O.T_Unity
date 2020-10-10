using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    [CreateAssetMenu(fileName = "NewHeatSinkPatternLib", menuName = "HeatSink/New HeatSink PatternLib")]
    public class HeatSinkPatternLib : ScriptableObject
    {
        public List<HeatSinkPattern> Lib;
        public int Count => Lib.Count;


    }
}