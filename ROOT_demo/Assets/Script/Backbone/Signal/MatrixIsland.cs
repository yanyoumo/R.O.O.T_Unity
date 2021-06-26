using System.Collections.Generic;
using System.Linq;
using ROOT.Consts;
using UnityEngine;

namespace ROOT.Signal
{
    public class MatrixIsland : List<Vector2Int>
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
            return dif == Vector2Int.up || dif == Vector2Int.down || dif == Vector2Int.left || dif == Vector2Int.right;
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
            foreach (var vector2 in src.Where(v => GridTotalSurroundingCount(v, src) != 4))//RISK 这里AsParallel不好使，但是需要具体去看下原因。
            {
                foreach (var dir in StaticNumericData.V2Int4DirLib)
                {
                    var offsetV2 = vector2 + dir;
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
            return pool.Count(v0 => Vec2IntIsFourDirNeighbouring(v, v0));
        }

        private int OrderByCenterPos_Discrete(Vector2Int v)
        {
            var dist_f = Vector2.Distance(v, CenterPos);
            return Mathf.RoundToInt(dist_f * 1000);//保留小数点后三位，量化所较数据。
        }

        private int OrderByPosID(Vector2Int v) => Board.BoardLength * v.y + v.x;
        
        private int TotalGridCount => _connectingVal;
        
        public IEnumerable<Vector2Int> GetMatrixIslandInfoZone()
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

        public MatrixIsland(IEnumerable<Vector2Int> lv2, IEnumerable<int> unitTiers)
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