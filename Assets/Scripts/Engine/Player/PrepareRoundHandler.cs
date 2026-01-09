using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Engine.UI;

namespace ProjectABC.Engine
{
    public sealed class PrepareRoundHandler : IConfirmHandler<PrepareRoundEvent>
    {
        public void StartListening()
        {
            this.StartListening<PrepareRoundEvent>();
        }

        public void StopListening()
        {
            this.StopListening<PrepareRoundEvent>();
        }

        public async Task WaitUntilConfirmAsync()
        {
            var roundAnnounceUI = UIManager.Instance.GetUI<RoundAnnounceUI>();
            await roundAnnounceUI.WaitUntilCloseAsync();
        }

        public void OnEvent(PrepareRoundEvent contextEvent)
        {
            RoundAnnounceUI.AnnounceRound(contextEvent.Round);
        }
    }
}