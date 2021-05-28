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
        [PropertyOrder(-10)]
        public Color MASTER_WHITE;
        [PropertyOrder(-9)]
        public Color MASTER_BLACK;
        [PropertyOrder(-8)]
        public Color MASTER_RED;
        [PropertyOrder(-7)]
        public Color MASTER_YELLOW;
        [PropertyOrder(-6)]
        public Color MASTER_GREEN;
        [PropertyOrder(-5)]
        public Color MASTER_CYAN;
        [PropertyOrder(-4)]
        public Color MASTER_BLUE;
        [PropertyOrder(-3)]
        public Color MASTER_PURPLE;
        [PropertySpace]
        [PropertyOrder(-2)]
        public Color ROOT_MASTER_SCAN;
        [PropertyOrder(-1)]
        public Color ROOT_MASTER_MATRIX;
        [PropertyOrder(0)]
        public Color ROOT_MASTER_THERMO;
        [PropertyOrder(1)]
        public Color ROOT_MASTER_DISASTER;
        
        [PropertySpace]
        [Title("TimeLine")]
        [PropertyOrder(2)]
        [ShowInInspector]
        [PropertyTooltip("Same as ROOT_MASTER_MATRIX")]
        public Color ROOT_TIMELINE_MATRIX => ROOT_MASTER_MATRIX;
        [PropertyOrder(3)]
        [ShowInInspector]
        [PropertyTooltip("Same as ROOT_MASTER_MATRIX")]
        public Color ROOT_TIMELINE_SCAN => ROOT_MASTER_MATRIX;//这个是遗留问题、TimeLineToken的逻辑要变。
        [PropertyOrder(4)] 
        [ShowInInspector]
        [PropertyTooltip("Same as ROOT_TIMELINE_SHOPOPENED")]
        public Color ROOT_TIMELINE_HEATSINKSWITCH => ROOT_TIMELINE_SHOPOPENED;//同上
        [PropertyOrder(5)]
        public Color ROOT_TIMELINE_SHOPOPENED;
        [PropertyOrder(6)]
        public Color ROOT_TIMELINE_BOSS;
        [ShowInInspector]
        [PropertyOrder(7)]
        [PropertyTooltip("Same as ROOT_MASTER_DISASTER")]
        public Color ROOT_TIMELINE_DISASTER => ROOT_MASTER_DISASTER;
        [PropertyOrder(8)]
        public Color ROOT_TIMELINE_ENDING;

        [PropertySpace]
        [Title("Event")]
        [PropertyOrder(9)]
        [ShowInInspector]
        [PropertyTooltip("Same as ROOT_MASTER_DISASTER")]
        public Color ROOT_EVENT_DISASTER_RED => ROOT_MASTER_DISASTER;

        [PropertySpace]
        [Title("Signal")]
        [ShowInInspector]
        [PropertyOrder(10)]
        [PropertyTooltip("Same as ROOT_MASTER_SCAN")]
        public Color ROOT_SIGNAL_SCAN => ROOT_MASTER_SCAN;
        [ShowInInspector]
        [PropertyOrder(11)]
        [PropertyTooltip("Same as ROOT_MASTER_MATRIX")]
        public Color ROOT_SIGNAL_MATRIX => ROOT_MASTER_MATRIX;
        [ShowInInspector]
        [PropertyOrder(12)]
        [PropertyTooltip("Same as ROOT_MASTER_THERMO")]
        public Color ROOT_SIGNAL_THREMO => ROOT_MASTER_THERMO;
        
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
        [ShowInInspector]
        [PropertyOrder(22)] 
        [PropertyTooltip("Same as ROOT_MASTER_THERMO")]
        public Color ROOT_MAT_BOARDGRID_ZONE_THERMO => ROOT_MASTER_THERMO;
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
        public Color ROOT_SKILL_SWAP_UNITA;
        [PropertyOrder(29)]
        public Color ROOT_SKILL_SWAP_UNITB;
        [PropertyOrder(30)]
        public Color ROOT_SHOP_CHEAP_PURPLE;
        
        [PropertySpace]
        [Title("Unit Activation LED")]
        [PropertyOrder(31)]
        public Color ROOT_UNIT_ACTIVATION_LED_0;
        [PropertyOrder(32)]
        public Color ROOT_UNIT_ACTIVATION_LED_1;
        [PropertyOrder(33)]
        public Color ROOT_UNIT_ACTIVATION_LED_2;
        [PropertyOrder(34)]
        public Color ROOT_UNIT_ACTIVATION_LED_3;

        [PropertySpace] 
        [Title("Cursor")]
        [PropertyOrder(35)]
        public Color ROOT_CURSOR_DEFAULT;
        [PropertyOrder(36)]
        public Color ROOT_CURSOR_INFOMODE;
        [PropertyOrder(37)]
        public Color ROOT_CURSOR_BLINK;
    }
}
