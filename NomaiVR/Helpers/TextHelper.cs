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

        public static string RedText(string text) => TextWithColor(text, RED);
        public static string GreenText(string text) => TextWithColor(text, GREEN);
        public static string BlueText(string text) => TextWithColor(text, BLUE);
        public static string OrangeText(string text) => TextWithColor(text, ORANGE);
    }
}
