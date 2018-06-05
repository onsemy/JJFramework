using System.Threading.Tasks;

namespace JJFramework.Runtime.Resource
{
    public interface IResourceLoader
    {
        T Load<T>(string name) where T : UnityEngine.Object;

        Task<T> LoadAsync<T>(string name) where T : UnityEngine.Object;
    }
}