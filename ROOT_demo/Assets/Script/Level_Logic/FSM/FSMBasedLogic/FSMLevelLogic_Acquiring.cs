using System;
using System.Collections.Generic;

namespace ROOT
{
    public class FSMLevelLogic_Acquiring : FSMLevelLogic_Career
    {
        protected override Dictionary<RootFSMStatus, Action> fsmActions => throw new NotImplementedException();
        protected override HashSet<RootFSMTransition> RootFSMTransitions => throw new NotImplementedException();
    }
}