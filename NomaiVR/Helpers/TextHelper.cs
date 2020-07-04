namespace NomaiVR
{
    public static class TextHelper
    {
        public const string RED = "#FF0000";
        public const string GREEN = "#00FF00";
        public const string BLUE = "#0000FF";
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
