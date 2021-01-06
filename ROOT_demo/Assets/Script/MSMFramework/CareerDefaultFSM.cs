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
                new PreInit_UpKeep_0(),
                new UpKeep_FCycle_2(),
                new UpKeep_RIO_1(),
                new UpKeep_UpKeep_0(),
                new RIO_FCycle_0(),
                new FCycle_Animate_1(),
                new FCycle_Clean_0(),
                new Animate_Clean_0(),
                new Animate_Animate_1(),
                new Clean_UpKeep_0(),
            };
            foreach (var msmTransition in Actions)
            {
                msmTransition.owner = this;
            }
        }
    }
}
