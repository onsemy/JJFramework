using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace JJFramework.Runtime.Resource
{
    public class RuntimeExternalResourceManager : BaseExternalResourceManager
    {
        private AssetBundleManager _assetBundleManager;

        ~RuntimeExternalResourceManager()
        {
            if (ReferenceEquals(_assetBundleManager, null) == false)
            {
                _assetBundleManager = null;
            }
        }
        
        public override T Load<T>(string assetName)
        {
            return Load<T>(assetName, assetName);
        }

        public T Load<T>(string assetBundleName, string assetName) where T : UnityEngine.Object
        {
            if (isInitialized == false)
            {
                Debug.LogWarning("[RuntimeExternalResourceManager|Load<T>] Initialized FIRST!");
                return null;
            }
            
            var result = _assetBundleManager.LoadAsset<T>(assetBundleName, assetName);
            return result;
        }

        public override async Task<T> LoadAsync<T>(string assetName)
        {
            return await LoadAsync<T>(assetName, assetName);
        }

        public async Task<T> LoadAsync<T>(string assetBundleName, string assetName) where T : UnityEngine.Object
        {
            if (isInitialized == false)
            {
                Debug.LogWarning("[RuntimeExternalResourceManager|Load<T>] Initialized FIRST!");
                return null;
            }
            
            var result = await Observable.Start(async () =>
            {
                var asset = await _assetBundleManager.LoadAssetAsync<T>(assetBundleName, assetName);

                if (asset == null)
                {
                    Debug.LogError($"{assetName} is null");
                    return null;
                }

                return asset;
            });

            return result.Result;
        }

        public override void Initialize()
        {
            if (isInitialized ||
                ReferenceEquals(_assetBundleManager, null) == false)
            {
                Debug.LogWarning("[RuntimeExternalResourceManager|Initialize] Already Initialized AssetBundleManager!");
                return;
            }

            _assetBundleManager = AssetBundleManager.Instance;

            isInitialized = true;
        }
    }
}
