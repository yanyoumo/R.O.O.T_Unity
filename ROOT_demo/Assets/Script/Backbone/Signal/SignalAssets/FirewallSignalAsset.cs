using System;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Messages;
using ROOT.SetupAsset;
using Sirenix.Utilities;
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

        private List<FirewallCircle> getFireWallCircleLists(Board _gameBoard)
        {
            //TODO
            return new List<FirewallCircle>();
        }

        private void updateFireWallCircle(Unit[] units)
        {
            //TODO 
            _firewallCircle = new FirewallCircle(); //往这个函数里面填东西。
        }

        private void BoardDataUpdatedHandler(IMessage rMessage)
        {
            var data = new Unit[0];
            SignalMasterMgr.Instance.Paths[SignalType.Firewall].ForEach(u => data.AddRange(
                u.Where(u0=>u0.UnitHardware==HardwareType.Field)));
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