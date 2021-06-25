using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ROOT.Signal
{
    public class MatrixIsland : List<Vector2Int>
    {
        private List<int> matrixUnitTierList;
        
        private int _connectingVal;
        
        private readonly Vector2Int[] NebrList = {Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right};

        private bool Vec2IntIsFourDirNeighbouring(Vector2Int A, Vector2 B)
        {
            if (A == B)
            {
                Debug.LogWarning("Two vector2Int is same.");
                return false;
            }

            var dif = A - B;
            return dif == Vector2.up || dif == Vector2.down || dif == Vector2.left || dif == Vector2.right;
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

        private List<Vector2Int> TotalSurroundingGrid(IEnumerable<Vector2Int> src)
        {
            var res = new List<Vector2Int>();
            foreach (var vector2 in src)
            {
                foreach (var dir in NebrList)
                {
                    var offsetV2 = vector2 + dir;
                    if (!src.Contains(offsetV2) && !res.Contains(offsetV2))
                    {
                        res.Add(offsetV2);
                    }
                }
            }
            
            return res.Distinct().ToList();
        }

        private int GridTotalSurroundingCount(Vector2Int v, IEnumerable<Vector2Int> pool)
        {
            return pool.Count(v0 => Vec2IntIsFourDirNeighbouring(v, v0));
        }

        private float OrderByCenterPos(Vector2Int v) => Vector2.Distance(v, CenterPos);

        private int TotalGridCount => _connectingVal;
        
        public List<Vector2Int> GetMatrixIslandInfoZone()
        {
            var res = this.Select(p => p).ToList();
            var extraGridCount = TotalGridCount - Count;
            if (extraGridCount <= 0)
            {
                return res;
            }

            for (var i = 0; i < extraGridCount; i++)
            {
                //RISK 这个过程有一定的"随机性"，可能不是特别好。
                var pendingExtraGrid = TotalSurroundingGrid(res);
                var maxSurroundingCount = pendingExtraGrid.Max(v => GridTotalSurroundingCount(v, res));
                var maxSurroundingCountList = pendingExtraGrid.Where(v => GridTotalSurroundingCount(v, res) == maxSurroundingCount);
                var nextGridListOrdered = maxSurroundingCountList.OrderBy(OrderByCenterPos).ToList();
                res.Add(nextGridListOrdered[0]);
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