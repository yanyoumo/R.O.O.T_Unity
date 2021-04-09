using System;
using System.Collections.Generic;
using ROOT.Common;

namespace ROOT.FSM
{
    public class TutorialFSMModule
    {
        protected FSMLevelLogic owner;
        protected GameAssets LevelAsset => owner.LevelAsset;

        protected void ModifyFSMActions(ref Dictionary<RootFSMStatus, Action> actions)
        {
            
        }

        protected void ModifyRootFSMTransitions(ref HashSet<RootFSMTransition> RootFSMTransitions)
        {
            
        }
    }
}