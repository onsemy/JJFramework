using UnityEditor;
using UnityEngine;
using Debug = JJFramework.Runtime.Extension.Debug;

namespace JJFramework.Editor.Tool
{
    public class PathWindow : EditorWindow
    {
        [MenuItem("@JJFramework/PathWindow")]
        private static void Init()
        {
            EditorWindow.GetWindow<PathWindow>(false, "PathWindow");
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Application.dataPath"))
            {
                Debug.Log(Application.dataPath);
                EditorUtility.RevealInFinder(Application.dataPath);
            }
            if (GUILayout.Button("Application.persistentDataPath"))
            {
                Debug.Log(Application.persistentDataPath);
                EditorUtility.RevealInFinder(Application.persistentDataPath);
            }

            if (GUILayout.Button("Application.streamingAssetsPath"))
            {
                Debug.Log(Application.streamingAssetsPath);
                EditorUtility.RevealInFinder(Application.streamingAssetsPath);
            }

            if (GUILayout.Button("Application.temporaryCachePath"))
            {
                Debug.Log(Application.temporaryCachePath);
                EditorUtility.RevealInFinder(Application.temporaryCachePath);
            }
        }
    }
}
