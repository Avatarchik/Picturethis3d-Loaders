using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Picturethis3d.Loaders
{
    public class SpriteLoaders
    {
        private static readonly Queue<AssetReferenceSprite> AssetRefs = new Queue<AssetReferenceSprite>();

        public static event Action<AsyncOperationHandle<Sprite>> Completed =
            delegate(AsyncOperationHandle<Sprite> operation) { };

        public static event Action CompletedAll = delegate { };

        public static void Add(AssetReferenceSprite assetRef)
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

        public static void Add(AssetReferenceSprite[] assetRefs)
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
            AssetRefs.Clear();
            DispatchCompletedAllIfEmpty();
        }

        private static void LoadNext()
        {
            if (AssetRefs.Count > 0)
            {
                var loadingRef = AssetRefs.Dequeue();
                if (loadingRef.Asset == null && loadingRef.RuntimeKeyIsValid())
                {
                    var operation = loadingRef.LoadAssetAsync<Sprite>();
                    if (!operation.IsValid() || operation.IsDone)
                    {
                        LoadNext();
                    }
                    else
                    {
                        operation.Completed += OperationOnCompleted;
                        operation.Destroyed += OnDestroyed;
                    }
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

        private static void OperationOnCompleted(AsyncOperationHandle<Sprite> operation)
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