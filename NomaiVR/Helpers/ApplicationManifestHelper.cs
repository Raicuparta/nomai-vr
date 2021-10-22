using System;
using System.IO;
using System.Reflection;
using Valve.Newtonsoft.Json;
using Valve.VR;

namespace NomaiVR.Helpers
{
    public static class ApplicationManifestHelper
    {
        public static void UpdateManifest(string manifestPath, string appKey, string imagePath, string name, string description, int steamAppId = 0, bool steamBuild = false)
        {
            var launchType = steamBuild ? GetSteamLaunchString(steamAppId) : GetBinaryLaunchString();
            var appManifestContent = $@"{{
                                            ""source"": ""builtin"",
                                            ""applications"": [{{
                                                ""app_key"": {JsonConvert.ToString(appKey)},
                                                ""image_path"": { JsonConvert.ToString(imagePath) },
                                                {launchType}
                                                ""last_played_time"":""{CurrentUnixTimestamp()}"",
                                                ""strings"": {{
                                                    ""en_us"": {{
                                                        ""name"": { JsonConvert.ToString(name) }
                                                    }}
                                                }}
                                            }}]
                                        }}";

            File.WriteAllText(manifestPath, appManifestContent);

            var error = OpenVR.Applications.AddApplicationManifest(manifestPath, false);
            if (error != EVRApplicationError.None)
            {
                Logs.WriteError("Failed to set AppManifest " + error);
            }

            int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
            EVRApplicationError applicationIdentifyErr = OpenVR.Applications.IdentifyApplication((uint)processId, appKey);
            if (applicationIdentifyErr != EVRApplicationError.None)
            {
                Logs.WriteError("Error identifying application: " + applicationIdentifyErr.ToString());
            }
        }

        private static string GetSteamLaunchString(int steamAppId)
        {
            return $@"""launch_type"": ""url"",
                      ""url"": ""steam://launch/{steamAppId}/VR"",";
        }

        private static string GetBinaryLaunchString()
        {
            string workingDir = Directory.GetCurrentDirectory();
            string executablePath = Assembly.GetExecutingAssembly().Location;
            return $@"""launch_type"": ""binary"",
                      ""binary_path_windows"": {JsonConvert.ToString(executablePath)},
                      ""working_directory"": {JsonConvert.ToString(workingDir)},";
        }

        private static long CurrentUnixTimestamp()
        {
            DateTime foo = DateTime.Now;
            return ((DateTimeOffset)foo).ToUnixTimeSeconds();
        }
    }
}
