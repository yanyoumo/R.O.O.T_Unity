using UnityEngine;

namespace I2.Loc
{
	public static class ScriptLocalization
	{

		public static string PlayLevel 		{ get{ return LocalizationManager.GetTranslation ("PlayLevel"); } }
		public static string StartGame 		{ get{ return LocalizationManager.GetTranslation ("StartGame"); } }
		public static string Tutorial 		{ get{ return LocalizationManager.GetTranslation ("Tutorial"); } }
		public static string TutorialBasicControl 		{ get{ return LocalizationManager.GetTranslation ("TutorialBasicControl"); } }
		public static string TutorialGoalAndCycle 		{ get{ return LocalizationManager.GetTranslation ("TutorialGoalAndCycle"); } }
		public static string TutorialSignalBasic 		{ get{ return LocalizationManager.GetTranslation ("TutorialSignalBasic"); } }
		public static string TutorialDestroyer 		{ get{ return LocalizationManager.GetTranslation ("TutorialDestroyer"); } }
		public static string TutorialShop 		{ get{ return LocalizationManager.GetTranslation ("TutorialShop"); } }
	}

    public static class ScriptTerms
	{

		public const string PlayLevel = "PlayLevel";
		public const string StartGame = "StartGame";
		public const string Tutorial = "Tutorial";
		public const string TutorialBasicControl = "TutorialBasicControl";
		public const string TutorialGoalAndCycle = "TutorialGoalAndCycle";
		public const string TutorialSignalBasic = "TutorialSignalBasic";
		public const string TutorialDestroyer = "TutorialDestroyer";
		public const string TutorialShop = "TutorialShop";
	}
}