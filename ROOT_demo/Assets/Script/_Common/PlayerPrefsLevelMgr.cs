
using System.Collections.Generic;
using UnityEngine;

namespace ROOT.LevelAccessMgr
{
    public enum LevelStatus
    {
        Locked = 0,
        Unlocked = 1,
        Played = 2,
        Passed = 3,
    }
    
    public static class PlayerPrefsLevelMgr
    {
        public static LevelStatus GetLevelStatus(string levelNameTerm)
        {
            if (PlayerPrefs.HasKey(levelNameTerm))
            {
                return (LevelStatus) PlayerPrefs.GetInt(levelNameTerm);
            }

            ReplaceLevelStatus(levelNameTerm, LevelStatus.Locked);
            return LevelStatus.Locked;
        }

        public static void CompleteThisLevel(string completedLevel)
        {
            UpdateLevelStatus(completedLevel, LevelStatus.Passed);
        }
        
        public static void CompleteThisLevelAndUnlockFollowing(string completedLevel, IEnumerable<string> unlockedCompletedLevel)
        {
            UpdateLevelStatus(completedLevel, LevelStatus.Passed);
            foreach (var s in unlockedCompletedLevel)
            {
                UpdateLevelStatus(s, LevelStatus.Unlocked);
            }
        }

        public static void PlayedThisLevel(string completedLevel)
        {
            UpdateLevelStatus(completedLevel, LevelStatus.Played);
        }

        public static void SetUpRootLevelStatus(string rootLevel)
        {
            UpdateLevelStatus(rootLevel, LevelStatus.Unlocked);
        }
        
        private static void UpdateLevelStatus(string completedLevel,LevelStatus desiredStatus)
        {
            if (PlayerPrefs.HasKey(completedLevel))
            {
                var existingStatus = (LevelStatus) PlayerPrefs.GetInt(completedLevel);
                if (desiredStatus > existingStatus)//LevelStatus是从大到小优先级高的、在Update模式下：只能往上更新、保持不会“反写”的可能。
                {
                    ReplaceLevelStatus(completedLevel, desiredStatus);
                }
            }
            else
            {
                Debug.LogWarning("Trying update new level status info, replace instead!!");
                ReplaceLevelStatus(completedLevel, desiredStatus);
            }
        }

        private static void ReplaceLevelStatus(string completedLevel, LevelStatus desiredStatus)
        {
            PlayerPrefs.SetInt(completedLevel, (int) desiredStatus);
            PlayerPrefs.Save();
        }
    }
}

