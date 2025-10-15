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
        
        public override void OnSpawned(CardSpawnArgs args)
        {
            throw new System.NotImplementedException();
        }

        public override void OnDespawned()
        {
            throw new System.NotImplementedException();
        }

        protected override void ApplyArgs(CardSpawnArgs args)
        {
            throw new System.NotImplementedException();
        }
    }
}
