using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Engine.UI;

namespace ProjectABC.Engine
{
    public class DeckConstructionHandler : IConfirmHandler<DeckConstructionEvent>
    {
        public void StartListening()
        {
            this.StartListening<DeckConstructionEvent>();
        }

        public void StopListening()
        {
            this.StopListening<DeckConstructionEvent>();
        }

        public Task WaitUntilConfirmAsync()
        {
            return Task.CompletedTask;
        }

        public void OnEvent(DeckConstructionEvent contextEvent)
        {
            var resultUI = UIManager.Instance.GetUI<DeckConstructionResultUI>();
            resultUI.AddDeckConstructionResult(contextEvent);
        }
    }
}
