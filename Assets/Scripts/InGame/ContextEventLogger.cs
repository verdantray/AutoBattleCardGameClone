using ProjectABC.Core;
using UnityEngine;

namespace ProjectABC.InGame
{
    public class ContextEventLogger : MonoBehaviour,
        IContextEventListener<CommonConsoleEvent>,
        IContextEventListener<CardPilesConstructionConsoleEvent>,
        IContextEventListener<DeckConstructionConsoleEvent>,
        IContextEventListener<RecruitConsoleEvent>,
        IContextEventListener<MatchFlowConsoleEvent>
    {
        private void Start()
        {
            this.StartListening<CommonConsoleEvent>();
            this.StartListening<CardPilesConstructionConsoleEvent>();
            this.StartListening<DeckConstructionConsoleEvent>();
            this.StartListening<RecruitConsoleEvent>();
            this.StartListening<MatchFlowConsoleEvent>();
            
            Debug.Log("logger ready");
        }

        private void OnDestroy()
        {
            this.StopListening<CommonConsoleEvent>();
            this.StopListening<CardPilesConstructionConsoleEvent>();
            this.StopListening<DeckConstructionConsoleEvent>();
            this.StopListening<RecruitConsoleEvent>();
            this.StopListening<MatchFlowConsoleEvent>();
        }

        public void DisplayLog(IContextEvent contextEvent)
        {
            Debug.Log(contextEvent);
        }

        public void OnEvent(CommonConsoleEvent contextEvent)
        {
            DisplayLog(contextEvent);
        }

        public void OnEvent(CardPilesConstructionConsoleEvent contextEvent)
        {
            DisplayLog(contextEvent);
        }

        public void OnEvent(DeckConstructionConsoleEvent contextEvent)
        {
            DisplayLog(contextEvent);
        }

        public void OnEvent(RecruitConsoleEvent contextEvent)
        {
            DisplayLog(contextEvent);
        }

        public void OnEvent(MatchFlowConsoleEvent contextEvent)
        {
            DisplayLog(contextEvent);
        }
    }
}
