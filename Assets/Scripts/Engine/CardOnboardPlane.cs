using System.Globalization;
using System.Linq;
using ProjectABC.Core;
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

            CardData cardData = _spawnArgs.CardReference.CardData;
            
            string gradeSpriteName = $"grade_{cardData.gradeType.GradeTypeToOrdinalString()}";
            
            gradeRenderer.sprite = GlobalAssetBinder.Instance.AtlasBinder.GetCardSprite(gradeSpriteName);

            int totalPower = cardData.basePower;
            totalPower += args.CardReference.Buffs.Sum(buff => buff.AdditivePower);
            
            powerText.text = totalPower.ToString(CultureInfo.InvariantCulture);
            rollText.text = LocalizationHelper.Instance.Localize(cardData.titleKey);
            nameText.text = LocalizationHelper.Instance.Localize(cardData.nameKey);
        }
    }
}
