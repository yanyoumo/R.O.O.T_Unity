using UnityEngine;

namespace I2.Loc
{
	public static class ScriptLocalization
	{

		public static string Back 		{ get{ return LocalizationManager.GetTranslation ("Back"); } }
		public static string EndingMessageNormal_EarnedMoney 		{ get{ return LocalizationManager.GetTranslation ("EndingMessageNormal_EarnedMoney"); } }
		public static string EndingMessageNormal_NoEarnedMoney 		{ get{ return LocalizationManager.GetTranslation ("EndingMessageNormal_NoEarnedMoney"); } }
		public static string EndingMessageNormal_NoMoney 		{ get{ return LocalizationManager.GetTranslation ("EndingMessageNormal_NoMoney"); } }
		public static string EndingMessageTutorial 		{ get{ return LocalizationManager.GetTranslation ("EndingMessageTutorial"); } }
		public static string GameOver 		{ get{ return LocalizationManager.GetTranslation ("GameOver"); } }
		public static string NextTutorial 		{ get{ return LocalizationManager.GetTranslation ("NextTutorial"); } }
		public static string PlayLevel 		{ get{ return LocalizationManager.GetTranslation ("PlayLevel"); } }
		public static string Restart 		{ get{ return LocalizationManager.GetTranslation ("Restart"); } }
		public static string StartGame 		{ get{ return LocalizationManager.GetTranslation ("StartGame"); } }
		public static string Tutorial 		{ get{ return LocalizationManager.GetTranslation ("Tutorial"); } }
		public static string TutorialBasicControl 		{ get{ return LocalizationManager.GetTranslation ("TutorialBasicControl"); } }
		public static string TutorialDestroyer 		{ get{ return LocalizationManager.GetTranslation ("TutorialDestroyer"); } }
		public static string TutorialGoalAndCycle 		{ get{ return LocalizationManager.GetTranslation ("TutorialGoalAndCycle"); } }
		public static string TutorialSectionOver 		{ get{ return LocalizationManager.GetTranslation ("TutorialSectionOver"); } }
		public static string TutorialShop 		{ get{ return LocalizationManager.GetTranslation ("TutorialShop"); } }
		public static string TutorialSignalBasic 		{ get{ return LocalizationManager.GetTranslation ("TutorialSignalBasic"); } }
	}

    public static class ScriptTerms
	{

		public const string Back = "Back";
		public const string EndingMessageNormal_EarnedMoney = "EndingMessageNormal_EarnedMoney";
		public const string EndingMessageNormal_NoEarnedMoney = "EndingMessageNormal_NoEarnedMoney";
		public const string EndingMessageNormal_NoMoney = "EndingMessageNormal_NoMoney";
		public const string EndingMessageTutorial = "EndingMessageTutorial";
		public const string GameOver = "GameOver";
		public const string NextTutorial = "NextTutorial";
		public const string PlayLevel = "PlayLevel";
		public const string Restart = "Restart";
		public const string StartGame = "StartGame";
		public const string Tutorial = "Tutorial";
		public const string TutorialBasicControl = "TutorialBasicControl";
		public const string TutorialDestroyer = "TutorialDestroyer";
		public const string TutorialGoalAndCycle = "TutorialGoalAndCycle";
		public const string TutorialSectionOver = "TutorialSectionOver";
		public const string TutorialShop = "TutorialShop";
		public const string TutorialSignalBasic = "TutorialSignalBasic";
	}
}