using System;
using System.Reflection;
using ROOT.RTAttribute;
using UnityEngine;

namespace ROOT.Clock
{
    [RequireComponent(typeof(MasterClock))]
    public abstract class ClockModuleBase : MonoBehaviour
    {
        //[ReplaceableAction] public Action TestFunction = () => Debug.Log("TestFunction_Derived");

        private void Awake()
        {
            GetComponent<MasterClock>().RegisterClockModule(this);
        }
    }
}