using System;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT.SetupAsset
{
    [Serializable]
    public class AdditionalGameSetup
    {
        //这个就稍微有些蠢、这个类需要能静态指定一个默认值、但是struct搞不了这件事儿；就只能用class……
        public SignalType PlayingSignalTypeA;
        public SignalType PlayingSignalTypeB;
        public int[] AdditionalGameplayHintPages;

        public bool IsPlayingCertainSignal(SignalType signal)
        {
            return PlayingSignalTypeA == signal || PlayingSignalTypeB == signal;
        }

        public void OrderingSignal()
        {
            if (PlayingSignalTypeA > PlayingSignalTypeB)
            {
                var tmp = PlayingSignalTypeB;
                PlayingSignalTypeB = PlayingSignalTypeA;
                PlayingSignalTypeA = tmp;
            }
        }
    }
}