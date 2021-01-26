using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT
{
    using Direction = RotationDirection;
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
        private (SignalType, CoreGenre)?[] UnitList;
        //private CoreType?[] UnitList;
        //RISK 一个大坑，bool？即使是null也会被解释为false而不是null。
        public bool?[] ConnectionList { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_boardLength">每个Gist都可以有个不同的板长。</param>
        public BoardGist(int _boardLength)
        {
            BoardLength = _boardLength;
            var flag=VaildConnectionID(0);
            UnitList = new (SignalType, CoreGenre)?[BoardLength * BoardLength];
            UnitList.ForEach(tmp => tmp = null);
            ConnectionList = new bool?[2 * BoardLength * BoardLength];
            for (var i = 0; i < ConnectionList.Length; i++)
            {
                ConnectionList[i] = VaildConnectionID(i) ? (bool?) false : null;
            }
        }

        /// <summary>
        /// 设置某个位置上的CoreType
        /// </summary>
        /// <param name="pos">所需的位置</param>
        /// <param name="signal">所需信号的种类</param>
        /// <param name="genre">所需硬件的种类</param>
        public void SetCoreType(Vector2Int pos, SignalType signal,CoreGenre genre)
        {
            UnitList[PosToID(pos)] = (signal, genre);
        }

        /// <summary>
        /// 获得某个位置上的CoreType
        /// </summary>
        /// <param name="Pos">所需的位置</param>
        /// <returns>所查询的结果，如果没有Unit，则返回NULL</returns>
        public (SignalType, CoreGenre)? GetCoreType(Vector2Int Pos)
        {
            return UnitList[PosToID(Pos)];
        }

        /// <summary>
        /// 给予Unit位置和方向设置某个接口的联通性
        /// </summary>
        /// <param name="Pos">输入Unit位置</param>
        /// <param name="dir">输入方向</param>
        /// <param name="Connectivity">设置成的联通性</param>
        /// <returns>输入的位置和方向是否合法</returns>
        public bool SetConnectivity(Vector2Int Pos, Direction dir,bool Connectivity)
        {
            var (boardID, conID) = PosDirToID(Pos, dir);
            if (!VaildConnectionOfUnit(boardID, conID))
                return false;
            ConnectionList[conID] = Connectivity;
            return true;
        }

        /// <summary>
        /// 根据输入的Unit位置和方向读取某个接口的联通性
        /// </summary>
        /// <param name="Pos">特定Unit位置</param>
        /// <param name="dir">输入方向</param>
        /// <returns>返回的联通性结果，如果输入的位置和方向不合法则返回NULL</returns>
        public bool? GetConnectivity(Vector2Int Pos, Direction dir)
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

        private int GetConnectionID(int boardID, Direction desiredWorldDirection)
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