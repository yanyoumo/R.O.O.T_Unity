using System;

namespace ROOT
{
    /// <summary>
    /// This class stores all the color code(HEX string) that have been used in this game
    /// </summary>
    public static class ColorCode
    {
        /// <summary>
        /// This method would return red color code
        /// </summary>
        public static String getRedColorCode { get { return ColorName.RED; } }
        /// <summary>
        /// This method would return purple color code
        /// </summary>
        public static String getPurpleColorCode { get { return ColorName.PURPLE; } }
        /// <summary>
        /// This method would return blue color code
        /// </summary>
        public static String getBlueColorCode { get { return ColorName.BLUE; } }
        /// <summary>
        /// This method would return green color code
        /// </summary>
        public static String getGreenColorCode { get { return ColorName.GREEN; } }
        /// <summary>
        /// This method would return orange color code
        /// </summary>
        public static String getOrangeColorCode { get { return ColorName.ORANGE; } }

    }

    static class ColorName
    {
        public const string RED = "#FF1515";
        public const string PURPLE = "#9D33FF";
        public const string BLUE = "#2D39FF";
        public const string GREEN = "#2DFF3F";
        public const string ORANGE = "#FF5733";
    }
}