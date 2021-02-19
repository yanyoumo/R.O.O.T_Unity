using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public enum LevelType
    {
        Tutorial,
        Career,
        //Classic
    }

    public enum TutorialActionType
    {
        //这个的顺序不能变！
        Text,
        CreateUnit,
        CreateCursor,
        ShowText,
        HideText,
        End,
        ShowCheckList,
        HideCheckList,
    }

    public enum StageType{
        Shop,
        Require,
        Destoryer,
        Telemetry,
        Acquiring,
        Ending,
    }
    
    public enum RoundType
    {
        Normal,
        Boss,//Telemetry...So on.
    }
}
