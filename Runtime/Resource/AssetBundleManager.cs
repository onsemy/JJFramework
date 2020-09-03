using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace JJFramework.Runtime
{
    public class AssetBundleManager
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
        private readonly Dictionary<string, uint> _downloadingAssetBundleList = new Dictionary<string, uint>(0);

        private static readonly string MANIFEST_NAME = "AssetBundleManifest";
        private static readonly string CONTENT_LENGTH = "Content-Length";
        
        public string downloadUrl { get; private set; }
        public string localAssetBundlePath { get; private set; }

        public STATE state { get; private set; }

        public int maximumAssetBundleCount { get; private set; }
        public int downloadedAssetBundleCount { get; private set; }
        public int loadedAssetBundleCount { get; private set; }
        public float currentAssetBundleProgress { get; private set; }
        public ulong currentAssetBundleSize { get; private set; }
        public ulong assetBundleTotalSize { get; private set; }

        private int _skippedAssetBundleCount;

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
                if (_assetBundleManifestObject)
                {
                    _assetBundleManifestObject.Unload(true);
                    _assetBundleManifestObject = null;
                }

                if (_assetBundleManifest)
                {
                    _assetBundleManifest = null;
                }
            }
        }

        private uint GetCRC(string manifestRaw)
        {
            if (string.IsNullOrEmpty(manifestRaw))
            {
                return 0U;
            }
            
            var filteredManifest = manifestRaw.Split('\n');
            var crcRaw = filteredManifest[1];
            var index = crcRaw.IndexOf(':') + 1;
            crcRaw = crcRaw.Substring(index, crcRaw.Length - index);

            uint result;
            uint.TryParse(crcRaw, out result);

            return result;
        }

        public void Cleanup()
        {
            UnloadAllAssetBundle(true);
            
            _assetBundleList.Clear();
            _assetBundleManifest = null;
            _assetBundleManifestObject = null;
        }

        private IEnumerator DownloadAssetBundle(string assetBundleName, uint crc)
        {
            var path = $"{downloadUrl}{assetBundleName}";
            var hash = _assetBundleManifest.GetAssetBundleHash(assetBundleName);
            using (var request = UnityWebRequestAssetBundle.GetAssetBundle(path, hash, crc))
            {
                request.SendWebRequest();
                while (request.isDone == false)
                {
                    currentAssetBundleProgress = request.downloadProgress;
                    currentAssetBundleSize = request.downloadedBytes;
                    yield return null;
                }

                if (string.IsNullOrEmpty(request.error) == false)
                {
                    Debug.LogError($"[DownloadAssetBundle] Network Error! - {assetBundleName} / {crc}\n{request.error}");
                    state = STATE.ERROR;
                    yield break;
                }
                
                var bundle = DownloadHandlerAssetBundle.GetContent(request);
                _assetBundleList.Add(assetBundleName, bundle);
                
                ++downloadedAssetBundleCount;
                Debug.Log($"[DownloadAssetBundle] Succeeded to download - {assetBundleName} / {crc}");
            }
        }

        public IEnumerator PrepareDownload(string url, string manifestName)
        {
            // NOTE(JJO): url이 비어있다면 EDITOR모드임을 가정함.
            if (string.IsNullOrEmpty(url))
            {
                state = STATE.LOADED;
                yield break;
            }
            
            state = STATE.PREPARING;

            downloadUrl = url;
            
            // TODO(JJO): StreamingAssets에서 로드한 흔적을 찾는다.
            string manifestSA = null;
            string manifestRemote = null;
            
            var pathSA = Path.Combine(Application.streamingAssetsPath, "Bundle");
            var path = Path.Combine(pathSA, $"{manifestName}.manifest");
            using (var request = UnityWebRequest.Get(path))
            {
                yield return request.SendWebRequest();

                if (string.IsNullOrEmpty(request.error) == false)
                {
                    Debug.LogWarning($"[AssetBundleManager|PrepareDownload] {manifestName} is NOT FOUND from StreamingAssets");
                }
                else
                {
                    manifestSA = request.downloadHandler.text;
                }
            }

            path = $"{downloadUrl}{manifestName}.manifest";
            using (var request = UnityWebRequest.Get(path))
            {
                yield return request.SendWebRequest();

                if (string.IsNullOrEmpty(request.error) == false)
                {
                    Debug.LogError($"[AssetBundleManager|PrepareDownload] Failed to download - {path}\nReason: {request.error}");
                    state = STATE.ERROR;
                    yield break;
                }

                manifestRemote = request.downloadHandler.text;
            }

            if (string.IsNullOrEmpty(manifestSA) == false &&
                manifestSA == manifestRemote)
            {
                // NOTE(JJO): 그냥 여기에서 바로 S.A로부터 로드해버리자.
                path = Path.Combine(pathSA, manifestName);
                var request = AssetBundle.LoadFromFileAsync(path);

                yield return request;
            
                _assetBundleManifestObject = request.assetBundle;
                if (_assetBundleManifestObject == null)
                {
                    Debug.LogError($"[AssetBundleManager|PrepareDownload] Failed to load - {path}\nReason: Unknown");
                    state = STATE.ERROR;
                    yield break;
                }

                _assetBundleManifest = _assetBundleManifestObject.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));

                var assetList = _assetBundleManifest.GetAllAssetBundles();
                var listCount = assetList.Length;
                for (int i = 0; i < listCount; ++i)
                {
                    path = Path.Combine(pathSA, assetList[i]);
                    request = AssetBundle.LoadFromFileAsync(path);

                    yield return request;

                    var bundle = request.assetBundle;
                    if (bundle == null)
                    {
                        Debug.LogError($"[AssetBundleManager|PrepareDownload] Failed to load - {path}\nReason: Unknown");
                        state = STATE.ERROR;
                        yield break;
                    }
                    
                    _assetBundleList.Add(assetList[i], bundle);
                    Debug.Log($"[AssetBundleManager|PrepareDownload] Preload from StreamingAssets - {assetList[i]}");
                }
            }
            else
            {
                path = $"{downloadUrl}{manifestName}";
                using (var req = UnityWebRequestAssetBundle.GetAssetBundle(path))
                {
                    yield return req.SendWebRequest();

                    if (string.IsNullOrEmpty(req.error) == false)
                    {
                        Debug.LogError($"[AssetBundleManager|PrepareDownload] Failed to download - {path}\nReason: Unknown");
                        state = STATE.ERROR;
                        yield break;
                    }

                    _assetBundleManifestObject = DownloadHandlerAssetBundle.GetContent(req);
                    _assetBundleManifest = _assetBundleManifestObject.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));
                }

                var assetList = _assetBundleManifest.GetAllAssetBundles();
                var listCount = assetList.Length; 
                for (int i = 0; i < listCount; ++i)
                {
                    path = Path.Combine(pathSA, $"{assetList[i]}.manifest");
                    using (var req = UnityWebRequest.Get(path))
                    {
                        yield return req.SendWebRequest();

                        // NOTE(JJO): StreamingAssets에 파일이 없다면 바로 다운로드 리스트에 추가함.
                        if (string.IsNullOrEmpty(req.error))
                        {
                            manifestSA = req.downloadHandler.text;
                        }
                    }

                    path = $"{downloadUrl}{assetList[i]}.manifest";
                    using (var req = UnityWebRequest.Get(path))
                    {
                        yield return req.SendWebRequest();

                        if (string.IsNullOrEmpty(req.error) == false)
                        {
                            Debug.LogError($"[AssetBundleManager|PrepareDownload] Network Error - {assetList[i]}\n{req.error}");
                            state = STATE.ERROR;
                            yield break;
                        }

                        manifestRemote = req.downloadHandler.text;
                    }

                    // NOTE(JJO): Manifest가 다르다면 다운로드 리스트에 추가함.
                    if (string.IsNullOrEmpty(manifestSA) ||
                        manifestSA != manifestRemote)
                    {
                        _downloadingAssetBundleList.Add(assetList[i], GetCRC(manifestRemote));
                    }
                    else
                    {
                        path = Path.Combine(pathSA, assetList[i]);
                        var request = AssetBundle.LoadFromFileAsync(path);

                        yield return request;

                        var bundle = request.assetBundle;
                        if (bundle == null)
                        {
                            Debug.LogError($"[AssetBundleManager|PrepareDownload] {assetList[i]} NOT FOUND!");
                            state = STATE.ERROR;
                            yield break;
                        }
                        
                        _assetBundleList.Add(assetList[i], bundle);
                        Debug.Log($"[AssetBundleManager|PrepareDownload] Preload from StreamingAssets - {assetList[i]}");
                    }
                }
                
                maximumAssetBundleCount = _downloadingAssetBundleList.Count;
            }
        }

        public IEnumerator PrepareDownload(string url, string manifestName, string assetBundleDirectoryName)
        {
            state = STATE.PREPARING;

            downloadUrl = url;
            localAssetBundlePath = Path.Combine(Application.persistentDataPath, assetBundleDirectoryName);
            if (Directory.Exists(localAssetBundlePath) == false)
            {
                Directory.CreateDirectory(localAssetBundlePath);
            }
            
            var localManifestRawPath = Path.Combine(localAssetBundlePath, $"{manifestName}.manifest");
            var localManifestPath = Path.Combine(localAssetBundlePath, manifestName);
            string localManifestRaw = string.Empty;
            string remoteManifestRaw = string.Empty;
            
            // NOTE(JJO): 로컬에 저장된 데이터가 없다면 StreamingAssets를 검사하여 모든 StreamingAssets의 AssetBundle을 가져옴
            if (File.Exists(localManifestRawPath) == false)
            {
                var streamingAssetsPath = Path.Combine(Application.streamingAssetsPath, "Bundle");
                if (Directory.Exists(streamingAssetsPath))
                {
                    var fileList = Directory.GetFiles(streamingAssetsPath);
                    var fileListCount = fileList.Length;
                    for (int i = 0; i < fileListCount; ++i)
                    {
                        if (fileList[i].Contains(".meta"))
                        {
                            continue;
                        }
                        
                        var fileName = Path.GetFileName(fileList[i]);
                        var destinationPath = Path.Combine(localAssetBundlePath, fileName);
                        File.Copy(fileList[i], destinationPath);
                        Debug.Log($"[PrepareDownload] Copy {fileList[i]} to {destinationPath}");
                    }
                }
            }

            // NOTE(JJO): 다시 로컬에 있는지 검사하여 가져옴
            if (File.Exists(localManifestRawPath))
            {
                localManifestRaw = File.ReadAllText(localManifestRawPath);
            }

            var remoteManifestRawPath = downloadUrl.EndsWith("/") == false ? $"{downloadUrl}/{manifestName}.manifest" : $"{downloadUrl}{manifestName}.manifest";
            using (var request = UnityWebRequest.Get(remoteManifestRawPath))
            {
                yield return request.SendWebRequest();

                if (string.IsNullOrEmpty(request.error) == false)
                {
                    Debug.LogError($"[PrepareDownload] Failed to load a manifest from {remoteManifestRawPath}\n{request.error}");
                    state = STATE.ERROR;
                    yield break;
                }

                remoteManifestRaw = request.downloadHandler.text;
            }
            
            if (string.IsNullOrEmpty(localManifestRaw) ||
                localManifestRaw != remoteManifestRaw)
            {
                // NOTE(JJO): 먼저 AssetBundleManifest를 받아서 정보를 가져옴.
                var remoteManifestPath = downloadUrl.EndsWith("/") == false ? $"{downloadUrl}/{manifestName}" : $"{downloadUrl}{manifestName}";

                using (var request = UnityWebRequest.Get(remoteManifestPath))
                {
                    yield return request.SendWebRequest();

                    if (request.isNetworkError ||
                        request.isHttpError ||
                        string.IsNullOrEmpty(request.error) == false)
                    {
                        Debug.LogError($"[PrepareDownload] Failed to load a manifest from {remoteManifestPath}\n{request.error}");
                        state = STATE.ERROR;
                        yield break;
                    }

                    _assetBundleManifestObject = AssetBundle.LoadFromMemory(request.downloadHandler.data, 0);
                    
                    File.WriteAllBytes(localManifestPath, request.downloadHandler.data);
                    
                    // NOTE(JJO): Manifest 쓰기가 완료되고 나서 txt도 쓴다.
                    File.WriteAllText(localManifestRawPath, remoteManifestRaw);
                }
            }
            else
            {
                _assetBundleManifestObject = AssetBundle.LoadFromFile(localManifestPath);
            }
            
            _assetBundleManifest = _assetBundleManifestObject.LoadAsset<AssetBundleManifest>(MANIFEST_NAME);

            maximumAssetBundleCount = _assetBundleManifest.GetAllAssetBundles().Length;

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
                        if (fileName == manifestName ||
                            fileName == $"{manifestName}.manifest" ||
                            fileName == assetList[assetIndex] ||
                            fileName == $"{assetList[assetIndex]}.manifest")
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

            // NOTE(JJO): 받아야하는 에셋의 크기를 계산한다!
            assetBundleTotalSize = 0;
            for (int i = 0; i < listCount; ++i)
            {
                var downloadPath = downloadUrl.EndsWith("/") == false ? $"{downloadUrl}/{assetList[i]}" : $"{downloadUrl}{assetList[i]}";
                remoteManifestRawPath = $"{downloadPath}.manifest";
                remoteManifestRaw = string.Empty;
                localManifestRawPath = Path.Combine(localAssetBundlePath, $"{assetList[i]}.manifest");
                if (File.Exists(localManifestRawPath))
                {
                    localManifestRaw = File.ReadAllText(localManifestRawPath);
                }
                else
                {
                    localManifestRaw = string.Empty;
                }

                using (var request = UnityWebRequest.Get(remoteManifestRawPath))
                {
                    yield return request.SendWebRequest();

                    if (string.IsNullOrEmpty(request.error) == false)
                    {
                        Debug.LogError($"[PrepareDownload] Failed to download a {assetList[i]}.manifest from {remoteManifestRawPath}!\n{request.error}");
                        state = STATE.ERROR;
                        yield break;
                    }

                    remoteManifestRaw = request.downloadHandler.text;
                }

                if (string.IsNullOrEmpty(localManifestRaw) ||
                    localManifestRaw != remoteManifestRaw)
                {
                    // NOTE(JJO): 받아야 하는 에셋의 크기를 구한다.
                    using (var request = UnityWebRequest.Head(downloadPath))
                    {
                        yield return request.SendWebRequest();

                        if (request.isNetworkError ||
                            request.isHttpError)
                        {
                            Debug.LogError($"[PrepareDownload] Failed to get size of a {assetList[i]} from {downloadPath}!\n{request.error}");
                            state = STATE.ERROR;
                            yield break;
                        }

                        ulong assetBundleSize;
                        ulong.TryParse(request.GetResponseHeader(CONTENT_LENGTH), out assetBundleSize);
                        assetBundleTotalSize += assetBundleSize;
                        
                        _downloadingAssetBundleList.Add(assetList[i], GetCRC(remoteManifestRaw));
                    }
                }
            }
        }

        public IEnumerator DownloadAllAssetBundle()
        {
            _skippedAssetBundleCount = 0;
            downloadedAssetBundleCount = 0;
            currentAssetBundleProgress = 0f;

            var assetList = _downloadingAssetBundleList;// _assetBundleManifest.GetAllAssetBundles();
            var listCount = assetList.Count;
        
            state = STATE.DOWNLOADING;

            using (var iter = _downloadingAssetBundleList.GetEnumerator())
            {
                while (iter.MoveNext())
                {
                    var data = iter.Current;

                    yield return DownloadAssetBundle(data.Key, data.Value);
                    if (state == STATE.ERROR)
                    {
                        yield break;
                    }
                }
            }

            // NOTE(JJO): 모두 Skip이 아니라면 캐시를 비우도록 함! (Test)
//            if (_skippedAssetBundleCount != listCount)
//            {
//                Caching.ClearCache();
//            }
        }

        public IEnumerator PreloadAllAssetBundle()
        {
            if (_assetBundleManifest == null)
            {
                Debug.LogError($"[PreloadAllAssetBundle] AssetBundleManifest is NULL!");
                yield break;
            }

            state = STATE.LOADING;
            
            loadedAssetBundleCount = 0;

            var assetList = _assetBundleManifest.GetAllAssetBundles();
            var listCount = assetList.Length;
            for (var i = 0; i < listCount; ++i)
            {
                yield return LoadAssetBundle(assetList[i]);
                if (state == STATE.ERROR)
                {
                    yield break;
                }
                
                // TODO(JJO): Dependency Load
                var dependencies = _assetBundleManifest.GetAllDependencies(assetList[i]);
                var dependencyCount = dependencies.Length;
                maximumAssetBundleCount += dependencyCount;
                for (var depIndex = 0; depIndex < dependencyCount; ++depIndex)
                {
                    yield return LoadAssetBundle(dependencies[depIndex]);
                    if (state == STATE.ERROR)
                    {
                        yield break;
                    }
                }
            }

            state = STATE.LOADED;
        }

        private IEnumerator LoadAssetBundle(string assetBundle)
        {
            if (_assetBundleList.ContainsKey(assetBundle))
            {
                Debug.LogWarning($"[LoadAssetBundle] Already loaded asset - {assetBundle}");
                ++loadedAssetBundleCount;
                yield break;
            }
            
            var path = Path.Combine(localAssetBundlePath, assetBundle);
            var request = AssetBundle.LoadFromFileAsync(path, 0);
            while (request.isDone == false)
            {
                currentAssetBundleProgress = request.progress;
                yield return null;
            }

            var bundle = request.assetBundle;
            if (bundle == null)
            {
                Debug.LogError($"[LoadAssetBundle] Failed to load - {assetBundle} is NULL!");
                state = STATE.ERROR;
                yield break;
            }
                
            _assetBundleList.Add(assetBundle, bundle);
                    
            ++loadedAssetBundleCount;
            Debug.Log($"[LoadAssetBundle] Succeeded to load - {assetBundle}");
        }

        public AssetBundle GetAssetBundle(string assetBundleName)
        {
            AssetBundle bundle;
            return _assetBundleList.TryGetValue(assetBundleName, out bundle) == false ? null : bundle;
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
                Debug.LogError($"[LoadAsset] Failed to load - {assetBundleName} / {assetName} is NOT EXIST!");
                return null;
            }
            
            var result = assetBundle.LoadAsset<T>(assetName);
            if (result != null)
            {
                return result;
            }

            Debug.LogError($"[LoadAsset] Failed to load - {assetBundleName} / {assetName} is NULL!");
            return null;
        }

        public IEnumerator LoadAssetAsync<T>(string assetBundleName, System.Action<T> outAction) where T : Object
        {
            yield return LoadAssetAsync<T>(assetBundleName, assetBundleName, outAction);
        }

        public IEnumerator LoadAssetAsync<T>(string assetBundleName, string assetName, System.Action<T> outAction)
            where T : Object
        {
//#if UNITY_EDITOR
//            if (SimulateAssetBundleInEditor)
//            {
//                var result = AssetDatabase.LoadAssetAtPath<T>(assetName);
//                if (result == null)
//                {
//                    Log.Error($"[LoadAssetBundle] Failed to load - {assetName} is NULL!");
//                }
//                
//                outAction(result);
//                
//                yield break;
//            }
//#endif
            AssetBundle assetBundle;
            if (_assetBundleList.TryGetValue(assetBundleName, out assetBundle) == false)
            {
                Debug.LogError($"[LoadAssetAsync] Failed to load - {assetBundleName} is NOT EXIST!");
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
                Debug.LogError($"[LoadAssetAsync] Failed to load - {assetName} is NULL!");
            }
        }

        public IEnumerator LoadLevelAsync(string assetBundleName, string levelName, bool isAdditive)
        {
            if (_assetBundleList.ContainsKey(assetBundleName) == false)
            {
                Debug.LogError($"[LoadLevelAsync] {assetBundleName} is NULL!");
                yield break;
            }
            
            yield return SceneManager.LoadSceneAsync(levelName, isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
        }

        public async Task<T> LoadAssetAsync<T>(string assetBundleName) where T : Object
        {
            var result = await LoadAssetAsync<T>(assetBundleName, assetBundleName);
            return result;
        }

        public async Task<T> LoadAssetAsync<T>(string assetBundleName, string assetName) where T : Object
        {
            AssetBundle assetBundle;
            if (_assetBundleList.TryGetValue(assetBundleName, out assetBundle) == false)
            {
                Debug.LogError($"[LoadAssetBundleAsync] Failed to load - {assetBundleName} is NOT EXIST!");
                return null;
            }

            var request = assetBundle.LoadAssetAsync<T>(assetName);
            await request;

            return request.asset as T;
        }
    }
}
