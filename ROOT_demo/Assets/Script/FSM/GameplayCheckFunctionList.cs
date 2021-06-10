using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public static class GameplayCheckFunctionList
    {
        public static bool GameplayCheck0(FSMLevelLogic fsm, Board board)
        {
            return fsm.LevelAsset.DeltaCurrency >= 15;
        }

        public static bool GameplayCheck1(FSMLevelLogic fsm, Board board)
        {
            return fsm.LevelAsset.DeltaCurrency >= 25;
        }

        public static bool GameplayCheck2(FSMLevelLogic fsm, Board board)
        {
            return fsm.LevelAsset.DeltaCurrency >= 14;
        }

        public static bool GameplayCheck3(FSMLevelLogic fsm, Board board)
        {
            return fsm.LevelAsset.DeltaCurrency >= 21;
        }

        public static bool GameplayCheck4(FSMLevelLogic fsm, Board board)
        {
            throw new NotImplementedException();
        }
    }
}