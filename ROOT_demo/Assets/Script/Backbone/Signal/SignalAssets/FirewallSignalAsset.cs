using System;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT.Signal
{
    using FirewallCircle=List<Vector2Int>;
    public class FirewallSignalAsset : SignalAssetBase
    {
        public override Type UnitSignalCoreType => typeof(FirewallUnitSignalCore);
        public override SignalType SignalType => SignalType.Firewall;

        private List<FirewallCircle> getFireWallCircleLists(Board _gameBoard)
        {
            return new List<FirewallCircle>();
        }
    }
}