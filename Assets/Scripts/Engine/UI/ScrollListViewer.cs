using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace ProjectABC.Engine.UI
{
    public class ScrollListItem<T> : PoolableObject
    {
        [SerializeField] private int poolSize;

        public override int PoolSize => poolSize;

        protected T Data;
        
        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }

                return _rectTransform;
            }
        }
        
        private RectTransform _rectTransform;

        public virtual void ApplyData(T data)
        {
            Data = data;
        }
    }
    
    [RequireComponent(typeof(ScrollRect))]
    public abstract class ScrollListViewer<T> : MonoBehaviour
    {
        [SerializeField] protected ScrollRect scrollRect;
        [SerializeField] protected AssetReferenceGameObject scrollListItemReference;
        
        protected abstract float ScrollPosition { get; set; }
        
        protected readonly List<T> Data = new List<T>();
        protected readonly List<ScrollListItem<T>> ActiveItems = new List<ScrollListItem<T>>();
        
        protected ObjectPooler Pooler = null;

        public bool Initialized { get; protected set; } = false;

        public void FetchData(IEnumerable<T> data)
        {
            Data.Clear();
            Data.AddRange(data);
            
            // Debug.Log($"Data count : {Data.Count} / Initialized : {Initialized}");

            if (!Initialized)
            {
                Initialize();
            }
            else
            {
                CalculateContents();
                OnMoveScroll(Vector2.zero);
            }
        }

        public abstract void MoveToIndex(int index);

        protected virtual void Initialize()
        {
            if (Initialized)
            {
                return;
            }
            
            GameObject poolerObject = new GameObject();
            poolerObject.transform.SetParent(transform);
            
            RectTransform poolerRectTransform = poolerObject.AddComponent<RectTransform>();
            poolerRectTransform.anchoredPosition3D = Vector3.zero;
            poolerRectTransform.localScale = Vector3.one;

            Pooler = poolerObject.AddComponent<ObjectPooler>();
            Pooler.InitializePooler(scrollListItemReference, OnItemPooled);
            
            scrollRect.normalizedPosition = Vector2.zero;
            scrollRect.onValueChanged.RemoveListener(OnMoveScroll);
            scrollRect.onValueChanged.AddListener(OnMoveScroll);
        }

        public Task GetInitializingTask()
        {
            if (Pooler == null)
            {
                Debug.LogWarning($"{nameof(ScrollListViewer<T>)} : You tried get initializing task before call initialize...");
                return Task.CompletedTask;
            }
            
            return Pooler.GetInitializingTask();
        }

        protected void OnMoveScroll(Vector2 _)
        {
            AdjustElements(ScrollPosition);
        }

        protected virtual void OnItemPooled(AsyncOperationHandle<GameObject> handle)
        {
            Initialized = true;
        }

        protected virtual ScrollListItem<T> GetItem()
        {
            var get = Pooler.Get();
            get.gameObject.SetActive(true);
            
            ScrollListItem<T> item = get.GetComponent<ScrollListItem<T>>();
            item.transform.SetParent(scrollRect.content, false);
            
            ActiveItems.Add(item);
            
            return item;
        }

        protected virtual void ReleaseItem(ScrollListItem<T> item)
        {
            ActiveItems.Remove(item);
            item.transform.SetParent(Pooler.transform);
            
            Pooler.Release(item);
        }
        
        protected abstract void CalculateContents();
        protected abstract void AdjustElements(float scrollPosition);
    }
}
