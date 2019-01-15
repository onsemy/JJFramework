using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.Assertions;
using JJFramework.Runtime.Extension;

namespace JJFramework.Runtime.Resource
{
    public class EditorExternalResourceManager : BaseExternalResourceManager
    {
        public override T Load<T>(string name)
        {
#if UNITY_EDITOR
            var assetList = Directory.GetFiles(UnityEngine.Application.dataPath + "/_resources", $"{name}.*", SearchOption.AllDirectories);
            var fpath = Array.Find(assetList, x => Path.GetFileNameWithoutExtension(x) == name);
            Assert.IsNotNull(fpath, $"Couldn't find : {name}");

            var assetPath = fpath.ToAssetPath();
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
            return asset;
#else
            return null;
#endif
        }

        public override async Task<T> LoadAsync<T>(string name)
        {
            return await Task.FromResult(this.Load<T>(name));
        }
    }
}
