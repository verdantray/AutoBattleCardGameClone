using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Engine.UI;
using UnityEngine;

namespace ProjectABC.Engine
{
    public class DeckConstructionEventHandler : IContextEventUIHandler<DeckConstructionEvent>
    {
        public bool IsWaitConfirm { get; private set; }
        
        private readonly List<DeckConstructionEvent> _deckConstructionEvents = new List<DeckConstructionEvent>();
        

        public DeckConstructionEventHandler()
        {
            this.StartListening();
        }
        
        public Task WaitUntilConfirmAsync()
        {
            var resultUI = UIManager.Instance.OpenUI<DeckConstructionResultUI>();
            resultUI.SetResults(_deckConstructionEvents);

            return resultUI.WaitUntilCloseAsync();
        }

        public void OnEvent(DeckConstructionEvent contextEvent)
        {
            _deckConstructionEvents.Add(contextEvent);
            IsWaitConfirm = _deckConstructionEvents.Count == GameConst.GameOption.MAX_MATCHING_PLAYERS;

            // if (IsWaitConfirm)
            // {
            //     Debug.Log($"all result arrived : {string.Join(", ", _deckConstructionEvents.Select(ev => ev.Player.Name))}");
            // }
        }
        
        public void Dispose()
        {
            this.StopListening();
        }
    }
}
