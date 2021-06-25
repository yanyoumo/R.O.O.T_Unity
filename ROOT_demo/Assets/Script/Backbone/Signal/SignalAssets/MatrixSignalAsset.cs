using System;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Messages;
using UnityEngine;

namespace ROOT.Signal
{
    public class MatrixSignalAsset : SignalAssetBase
    {
        public override Type UnitSignalCoreType => typeof(MatrixUnitSignalCore);
        public override SignalType SignalType => SignalType.Matrix;

        public static List<MatrixIsland> MatrixIslandPack => _matrixIslandPack;
        private static List<MatrixIsland> _matrixIslandPack;

        private readonly Vector2Int[] NebrList = {Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right};

        private List<Vector2Int> FindSingleIsland_Iter(Vector2Int crt)
        {
            var res = new List<Vector2Int>();
            Debug.Assert(MatrixIslandMap.ContainsKey(crt));
            MatrixIslandMap[crt] = true;
            res.Add(crt);
            foreach (var vector2Int in NebrList)
            {
                var ccrt = crt + vector2Int;
                if (MatrixIslandMap.ContainsKey(ccrt) && !MatrixIslandMap[ccrt])
                {
                    var localRes = FindSingleIsland_Iter(ccrt);
                    if (localRes.Count>0)
                    {
                        res.AddRange(localRes.Select(p => p).ToList());
                    }
                }
            }
            return res;
        }

        private Dictionary<Vector2Int, bool> MatrixIslandMap;
        
        private void updateMatrixIsland(IEnumerable<Unit> units)
        {
            MatrixIslandMap = new Dictionary<Vector2Int, bool>();
            _matrixIslandPack = new List<MatrixIsland>();
            foreach (var pos in units.Select(u=>u.CurrentBoardPosition))
            {
                MatrixIslandMap.Add(pos, false);
            }

            do
            {
                var currentV2I = MatrixIslandMap.First(t => !t.Value).Key;
                var singleIsland = FindSingleIsland_Iter(currentV2I);
                var unitTierParallel = singleIsland.Select(p => units.First(u => u.CurrentBoardPosition == p).Tier);
                _matrixIslandPack.Add(new MatrixIsland(singleIsland, unitTierParallel));
            } while (MatrixIslandMap.Any(t => !t.Value));
        }

        private void BoardDataUpdatedHandler(IMessage rMessage)
        {            
            var data = SignalMasterMgr.Instance.GetActiveUnitByUnitType(SignalType, HardwareType.Field).ToArray();
            if (data.Length == 0) return;
            updateMatrixIsland(data);
        }
        
        protected virtual void Awake()
        {
            MessageDispatcher.AddListener(WorldEvent.BoardSignalUpdatedEvent, BoardDataUpdatedHandler);
        }

        private void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.BoardSignalUpdatedEvent, BoardDataUpdatedHandler);
        }
    }
}