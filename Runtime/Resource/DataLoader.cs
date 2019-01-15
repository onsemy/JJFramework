using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using System.IO;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;

namespace JJFramework.Runtime.Resource
{
    [DisallowMultipleComponent]
    public class DataLoader
    {
        public static T Load<T>()
        {
            string fileName = $"{typeof(T).Name}.bson";
            string destinationPath = System.IO.Path.Combine(Application.persistentDataPath, fileName);
            if (File.Exists(destinationPath) == false)
            {
                string sourcePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);

                //if DB does not exist in persistent data folder (folder "Documents" on iOS) or source DB is newer then copy it
                if (!System.IO.File.Exists(destinationPath) || (System.IO.File.GetLastWriteTimeUtc(sourcePath) > System.IO.File.GetLastWriteTimeUtc(destinationPath)))
                {
                    if (sourcePath.Contains("://"))
                    {// Android  
                        var www = new WWW(sourcePath);
                        while (!www.isDone) {; }                // Wait for download to complete - not pretty at all but easy hack for now 
                        if (string.IsNullOrEmpty(www.error))
                        {
                            System.IO.File.WriteAllBytes(destinationPath, www.bytes);
                        }
                        else
                        {
                            Debug.Log("ERROR: the file DB named " + fileName + " doesn't exist in the StreamingAssets Folder, please copy it there.");
                        }
                    }
                    else
                    {                // Mac, Windows, Iphone                
                                     //validate the existens of the DB in the original folder (folder "streamingAssets")
                        if (System.IO.File.Exists(sourcePath))
                        {
                            //copy file - alle systems except Android
                            System.IO.File.Copy(sourcePath, destinationPath, true);
                        }
                        else
                        {
                            Debug.Log("ERROR: the file DB named " + fileName + " doesn't exist in the StreamingAssets Folder, please copy it there.");
                        }
                    }
                }
            }

            if (File.Exists(destinationPath) == true)
            {
                var bf = new BinaryFormatter();
                using (var file = File.Open(destinationPath, FileMode.Open))
                {
                    var reader = new BsonReader(file);
                    var serializer = new JsonSerializer();
                    T result = serializer.Deserialize<T>(reader);

                    return result;
                }
            }

            Debug.Log($"NOTE(jjo): cannot find {typeof(T).Name}");

            return default(T);
        }

        public static bool Load<T>(out T result)
        {
            result = Load<T>();

            return result != null;
        }
    }
}
