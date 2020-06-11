﻿using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UniRx;
using UnityEditor;

namespace JJFramework.Editor.Batch
{
    [DisallowMultipleComponent]
    public class EditorBatchMethods : MonoBehaviour
    {

        [MenuItem("@JJFramework/ResetPlayerPrefData")]
        public static void ResetPlayerPrefData()
        {
            UnityEngine.Debug.Log("reset playerpref data");
            UnityEngine.PlayerPrefs.DeleteAll();
        }

        [MenuItem("@JJFramework/Build Android")]
        public static void BuildAndroid()
        {
            var scenes = EditorBuildSettings.scenes;
            List<string> sceneList = new List<string>();

            foreach (var scene in scenes)
            {
                if (scene.enabled)
                    sceneList.Add(scene.path);
            }

            var sceneArray = sceneList.ToArray();
            BuildPipeline.BuildPlayer(sceneArray, "UnityAndroid.apk", BuildTarget.Android, BuildOptions.None);
        }

        [MenuItem("@JJFramework/Build Android AssetBundles")]
        public static void BuildAndroidAssetBundles()
        {
            var path = System.IO.Path.Combine(Application.dataPath, "..", "AssetBundles", "Android");
            if (System.IO.Directory.Exists(path) == false)
            {
                System.IO.Directory.CreateDirectory(path);
            }
            
            var manifest = BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, BuildTarget.Android);
            UnityEngine.Debug.Log(manifest.GetAllAssetBundles());

            var args = System.Environment.GetCommandLineArgs();
            if (args.Contains("-noRevealInFinder") == false)
            {
                EditorUtility.DisplayDialog("Info",
                    $"Completed Android AssetBundles!\nCount: {manifest.GetAllAssetBundles().Length}", "OK");

                EditorUtility.RevealInFinder(path);
            }
        }

        [MenuItem("@JJFramework/Build LocalDB")]
        public static async void BuildLocalDB()
        {
            await Task.Delay(0);
            /* FIXME(jjo): 리펙토링 예정
            SetDefineSymbol("__USE_LOCALDB");

            bool isDone = false;

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"..\tools\data_exporter\PyExcelToSqlite.py";
            startInfo.Arguments = @"-e ..\excel\table.xlsx -o Assets\Resources\master.db -p Assets\Scripts\Core\DB -t ..\tools\data_exporter\template\class.mustache -c ..\tools\data_exporter\template\master_db.mustache";
            Process buildProcess = Process.Start(startInfo);
            buildProcess.EnableRaisingEvents = true;
            buildProcess.Exited += (sender, args) =>
            {
                //string sourcePath = @"..\tools\data_exporter\__LOCAL_BUILD\";
                //string dbPath = @"..\unity_project\Assets\Script\Core\DB\";
                //string masterDbPath = @"..\unity_project\Assets\Resources\";

                //string dbDllFile = "AutoGenerated.DB.dll";
                //string attrDllFile = "Sqlite4Unity3d.Attribute.dll";
                //string masterDbCachedFile = "MasterDB_Cached.cs";
                //string masterDbFile = "master.db.bytes";

                ////copy db dll file
                //System.IO.File.Copy($"{sourcePath}{dbDllFile}", $"{dbPath}{dbDllFile}", true);

                ////copy attr dll file
                //System.IO.File.Copy($"{sourcePath}{attrDllFile}", $"{dbPath}{attrDllFile}", true);

                ////copy master db cache file
                //System.IO.File.Copy($"{sourcePath}{masterDbCachedFile}", $"{dbPath}{masterDbCachedFile}", true);

                ////copy master db file
                //System.IO.File.Copy($"{sourcePath}{masterDbFile}", $"{masterDbPath}{masterDbFile}", true);

                UnityEngine.Debug.Log("complete localdb build");
                isDone = true;
            };

            while (!buildProcess.HasExited)
            {
                await Task.Delay(0);
            }

            UnityEngine.Debug.Log("request refresh asset database...");
            AssetDatabase.Refresh();
            */
        }

        [MenuItem("@JJFramework/Clear LocalDB Symbol")]
        public static void ClearLocalDBSymbol()
        {
            // FIXME(jjo): 리펙토링 예정
            //RemoveDefineSymbol("__USE_LOCALDB");
        }

        public static void RemoveDefineSymbol(string symbol)
        {
            string defineSymbol = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            defineSymbol = defineSymbol.Replace($";{symbol}", "");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defineSymbol);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, defineSymbol);
            UnityEngine.Debug.Log(defineSymbol);
        }

        public static void SetDefineSymbol(string _symbol)
        {
            string defineSymbol = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);

            if (defineSymbol.Contains(_symbol) == false)
            {
                defineSymbol += ";" + _symbol;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defineSymbol);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, defineSymbol);
                UnityEngine.Debug.Log(defineSymbol);
            }
        }
    }
}
