using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Picturethis3d.Loaders
{
    public static class GameObjectLoaders
    {
        private static readonly Queue<AssetReferenceGameObject> AssetRefs = new Queue<AssetReferenceGameObject>();

        public static event System.Action<AsyncOperationHandle<GameObject>> Completed =
            delegate(AsyncOperationHandle<GameObject> operation) { };

        public static event Action CompletedAll = delegate { };

        public static void Add(AssetReferenceGameObject assetRef)
        {
            if (assetRef.RuntimeKeyIsValid())
            {
                if (assetRef.Asset == null && !AssetRefs.Contains(assetRef))
                {
                    AssetRefs.Enqueue(assetRef);

                    if (AssetRefs.Count == 1)
                    {
                        LoadNext();
                    }
                }
                else
                {
                    DispatchCompletedAllIfEmpty();
                }
            }
            else
            {
                DispatchCompletedAllIfEmpty();
            }
        }

        public static void Add(AssetReferenceGameObject[] assetRefs)
        {
            var updated = false;
            foreach (var assetRef in assetRefs)
            {
                if (assetRef.RuntimeKeyIsValid())
                {
                    if (assetRef.Asset == null && !AssetRefs.Contains(assetRef))
                    {
                        AssetRefs.Enqueue(assetRef);
                        updated = true;
                    }
                }
            }

            if (updated)
            {
                LoadNext();
            }
            else
            {
                DispatchCompletedAllIfEmpty();
            }
        }

        public static void Clear()
        {
            // if (operation.IsValid())
            // {
            //     Addressables.Release(operation);
            // }

      //      AssetRefs.Clear();
        }

        private static AsyncOperationHandle<GameObject> operation;

        private static void LoadNext()
        {
            if (AssetRefs.Count > 0)
            {
                var loadingRef = AssetRefs.Dequeue();
                if (loadingRef.Asset == null && loadingRef.RuntimeKeyIsValid())
                {
                    operation = loadingRef.LoadAssetAsync<GameObject>();

                    operation.Completed += OperationOnCompleted;
                    operation.Destroyed += OnDestroyed;
                }
                else
                {
                    LoadNext();
                }
            }
            else
            {
                DispatchCompletedAllIfEmpty();
            }
        }

        private static void OperationOnCompleted(AsyncOperationHandle<GameObject> operation)
        {
            Completed(operation);
            LoadNext();
        }

        private static void OnDestroyed(AsyncOperationHandle operation)
        {
            Clear();
        }

        private static void DispatchCompletedAllIfEmpty()
        {
            if (AssetRefs.Count <= 0)
            {
                CompletedAll();
            }
        }
    }
}