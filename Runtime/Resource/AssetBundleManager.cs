using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JJFramework.Runtime.Extension;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace JJFramework.Runtime
{
    public class AssetBundleManager : MonoSingleton<AssetBundleManager>
    {
        private AssetBundleManifest _assetBundleManifest;
        private readonly Dictionary<string, AssetBundle> _assetBundleList = new Dictionary<string, AssetBundle>(0);

        public int maximumAssetBundleCount { get; private set; }
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
//                using (var request = UnityWebRequestAssetBundle.GetAssetBundle(manifestPath, 0))
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

//                    var bundle = DownloadHandlerAssetBundle.GetContent(request);
                    _assetBundleManifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                }
            }

            var localAssetBundlePath = Path.Combine(Application.persistentDataPath, "bundles");
            var localAssetBundleFileNameList = Directory.GetFiles(localAssetBundlePath).ToList();
            var assetList = _assetBundleManifest.GetAllAssetBundles();
            var listCount = assetList.Length;
            
            // TODO(JJO): 쓰지 않는 AssetBundle 제거
            for (int assetIndex = 0; assetIndex < listCount; ++assetIndex)
            {
                var localAssetBundleFileNameListCount = localAssetBundleFileNameList.Count;
                for (int fileIndex = localAssetBundleFileNameListCount - 1; fileIndex >= 0; --fileIndex)
                {
                    if (localAssetBundleFileNameList[fileIndex].Contains(assetList[assetIndex]))
                    {
                        localAssetBundleFileNameList.Remove(assetList[assetIndex]);
                    }
                }
            }

            if (localAssetBundleFileNameList.Count > 0)
            {
                var localAssetBundleFileNameListCount = localAssetBundleFileNameList.Count;
                for (int fileIndex = localAssetBundleFileNameListCount - 1; fileIndex >= 0; --fileIndex)
                {
                    File.Delete(localAssetBundleFileNameList[fileIndex]);
                }
            }
            /////

            maximumAssetBundleCount = listCount;
            loadedAssetBundleCount = 0;
            currentDownloadAssetBundleProgress = 0f;

            for (var i = 0; i < listCount; ++i)
            {
                string path;
                if (url.EndsWith("/") == false)
                {
                    path = $"{url}/{assetList[i]}";
                }
                else
                {
                    path = $"{url}{assetList[i]}";
                }

                using (var inRequest = UnityWebRequestAssetBundle.GetAssetBundle(path))
                {
                    inRequest.SendWebRequest();
                    while (inRequest.isDone == false)
                    {
                        currentDownloadAssetBundleProgress = inRequest.downloadProgress;
                        yield return null;
                    }

                    var assetBundle = DownloadHandlerAssetBundle.GetContent(inRequest);
                    if (assetBundle == null)
                    {
                        continue;
                    }

                    _assetBundleList.Add(assetList[i], assetBundle);

                    ++loadedAssetBundleCount;
                }
            }
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
