using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    [CreateAssetMenu(fileName = "NewHeatSinkPattern")]
    [Serializable]
    public class HeatSinkPattern : ScriptableObject
    {
        public List<Vector2Int> Lib;
        public int Count => Lib.Count;
    }
}