using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT
{
    using Direction = RotationDirection;

    public partial class BoardGistTestScript
    {
    }
    
    /// <summary>
    /// 这段是一部分试图重构的数据结构，其目的是将棋盘上全部可能的链接编号并管理。
    /// 但是在再次讨论后，发现毫无意义，于是就此弃之。
    ///
    /// 核心思路是从棋盘左下角开始将每个格点编号，并且将所有可能的格点之间的链接进行编号。
    /// 编号逻辑是：
    /// 单元南侧的链接编号是单元ID的二倍。（2n）
    /// 单元东侧的链接编号是单元ID的二倍加一。(2n+1)
    ///
    /// 所有单元不去管理自己北侧和西侧的接口。（防止重复计算）
    /// 这样的问题就是棋盘上Connection的链接是不连续的。
    /// </summary>
    public class BoardGist
    {
        private readonly int BoardLength = 6;
        private CoreType?[] UnitList;
        //RISK 一个大坑，bool？即使是null也会被解释为false而不是null。
        public bool?[] ConnectionList { get; }

        public BoardGist(int _boardLength)
        {
            BoardLength = _boardLength;
            var flag=VaildConnectionID(0);
            UnitList = new CoreType?[BoardLength * BoardLength];
            UnitList.ForEach(tmp => tmp = null);
            ConnectionList = new bool?[2 * BoardLength * BoardLength];
            for (var i = 0; i < ConnectionList.Length; i++)
            {
                ConnectionList[i] = VaildConnectionID(i) ? (bool?) false : null;
            }
        }
        
        public void SetCoreType(Vector2Int Pos, CoreType Core)
        {
            UnitList[PosToID(Pos)] = Core;
        }

        public CoreType? GetCoreType(Vector2Int Pos)
        {
            return UnitList[PosToID(Pos)];
        }

        public bool SetConnectivity(Vector2Int Pos, Direction dir,bool Connectivity)
        {
            var (boardID, conID) = PosDirToID(Pos, dir);
            if (!VaildConnectionOfUnit(boardID, conID))
                return false;
            ConnectionList[conID] = Connectivity;
            return true;
        }

        public bool? CheckConnectivity(Vector2Int Pos, Direction dir)
        {
            var (boardID, conID) = PosDirToID(Pos, dir);
            return VaildConnectionOfUnit(boardID,conID) ? ConnectionList[conID] : null;
        }

        private int PosToID(Vector2Int Pos)
        {
            return Pos.x + Pos.y * BoardLength;
        }

        private (int, int) PosDirToID(Vector2Int Pos, Direction dir)
        {
            var BoardID = PosToID(Pos);
            var conID = GetConnectionID(BoardID, dir);
            return (BoardID, conID);
        }
        
        private bool VaildConnectionOfUnit(int boardID,int conID)
        {
            //[0,0]位置的West方向算出来conID是-1，这个需要排除一下。所以最后有个大于0的判断。
            return VaildConnectionID(conID) && conID < ConnectionList.Length && conID >= 0;
        }


        private bool VaildConnectionID(int ID)
        {
            if (ID % 2 == 0)
            {
                //EVEN Number
                return ID / 2 >= BoardLength;
            }
            else
            {
                //ODD Number
                var uID = (ID - 1) / 2;
                return uID % BoardLength != BoardLength - 1;
            }
        }

        public int GetConnectionID(int boardID, Direction desiredWorldDirection)
        {
            switch (desiredWorldDirection)
            {
                case Direction.North:
                    return 2 * (boardID + BoardLength);
                case Direction.West:
                    return 2 * (boardID - 1) + 1;
                case Direction.East:
                    return 2 * boardID + 1;
                case Direction.South:
                    return 2 * boardID;
                default:
                    throw new ArgumentOutOfRangeException(nameof(desiredWorldDirection), desiredWorldDirection, null);
            }
        }
    }
}