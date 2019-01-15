using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace JJFramework.Runtime.Resource
{
    public abstract class BaseExternalResourceManager : IResourceLoader
    {
        public readonly string bundlePath = Application.persistentDataPath + "/";
        public readonly string manifestPath = Application.persistentDataPath + "/__LATEST";

        protected AssetBundle _manifestBundle;
        protected AssetBundleManifest _manifest;
        public AssetBundleManifest MainManifest
        {
            get
            {
                if (File.Exists(manifestPath) == false)
                {
                    return null;
                }

                if (_manifest == null)
                {
                    _manifestBundle = AssetBundle.LoadFromFile(manifestPath);

                    var assetList = _manifestBundle.GetAllAssetNames();
                    _manifest = _manifestBundle.LoadAsset<AssetBundleManifest>(assetList[0]);
                }

                return _manifest;
            }
        }
        
        public class AssetBundleRef
        {
            private AssetBundle __assetBundle;
            public int referenceCount { get; private set; }

            public AssetBundleRef(AssetBundle bundle)
            {
                __assetBundle = bundle;
            }

            public AssetBundle GetAssetBundle()
            {
                ++referenceCount;
                return __assetBundle;
            }

            public int GetReferenceCount()
            {
                return referenceCount;
            }

            public bool Unload(bool allObject)
            {
                if (--referenceCount == 0 ||
                    allObject)
                {
                    __assetBundle?.Unload(allObject);
                    __assetBundle = null;

                    return true;
                }

                return false;
            }
        }

        public class LoadedAssetBundle
        {
            private readonly Dictionary<string, AssetBundleRef> __cachedAssetBundle = new Dictionary<string, AssetBundleRef>();

            public bool IsLoaded(string bundleName)
            {
                return __cachedAssetBundle.ContainsKey(bundleName);
            }

            public void Add(string bundleName, AssetBundleRef reference)
            {
                __cachedAssetBundle.Add(bundleName, reference);
            }

            public AssetBundleRef Get(string bundleName)
            {
                if (IsLoaded(bundleName) == true)
                {
                    return __cachedAssetBundle[bundleName];
                }

                return null;
            }

            public void Remove(string bundleName)
            {
                if (__cachedAssetBundle.ContainsKey(bundleName) == true)
                {
                    __cachedAssetBundle[bundleName].Unload(false);
                    __cachedAssetBundle.Remove(bundleName);
                }
            }

            public void RemoveAll()
            {
                foreach (var item in __cachedAssetBundle)
                {
                    item.Value.Unload(false);
                }

                __cachedAssetBundle.Clear();
            }
        }

        private readonly LoadedAssetBundle __loadedAssetBundle = new LoadedAssetBundle();

        protected BaseExternalResourceManager()
        {
            //ReadManifest();
        }

        #region Management

        public void Cleanup()
        {
            UnloadManifest();
        }

        public void UnloadAssetBundle(string bundleName, bool allObject)
        {
            __loadedAssetBundle.Remove(bundleName);
        }

        public void UnloadAllAssetBundle()
        {
            __loadedAssetBundle.RemoveAll();
        }

        void UnloadManifest()
        {
            _manifestBundle?.Unload(true);
        }

        #endregion Management

        #region IResourceLoader

        public abstract T Load<T>(string name) where T : UnityEngine.Object;
        public abstract Task<T> LoadAsync<T>(string name) where T : UnityEngine.Object;

        #endregion IResourceLoader

        #region External

        public AssetBundle LoadAssetBundle(string bundleName)
        {
            if (__loadedAssetBundle.IsLoaded(bundleName) == false)
            {
                var path = $"{Application.persistentDataPath}/{bundleName}";
                if (File.Exists(path) == false)
                {
                    Debug.LogError($"paht not exist : {path}");
                    return null;
                }

                var bundle = AssetBundle.LoadFromFile(path);
                if (bundle == null)
                {
                    // HACK(jjo): something has wrong!
                    Debug.LogError(bundleName + " is null!");
                    return null;
                }

                //// NOTE(jjo): dependency
                var dependencyNames = MainManifest?.GetAllDependencies(bundleName);
                if (dependencyNames != null)
                {
                    foreach (var name in dependencyNames)
                    {
                        if (__loadedAssetBundle.IsLoaded(name) == false)
                        {
                            LoadAssetBundle(name);
                        }
                    }
                }

                var assetBundleRef = new AssetBundleRef(bundle);
                __loadedAssetBundle.Add(bundleName, assetBundleRef);
            }

            var bundleRef = __loadedAssetBundle.Get(bundleName);

            return bundleRef.GetAssetBundle();
        }

        public async Task<AssetBundle> LoadAssetBundleAsync(string bundleName, System.Action<float> action = null)
        {
            var job = await Observable.Start(async () =>
            {
                bool isLoading = false;
                if (__loadedAssetBundle.IsLoaded(bundleName) == false)
                {
                    var dependencyNames = MainManifest?.GetAllDependencies(bundleName);
                    if (dependencyNames != null)
                    {
                        foreach (var name in dependencyNames)
                        {
                            if (__loadedAssetBundle.IsLoaded(name) == false)
                            {
                                await LoadAssetBundleAsync(name);
                            }
                        }
                    }

                    var path = $"{Application.persistentDataPath}/{bundleName}";
                    if (File.Exists(path) == false)
                    {
                        Debug.LogError($"paht not exist : {path}");
                        return null;
                    }

                    if (!File.Exists(path))
                    {
                        Debug.LogError($"{path} is not exist");
                        return null;
                    }

                    AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);

                    while (request.isDone == false)
                    {
                        action?.Invoke(request.progress);
                        await Observable.NextFrame();
                    }

                    if (request.assetBundle == null)
                    {
                        // HACK(jjo): something has wrong!
                        Debug.LogError(bundleName + " is null!");
                        return null;
                    }

                    var assetBundleRef = new AssetBundleRef(request.assetBundle);
                    __loadedAssetBundle.Add(bundleName, assetBundleRef);

                    isLoading = true;
                }

                action?.Invoke(1f);
                if (isLoading == false)
                {
                    await Observable.NextFrame();
                }

                var bundleRef = __loadedAssetBundle.Get(bundleName);

                return bundleRef.GetAssetBundle();
            });

            Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            return job.Result;
        }

        public virtual void PreloadAssetBundleAsync(string[] bundleList, System.Action<int, float, string> action) { }

        #endregion External

    }
}
