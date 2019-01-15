using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace JJFramework.Runtime.Resource
{
    public class RuntimeExternalResourceManager : BaseExternalResourceManager
    {
        public override T Load<T>(string name)
        {
            var bundle = base.LoadAssetBundle($"bundle/{name}");
            return bundle?.LoadAsset<T>(name);
        }

        public override async Task<T> LoadAsync<T>(string name)
        {
            var result = await Observable.Start(async () =>
            {
                AssetBundle bundle = await base.LoadAssetBundleAsync($"bundle/{name}");

                if (bundle == null)
                {
                    Debug.LogError($"{name} is null");
                    return null;
                }

                var request = bundle.LoadAssetAsync<T>(name);
                await request;
                return request.asset as T;
            });

            return result.Result;
        }

        public override async void PreloadAssetBundleAsync(string[] bundleList, System.Action<int, float, string> action)
        {
            for (int listLoop = 0; listLoop <= bundleList.Length; ++listLoop)
            {
                await LoadAssetBundleAsync($"bundle/{bundleList[listLoop]}", progress => action?.Invoke(listLoop + 1, progress, bundleList[listLoop]));
            }
        }
    }
}
