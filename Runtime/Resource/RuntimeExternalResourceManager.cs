using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace JJFramework.Runtime.Resource
{
    public class RuntimeExternalResourceManager : BaseExternalResourceManager
    {
        public override T Load<T>(string assetName)
        {
            return Load<T>(assetName, assetName);
        }

        public override T Load<T>(string assetBundleName, string assetName)
        {
            if (isInitialized == false)
            {
                Debug.LogWarning("[RuntimeExternalResourceManager|Load<T>] Initialized FIRST!");
                return null;
            }
            
            var result = assetBundleManager.LoadAsset<T>(assetBundleName, assetName);
            return result;
        }

        public override async Task<T> LoadAsync<T>(string assetName)
        {
            return await LoadAsync<T>(assetName, assetName);
        }

        public override async Task<T> LoadAsync<T>(string assetBundleName, string assetName)
        {
            if (isInitialized == false)
            {
                Debug.LogWarning("[RuntimeExternalResourceManager|Load<T>] Initialized FIRST!");
                return null;
            }
            
            var result = await Observable.Start(async () =>
            {
                var asset = await assetBundleManager.LoadAssetAsync<T>(assetBundleName, assetName);

                if (asset == null)
                {
                    Debug.LogError($"{assetName} is null");
                    return null;
                }

                return asset;
            });

            return result.Result;
        }
    }
}
