using ProjectABC.Core;
using UnityEngine;

namespace ProjectABC.InGame
{
    public class ContextEventLogger : MonoBehaviour,
        IContextEventListener<CommonConsoleEvent>,
        IContextEventListener<DeckConstructionConsoleEvent>,
        IContextEventListener<RecruitConsoleEvent>
    {
        private void Start()
        {
            this.StartListening<CommonConsoleEvent>();
            this.StartListening<DeckConstructionConsoleEvent>();
            this.StartListening<RecruitConsoleEvent>();
            
            Debug.Log("logger ready");
        }

        private void OnDestroy()
        {
            this.StopListening<CommonConsoleEvent>();
            this.StopListening<DeckConstructionConsoleEvent>();
            this.StopListening<DeckConstructionConsoleEvent>();
            this.StopListening<RecruitConsoleEvent>();
        }

        public void DisplayLog(IContextEvent contextEvent)
        {
            Debug.Log(contextEvent);
        }

        public void OnEvent(CommonConsoleEvent contextEvent)
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
    }
}
