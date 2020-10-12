using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    [Serializable]
    [CreateAssetMenu(fileName = "NewGameModeAsset", menuName = "ActionAsset/New GameModeAsset")]
    public class GameModeAsset : ScriptableObject
    {
        [Range(0, 10000)]
        public int InitialCurrency = 1000;
        [Range(0, 100)]
        public int InitialTime = 30;

        public bool ShopCost = true;
        public bool UnitCost = true;
        public bool InfiniteTime = false;
    }
}