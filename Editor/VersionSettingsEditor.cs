using System;
using System.Collections;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Networking;

namespace BuildTools
{
    [CustomEditor(typeof(VersionSettings))]
    public class VersionSettingsEditor : Editor, IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        private static string _endPoint = "http://staging.api.liminalvr.com/api/version/";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var settings = (VersionSettings)target;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Fetch"))
            {
                EditorCoroutineUtility.StartCoroutine(FetchVersion(settings), this);
            }

            if (GUILayout.Button("Set"))
            {
                EditorCoroutineUtility.StartCoroutine(UpdateVersion(settings), this);
            }
            GUILayout.EndHorizontal();
        }

        private static IEnumerator FetchVersion(VersionSettings settings)
        {
            var url = $"{_endPoint}bundleIdentifier/{PlayerSettings.applicationIdentifier}";
            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/json");

                yield return request.SendWebRequest();

                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.LogError("[RestService] Error: " + request.error);
                }
                else
                {
                    var body = request.downloadHandler.data;
                    var text = request.downloadHandler.text;
                    var model = JsonUtility.FromJson<VersionRequestModel>(text);
                    settings.Version.Major = model.major;
                    settings.Version.Minor = model.minor;
                    settings.Version.Revision = model.revision;
                }
            }
        }

        private static IEnumerator UpdateVersion(VersionSettings settings, bool fetch = false)
        {
            if (fetch)
            {
                yield return FetchVersion(settings);
                settings.Version.Revision++;
            }

            var model = new VersionRequestModel
            {
                bundleIdentifier = PlayerSettings.applicationIdentifier,
                major = settings.Version.Major,
                minor = settings.Version.Minor,
                revision = settings.Version.Revision,
            };

            var json = JsonUtility.ToJson(model, true);
            using (var request = UnityWebRequest.Put(_endPoint, json))
            {
                request.method = UnityWebRequest.kHttpVerbPOST;
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/json");

                yield return request.SendWebRequest();

                Debug.Log($"Updated Version: {settings.Version}");
            }
        }

        public int callbackOrder { get; }

        [InitializeOnLoadMethod]
        public static void OnProjectLoaded()
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(FetchVersion(VersionToolsEditorUtility.GetOrCreateScriptableInstance<VersionSettings>()));
            VersionToolsEditorUtility.GetOrCreateScriptableInstance<VersionToolOptions>();
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            var settings = VersionToolsEditorUtility.GetOrCreateScriptableInstance<VersionSettings>();

            var options = VersionToolsEditorUtility.GetOrCreateScriptableInstance<VersionToolOptions>();
            // if there's an instance of VersionToolOptions, and AppendVersionNumber is true
            if (options?.AppendVersionNumber ?? false)
            {
                try
                {
                    bool appended = false;

                    foreach (var file in report.files)
                    {
                        foreach (var fileEnding in options.FileEndings)
                        {
                            if (file.path.EndsWith($".{fileEnding}"))
                            {
                                EditorCoroutineUtility.StartCoroutineOwnerless(RenameBuildFile(settings.Version.ToString(), file.path, $".{fileEnding}"));
                                appended = true;
                                break;
                            }

                            if (appended) break;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }

            EditorCoroutineUtility.StartCoroutineOwnerless(UpdateVersion(settings, fetch: true));
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            var settings = VersionToolsEditorUtility.GetOrCreateScriptableInstance<VersionSettings>();
            EditorCoroutineUtility.StartCoroutineOwnerless(FetchVersion(settings));
        }

        private static IEnumerator RenameBuildFile(string settingsNumber, string path, string fileEnd, float delay = 1f)
        {
            yield return new WaitForSeconds(delay);

            string subPath = $"{path.Substring(0, path.Length - fileEnd.Length)}-{settingsNumber}{fileEnd}";
            System.IO.File.Move(path, subPath);
        }
    }

    [Serializable]
    public class VersionRequestModel
    {
        public string bundleIdentifier;
        public int major, minor, revision;
    }
}