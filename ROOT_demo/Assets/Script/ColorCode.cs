using System;
using UnityEngine;
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace ROOT
{
    public static class ColorUtilityWrapper
    {
        /// <summary>
        /// Unity默认给的哪个TryParseHtmlString是out出来的，不好用，用这个Wrapper搞一下。
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns>保证传出来一个Color，如果字符不正确则throw</returns>
        public static Color ParseHtmlStringNotNull(string htmlString)
        {
            var col = ParseHtmlString(htmlString);
            if (col.HasValue)
            {
                return col.Value;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        /// <summary>
        /// Unity默认给的哪个TryParseHtmlString是out出来的，不好用，用这个Wrapper搞一下。
        /// </summary>
        /// <returns>如果转换失败返回的null</returns>
        public static Color? ParseHtmlString(string htmlString)
        {
            bool res = ColorUtility.TryParseHtmlString(htmlString, out Color color);
            return res ? (Color?) color : null;
        }
    }

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
    public static class ColorName
    {
        private const string ROOT_MASTER_NETWORK = "#2D39FF";
        private const string ROOT_MASTER_GENERAL = "#2DFF3F";
        private const string ROOT_MASTER_DISASTER = "#FF1515";
        
        public static string ROOT_TIMELINE_GENERAL => ROOT_MASTER_GENERAL;
        public static string ROOT_TIMELINE_NETWORK => ROOT_MASTER_NETWORK;
        public static string ROOT_TIMELINE_DISASTER => ROOT_MASTER_DISASTER;
        public const string ROOT_TIMELINE_ENDING = "#000000";

        public static string ROOT_EVENT_DISASTER_RED => ROOT_MASTER_DISASTER;
        public const string ROOT_SHOP_CHEAP_PURPLE = "#9D33FF";
        public static string ROOT_DATA_NETWORK_BLUE => ROOT_MASTER_NETWORK;
        public static string ROOT_DATA_GENERAL_GREEN => ROOT_MASTER_GENERAL;
        public const string ROOT_TEXT_EMPHASIZE_ORANGE = "#FF5733";

        public const string ROOT_MAT_BOARDGRID_NORMAL = "#15182F";
        public const string ROOT_MAT_BOARDGRID_WARNING = "#752412";
        public const string ROOT_MAT_BOARDGRID_HEATSINK = "#650011";

        public const string ROOT_DESTORYER_STRIKING = "#FF3300";
        public const string ROOT_DESTORYER_WARNING = "#FFEA04";
    }
}