using UnityEngine;
using System.IO;
using System.Reflection;
using JJFramework.Runtime.Attribute;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;

namespace JJFramework.Runtime.Resource
{
    public class DataLoader
    {
        private static IResourceLoader _loader;
        private static string _assetBundleName;

        ~DataLoader()
        {
            _loader = null;
        }
        
        public static void SetResourceLoader(IResourceLoader loader, string assetBundleName)
        {
            _loader = loader;
            _assetBundleName = assetBundleName;
        }
        
        public static T Load<T>()
        {
            if (_loader == null)
            {
                Debug.LogError($"NOTE(jjo): ResourceLoader is NOT INITIALIZED!");
                return default;
            }

            var tableName = GetTableName<T>();
            var fileName = $"{tableName}.bytes";
            var asset = _loader.Load<TextAsset>(_assetBundleName, fileName);
            if (asset == null)
            {
                Debug.LogError($"NOTE(jjo): cannot find {fileName}");
                return default;
            }
            
            Stream stream = new MemoryStream(asset.bytes);
            
            var reader = new BsonReader(stream);
            var serializer = new JsonSerializer();
            T result = serializer.Deserialize<T>(reader);
            
            Debug.Log($"Completed to load: {fileName}");
            
            return result;
        }

        private static string GetTableName<T>()
        {
            var classType = typeof(T);
            var attributeType = typeof(TableAttribute);

            var attribute = classType.GetCustomAttribute(attributeType, true) as TableAttribute;
            if (attribute == null)
            {
                return typeof(T).Name;
            }
            
            return attribute.name;
        }

        public static bool Load<T>(out T result)
        {
            result = Load<T>();

            return result != null;
        }
    }
}
