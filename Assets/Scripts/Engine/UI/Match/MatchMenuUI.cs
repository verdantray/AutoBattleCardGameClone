using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectABC.Engine.UI
{
    public sealed class MatchMenuUI : UIElement
    {
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI txtTimeScale;
        [SerializeField] private TextMeshProUGUI txtPause;
        [SerializeField] private Button btnTimeScale;
        [SerializeField] private Button btnSkip;
        [SerializeField] private Button btnPause;
        
        [Header("Time Scales")]
        [SerializeField] private float[] targetTimeScales;
        
        private int _timeScaleIndex = 0;

        private void Awake()
        {
            btnTimeScale.onClick.AddListener(OnChangeTimeScale);
            btnPause.onClick.AddListener(OnPause);
        }

        public override void OnOpen()
        {
            Refresh();
        }

        public override void Refresh()
        {
            float timeScale = targetTimeScales[_timeScaleIndex];
            ApplyTimeScale(timeScale);

            var timeScaler = MatchSimulationTimeScaler.CreateInstance();
            ApplyPause(timeScaler.Paused);
        }

        public override void OnClose()
        {
            btnSkip.onClick.RemoveAllListeners();
            btnPause.onClick.RemoveAllListeners();
        }

        private void OnChangeTimeScale()
        {
            _timeScaleIndex++;
            if (_timeScaleIndex >= targetTimeScales.Length)
            {
                _timeScaleIndex = 0;
            }
            
            float timeScale = targetTimeScales[_timeScaleIndex];
            ApplyTimeScale(timeScale);
        }

        private void ApplyTimeScale(float timeScale)
        {
            var timeScaler = MatchSimulationTimeScaler.CreateInstance();
            timeScaler.SetTimescale(timeScale);

            txtTimeScale.text = $"X {timeScale:F1}";
        }

        private void ApplyPause(bool pause)
        {
            var timeScaler = MatchSimulationTimeScaler.CreateInstance();
            timeScaler.SetPause(pause);

            txtPause.text = pause ? "Resume" : "Pause";
        }

        private void OnSkip()
        {
            
        }

        private void OnPause()
        {
            var timeScaler = MatchSimulationTimeScaler.CreateInstance();
            ApplyPause(!timeScaler.Paused);
        }
    }
}
