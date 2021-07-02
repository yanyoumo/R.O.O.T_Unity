using System;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Messages;
using ROOT.Consts;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT.Signal
{
    public class ClusterSignalAsset : SignalAssetBase
    {
        public override Type UnitSignalCoreType => typeof(ClusterUnitSignalCore);
        public override SignalType SignalType => SignalType.Cluster;

        public static IEnumerable<ClusterIsland> ClusterIslandPack => _clusterIslandPack;
        private static List<ClusterIsland> _clusterIslandPack;
        
        private List<Vector2Int> FindSingleIsland_Iter(Vector2Int crt)
        {
            var res = new List<Vector2Int>();
            Debug.Assert(ClusterIslandMap.ContainsKey(crt));
            ClusterIslandMap[crt] = true;
            res.Add(crt);
            foreach (var vector2Int in StaticNumericData.V2Int4DirLib)
            {
                var ccrt = crt + vector2Int;
                if (ClusterIslandMap.ContainsKey(ccrt) && !ClusterIslandMap[ccrt])
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

        private Dictionary<Vector2Int, bool> ClusterIslandMap;
        
        private void updateClusterIsland(IEnumerable<Unit> units)
        {
            ClusterIslandMap = new Dictionary<Vector2Int, bool>();
            _clusterIslandPack = new List<ClusterIsland>();
            units.Select(u => u.CurrentBoardPosition).ForEach(p => ClusterIslandMap.Add(p, false));

            do
            {
                var currentV2I = ClusterIslandMap.First(t => !t.Value).Key;
                var singleIsland = FindSingleIsland_Iter(currentV2I);
                var unitTierParallel = singleIsland.Select(p => units.First(u => u.CurrentBoardPosition == p).Tier);
                _clusterIslandPack.Add(new ClusterIsland(singleIsland, unitTierParallel));
            } while (ClusterIslandMap.Any(t => !t.Value));
        }

        private void BoardDataUpdatedHandler(IMessage rMessage)
        {            
            var data = SignalMasterMgr.Instance.GetActiveUnitByUnitType(SignalType, HardwareType.Field).ToArray();
            if (data.Length == 0) return;
            updateClusterIsland(data);
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