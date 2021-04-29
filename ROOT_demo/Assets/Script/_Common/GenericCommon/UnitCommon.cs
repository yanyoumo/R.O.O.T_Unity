using System;
using UnityEngine;

namespace ROOT
{
    //需要一个SignalType和CoreType之间的双相映射。
    //可能就是需要一个<CoreType,SignalType>的Dictionary，然后时常反着查？
    public enum SignalType
    {
        Matrix,
        Thermo,
        Scan,
        //Firewall,
    }

    public enum HardwareType
    {
        Core,
        Field,
        Support,
        Other
    }

    public enum SideType
    {
        NoConnection,
        Connection
    }

    public enum ConnectionMeshType
    {
        NoConnectionMesh,
        DtoDConnectedMesh,
        StDConnectedMesh,
        DtSConnectedMesh,
        StSConnectedMesh,
        NoChange
    }
    
    public enum PlayingSignalSelector
    {
        TypeA,
        TypeB,
    }

    public enum UnitActivationLEDColor
    {
        Deactivated=0,
        Dormant=1,
        Activated=2,
        HyperActivated=3,
    }
    
    public enum UnitTag
    {
        Red = 1 << 0,
        Orange = 1 << 1,
        Yellow = 1 << 2,
        Green = 1 << 3,
        Blue = 1 << 4,
        Cyan = 1 << 5,
        Purple = 1 << 6,
        Magenta = 1 << 7,
        Tan = 1 << 8,
        Brown = 1 << 9,
        White = 1 << 10,
        Black = 1 << 11,
        NoTag = 0,
    }

    [Serializable]
    public struct UnitGist
    {
        [Header("Basic")] public PlayingSignalSelector PlayingSignalSelector;
        public HardwareType CoreGenre;
        public SideType[] Sides;
        [Range(1, 5)] public int Tier;

        [Header("OnBoardInfo")] public Vector2Int Pos;
        public bool IsStation;
    }
}