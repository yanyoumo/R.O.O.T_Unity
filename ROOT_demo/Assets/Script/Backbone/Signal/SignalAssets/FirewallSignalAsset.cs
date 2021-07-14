using System;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Messages;
using ROOT.SetupAsset;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT.Signal
{
    using FirewallCircle = List<Vector2Int>;
    using FirewallInner = List<Vector2Int>;

    public class FirewallSignalAsset : SignalAssetBase
    {
        private static FirewallCircle _firewallCircle;
        private static FirewallInner _firewallInnerZone;
        public static FirewallCircle CurrentFirewallCircle => _firewallCircle;
        public static FirewallInner CurrentFirewallInner => _firewallInnerZone;
        public override Type UnitSignalCoreType => typeof(FirewallUnitSignalCore);
        public override SignalType SignalType => SignalType.Firewall;

        private static int N => Board.BoardLength;
        private int[,] _board = new int[N, N];
        /*
         * white space: 0 -> -1
         * unit:        1 ->  2
         */
        private List<FirewallCircle> _connectComponent;
        private static int[] dx4 = { -1, 0, 1, 0 };
        private static int[] dy4 = { 0, 1, 0, -1 };
        private static int[] dx8 = { -1, -1, 0, 1, 1, 1, 0, -1 };
        private static int[] dy8 = { 0, 1, 1, 1, 0, -1, -1, -1 };
        private Dictionary<Vector2Int, List<Vector2Int>> edge;
        private Dictionary<Vector2Int, int> low;
        private HashSet<Vector2Int> cutVertexSet;
        private Dictionary<Vector2Int, int> dfn;
        private int dfsClock;

        private FirewallCircle GetConnectComponent(int x, int y)
        {
            var res = new FirewallCircle();
            var queue = new Queue<Vector2Int>();
            res.Add(new Vector2Int(x, y));
            queue.Enqueue(new Vector2Int(x, y));
            _board[x, y] = 2;
            while (queue.Count != 0)
            {
                var now = queue.Dequeue();
                for (var i = 0; i < 4; ++i)
                {
                    int xx = now.x + dx4[i], yy = now.y + dy4[i];
                    //-1 white; 1 black; 2 visited
                    if (xx < 0 || xx >= N || yy < 0 || yy >= N || _board[xx, yy] == -1 || _board[xx, yy] == 2)
                        continue;
                    _board[xx, yy] = 2;
                    res.Add(new Vector2Int(xx, yy));
                    queue.Enqueue(new Vector2Int(xx, yy));
                }
            }
            return DeleteNonCircle(res);
        }

        private FirewallCircle DeleteNonCircle(FirewallCircle now)
        {
            if (now.Count == 1)
            {
                _board[now[0].x, now[0].y] = -1;
                return new FirewallCircle();
            }
            edge = new Dictionary<Vector2Int, List<Vector2Int>>();
            var inDegree = new Dictionary<Vector2Int, int>();
            foreach (var point in now)
            {
                edge[point] = new List<Vector2Int>();
                inDegree[point] = 0;
                for (var i = 0; i < 8; ++i)
                {
                    int xx = point.x + dx8[i], yy = point.y + dy8[i];
                    if (xx < 0 || xx >= N || yy < 0 || yy >= N || _board[xx, yy] != 2)
                        continue;
                    ++inDegree[point];
                    edge[point].Add(new Vector2Int(xx, yy));
                }
            }

            var queue = new Queue<Vector2Int>();
            foreach (var tmp in inDegree.Where(tmp => tmp.Value == 1))
                queue.Enqueue(tmp.Key);
            while (queue.Count != 0)
            {
                var degreeIsOne = queue.Dequeue();
                _board[degreeIsOne.x, degreeIsOne.y] = -1;
                if (inDegree[degreeIsOne] != 1)
                    continue;
                foreach (var tmp in edge[degreeIsOne])
                {
                    --inDegree[tmp];
                    if (inDegree[tmp] == 1)
                        queue.Enqueue(tmp);
                }
            }

            return now.Where(point => _board[point.x, point.y] == 2).ToList();
        }
        private void bfs(int x, int y)
        {
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(new Vector2Int(x, y));
            _board[x, y] = -1;
            while (queue.Count != 0)
            {
                var now = queue.Dequeue();
                for (var i = 0; i < 4; ++i)
                {
                    int xx = now.x + dx4[i], yy = now.y + dy4[i];
                    //0 white; 1 black; -1 visited
                    if (xx < 0 || xx >= N || yy < 0 || yy >= N || _board[xx, yy] != 0)
                        continue;
                    _board[xx, yy] = -1;
                    queue.Enqueue(new Vector2Int(xx, yy));
                }
            }
        }
        private List<FirewallCircle> DeleteWhiteSpace()
        {
            var res = new List<FirewallCircle>();
            for (var i = 0; i < N; ++i)
            {
                if (_board[0, i] == 0)
                    bfs(0, i);
                if (_board[N - 1, i] == 0)
                    bfs(N - 1, i);
                if (_board[i, 0] == 0)
                    bfs(i, 0);
                if (_board[i, N - 1] == 0)
                    bfs(i, N - 1);
            }
            print("DelereWhite");
            for (var i = 0; i < N; ++i)
                for (var j = 0; j < N; ++j)
                    if (_board[i, j] == 1)
                        res.Add(GetConnectComponent(i, j));
            print("DelereNoCircle");
            return res;
        }

        private void dfs(Vector2Int u, Vector2Int f)
        {
            var child = 0;
            dfn[u] = low[u] = ++dfsClock;
            foreach (var v in edge[u])
            {
                if (dfn[v] == 0) //时间截为零，说明没被访问过 
                {
                    dfs(v, u);
                    ++child;
                    low[u] = Math.Min(low[v], low[u]); //更新当前顶点cur能否访问到最早顶点的时间截 
                    if (low[v] >= dfn[u] && !f.Equals(new Vector2Int(-1, -1)))
                        cutVertexSet.Add(u); //v往回找到的最大时间截所在点还在u的子树里，则为u割点 
                }
                else
                {
                    low[u] = Math.Min(low[u], dfn[v]); //如果顶点u曾经被访问过，更新u的low值 
                }
            }

            if (f.Equals(new Vector2Int(-1, -1)) && child == 1 && cutVertexSet.Contains(u)) cutVertexSet.Remove(u); //源点就一条出边，一定不是割点 
        }

        private List<FirewallCircle> DeleteCutVertex(List<FirewallCircle> testList)
        {
            var res = new List<FirewallCircle>();
            foreach (var test in testList)
            {
                edge = new Dictionary<Vector2Int, List<Vector2Int>>();
                low = new Dictionary<Vector2Int, int>();
                cutVertexSet = new HashSet<Vector2Int>();
                dfn = new Dictionary<Vector2Int, int>();
                dfsClock = 0;
                foreach (var point in test)
                {
                    edge[point] = new List<Vector2Int>();
                    low[point] = 0;
                    dfn[point] = 0;
                    for (var i = 0; i < 8; ++i)
                    {
                        int xx = point.x + dx8[i], yy = point.y + dy8[i];
                        if (xx < 0 || xx >= N || yy < 0 || yy >= N || _board[xx, yy] != 2)
                            continue;
                        edge[point].Add(new Vector2Int(xx, yy));
                    }
                }

                foreach (var point in test)
                {
                    if (dfn[point] == 0)
                    {
                        dfs(point, new Vector2Int(-1, -1));
                    }
                }

                if (cutVertexSet.Count == 0)
                {
                    res.Add(test);
                    continue;
                }

                var s = cutVertexSet.Aggregate("Vertex", (current, point) => current + ("(" + point.x + "," + point.y + ")"));
                RootDebug.Log(s, NameID.JiangDigong_Log);
                foreach (var point in test)
                {
                    if (!cutVertexSet.Contains(point) && _board[point.x, point.y] == 2)
                    {
                        int x = point.x, y = point.y;
                        var tmp = new FirewallCircle();
                        var queue = new Queue<Vector2Int>();
                        tmp.Add(new Vector2Int(x, y));
                        queue.Enqueue(new Vector2Int(x, y));
                        _board[x, y] = 3;
                        while (queue.Count != 0)
                        {
                            var now = queue.Dequeue();
                            for (var i = 0; i < 4; ++i)
                            {
                                int xx = now.x + dx4[i], yy = now.y + dy4[i];
                                if (xx < 0 || xx >= N || yy < 0 || yy >= N || _board[xx, yy] == -1 || _board[xx, yy] == 3) continue;
                                tmp.Add(new Vector2Int(xx, yy));
                                if (cutVertexSet.Contains(new Vector2Int(xx, yy))) continue;
                                _board[xx, yy] = 3;
                                queue.Enqueue(new Vector2Int(xx, yy));
                            }
                        }

                        res.Add(tmp);
                    }
                }
            }
            print("DelereCutVertux");
            return res;
        }

        private FirewallCircle DeleteInnerCircle(FirewallCircle test)
        {
            var res = new FirewallCircle();
            foreach (var point in test)
            {
                var cnt = 0;
                for (var i = 0; i < 4; ++i)
                {
                    int xx = point.x + dx4[i], yy = point.y + dy4[i];
                    /*
                     * white -1
                     * input 2,3
                     * change inner into 4
                     */
                    if (xx < 0 || xx >= N || yy < 0 || yy >= N || _board[xx, yy] == -1)
                        break;
                    ++cnt;
                }

                if (cnt == 4)
                {
                    _board[point.x, point.y] = 4;
                    _firewallInnerZone.Add(point);
                    continue;
                }
                res.Add(point);
            }
            return res;
        }

        private void print(string info)
        {
            var s = info + "\n";
            for (var j = N - 1; j >= 0; --j)
            {
                for (var i = 0; i < N; ++i)
                {
                    s += _board[i, j];
                    s += ' ';
                }

                s += "\n";
            }
            RootDebug.Log(s, NameID.JiangDigong_Log);
        }
        private void updateFireWallCircle(Unit[] units)
        {
            //TODO 
            for (var i = 0; i < N; ++i)
                for (var j = 0; j < N; ++j)
                    _board[i, j] = 0;
            foreach (var unit in units)
                _board[unit.CurrentBoardPosition.x, unit.CurrentBoardPosition.y] = 1;
            print("Origin");
            _connectComponent = DeleteCutVertex(DeleteWhiteSpace());
            var maxArea = 0;
            _firewallCircle = new FirewallCircle();
            _firewallInnerZone = new FirewallInner();
            foreach (var circle in _connectComponent)
            {
                if (circle.Count <= maxArea) continue;
                maxArea = circle.Count;
                _firewallCircle = circle;
            }
            if (maxArea != 0)
                _firewallCircle = DeleteInnerCircle(_firewallCircle);
            RootDebug.Log("area" + maxArea, NameID.JiangDigong_Log);
            print("DeleteInner");
        }

        private int _cacheBoardHash;
        private bool initCache = true;

        public override void SignalDataUpdatedCallback()
        {
            if (_cacheBoardHash == Board.BoardHashCode && !initCache)
            {
                return;
            }
            _cacheBoardHash = Board.BoardHashCode;
            initCache = false;

            var data = SignalMasterMgr.Instance.GetActiveUnitByUnitType(SignalType, HardwareType.Field).ToArray();
            if (data.Length == 0)
            {
                _firewallCircle = new FirewallCircle();
                _firewallInnerZone = new FirewallInner();
                return;
            }
            updateFireWallCircle(data);
        }
    }
}