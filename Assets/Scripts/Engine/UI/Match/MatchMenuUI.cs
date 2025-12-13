using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectABC.Engine.UI
{
    public sealed class MatchMenuUI : UIElement
    {
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI txtTimeScale;
        [SerializeField] private Button btnTimeScale;
        [SerializeField] private Button btnSkip;
        [SerializeField] private Button btnPause;
        
        [Header("Time Scales")]
        [SerializeField] private float[] targetTimeScales;
        
        private int _timeScaleIndex = 0;

        public override void OnOpen()
        {
            Refresh();
        }

        public override void Refresh()
        {
            float timeScale = targetTimeScales[_timeScaleIndex];
            ApplyTimeScale(timeScale);
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
            var timeScaler = MatchPlayTimeScaler.CreateInstance();
            timeScaler.SetScaleTimes(timeScale);

            txtTimeScale.text = $"X {timeScale:F1}";
        }

        private void OnSkip()
        {
            
        }

        private void OnPause()
        {
            
        }
    }
}
