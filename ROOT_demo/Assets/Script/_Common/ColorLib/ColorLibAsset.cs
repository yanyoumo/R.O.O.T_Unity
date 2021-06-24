using System;
using Sirenix.OdinInspector;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace ROOT.SetupAsset
{
    /// <summary>
    /// This class stores all the color code(HEX string) that have been used in this game
    /// the naming schema for this class would be:
    ///     ROOT_{color_environment}_{description}_{color name}
    ///{color environment} means what kind of scenario that color is used in, like used in SHOP or text
    ///{description} please try us use one word to describe the color meaning the color environment/scenario
    ///     For example, if you use ROOT_DATA_NETWORK_BLUE to mark some text, the NETWORK describe the text would
    ///     have network data meaning in the DATA scenario.
    ///{color name} the color you have used
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "ColorLibAsset", menuName = "New ColorLibAsset")]
    public class ColorLibAsset : ScriptableObject
    {   
        [Title("Master")]
        [PropertyOrder(-13)]
        public Color MASTER_WHITE;
        [PropertyOrder(-12)]
        public Color MASTER_BLACK;
        [PropertyOrder(-11)]
        public Color MASTER_RED;
        [PropertyOrder(-10)]
        public Color MASTER_YELLOW;
        [PropertyOrder(-9)]
        public Color MASTER_GREEN;
        [PropertyOrder(-8)]
        public Color MASTER_CYAN;
        [PropertyOrder(-7)]
        public Color MASTER_BLUE;
        [PropertyOrder(-6)]
        public Color MASTER_PURPLE;
        [PropertySpace]
        [PropertyOrder(-5)]
        public Color ROOT_MASTER_BASIC;
        [PropertyOrder(-4)]
        public Color ROOT_MASTER_THERMO;
        [PropertyOrder(-3)]
        public Color ROOT_MASTER_MATRIX;
        [PropertyOrder(-2)]
        public Color ROOT_MASTER_FIREWALL;
        [PropertyOrder(-1)]
        public Color ROOT_MASTER_SCAN;
        [PropertyOrder(0)]
        public Color ROOT_MASTER_DISASTER;
        
        [PropertySpace]
        [Title("TimeLine")]
        [PropertyOrder(1)]
        [ShowInInspector]
        [PropertyTooltip("Same as ROOT_MASTER_MATRIX")]
        public Color ROOT_TIMELINE_MATRIX => ROOT_MASTER_MATRIX;
        [PropertyOrder(2)]
        [ShowInInspector]
        [PropertyTooltip("Same as ROOT_MASTER_MATRIX")]
        public Color ROOT_TIMELINE_SCAN => ROOT_MASTER_MATRIX;//这个是遗留问题、TimeLineToken的逻辑要变。
        [PropertyOrder(3)] 
        [ShowInInspector]
        [PropertyTooltip("Same as ROOT_TIMELINE_SHOPOPENED")]
        public Color ROOT_TIMELINE_HEATSINKSWITCH => ROOT_TIMELINE_SHOPOPENED;//同上
        [PropertyOrder(4)]
        public Color ROOT_TIMELINE_SHOPOPENED;
        [PropertyOrder(5)]
        public Color ROOT_TIMELINE_BOSS;
        [ShowInInspector]
        [PropertyOrder(6)]
        [PropertyTooltip("Same as ROOT_MASTER_DISASTER")]
        public Color ROOT_TIMELINE_DISASTER => ROOT_MASTER_DISASTER;
        [PropertyOrder(7)]
        public Color ROOT_TIMELINE_ENDING;

        [PropertySpace]
        [Title("Event")]
        [PropertyOrder(8)]
        [ShowInInspector]
        [PropertyTooltip("Same as ROOT_MASTER_DISASTER")]
        public Color ROOT_EVENT_DISASTER_RED => ROOT_MASTER_DISASTER;

        [PropertySpace]
        [Title("Signal")]
        [ShowInInspector]
        [PropertyOrder(9)]
        [PropertyTooltip("Same as ROOT_MASTER_SCAN")]
        public Color ROOT_SIGNAL_SCAN => ROOT_MASTER_SCAN;
        [ShowInInspector]
        [PropertyOrder(10)]
        [PropertyTooltip("Same as ROOT_MASTER_MATRIX")]
        public Color ROOT_SIGNAL_MATRIX => ROOT_MASTER_MATRIX;
        [ShowInInspector]
        [PropertyOrder(11)]
        [PropertyTooltip("Same as ROOT_MASTER_THERMO")]
        public Color ROOT_SIGNAL_THREMO => ROOT_MASTER_THERMO;
        [ShowInInspector]
        [PropertyOrder(12)]
        public Color ROOT_SIGNAL_FIREWALL;
        
        [PropertySpace]
        [Title("BoardGrid")]
        [PropertyOrder(13)]
        public Color ROOT_MAT_BOARDGRID_NORMAL;
        [PropertyOrder(14)]
        public Color ROOT_MAT_BOARDGRID_WARNING;
        [PropertyOrder(15)]
        public Color ROOT_MAT_BOARDGRID_HEATSINK;
        [PropertyOrder(16)]
        public Color ROOT_MAT_BOARDGRID_INFO;
        [PropertyOrder(17)]
        public Color ROOT_MAT_BOARDGRID_PREWARNING;
        [Title("BoardGrid Stroke")]
        [PropertyOrder(18)]
        public Color ROOT_MAT_BOARDGRIDSTROKE_NORMAL;
        [PropertyOrder(19)]
        public Color ROOT_MAT_BOARDGRIDSTROKE_FLOATING;
        [PropertyOrder(20)]
        public Color ROOT_MAT_BOARDGRIDSTROKE_HIGHLIGHTED;
        [Title("BoardGrid Zone")]
        [PropertyOrder(21)]
        public Color ROOT_MAT_BOARDGRID_ZONE_INFO;
        [PropertyOrder(22)] 
        public Color ROOT_MAT_BOARDGRID_ZONE_SINGLE;
        [Title("BoardGrid Custom HighLighting")]
        [PropertyOrder(23)]
        public Color ROOT_MAT_BOARDGRID_CUSTOM_HIGHLIGHTING_A;
        [PropertyOrder(24)]
        public Color ROOT_MAT_BOARDGRID_CUSTOM_HIGHLIGHTING_B;
        [PropertyOrder(25)]
        public Color ROOT_MAT_BOARDGRID_CUSTOM_HIGHLIGHTING_C;
        
        [PropertySpace]
        [Title("Destoryer")]
        [PropertyOrder(26)]
        public Color ROOT_DESTORYER_STRIKING;
        [PropertyOrder(27)]
        public Color ROOT_DESTORYER_WARNING;

        [PropertySpace]
        [Title("Skill")]
        [PropertyOrder(28)]
        public Color ROOT_SKILL_NAME_MAIN;
        [PropertyOrder(29)]
        public Color ROOT_SKILL_NAME_SUB;
        [PropertyOrder(30)]
        public Color ROOT_SKILL_NAME_RMN;
        [PropertyOrder(31)]
        public Color ROOT_SKILL_SWAP_UNITA;
        [PropertyOrder(32)]
        public Color ROOT_SKILL_SWAP_UNITB;
        [PropertyOrder(33)]
        public Color ROOT_SHOP_CHEAP_PURPLE;
        
        [PropertySpace]
        [Title("Unit Activation LED")]
        [PropertyOrder(34)]
        public Color ROOT_UNIT_ACTIVATION_LED_0;
        [PropertyOrder(35)]
        public Color ROOT_UNIT_ACTIVATION_LED_1;
        [PropertyOrder(36)]
        public Color ROOT_UNIT_ACTIVATION_LED_2;
        [PropertyOrder(37)]
        public Color ROOT_UNIT_ACTIVATION_LED_3;

        [PropertySpace] 
        [Title("Cursor")]
        [PropertyOrder(38)]
        public Color ROOT_CURSOR_DEFAULT;
        [PropertyOrder(39)]
        public Color ROOT_CURSOR_INFOMODE;
        [PropertyOrder(40)]
        public Color ROOT_CURSOR_BLINK;
        
        [PropertySpace] 
        [Title("BST Level Selection")]
        [PropertyOrder(41)]
        public Color ROOT_SELECTIONQUAD_SELECTABLE;
        [PropertyOrder(42)]
        public Color ROOT_SELECTIONQUAD_UNSELECTABLE;

        [PropertySpace]
        [Title("UI Section")] 
        [PropertyOrder(43)]
        public Color ROOT_UI_DEFAULT_BLACK;
        [PropertyOrder(44)]
        public Color ROOT_UI_DEFAULT_DARKGRAY;
        [PropertyOrder(45)]
        public Color ROOT_UI_DEFAULT_WHITE;
        [PropertyOrder(46)]
        public Color ROOT_UI_HIGHLIGHTING_GREEN;
        [PropertyOrder(47)]
        public Color ROOT_UI_HIGHLIGHTING_RED;
        [PropertyOrder(48)]
        public Color ROOT_UI_HIGHLIGHTING_BLUE;
    }
}
