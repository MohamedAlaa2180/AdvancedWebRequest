using UnityEditor;
using UnityEngine;
using AdvancedWebRequest.Core;

namespace AdvancedWebRequest.Editor
{
    [InitializeOnLoad]
    public static class AdvancedWebRequestSettings
    {
        static AdvancedWebRequestSettings()
        {
            ApiClientConfig.OnCreate = ApplyTo;
        }

        private const string KeyLogLevel        = "AdvancedWebRequest.LogLevel";
        private const string KeyLogRequestBody  = "AdvancedWebRequest.LogRequestBody";
        private const string KeyLogResponseBody = "AdvancedWebRequest.LogResponseBody";
        private const string KeyMaxBodyLength   = "AdvancedWebRequest.MaxLogBodyLength";
        private const string KeyDefaultTimeout  = "AdvancedWebRequest.DefaultTimeout";

        public static LogLevel LogLevel
        {
            get => (LogLevel)EditorPrefs.GetInt(KeyLogLevel, (int)LogLevel.Basic);
            set => EditorPrefs.SetInt(KeyLogLevel, (int)value);
        }

        public static bool LogRequestBody
        {
            get => EditorPrefs.GetBool(KeyLogRequestBody, false);
            set => EditorPrefs.SetBool(KeyLogRequestBody, value);
        }

        public static bool LogResponseBody
        {
            get => EditorPrefs.GetBool(KeyLogResponseBody, false);
            set => EditorPrefs.SetBool(KeyLogResponseBody, value);
        }

        public static int MaxLogBodyLength
        {
            get => EditorPrefs.GetInt(KeyMaxBodyLength, 500);
            set => EditorPrefs.SetInt(KeyMaxBodyLength, value);
        }

        public static float DefaultTimeout
        {
            get => EditorPrefs.GetFloat(KeyDefaultTimeout, 30f);
            set => EditorPrefs.SetFloat(KeyDefaultTimeout, value);
        }

        public static void ApplyTo(ApiClientConfig config)
        {
            config.LogLevel         = LogLevel;
            config.LogRequestBody   = LogRequestBody;
            config.LogResponseBody  = LogResponseBody;
            config.MaxLogBodyLength = MaxLogBodyLength;
            config.DefaultRequestOptions = new RequestOptions { TimeoutSeconds = DefaultTimeout };
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider("Project/Advanced Web Request", SettingsScope.Project)
            {
                label = "Advanced Web Request",
                guiHandler = _ =>
                {
                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField("Logging", EditorStyles.boldLabel);

                    EditorGUI.BeginChangeCheck();

                    var logLevel   = (LogLevel)EditorGUILayout.EnumPopup("Log Level", LogLevel);
                    var reqBody    = EditorGUILayout.Toggle("Log Request Body", LogRequestBody);
                    var resBody    = EditorGUILayout.Toggle("Log Response Body", LogResponseBody);
                    var bodyLen    = EditorGUILayout.IntField("Max Log Body Length", MaxLogBodyLength);

                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Requests", EditorStyles.boldLabel);

                    var timeout = EditorGUILayout.FloatField("Default Timeout (s)", DefaultTimeout);

                    if (EditorGUI.EndChangeCheck())
                    {
                        LogLevel         = logLevel;
                        LogRequestBody   = reqBody;
                        LogResponseBody  = resBody;
                        MaxLogBodyLength = bodyLen;
                        DefaultTimeout   = timeout;
                    }

                    EditorGUILayout.Space(10);
                    EditorGUILayout.HelpBox(
                        "These defaults are applied when ApiClientConfig.Create() is called. Individual clients can still override them.",
                        MessageType.Info);
                },
                keywords = new[] { "API", "Web", "Request", "Log", "Timeout", "HTTP" }
            };
        }

        [MenuItem("Advanced Web Request/Settings")]
        private static void OpenSettings()
        {
            SettingsService.OpenProjectSettings("Project/Advanced Web Request");
        }
    }
}
