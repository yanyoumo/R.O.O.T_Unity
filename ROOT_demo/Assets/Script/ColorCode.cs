using System;

namespace ROOT
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
    public static class ColorName
    {
        public const string ROOT_EVENT_DISASTER_RED = "#FF1515";
        public const string ROOT_SHOP_CHEAP_PURPLE = "#9D33FF";
        public const string ROOT_DATA_NETWORK_BLUE = "#2D39FF";
        public const string ROOT_DATA_GENERAL_GREEN = "#2DFF3F";
        public const string ROOT_TEXT_EMPHASIZE_ORANGE = "#FF5733";
    }
}