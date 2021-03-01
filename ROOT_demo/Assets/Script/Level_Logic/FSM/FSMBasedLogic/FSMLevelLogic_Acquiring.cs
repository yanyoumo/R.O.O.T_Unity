using System;
using System.Collections.Generic;

namespace ROOT
{
    public class FSMLevelLogic_Acquiring : FSMLevelLogic_Career
    {
        public override bool IsTutorial => false;
        public override bool CouldHandleSkill => true;
        public override bool CouldHandleBoss => true;
        public override BossStageType HandleBossType => BossStageType.Acquiring;
    }
}