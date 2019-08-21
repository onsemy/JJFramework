using System.Threading.Tasks;

namespace JJFramework.Runtime.Resource
{
    public interface IResourceLoader
    {
        T Load<T>(string assetName) where T : UnityEngine.Object;

        Task<T> LoadAsync<T>(string assetName) where T : UnityEngine.Object;
    }
}
