using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Engine.UI;


namespace ProjectABC.Engine
{
    public class DeckConstructionOverviewHandler : IConfirmHandler<DeckConstructionOverviewEvent>
    {
        public void StartListening()
        {
            this.StartListening<DeckConstructionOverviewEvent>();
        }

        public void StopListening()
        {
            this.StopListening<DeckConstructionOverviewEvent>();
        }

        public async Task WaitUntilConfirmAsync()
        {
            var resultUI = UIManager.Instance.GetUI<DeckConstructionResultUI>();
            await resultUI.WaitUntilCloseAsync();
        }

        public void OnEvent(DeckConstructionOverviewEvent contextEvent)
        {
            UIManager.Instance.OpenUI<DeckConstructionResultUI>();
        }
    }
}
