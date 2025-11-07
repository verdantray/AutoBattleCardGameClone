using ProjectABC.Core;
using UnityEngine;

namespace ProjectABC.Engine.UI
{
    public sealed class PlayerSelectedClubListViewer : GridVerticalListViewer<DeckConstructionEvent>, IDeckConstructionResultPage
    {
        public bool IsOpen => gameObject.activeInHierarchy;
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void Refresh()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            
            OnMoveScroll(Vector2.zero);
        }
    }
}
