using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Engine.UI;

namespace ProjectABC.Engine
{
    public class GainWinPointsHandler : IConfirmHandler<GainWinPointsEvent>
    {
        public void StartListening()
        {
            this.StartListening<GainWinPointsEvent>();
        }

        public void StopListening()
        {
            this.StopListening<GainWinPointsEvent>();
        }

        public async Task WaitUntilConfirmAsync()
        {
            var announceUI = UIManager.Instance.GetUI<GainWinPointsAnnounceUI>();
            await announceUI.WaitUntilCloseAsync();
        }

        public void OnEvent(GainWinPointsEvent contextEvent)
        {
            GainWinPointsAnnounceUI.ShowGainWinPoints(contextEvent);
        }
    }
}
