using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ROOT;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT
{
    public class ThermoSignalAsset : SignalAssetBase
    {
        void Awake()
        {
            UnitSignalCoreType = gameObject.AddComponent<ThermoUnitSignalCore>().GetType();
        }

        public override SignalType Type => SignalType.Thermo;

        public override bool ShowSignal(RotationDirection dir, Unit unit, Unit otherUnit)
        {
            //TODO
            var ShowNetLED = true;
            
            return ShowNetLED;
        }

        public override int SignalVal(RotationDirection dir, Unit unit, Unit otherUnit)
        {
            //TODO
            var showSig = ShowSignal(dir, unit, otherUnit);
            return showSig ? 1 : 0;
        }

        private void InitCounting(Unit unit)
        {
            unit.SignalCore.Visited = false;
            unit.IsActiveThermoFieldUnit = false;
        }

        private void FindActiveThermoFieldUnit(Board gameBoard, out int thermoItemCountInt)
        {
            thermoItemCountInt = 0;
            foreach (var ThermoCoreUnit in gameBoard.FindUnitWithCoreType(Type, HardwareType.Core))
            {
                if (ThermoCoreUnit.SignalCore.Visited)
                    continue;
                var queue=new Queue<Unit>();
                queue.Enqueue(ThermoCoreUnit);
                ThermoCoreUnit.SignalCore.Visited = true;
                while (queue.Count != 0)
                {
                    var now = queue.Dequeue();
                    foreach (var unit in now.GetConnectedOtherUnit.Where(unit => unit.SignalCore.Visited ==false))
                    {
                        unit.SignalCore.Visited = true;
                        queue.Enqueue(unit);
                        if (unit.UnitSignal != SignalType.Thermo || unit.UnitHardware != HardwareType.Field) continue;
                        unit.IsActiveThermoFieldUnit = true;
                        ++thermoItemCountInt;
                    }
                }
            }
        }
        public override float CalAllScore(Board gameBoard, out int thermoItemCountInt)
        {
            gameBoard.Units.ForEach(InitCounting);
            FindActiveThermoFieldUnit(gameBoard, out thermoItemCountInt);
            return gameBoard.FindUnitWithCoreType(Type, HardwareType.Core)
                .Where(unit => unit.UnitSignal == SignalType.Thermo && unit.UnitHardware == HardwareType.Field && unit.IsActiveThermoFieldUnit)
                .Select(unit => unit.SignalCore as ThermoUnitSignalCore)
                .Sum(ThermoField => ThermoField.CalScore());
        }
    }
}
