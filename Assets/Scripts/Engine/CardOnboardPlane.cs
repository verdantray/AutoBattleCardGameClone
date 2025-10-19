using System.Globalization;
using ProjectABC.Data;
using TMPro;
using UnityEngine;

namespace ProjectABC.Engine
{
    public sealed class CardOnboardPlane : CardObject
    {
        [SerializeField] private SpriteRenderer studentRenderer;
        [SerializeField] private SpriteRenderer sleeveRenderer;
        [SerializeField] private SpriteRenderer gradeRenderer;
        [SerializeField] private TMP_Text powerText;
        [SerializeField] private TMP_Text rollText;
        [SerializeField] private TMP_Text nameText;
        
        private CardSpawnArgs _spawnArgs;
        
        public override void OnSpawned(CardSpawnArgs args)
        {
            ApplyArgs(args);
            transform.position = args.Position;
            transform.rotation = args.Rotation;
        }

        public override void OnDespawned()
        {
            
        }

        protected override void ApplyArgs(CardSpawnArgs args)
        {
            _spawnArgs = args;
            if (_spawnArgs == null)
            {
                return;
            }

            string gradeSpriteName = $"grade_{_spawnArgs.CardSnapshot.GradeType.GradeTypeToOrdinalString()}";
            
            gradeRenderer.sprite = GlobalAssetBinder.Instance.AtlasBinder.GetCardSprite(gradeSpriteName);
            powerText.text = _spawnArgs.CardSnapshot.Power.ToString(CultureInfo.InvariantCulture);
            rollText.text = _spawnArgs.CardSnapshot.Title;
            nameText.text = _spawnArgs.CardSnapshot.Name;
        }
    }
}
