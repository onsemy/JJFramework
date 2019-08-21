using System.Threading.Tasks;
using UniRx;

namespace JJFramework.Runtime.Resource
{
    public class InternalResourceManager : IResourceLoader
    {
        public T Load<T>(string assetName) where T : UnityEngine.Object
        {
            return UnityEngine.Resources.Load<T>(assetName);
        }

        public async Task<T> LoadAsync<T>(string assetName) where T : UnityEngine.Object
        {
            var request = UnityEngine.Resources.LoadAsync<T>(assetName);
            await request;
            return request.asset as T;
        }
    }
}
