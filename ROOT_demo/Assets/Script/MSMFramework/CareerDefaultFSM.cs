using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class CareerDefaultFSM : RootFSMBase
    {
        protected override void InitActions()
        {
            Actions = new List<RootFSMTransition>
            {
                new PreInit_Idle_0(),
                new Idle_Upkeep_1(),
                new Idle_Idle_0(),
                new Upkeep_Cycle_0(),
                new Cycle_Animate_1(),
                new Cycle_Clean_0(),
                new Animate_Clean_1(),
                new Animate_Animate_0(),
                new Clean_Idle_0(),
            };
            foreach (var msmTransition in Actions)
            {
                msmTransition.owner = this;
            }
        }
    }
}
