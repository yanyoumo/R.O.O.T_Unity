using System;
using System.Collections.Generic;
using com.ootii.Messages;
using UnityEngine;

namespace ROOT.Signal
{
    using FirewallCircle=List<Vector2Int>;
    public class FirewallSignalAsset : SignalAssetBase
    {
        private static FirewallCircle _firewallCircle;
        public static FirewallCircle CurrentFirewallCircle => _firewallCircle;//这个算完之后就给接进去。
        public override Type UnitSignalCoreType => typeof(FirewallUnitSignalCore);
        public override SignalType SignalType => SignalType.Firewall;
        private static int N => Board.BoardLength;
        private int [,] _board=new int[N,N];

        private List<FirewallCircle> _connectComponent;

        private List<FirewallCircle> deleteWhiteSpace()
        {
            return new List<FirewallCircle>();
        }
        private void updateFireWallCircle(Unit[] units)
        {
            //TODO 
            for (var i = 0; i < N; ++i) 
                for (var j = 0; j < N; ++j)
                    _board[i,j] = 0;
            foreach (var unit in units)
                _board[unit.CurrentBoardPosition.x, unit.CurrentBoardPosition.y] = 1;

            _connectComponent=deleteWhiteSpace();

            _firewallCircle = new FirewallCircle(); //往这个函数里面填东西。
            
        }

        private void BoardDataUpdatedHandler(IMessage rMessage)
        {
            updateFireWallCircle(new Unit[0]);
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