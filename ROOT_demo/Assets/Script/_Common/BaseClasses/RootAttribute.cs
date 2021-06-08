using System;

namespace ROOT.RTAttribute
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ValidLevelNameTermAttribute : Attribute
    {
        private LevelType validLevelType;

        public ValidLevelNameTermAttribute(LevelType _validLevelType)
        {
            validLevelType = _validLevelType;
        }
    }
}