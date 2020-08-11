using OWML.Common;

namespace NomaiVR
{
    public static class Logs
    {
        public static void Write(string message, MessageType messageType = MessageType.Message, bool debugOnly = true)
        {
            var isDebugMode = !debugOnly || (NomaiVR.Config == null || NomaiVR.Config.debugMode);
            if (NomaiVR.Helper != null && isDebugMode)
            {
                NomaiVR.Helper.Console.WriteLine(message, messageType);
            }
        }

        public static void WriteInfo(string message)
        {
            Write(message, MessageType.Info);
        }

        public static void WriteSuccess(string message)
        {
            Write(message, MessageType.Success);
        }

        public static void WriteWraning(string message)
        {
            Write(message, MessageType.Warning);
        }

        public static void WriteError(string message)
        {
            Write(message, MessageType.Error, false);
        }

        public static void WriteFatal(string message)
        {
            Write(message, MessageType.Fatal, false);
        }
    }
}
