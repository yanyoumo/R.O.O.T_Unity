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
    public class FirewallSignalAsset : SignalAssetBase
    {
        private static FirewallCircle _firewallCircle;
        public static FirewallCircle CurrentFirewallCircle => _firewallCircle;//这个算完之后就给接进去。
        public override Type UnitSignalCoreType => typeof(FirewallUnitSignalCore);
        public override SignalType SignalType => SignalType.Firewall;
        private static int N => Board.BoardLength;
        private int[,] _board = new int[N, N];
        /*
         * white space: 0 -> -1
         * unit:        1 ->  2
         */
        private List<FirewallCircle> _connectComponent;
        private static int[] dx4 = { 0, -1, 0, 1 };
        private static int[] dy4 = { -1, 0, 1, 0 };
        private static int[] dx8 = {0, -1, -1, -1, 0, 1, 1, 1};
        private static int[] dy8 = {-1, -1, 0, 1, 1, 1, 0, -1};
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
                    if (xx >= 0 && xx < N && yy >= 0 && yy < N && _board[xx, yy] == 0)
                    {
                        _board[xx, yy] = -1;
                        queue.Enqueue(new Vector2Int(xx, yy));
                    }
                }
            }
        }

        private FirewallCircle GetConnectComponent(int x, int y)
        {
            var res = new FirewallCircle();
            var queue = new Queue<Vector2Int>();
            res.Add(new Vector2Int(x, y));
            queue.Enqueue(new Vector2Int(x, y));
            _board[x, y] = -1;
            while (queue.Count != 0)
            {
                var now = queue.Dequeue();
                for (var i = 0; i < 4; ++i)
                {
                    int xx = now.x + dx4[i], yy = now.y + dy4[i];
                    if (xx >= 0 && xx < N && yy >= 0 && yy < N && _board[xx, yy] != -1)
                    {
                        _board[xx, yy] = 2;
                        res.Add(new Vector2Int(xx, yy));
                        queue.Enqueue(new Vector2Int(xx, yy));
                    }
                }
            }
            return DeleteNonCircle(res);
        }

        private FirewallCircle DeleteNonCircle(FirewallCircle now)
        {
            var res = new FirewallCircle();
            var edge = new Dictionary<Vector2Int, List<Vector2Int>>();
            var inDegree = new Dictionary<Vector2Int, int>();
            foreach (var point in now)
            {
                edge[point] = new List<Vector2Int>();
                for (int i = 0; i < 8; ++i)
                {
                    inDegree[point] = 0;
                    int xx = point.x + dx8[i], yy = point.y + dy8[i];
                    if (xx >= 0 && xx < N && yy >= 0 && yy < N && _board[xx, yy] == 2)
                    {
                        ++inDegree[point];
                        edge[point].Add(new Vector2Int(xx,yy));
                    }
                }
            }

            foreach (var tmp in inDegree)
            {
                
            }
            return res;
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
            for (var i = 0; i < N; ++i)
                for (var j = 0; j < N; ++j)
                    if (_board[i, j] == 1)
                        res.Add(GetConnectComponent(i, j));
            return res;
        }
        private void updateFireWallCircle(Unit[] units)
        {
            //TODO 
            for (var i = 0; i < N; ++i)
                for (var j = 0; j < N; ++j)
                    _board[i, j] = 0;
            foreach (var unit in units)
                _board[unit.CurrentBoardPosition.x, unit.CurrentBoardPosition.y] = 1;

            _connectComponent = DeleteWhiteSpace();

            _firewallCircle = new FirewallCircle(); //往这个函数里面填东西。

        }

        private void BoardDataUpdatedHandler(IMessage rMessage)
        {
            var data = new Unit[0];
            try
            {
                SignalMasterMgr.Instance.Paths[SignalType.Firewall].ForEach(u => data.AddRange(u.Where(u0=>u0.UnitHardware==HardwareType.Field)));
            }
            catch (KeyNotFoundException)
            {
                return;
            }

            if (data.Length != 0)
            {
                updateFireWallCircle(data);
            }
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