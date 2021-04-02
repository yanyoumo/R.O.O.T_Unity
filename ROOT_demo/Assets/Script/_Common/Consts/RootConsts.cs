using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

namespace ROOT.Consts
{
    public static class StaticNumericData
    {
        public static int BoardLength = 6;
        public static int StageWarningThreshold = 3;
        public static int RandomBoardRowIndex = Random.Range(0, BoardLength);
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
        public static readonly string TUTORIAL_PROGRESS = "TutorialProgress";
        public static readonly string GAMEPLAY_PROGRESS = "GameplayProgress";
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