using System.Threading.Tasks;
using UniRx;

namespace JJFramework.Runtime.Resource
{
    public class InternalResourceManager : IResourceLoader
    {
        public T Load<T>(string name) where T : UnityEngine.Object
        {
            return UnityEngine.Resources.Load<T>(name);
        }

        public async Task<T> LoadAsync<T>(string name) where T : UnityEngine.Object
        {
            var request = UnityEngine.Resources.LoadAsync<T>(name);
            await request;
            return request.asset as T;
        }
    }
}