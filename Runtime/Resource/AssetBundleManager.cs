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
        
        private AssetBundle _assetBundleManifestObject;
        private AssetBundleManifest _assetBundleManifest;
        private readonly Dictionary<string, AssetBundle> _assetBundleList = new Dictionary<string, AssetBundle>(0);
        
        public STATE state { get; private set; }

        public int maximumAssetBundleCount { get; private set; }
        public int downloadedAssetBundleCount { get; private set; }
        public int loadedAssetBundleCount { get; private set; }
        public float currentAssetBundleProgress { get; private set; }

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

        public IEnumerator PreloadAllAssetBundle(string url, string manifestName, string assetBundleDirectoryName)
        {
            state = STATE.INITIALIZING;

            if (_assetBundleManifestObject != null ||
                _assetBundleManifest != null)
            {
                _assetBundleManifest = null;
                _assetBundleManifestObject.Unload(true);
                _assetBundleManifestObject = null;
            }

            var manifestPath = url.EndsWith("/") == false ? $"{url}/{manifestName}" : $"{url}{manifestName}";

            using (var request = UnityWebRequest.Get(manifestPath))
            {
                yield return request.SendWebRequest();

                if (request.isNetworkError ||
                    request.isHttpError ||
                    string.IsNullOrEmpty(request.error) == false)
                {
                    Debug.LogError($"[PreloadAllAssetBundle] Network Error!\n{request.error}");
                    state = STATE.ERROR;
                    yield break;
                }

                _assetBundleManifestObject = AssetBundle.LoadFromMemory(request.downloadHandler.data, 0);
                _assetBundleManifest = _assetBundleManifestObject.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }

            state = STATE.PREPARING;

            var localAssetBundlePath = Path.Combine(Application.persistentDataPath, assetBundleDirectoryName);
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
                for (var assetIndex = 0; assetIndex < listCount; ++assetIndex)
                {
                    var localAssetBundleFileNameListCount = localAssetBundleFileNameList.Count;
                    for (var fileIndex = localAssetBundleFileNameListCount - 1; fileIndex >= 0; --fileIndex)
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
                for (var fileIndex = localAssetBundleFileNameListCount - 1; fileIndex >= 0; --fileIndex)
                {
                    File.Delete(localAssetBundleFileNameList[fileIndex]);
                }
            }
            
            maximumAssetBundleCount = listCount;
            downloadedAssetBundleCount = 0;
            loadedAssetBundleCount = 0;
            currentAssetBundleProgress = 0f;

            state = STATE.DOWNLOADING;
            
            // NOTE(JJO): 로컬에 AssetBundle Download
            for (var i = 0; i < listCount; ++i)
            {
                var downloadPath = url.EndsWith("/") == false ? $"{url}/{assetList[i]}" : $"{url}{assetList[i]}";

                using (var request = UnityWebRequest.Get(downloadPath))
                {
                    request.SendWebRequest();
                    while (request.isDone == false)
                    {
                        currentAssetBundleProgress = request.downloadProgress;
                        yield return null;
                    }

                    if (request.isNetworkError ||
                        request.isHttpError ||
                        string.IsNullOrEmpty(request.error) == false)
                    {
                        Debug.LogError($"[PreloadAllAssetBundle] Network Error!\n{request.error}");
                        state = STATE.ERROR;
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
                        state = STATE.ERROR;
                        continue;
                    }

                    ++downloadedAssetBundleCount;
                    Debug.Log($"[PreloadAllAssetBundle] Succeeded to download - {path}");
                }
            }

            state = STATE.LOADING;
            
            for (var i = 0; i < listCount; ++i)
            {
                yield return LoadAssetBundle(localAssetBundlePath, assetList[i]);
                
                // TODO(JJO): Dependency Load
                var dependencies = _assetBundleManifest.GetAllDependencies(assetList[i]);
                var dependencyCount = dependencies.Length;
                maximumAssetBundleCount += dependencyCount;
                for (var depIndex = 0; depIndex < dependencyCount; ++depIndex)
                {
                    yield return LoadAssetBundle(localAssetBundlePath, dependencies[depIndex]);
                }
            }

            state = STATE.LOADED;
        }

        private IEnumerator LoadAssetBundle(string localAssetBundlePath, string assetBundle)
        {
            if (_assetBundleList.ContainsKey(assetBundle))
            {
                Debug.LogWarning($"[PreloadAllAssetBundle] Already loaded asset - {assetBundle}");
                ++loadedAssetBundleCount;
                yield break;
            }
            
            var path = Path.Combine(localAssetBundlePath, assetBundle);
            using (var request = UnityWebRequestAssetBundle.GetAssetBundle(path, 0))
            {
                request.SendWebRequest();
                while (request.isDone == false)
                {
                    currentAssetBundleProgress = request.downloadProgress;
                    yield return null;
                }

                var bundle = DownloadHandlerAssetBundle.GetContent(request);
                if (bundle == null)
                {
                    Debug.LogError($"[PreloadAllAssetBundle] Failed to load - {assetBundle} is NULL\n{request.error}");
                    state = STATE.ERROR;
                    yield break;
                }
                
                _assetBundleList.Add(assetBundle, bundle);
                    
                ++loadedAssetBundleCount;
                Debug.Log($"[PreloadAllAssetBundle] Succeeded to load - {assetBundle}");
            }
        }

        public T LoadAsset<T>(string assetBundleName) where T : Object
        {
            return LoadAsset<T>(assetBundleName, assetBundleName);
        }

        public T LoadAsset<T>(string assetBundleName, string assetName) where T : Object
        {
            AssetBundle assetBundle;
            if (_assetBundleList.TryGetValue(assetBundleName, out assetBundle) == false)
            {
                Debug.LogError($"[LoadAssetBundle] Failed to load - {assetBundleName} is NOT EXIST!");
                return null;
            }
            
            var result = assetBundle.LoadAsset<T>(assetName);
            if (result != null)
            {
                return result;
            }

            Debug.LogError($"[LoadAssetBundle] Failed to load - {assetName} is NULL!");
            return null;
        }

        public IEnumerator LoadAssetAsync<T>(string assetBundleName, System.Action<T> outAction) where T : Object
        {
            yield return LoadAssetAsync<T>(assetBundleName, assetBundleName, outAction);
        }

        public IEnumerator LoadAssetAsync<T>(string assetBundleName, string assetName, System.Action<T> outAction)
            where T : Object
        {
            AssetBundle assetBundle;
            if (_assetBundleList.TryGetValue(assetBundleName, out assetBundle) == false)
            {
                Debug.LogError($"[LoadAssetBundle] Failed to load - {assetBundleName} is NOT EXIST!");
                yield break;
            }

            var request = assetBundle.LoadAssetAsync<T>(assetName);
            yield return request;

            if (request.asset != null)
            {
                outAction(request.asset as T);
            }
            else
            {
                Debug.LogError($"[LoadAssetBundle] Failed to load - {assetName} is NULL!");
            }
        }

        public async Task<T> LoadAssetBundleAsync<T>(string assetBundleName) where T : Object
        {
            var result = await LoadAssetBundleAsync<T>(assetBundleName, assetBundleName);
            return result;
        }

        public async Task<T> LoadAssetBundleAsync<T>(string assetBundleName, string assetName) where T : Object
        {
            AssetBundle assetBundle;
            if (_assetBundleList.TryGetValue(assetBundleName, out assetBundle) == false)
            {
                Debug.LogError($"[LoadAssetBundle] Failed to load - {assetBundleName} is NOT EXIST!");
                return null;
            }

            var request = assetBundle.LoadAssetAsync<T>(assetName);
            await request;

            return request.asset as T;
        }
    }
}
