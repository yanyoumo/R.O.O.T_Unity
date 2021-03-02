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
        public override int LEVEL_ART_SCENE_ID => StaticName.SCENE_ID_ADDITIONAL_VISUAL_ACQUIRING;
    }
}