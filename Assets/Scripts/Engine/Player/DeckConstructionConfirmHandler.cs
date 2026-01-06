using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Engine.UI;

namespace ProjectABC.Engine
{
    public class DeckConstructionConfirmHandler : IConfirmHandler<DeckConstructionEvent>
    {
        private readonly List<DeckConstructionEvent> _deckConstructionEvents = new List<DeckConstructionEvent>();

        public DeckConstructionConfirmHandler()
        {
            this.StartListening();
        }
        
        public async Task WaitUntilConfirmAsync()
        {
            var resultUI = UIManager.Instance.GetUI<DeckConstructionResultUI>();
            await resultUI.WaitUntilCloseAsync();
        }

        public void OnEvent(DeckConstructionEvent contextEvent)
        {
            _deckConstructionEvents.Add(contextEvent);

            if (_deckConstructionEvents.Count < GameConst.GameOption.MAX_MATCHING_PLAYERS)
            {
                return;
            }
            
            var resultUI = UIManager.Instance.OpenUI<DeckConstructionResultUI>();
            resultUI.SetResults(_deckConstructionEvents);
        }
        
        public void Dispose()
        {
            this.StopListening();
        }
    }
}
