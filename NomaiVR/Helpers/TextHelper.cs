namespace NomaiVR.Helpers
{
    public static class TextHelper
    {
        public const string Red = "#ff8484";
        public const string Green = "#c2ff81";
        public const string Blue = "#8ed3ff";
        public const string Orange = "orange";

        public static string TextWithColor(string text, string color)
        {
            return $"<color={color}>{text}</color>";
        }
    }
}
