using System.Collections.Generic;
using System.Linq;
using ROOT.Consts;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT.Signal
{
    public class ClusterIsland : List<Vector2Int>//甚至这个代码可以不改，玩家其实对这个八方向还是四方向不敏感的。
    {
        private List<int> matrixUnitTierList;
        
        private int _connectingVal;
        
        private bool Vec2IntIsFourDirNeighbouring(Vector2Int A, Vector2Int B)
        {
            if (A == B)
            {
                Debug.LogWarning("Two vector2Int is same.");
                return false;
            }

            var dif = A - B;
            return StaticNumericData.V2Int4DirLib.Any(v => v == dif);
        }

        private Vector2 CenterPos
        {
            get
            {
                var centerAverageX = this.Sum(p => p.x) / (float) Count;
                var centerAverageY = this.Sum(p => p.y) / (float) Count;
                return new Vector2(centerAverageX, centerAverageY);
            }
        }

        private IEnumerable<Vector2Int> TotalSurroundingGrid(IEnumerable<Vector2Int> src)
        {
            var res = new List<Vector2Int>();
            foreach (var vector2Int in src)
            {
                foreach (var dir in StaticNumericData.V2Int4DirLib)
                {
                    var offsetV2 = vector2Int + dir;
                    if (!src.Contains(offsetV2) && !res.Contains(offsetV2))
                    {
                        res.Add(offsetV2);
                    }
                }
            }

            return res.Distinct();
        }

        private int GridTotalSurroundingCount(Vector2Int v, IEnumerable<Vector2Int> pool)
        {
            return StaticNumericData.V2Int4DirLib.Count(o => pool.Contains(v + o));
        }

        private int OrderByCenterPos_Discrete(Vector2Int v)
        {
            var dist_f = Vector2.Distance(v, CenterPos);
            return Mathf.RoundToInt(dist_f * 1000);//保留小数点后三位，量化所较数据。
        }

        private int OrderByPosID(Vector2Int v) => Board.BoardLength * v.y + v.x;
        
        private int TotalGridCount => _connectingVal;
        
        public List<Vector2Int> ClusterIslandInfoZone { get; private set; }
        
        private IEnumerable<Vector2Int> InitClusterIslandInfoZone()
        {
            var res = this.Where(v => true);
            var extraGridCount = TotalGridCount - Count;
            if (extraGridCount <= 0)
            {
                return res;
            }
            
            for (var i = 0; i < extraGridCount; i++)
            {
                //RISK 现有框架下程序是决定性的、但是从玩家角度看有一定随机性，这个有空看看。
                var pendingExtraGrid = TotalSurroundingGrid(res);
                var maxSurroundingCount = pendingExtraGrid.Max(v => GridTotalSurroundingCount(v, res));
                var maxSurroundingCountList = pendingExtraGrid.Where(v => GridTotalSurroundingCount(v, res) == maxSurroundingCount);
                var minGridDist = maxSurroundingCountList.Min(OrderByCenterPos_Discrete);
                var minGridDistList = maxSurroundingCountList.Where(v => OrderByCenterPos_Discrete(v) == minGridDist);
                var IDOrderedFinalist = minGridDistList.OrderBy(OrderByPosID);
                res = res.Append(IDOrderedFinalist.First());
            }

            return res;
        }

        public ClusterIsland(IEnumerable<Vector2Int> lv2, IEnumerable<int> unitTiers)
        {
            Debug.Assert(lv2.Count() == unitTiers.Count(),"position and tier count should be same!!");

            matrixUnitTierList = unitTiers.ToList();
            
            foreach (var pos in lv2)
            {
                Add(pos);
            }

            _connectingVal = 0;
            
            for (var i = 0; i < Count; i++)
            {
                //现在是根据Tier提供等倍数的数据
                _connectingVal +=
                    this.Where(v => v != this[i])
                        .Count(v => Vec2IntIsFourDirNeighbouring(v, this[i]))
                    * matrixUnitTierList[i];
            }

            _connectingVal /= 2;//等效为每个Tier提供0.5个倍数。

            ClusterIslandInfoZone = InitClusterIslandInfoZone().ToList();
        }

        public override string ToString()
        {
            var res = "";
            foreach (var vector2Int in this)
            {
                res += vector2Int + ",";
            }

            return res + "[" + _connectingVal + "]";
        }
    }
}