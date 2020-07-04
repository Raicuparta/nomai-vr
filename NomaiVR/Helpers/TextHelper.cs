namespace NomaiVR
{
    public static class TextHelper
    {
        public const string RED = "#ff8484";
        public const string GREEN = "#c2ff81";
        public const string BLUE = "#8ed3ff";
        public const string YELLOW = "#fffd69";
        public const string ORANGE = "orange";

        public static string TextWithColor(string text, string color)
        {
            return $"<color={color}>{text}</color>";
        }
    }
}
