using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectABC.Engine.UI
{
    public class GridVerticalListViewer<T> : ScrollListViewer<T>
    {
        private const int CONTENT_SIDE_BUFFER = 1;

        [SerializeField] private float spacing;
        [SerializeField] private bool useGrid;

        protected override float ScrollPosition
        {
            get => scrollRect.content.anchoredPosition.y;
            set
            {
                Vector2 contentPosition = scrollRect.content.anchoredPosition;
                contentPosition.y = value;
                
                scrollRect.content.anchoredPosition = contentPosition;
            }
        }

        private float CellWidth => _itemSize.x + spacing;
        private float CellHeight => _itemSize.y + spacing;

        private float ViewportWidth => scrollRect.viewport.rect.width;
        private float ViewportHeight => scrollRect.viewport.rect.height;
        private float ContentLeft => scrollRect.content.rect.width * -scrollRect.content.pivot.x;
        private float ContentHeight => scrollRect.content.sizeDelta.y;
        
        private Vector2 _itemSize;
        private int _columnsPerRows = 1;
        
        public override void MoveToIndex(int index)
        {
            if (!Initialized || Data.Count == 0)
            {
                return;
            }
            
            int rowIndex = Mathf.FloorToInt(index / (float)_columnsPerRows);
            float target = rowIndex * CellHeight;

            float maxScrollPos = Mathf.Max(0.0f, ContentHeight - ViewportHeight);
            target = Mathf.Clamp(target, 0.0f, maxScrollPos);
            ScrollPosition = target;
        }

        protected override void OnItemPooled(AsyncOperationHandle<GameObject> handle)
        {
            if (handle.Result == null)
            {
                Debug.LogWarning($"{nameof(GridVerticalListViewer<T>)} : item not loaded yet...");
                return;
            }

            if (!handle.Result.TryGetComponent(out ScrollListItem<T> item))
            {
                Debug.LogWarning($"{nameof(GridVerticalListViewer<T>)} : item has no ScrollListItem<T> component");
            }

            _itemSize = item.RectTransform.sizeDelta;
            
            if (_itemSize.x <= 0.0f)
            {
                _itemSize.x = ViewportWidth * (item.RectTransform.anchorMax.x - item.RectTransform.anchorMin.x);
            }
            
            Initialized = true;
            
            Debug.Log($"itemSize : {_itemSize}");
            
            CalculateContents();
            OnMoveScroll(Vector2.zero);
        }

        protected override void CalculateContents()
        {
            _columnsPerRows = useGrid ? Mathf.Max(1, Mathf.FloorToInt((ViewportWidth + spacing) / CellWidth)) : 1;
            
            int totalRows = Mathf.CeilToInt(Data.Count / (float)_columnsPerRows);
            float contentHeight = (totalRows > 0)
                ? (totalRows * _itemSize.y) + (Mathf.Max(0, totalRows - 1) * spacing)
                : 0.0f;
            
            Vector2 contentSize = scrollRect.content.sizeDelta;
            contentSize.y = contentHeight;
            
            scrollRect.content.sizeDelta = contentSize;
        }

        protected override void AdjustElements(float scrollPosition)
        {
            if (Data.Count == 0 || _itemSize.x <= 0.0f || _itemSize.y <= 0.0f)
            {
                return;
            }

            int firstVisibleRow = Mathf.Max(0, Mathf.FloorToInt(scrollPosition / CellHeight));
            int startRow = Mathf.Max(0, firstVisibleRow - CONTENT_SIDE_BUFFER);

            int visibleRowCount = Mathf.CeilToInt(ViewportHeight / CellHeight);
            int needRow = visibleRowCount + (CONTENT_SIDE_BUFFER * 2) + 1;
            
            int startIndex = startRow * _columnsPerRows;
            int remaining = Mathf.Max(0, Data.Count - startIndex);
            int needCount = Mathf.Clamp(needRow * _columnsPerRows, 0, remaining);

            while (ActiveItems.Count > needCount)
            {
                ReleaseItem(ActiveItems[^1]);
            }

            while (ActiveItems.Count < needCount)
            {
                GetItem();
            }

            float contentWidth = scrollRect.content.rect.width;
            float gridWidth = (_columnsPerRows > 0) ? ((_columnsPerRows * CellWidth) - spacing) : 0.0f;
            float leftBase = ContentLeft + Mathf.Max(0.0f, (contentWidth - gridWidth) * 0.5f);
            
            for (int i = 0; i < ActiveItems.Count; i++)
            {
                int dataIndex = startIndex + i;
                if (dataIndex >= Data.Count)
                {
                    break;
                }
                
                ActiveItems[i].ApplyData(Data[dataIndex]);

                int row = dataIndex / _columnsPerRows;
                int col = dataIndex % _columnsPerRows;

                Vector2 itemPosition = Vector2.zero;
                
                
                itemPosition.x = leftBase + (col * CellWidth) + (_itemSize.x * 0.5f);  // for center pivot
                itemPosition.y = row * CellHeight * -1.0f;

                ActiveItems[i].RectTransform.anchoredPosition = itemPosition;
            }
        }
    }
}
