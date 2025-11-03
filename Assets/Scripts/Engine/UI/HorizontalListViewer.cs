using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectABC.Engine.UI
{
    public class HorizontalListViewer<T> : ScrollListViewer<T>
    {
        private const int CONTENT_SIDE_BUFFER = 1;
        
        [SerializeField] private float spacing;

        protected override float ScrollPosition
        {
            get => scrollRect.content.anchoredPosition.x;
            set
            {
                Vector2 contentPosition = scrollRect.content.anchoredPosition;
                contentPosition.x = value;
                
                scrollRect.content.anchoredPosition = contentPosition;
            }
        }
        
        private float CellSize => _itemSize + spacing;
        private float ViewportWidth => scrollRect.viewport.rect.width;
        private float ContentWidth => scrollRect.content.sizeDelta.x;
        
        private float _itemSize;

        public override void MoveToIndex(int index)
        {
            if (!Initialized || Data.Count == 0)
            {
                return;
            }
            
            float target = CellSize * index * -1;
            float maxScrollPos = Mathf.Max(0.0f, ContentWidth - ViewportWidth) * -1.0f;

            target = Mathf.Clamp(target, maxScrollPos, 0.0f);
            ScrollPosition = target;
        }

        protected override void OnItemPooled(AsyncOperationHandle<GameObject> handle)
        {
            if (handle.Result == null)
            {
                Debug.LogWarning($"{nameof(HorizontalListViewer<T>)} : item not loaded yet...");
                return;
            }

            if (!handle.Result.TryGetComponent(out ScrollListItem<T> item))
            {
                Debug.LogWarning($"{nameof(HorizontalListViewer<T>)} : item has no ScrollListItem<T> component");
                return;
            }

            _itemSize = item.RectTransform.rect.width;
            Initialized = true;
            
            CalculateContents();
            OnMoveScroll(Vector2.zero);
        }

        protected override void CalculateContents()
        {
            Vector2 contentSize = scrollRect.content.sizeDelta;
            float contentWidth = (_itemSize * Data.Count) + (spacing * (Data.Count - 1));
            contentSize.x = contentWidth;
            
            scrollRect.content.sizeDelta = contentSize;
        }

        // horizontal scroll range : 0  ~ - (content size - viewport size)
        protected override void AdjustElements(float scrollPosition)
        {
            if (!Initialized || Data.Count == 0 || _itemSize <= 0.0f)
            {
                return;
            }

            int firstVisible = Mathf.Max(0, Mathf.FloorToInt(-scrollPosition / CellSize));
            int startIndex = Mathf.Max(0, firstVisible - CONTENT_SIDE_BUFFER);

            int visibleCount = Mathf.CeilToInt(ViewportWidth / CellSize);
            int needCount = visibleCount + (CONTENT_SIDE_BUFFER * 2) + 1;
            
            needCount = Mathf.Clamp(needCount, 0, Mathf.Max(0, Data.Count - startIndex));

            while (ActiveItems.Count > needCount)
            {
                ReleaseItem(ActiveItems[^1]);
            }

            while (ActiveItems.Count < needCount)
            {
                GetItem();
            }

            for (int i = 0; i < ActiveItems.Count; i++)
            {
                int dataIndex = startIndex + i;
                
                ActiveItems[i].ApplyData(Data[dataIndex]);

                Vector2 itemPos = ActiveItems[i].RectTransform.anchoredPosition;
                itemPos.x = dataIndex * CellSize;

                ActiveItems[i].RectTransform.anchoredPosition = itemPos;
            }
        }
    }
}
