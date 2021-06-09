using System.Collections.Generic;
using I2.Loc;
using ROOT.RTAttribute;
using UnityEngine;

namespace ROOT.Consts
{
    public static class StaticNumericData
    {
        public static readonly int BoardLength = 6;
        public static readonly int StageWarningThreshold = 3;
        public static int RandomBoardRowIndex => Random.Range(0, BoardLength);

        public static readonly int MaxUnitTier = 6;

        public static readonly float BlinkSingleDuration = 0.1f;
        public static readonly float BlinkTransferInterval = 0.05f;
        
        public static readonly float DefaultAnimationDuration = 0.15f; //都是秒
        public static readonly float AutoAnimationDuration = 1.5f; //都是秒
    }

    public static class ValidLevelNameTerm
    {
        [ValidLevelNameTerm(LevelType.Tutorial)]
        public static string LevelName_TL0 = "LevelName_TL0";
        [ValidLevelNameTerm(LevelType.Tutorial)]
        public static string LevelName_TL1 = "LevelName_TL1";
        [ValidLevelNameTerm(LevelType.Tutorial)]
        public static string LevelName_TL2 = "LevelName_TL2";
        [ValidLevelNameTerm(LevelType.Tutorial)]
        public static string LevelName_TL3 = "LevelName_TL3";
        [ValidLevelNameTerm(LevelType.Tutorial)]
        public static string LevelName_TL4 = "LevelName_TL4";
        [ValidLevelNameTerm(LevelType.Tutorial)]
        public static string LevelName_TL5 = "LevelName_TL5";
        [ValidLevelNameTerm(LevelType.Tutorial)]
        public static string LevelName_TL6 = "LevelName_TL6";
        [ValidLevelNameTerm(LevelType.Career)]
        public static string LevelName_GL0 = "LevelName_GL0";
        [ValidLevelNameTerm(LevelType.Career)]
        public static string LevelName_GL1 = "LevelName_GL1";
        [ValidLevelNameTerm(LevelType.Career)]
        public static string LevelName_GL2 = "LevelName_GL2";
        [ValidLevelNameTerm(LevelType.Career)]
        public static string LevelName_GL3 = "LevelName_GL3";
        [ValidLevelNameTerm(LevelType.Career)]
        public static string LevelName_GL4 = "LevelName_GL4";
        [ValidLevelNameTerm(LevelType.Career)]
        public static string LevelName_GL5 = "LevelName_GL5";
        [ValidLevelNameTerm(LevelType.Career)]
        public static string LevelName_GL6 = "LevelName_GL6";
        [ValidLevelNameTerm(LevelType.Career)]
        public static string LevelName_GL7 = "LevelName_GL7";
    }
    
    public static class GlobalResourcePath
    {
        public static string UNIT_MAT_PATH_PREFIX = "Unit/Material/";

        public static string UNIT_PROCESSOR_MAT_NAME = "UnitCore_CPU";
        public static string UNIT_SERVER_MAT_NAME = "UnitCore_Server";
        public static string UNIT_NETCABLE_MAT_NAME = "UnitCore_Cable";
        public static string UNIT_HDD_MAT_NAME = "UnitCore_HDD";
        public static string UNIT_PCB_MAT_NAME = "UnitCore_PCB";
        public static string UNIT_HQ_MAT_NAME = "UnitCore_HQ";
    }

    public static class StaticTagName
    {
        public static readonly string TAG_NAME_UNIT = "Unit";
        public static readonly string TAG_NAME_BOARD_GRID = "BoardGrid";
        public static readonly string TAG_NAME_BOARD_GRID_ROOT = "BoardGridRoot";
        public static readonly string TAG_NAME_SKILL_PALETTE = "SkillPalette";
        public static readonly string TAG_NAME_HELP_SCREEN = "HelpScreen";
        public static readonly string TAG_NAME_TUTORIAL_FRAME = "TutorialTextFrame";
        public static readonly string TAG_NAME_ADV_SHOP_PANEL = "AdvShopPanel";
        public static readonly string TAG_CONTROLLING_EVENT_MGR = "ControllingEventMgr";
    }

    public static class StaticPlayerPrefName
    {
        public static readonly string PLAYER_ID = "PlayerID";
        public static readonly string MOUSE_DRAG_SENSITIVITY = "MouseDragSensitivity";
        public static readonly string DEV_MODE = "Devmode";
        public static readonly string LEVEL_SELECTION_PANEL_POS_X = "LevelSelectionPanelPosX";
        public static readonly string LEVEL_SELECTION_PANEL_POS_Y = "LevelSelectionPanelPosY";
        public static readonly string UNLOCK_SCAN = "UnlockScan";
    }

    public static class StaticName
    {
        public static string GetNameTermForStage(StageType stage)
        {
            try
            {
                return StageNameTerm[stage];
            }
            catch (KeyNotFoundException)
            {
                Debug.LogError("Desired stage name is not present.");
                return "N\\A";
            }
        }
        
        private static readonly Dictionary<StageType, string> StageNameTerm = new Dictionary<StageType, string> {
            {StageType.Shop, ScriptTerms.Shop},
            {StageType.Require, ScriptTerms.Require},
            {StageType.Destoryer, ScriptTerms.Destoryer},
            {StageType.Boss, ScriptTerms.Boss},
            {StageType.Ending, ScriptTerms.Ending},
        };

        public static readonly string INPUT_BUTTON_TELEMETRY_PAUSE = "TelemetryPause";

        public static readonly string INPUT_BUTTON_NAME_CURSORUP = "CursorUp";
        public static readonly string INPUT_BUTTON_NAME_CURSORDOWN = "CursorDown";
        public static readonly string INPUT_BUTTON_NAME_CURSORLEFT = "CursorLeft";
        public static readonly string INPUT_BUTTON_NAME_CURSORRIGHT = "CursorRight";

        public static readonly string INPUT_BUTTON_NAME_MOVEUNIT = "MoveUnit";
        public static readonly string INPUT_BUTTON_NAME_ROTATEUNIT = "RotateUnit";

        public static readonly string INPUT_BUTTON_NAME_HINTHDD = "HintHDD";
        public static readonly string INPUT_BUTTON_NAME_HINTNET = "HintNetwork";
        public static readonly string INPUT_BUTTON_NAME_HINTCTRL = "HintControl";
        public static readonly string INPUT_BUTTON_NAME_CYCLENEXT = "CycleNext";

        private static readonly string INPUT_BUTTON_NAME_FUNC0 = "Func0";
        private static readonly string INPUT_BUTTON_NAME_FUNC1 = "Func1";
        private static readonly string INPUT_BUTTON_NAME_FUNC2 = "Func2";
        private static readonly string INPUT_BUTTON_NAME_FUNC3 = "Func3";
        private static readonly string INPUT_BUTTON_NAME_FUNC4 = "Func4";
        private static readonly string INPUT_BUTTON_NAME_FUNC5 = "Func5";
        private static readonly string INPUT_BUTTON_NAME_FUNC6 = "Func6";
        private static readonly string INPUT_BUTTON_NAME_FUNC7 = "Func7";
        private static readonly string INPUT_BUTTON_NAME_FUNC8 = "Func8";
        private static readonly string INPUT_BUTTON_NAME_FUNC9 = "Func9";

        public static readonly string[] INPUT_BUTTON_NAME_SHOPBUYS =
        {
            //这个字符串数组的顺序不能变，它的顺序就是ShopID。
            INPUT_BUTTON_NAME_FUNC1,
            INPUT_BUTTON_NAME_FUNC2,
            INPUT_BUTTON_NAME_FUNC3,
            INPUT_BUTTON_NAME_FUNC4,
            INPUT_BUTTON_NAME_FUNC5,
            INPUT_BUTTON_NAME_FUNC6,
            INPUT_BUTTON_NAME_FUNC7,
            INPUT_BUTTON_NAME_FUNC8,
            INPUT_BUTTON_NAME_FUNC9,
            INPUT_BUTTON_NAME_FUNC0,
        };

        public static readonly string[] INPUT_BUTTON_NAME_SKILLS =
        {
            INPUT_BUTTON_NAME_FUNC1,
            INPUT_BUTTON_NAME_FUNC2,
            INPUT_BUTTON_NAME_FUNC3,
            INPUT_BUTTON_NAME_FUNC4,
            INPUT_BUTTON_NAME_FUNC5,
            INPUT_BUTTON_NAME_FUNC6,
            INPUT_BUTTON_NAME_FUNC7,
            INPUT_BUTTON_NAME_FUNC8,
            INPUT_BUTTON_NAME_FUNC9,
            INPUT_BUTTON_NAME_FUNC0,
        };


        public static readonly string INPUT_BUTTON_NAME_CANCELED = "Cancel";
        public static readonly string INPUT_BUTTON_NAME_CONFIRM = "Confirm";
        public static readonly string INPUT_BUTTON_NAME_SHOPRANDOM = "ShopRandom";
        public static readonly string INPUT_BUTTON_NAME_REMOVEUNIT = "RemoveUnit";

        public static readonly string INPUT_BUTTON_NAME_QUIT = "Quit";
        public static readonly string INPUT_BUTTON_NAME_NEXT = "Next";
#if UNITY_EDITOR
        public static readonly string DEBUG_INPUT_BUTTON_NAME_FORCESTEP = "DebugForceStep";
#endif
        //
        public static readonly string NAME_CORE_PCB = "NoConnection";
        public static readonly string NAME_CORE_NETCABLE = "NetworkCable";
        public static readonly string NAME_CORE_SERVER = "Server";
        public static readonly string NAME_CORE_BRIDGE = "Bridge";
        public static readonly string NAME_CORE_DRIVER = "HardDrive";
        public static readonly string NAME_CORE_CPU = "Processor";
        public static readonly string NAME_CORE_COOLER = "Cooler";

        public static readonly string NAME_CORE_BACKPLATE = "BackPlate";

        //
        public static readonly int SCENE_ID_START = 0;
        public static readonly int SCENE_ID_LEVELMASTER = 1;
        public static readonly int SCENE_ID_GAMEOVER = 2;
        public static readonly int SCENE_ID_ADDTIVEVISUAL = 3;
        public static readonly int SCENE_ID_ADDTIVELOGIC = 4;
        public static readonly int SCENE_ID_TUTORIAL = 5;
        public static readonly int SCENE_ID_CAREER = 6;
        public static readonly int SCENE_ID_CAREERSETUP = 7;
        public static readonly int SCENE_ID_ADDITIONAL_VISUAL_CAREER = 8;
        public static readonly int SCENE_ID_ADDITIONAL_VISUAL_TUTORIAL = 9;
        public static readonly int SCENE_ID_ADDITIONAL_GAMEPLAY_UI = 10;
        public static readonly int SCENE_ID_ADDITIONAL_VISUAL_ACQUIRING = 11;
        public static readonly int SCENE_ID_ADDITIONAL_VISUAL_TELEM = 12;
        public static readonly int SCENE_ID_BST_CAREER = 13;

        public static readonly string SOURCE_CORE_NODE_NAME = "SourceCore";
        public static readonly string DEST_CORE_NODE_NAME = "DestCore";

        public static readonly string CORE_MESH_MASTER_NODE_NAME = "CoreMesh";

        public static readonly string SOURCE_CONNECTOR_MASTER_NODE_NAME = "SourceConnector";
        public static readonly string DEST_CONNECTOR_MASTER_NODE_NAME = "DestConnector";

        public static readonly string LOCAL_NORTH_SIDE_MESH_RENDERER_NAME = "LocalNorthSide";
        public static readonly string LOCAL_EAST_SIDE_MESH_RENDERER_NAME = "LocalEastSide";
        public static readonly string LOCAL_SOUTH_SIDE_MESH_RENDERER_NAME = "LocalSouthSide";
        public static readonly string LOCAL_WEST_SIDE_MESH_RENDERER_NAME = "LocalWestSide";
    }
}