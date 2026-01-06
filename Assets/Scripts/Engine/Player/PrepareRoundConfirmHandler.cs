using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Engine.UI;

namespace ProjectABC.Engine
{
    public sealed class PrepareRoundConfirmHandler : IConfirmHandler<PrepareRoundEvent>
    {
        public PrepareRoundConfirmHandler()
        {
            this.StartListening();
        }
        
        public async Task WaitUntilConfirmAsync()
        {
            var roundAnnounceUI = UIManager.Instance.GetUI<RoundAnnounceUI>();
            await roundAnnounceUI.WaitUntilCloseAsync();
        }

        public void OnEvent(PrepareRoundEvent contextEvent)
        {
            var roundAnnounceUI = UIManager.Instance.OpenUI<RoundAnnounceUI>();
            roundAnnounceUI.AnnounceRound(contextEvent.Round);
        }
        
        public void Dispose()
        {
            this.StopListening();
        }
    }
}