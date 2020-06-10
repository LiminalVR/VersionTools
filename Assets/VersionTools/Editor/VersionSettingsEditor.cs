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
        private string _endPoint = "http://staging.api.liminalvr.com/api/version/";

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

        private IEnumerator FetchVersion(VersionSettings settings)
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

        private IEnumerator UpdateVersion(VersionSettings settings, bool fetch = false)
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

        public void OnPostprocessBuild(BuildReport report)
        {
            var settings = Resources.FindObjectsOfTypeAll<VersionSettings>().FirstOrDefault();
            EditorCoroutineUtility.StartCoroutineOwnerless(UpdateVersion(settings, fetch: true));
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            var settings = Resources.FindObjectsOfTypeAll<VersionSettings>().FirstOrDefault();
            EditorCoroutineUtility.StartCoroutineOwnerless(FetchVersion(settings));
        }
    }

    [Serializable]
    public class VersionRequestModel
    {
        public string bundleIdentifier;
        public int major, minor, revision;
    }
}