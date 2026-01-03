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

        private CardReference _reference;
        
        public override void OnSpawned(CardSpawnArgs args)
        {
            ApplyReference(args.CardReference);
            transform.position = args.Position;
            transform.eulerAngles = args.Angles;
        }

        public override void OnDespawned()
        {
            
        }

        public override void ApplyReference(CardReference reference)
        {
            _reference = reference;
            if (_reference == null)
            {
                return;
            }

            CardData cardData = _reference.CardData;
            
            string gradeSpriteName = $"grade_{cardData.gradeType.GradeTypeToOrdinalString()}";
            
            gradeRenderer.sprite = GlobalAssetBinder.Instance.AtlasBinder.GetCardSprite(gradeSpriteName);

            int totalPower = cardData.basePower;
            totalPower += _reference.Buffs.Sum(buff => buff.AdditivePower);

            powerText.text = $"{totalPower:D}";
            rollText.text = LocalizationHelper.Instance.Localize(cardData.titleKey);
            nameText.text = LocalizationHelper.Instance.Localize(cardData.nameKey);
        }
    }
}
