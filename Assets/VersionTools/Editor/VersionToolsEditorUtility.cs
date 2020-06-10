using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

namespace BuildTools
{
    public static class VersionToolsEditorUtility
    {
        [MenuItem("Version Tools/Create Instances")]
        public static void CreateInstance()
        {
            GetOrCreateScriptableInstance<VersionToolOptions>();
            GetOrCreateScriptableInstance<VersionSettings>();
        }

        public static T GetOrCreateScriptableInstance<T>() where T : UnityEngine.ScriptableObject
        {
            T returningInstance = null;

            try
            {
                var guid = AssetDatabase.FindAssets($"t: {typeof(T).Name}").FirstOrDefault();
                var path = AssetDatabase.GUIDToAssetPath(guid);
                returningInstance = AssetDatabase.LoadAssetAtPath<T>(path);
            }
            catch (System.Exception e)
            {
                // report here?
            }

            // there was no instance, so we shall create one
            if (returningInstance == null)
            {
                T temporaryInstance = ScriptableObject.CreateInstance<T>();

                string directory = "Assets/VersionTools/Settings/";

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                AssetDatabase.CreateAsset(temporaryInstance, $"{directory}{typeof(T).Name}.asset");

                returningInstance = temporaryInstance;
            }

            return returningInstance;
        }
    }
}