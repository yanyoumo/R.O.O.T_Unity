using System;
using System.Collections.Generic;

namespace ROOT.FSM
{
    using FailedLib = Dictionary<TutorialFailedType, Func<FSMLevelLogic, Board, bool>>;

    public partial class TutorialFSMModule
    {
        private readonly FailedLib FailedLib = new FailedLib
        {
            {TutorialFailedType.NeverFailed, (fsm, board) => false},
        };
    }
}