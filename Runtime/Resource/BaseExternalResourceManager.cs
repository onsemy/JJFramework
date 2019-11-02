using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace JJFramework.Runtime.Resource
{
    public abstract class BaseExternalResourceManager : IResourceLoader
    {
        #region IResourceLoader

        public abstract T Load<T>(string assetName) where T : UnityEngine.Object;
        public abstract T Load<T>(string assetBundleName, string assetName) where T : UnityEngine.Object;
        public abstract Task<T> LoadAsync<T>(string assetName) where T : UnityEngine.Object;
        public abstract Task<T> LoadAsync<T>(string assetBundleName, string assetName) where T : UnityEngine.Object;

        #endregion IResourceLoader

        protected AssetBundleManager assetBundleManager;
        public void SetAssetBundleManager(AssetBundleManager manager)
        {
            assetBundleManager = manager;
        }

        public void Cleanup()
        {
            assetBundleManager = null;
        }

        protected bool isInitialized => ReferenceEquals(assetBundleManager, null) == false;
    }
}
