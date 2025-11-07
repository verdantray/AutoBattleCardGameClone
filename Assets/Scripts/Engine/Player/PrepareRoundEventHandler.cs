using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Engine.UI;

namespace ProjectABC.Engine
{
    public sealed class PrepareRoundEventHandler : IContextEventUIHandler<PrepareRoundEvent>
    { 
        public bool IsWaitConfirm { get; private set; }

        public PrepareRoundEventHandler()
        {
            this.StartListening();
        }
        
        public async Task WaitUntilConfirmAsync()
        {
            var roundAnnounceUI = UIManager.Instance.GetUI<RoundAnnounceUI>();
            await roundAnnounceUI.WaitUntilCloseAsync();

            IsWaitConfirm = false;
        }

        public void OnEvent(PrepareRoundEvent contextEvent)
        {
            var roundAnnounceUI = UIManager.Instance.OpenUI<RoundAnnounceUI>();
            roundAnnounceUI.AnnounceRound(contextEvent.Round);
            
            IsWaitConfirm = true;
        }
        
        public void Dispose()
        {
            this.StopListening();
        }
    }
}