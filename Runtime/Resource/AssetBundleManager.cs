using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JJFramework.Runtime.Extension;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace JJFramework.Runtime
{
    public class AssetBundleManager : MonoSingleton<AssetBundleManager>
    {
        public enum STATE
        {
            INITIALIZING,
            PREPARING,
            DOWNLOADING,
            LOADING,
            LOADED,
            
            ERROR
        }
        
        private AssetBundleManifest _assetBundleManifest;
        private readonly Dictionary<string, AssetBundle> _assetBundleList = new Dictionary<string, AssetBundle>(0);
        
        public STATE state { get; private set; }

        public int maximumAssetBundleCount { get; private set; }
        public int downloadedAssetBundleCount { get; private set; }
        public int loadedAssetBundleCount { get; private set; }
        public float currentDownloadAssetBundleProgress { get; private set; }

        public void UnloadAssetBundle(string assetBundleName, bool unloadAll = true)
        {
            AssetBundle assetBundle;
            if (!_assetBundleList.TryGetValue(assetBundleName, out assetBundle))
            {
                Debug.LogError($"[UnloadAssetBundle] Failed to unload - {assetBundleName} is NOT FOUND!");
                return;
            }

            assetBundle.Unload(unloadAll);
            _assetBundleList.Remove(assetBundleName);
        }

        public void UnloadAllAssetBundle(bool unloadManifest, bool unloadAll = true)
        {
            foreach (var assetBundle in _assetBundleList)
            {
                assetBundle.Value.Unload(unloadAll);
            }

            _assetBundleList.Clear();
            
            if (unloadManifest)
            {
                AssetBundle.UnloadAllAssetBundles(true);
            }
        }

        public IEnumerator PreloadAllAssetBundle(string url, string manifestName)
        {
            state = STATE.INITIALIZING;
            
            if (_assetBundleManifest == null)
            {
                string manifestPath;
                if (url.EndsWith("/") == false)
                {
                    manifestPath = $"{url}/{manifestName}";
                }
                else
                {
                    manifestPath = $"{url}{manifestName}";
                }

                using (var request = UnityWebRequest.Get(manifestPath))
                {
                    yield return request.SendWebRequest();

                    if (request.isNetworkError ||
                        request.isHttpError ||
                        string.IsNullOrEmpty(request.error) == false)
                    {
                        Debug.LogError($"[PreloadAllAssetBundle] Network Error!\n{request.error}");
                        yield break;
                    }

                    var bundle = AssetBundle.LoadFromMemory(request.downloadHandler.data, 0);

                    _assetBundleManifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                }
            }

            state = STATE.PREPARING;

            var localAssetBundlePath = Path.Combine(Application.persistentDataPath, "bundles");
            if (Directory.Exists(localAssetBundlePath) == false)
            {
                Directory.CreateDirectory(localAssetBundlePath);
            }
            var localAssetBundleFileNameList = Directory.GetFiles(localAssetBundlePath).ToList();
            var assetList = _assetBundleManifest.GetAllAssetBundles();
            var listCount = assetList.Length;
            
            // NOTE(JJO): 쓰지 않는 AssetBundle 제거
            if (localAssetBundleFileNameList.Count > 0)
            {
                for (int assetIndex = 0; assetIndex < listCount; ++assetIndex)
                {
                    var localAssetBundleFileNameListCount = localAssetBundleFileNameList.Count;
                    for (int fileIndex = localAssetBundleFileNameListCount - 1; fileIndex >= 0; --fileIndex)
                    {
                        var fileName = Path.GetFileName(localAssetBundleFileNameList[fileIndex]);
                        if (fileName == assetList[assetIndex])
                        {
                            localAssetBundleFileNameList.RemoveAt(fileIndex);
                        }
                    }
                }
            }

            if (localAssetBundleFileNameList.Count > 0)
            {
                var localAssetBundleFileNameListCount = localAssetBundleFileNameList.Count;
                for (int fileIndex = localAssetBundleFileNameListCount - 1; fileIndex >= 0; --fileIndex)
                {
                    var path = Path.Combine(Application.persistentDataPath, localAssetBundleFileNameList[fileIndex]);
                    File.Delete(localAssetBundleFileNameList[fileIndex]);
                }
            }
            
            maximumAssetBundleCount = listCount;
            downloadedAssetBundleCount = 0;
            loadedAssetBundleCount = 0;
            currentDownloadAssetBundleProgress = 0f;

            state = STATE.DOWNLOADING;
            
            // NOTE(JJO): 로컬에 AssetBundle Download
            for (var i = 0; i < listCount; ++i)
            {
                string downloadPath;
                if (url.EndsWith("/") == false)
                {
                    downloadPath = $"{url}/{assetList[i]}";
                }
                else
                {
                    downloadPath = $"{url}{assetList[i]}";
                }

                using (var request = UnityWebRequest.Get(downloadPath))
                {
                    request.SendWebRequest();
                    while (request.isDone == false)
                    {
                        currentDownloadAssetBundleProgress = request.downloadProgress;
                        yield return null;
                    }

                    if (request.isNetworkError ||
                        request.isHttpError ||
                        string.IsNullOrEmpty(request.error) == false)
                    {
                        Debug.LogError($"[PreloadAllAssetBundle] Network Error!\n{request.error}");
                        yield break;
                    }

                    var path = Path.Combine(localAssetBundlePath, assetList[i]);
                    try
                    {
                        File.WriteAllBytes(path, request.downloadHandler.data);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[PreloadAllAssetBundle] Failed to write - {assetList[i]}\n{e.Message}\n{e.StackTrace}");
                        continue;
                    }

                    ++downloadedAssetBundleCount;
                    Debug.Log($"[PreloadAllAssetBundle] Succeeded to download - {path}");
                }
            }

            state = STATE.LOADING;
            
            for (var i = 0; i < listCount; ++i)
            {
                // TODO(JJO): Dependency Load
                //
                
                var path = Path.Combine(localAssetBundlePath, assetList[i]);
                using (var request = UnityWebRequestAssetBundle.GetAssetBundle(path, 0))
                {
                    yield return request.SendWebRequest();

                    var bundle = DownloadHandlerAssetBundle.GetContent(request);
                    if (bundle == null)
                    {
                        Debug.LogError($"[PreloadAllAssetBundle] Failed to load - {assetList[i]} is NULL\n{request.error}");
                        continue;
                    }
                
                    _assetBundleList.Add(assetList[i], bundle);
                    
                    ++loadedAssetBundleCount;
                    Debug.Log($"[PreloadAllAssetBundle] Succeeded to load - {assetList[i]}");
                }
            }

            state = STATE.LOADED;
        }

        public T LoadAssetBundle<T>(string assetBundleName) where T : Object
        {
            AssetBundle assetBundle;
            if (_assetBundleList.TryGetValue(assetBundleName, out assetBundle) == false)
            {
                Debug.LogError($"[LoadAssetBundle] Failed to load - {assetBundleName} is NOT EXIST!");
                return null;
            }

            var result = assetBundle.LoadAsset<T>(assetBundleName);
            if (result != null)
            {
                return result;
            }

            Debug.LogError($"[LoadAssetBundle] Failed to load - {assetBundleName} is NULL!");
            return null;
        }

        public async Task<T> LoadAssetBundleAsync<T>(string assetBundleName) where T : Object
        {
            AssetBundle assetBundle;
            if (_assetBundleList.TryGetValue(assetBundleName, out assetBundle) == false)
            {
                Debug.LogError($"[LoadAssetBundle] Failed to load - {assetBundleName} is NOT EXIST!");
                return null;
            }

            var request = assetBundle.LoadAssetAsync<T>(assetBundleName);
            await request;

            return request.asset as T;
        }
    }
}
