using ROOT.Consts;
using ROOT.SetupAsset;

namespace ROOT
{
    public sealed class RoundLibDriver
    {
        public FSMLevelLogic owner;
        private LevelActionAsset _ActionAsset => owner.LevelAsset.ActionAsset;
        private RoundGist? GetRoundGistByStep(int step) => _ActionAsset?.GetCurrentRoundGist(step);
        private StageType? Stage(int step) => _ActionAsset?.GetCurrentType(step);

        public RoundGist? PreCheckRoundGist => GetRoundGistByStep(owner.LevelAsset.StepCount + StaticNumericData.StageWarningThreshold);
        public RoundGist? CurrentRoundGist => GetRoundGistByStep(owner.LevelAsset.StepCount);
        public RoundGist? PreviousRoundGist => (owner.LevelAsset.StepCount - 1)>=0 ? _ActionAsset.GetCurrentRoundGist(owner.LevelAsset.StepCount - 1) : CurrentRoundGist;
        public StageType? CurrentStage => Stage(owner.LevelAsset.StepCount);

        public bool IsShopRound => CurrentStage == StageType.Shop;
        public bool IsRequireRound => CurrentStage == StageType.Require;
        public bool IsDestoryerRound => CurrentStage == StageType.Destoryer;
        public bool IsBossRound => CurrentStage == StageType.Boss;
    }
}
    
