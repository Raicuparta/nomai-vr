namespace NomaiVR
{
    public enum MessageType
    {
        Message,
        Info,
        Success,
        Warning,
        Error,
        Fatal,
    }

    public static class Logs
    {
        private const string fatalMessageSufix =
            "\n\nIf you want to ignore this error and start the game anyway, edit NomaiVR/config.json, find \"bypassFatalErrors\", and set \"value\" to true. " +
            "But be aware that this will likely face a lot of problems.";

        public static void Write(string message, MessageType messageType = MessageType.Message, bool debugOnly = true)
        {
            if (debugOnly && !ModSettings.DebugMode) return;
            switch (messageType)
            {
                case MessageType.Error:
                case MessageType.Fatal:
                    UnityEngine.Debug.LogError(message);
                    break;

                case MessageType.Warning:
                    UnityEngine.Debug.LogWarning(message);
                    break;

                default:
                    UnityEngine.Debug.Log(message);
                    break;
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

        public static void WriteWarning(string message)
        {
            Write(message, MessageType.Warning);
        }

        public static void WriteError(string message)
        {
            Write(message, MessageType.Error, false);
        }

        public static void WriteFatal(string message)
        {
            if (ModSettings.BypassFatalErrors)
            {
                WriteError(message);
            }
            else
            {
                Write(message + fatalMessageSufix, MessageType.Fatal, false);
            }
        }
    }
}
