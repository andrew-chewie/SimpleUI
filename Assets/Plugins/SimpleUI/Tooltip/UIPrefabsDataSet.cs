using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SimpleUI
{
    public class UIPrefabsDataSet : ScriptableObject {
        public List<GameObject> Prefabs;

        void OnValidate()
        {
            Prefabs.RemoveAll(o => o == null);
        }

        public void Sort()
        {
            Prefabs = Prefabs.OrderBy(o => o.name).ToList();
        }

        public void Removeduplicates()
        {
            Prefabs = Prefabs.Distinct().ToList();
        }
    
#if UNITY_EDITOR
        public List<string> Paths = new List<string>();
        public Type Type;
    
        public void LoadFromAssets()
        {
            List<GameObject> result = new List<GameObject>();
            foreach (var path in Paths)
            {
                TryGetUnityObjectsOfTypeFromPath<GameObject>(path,result);
            }
        
            var assets = result.Where(o => o is GameObject go && go.GetComponent(Type.Name) != null).ToList();
            Prefabs = assets;
        }
 
        /// <summary>
        /// Adds newly (if not already in the list) found assets.
        /// Returns how many found (not how many added)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="assetsFound">Adds to this list if it is not already there</param>
        /// <returns></returns>
        public static int TryGetUnityObjectsOfTypeFromPath<T>(string path, List<T> assetsFound) where T : UnityEngine.Object
        {
            string[] filePaths = Directory.GetFiles(path, "*.*",SearchOption.AllDirectories);
 
            int countFound = 0;
 
            Debug.Log(filePaths.Length);
 
            if (filePaths != null && filePaths.Length > 0)
            {
                for (int i = 0; i < filePaths.Length; i++)
                {
                    UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath(filePaths[i], typeof(T));
                    if (obj is T asset)
                    {
                        countFound++;
                        if (!assetsFound.Contains(asset))
                        {
                            assetsFound.Add(asset);
                        }
                    }
                }
            }
 
            return countFound;
        }
 
#endif
    }
}