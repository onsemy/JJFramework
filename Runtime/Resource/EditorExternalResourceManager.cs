using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.Assertions;
using JJFramework.Runtime.Extension;
using UnityEditor;
using UnityEngine;

namespace JJFramework.Runtime.Resource
{
    public class EditorExternalResourceManager : BaseExternalResourceManager
    {
        public override T Load<T>(string assetName)
        {
#if UNITY_EDITOR
            var assetList = Directory.GetFiles(UnityEngine.Application.dataPath + "/_resources", $"{assetName}.*", SearchOption.AllDirectories);
            var filePath = Array.Find(assetList, x => Path.GetFileNameWithoutExtension(x) == assetName);
            Assert.IsNotNull(filePath, $"Couldn't find : {assetName}");

            var assetPath = filePath.ToAssetPath();
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
            return asset;
#else
            return null;
#endif
        }

        public override T Load<T>(string assetBundleName, string assetName)
        {
#if UNITY_EDITOR
            assetName = Path.GetFileNameWithoutExtension(assetName);
            var assetList = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleName, assetName);
            var listCount = assetList.Length;
            if (listCount == 0)
            {
                return null;
            }

            var asset = AssetDatabase.LoadAssetAtPath<T>(assetList[0]);
            return asset;
#else
            return null;
#endif
        }

        public override async Task<T> LoadAsync<T>(string assetName)
        {
            return await Task.FromResult(this.Load<T>(assetName));
        }

        public override async Task<T> LoadAsync<T>(string assetBundleName, string assetName)
        {
            return await Task.FromResult(this.Load<T>(assetBundleName, assetName));
        }
    }
}
