using ible.Foundation.ObjectPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using Debug = ible.Foundation.Utility.Debug;

namespace ible.Foundation.Atlas
{
    /// <summary>
    /// SpriteAtlas管理器
    /// </summary>
    public class AtlasManager : CoreSingleton<AtlasManager>
    {
        public delegate void DelegateCompleteCallBack(string addressName, SpriteAtlas spriteAtlas);
        public DelegateCompleteCallBack OnLoadedComplete;

        private readonly List<string> _loadingSpriteAtlas = new List<string>(10);
        private readonly Dictionary<string, AsyncOperationHandle<SpriteAtlas>> _loadedSpriteAtlas = new Dictionary<string, AsyncOperationHandle<SpriteAtlas>>(20);

        public int LoadedAtlasCount => _loadedSpriteAtlas.Count;
        public int LoadingAtlasCount => _loadingSpriteAtlas.Count;

        private IEnumerator _loadAtlasEnumrator;
        public void LoadAtlasAsync(List<string> addressName)
        {
            foreach (string address in addressName) 
            {
                _loadAtlasEnumrator = LoadAtlasAsync(address);
                while (_loadAtlasEnumrator.MoveNext()) ;
            }
        }

        public IEnumerator LoadAtlasAsync(string addressName) 
        {
            if (_loadingSpriteAtlas.Contains(addressName))
            {
                Debug.LogError($"Load Atlas Fail! duplicate loading {addressName}");
                yield break;
            }
            if (_loadedSpriteAtlas.ContainsKey(addressName))
            {
                Debug.LogError($"Atlas already Loaded! {addressName}");
                yield break;
            }

            var handler = Addressables.LoadAssetAsync<SpriteAtlas>(addressName);
            _loadingSpriteAtlas.Add(addressName);
            handler.Completed += handler =>
            {
                OnLoadComplete(addressName, handler);
            };
            yield return handler;
        }

        public bool Release(string addressName) 
        {
            bool isFound = false;
            foreach(var spriteAtlas in _loadedSpriteAtlas)
            {
                if (addressName == spriteAtlas.Key)
                {
                    Addressables.Release(spriteAtlas.Value);
                    isFound = true;
                }
            }
            _loadingSpriteAtlas.Remove(addressName);
            return isFound;
        }

        public Sprite GetSprite(string spriteName)
        {
            foreach (var atlas in _loadedSpriteAtlas)
            {
                var reSprite = atlas.Value.Result.GetSprite(spriteName);
                if(reSprite !=  null)
                    return reSprite;
            }
            return null;
        }


        private void OnLoadComplete(string addressName, AsyncOperationHandle<SpriteAtlas> handler)
        {
            _loadingSpriteAtlas.Remove(addressName);
            if(handler.Status == AsyncOperationStatus.Succeeded && handler.Result != null)
            {
                _loadedSpriteAtlas.Add(addressName, handler);
                OnLoadedComplete?.Invoke(addressName, handler.Result);
            }
            else
            {
                Debug.LogError($"Load Atlas {addressName} Failed!, exception:{handler.OperationException}");
            }
        }
    }
}
