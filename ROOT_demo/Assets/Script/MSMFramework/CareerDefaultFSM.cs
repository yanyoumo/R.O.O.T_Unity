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
                new Idle_Upkeep_0(),
                new Upkeep_Major_0(),
                new Major_CleanUp_0(),
                new CleanUp_Idle_0(),
            };
            foreach (var msmTransition in Actions)
            {
                msmTransition.owner = this;
            }
        }
    }
}
