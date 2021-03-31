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
        Connection,
        SIDETYPECOUNT
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