using TMPro;
using UnityEngine;

namespace ProjectABC.Engine
{
    public sealed class CardOnboard : CardObject
    {
        private const int MATERIAL_INDEX_INCLUDING_MAIN_TEX = 0;
        private static readonly int MAIN_TEX_PROPERTY = Shader.PropertyToID("_MainTex");
        
        [SerializeField] private MeshRenderer studentRender;
        [SerializeField] private MeshRenderer sleeveRenderer;
        [SerializeField] private SpriteRenderer gradeRenderer;
        [SerializeField] private TMP_Text powerText;
        [SerializeField] private TMP_Text rollText;
        [SerializeField] private TMP_Text nameText;

        private CardSpawnArgs _spawnArgs;
        
        public override void OnSpawned(CardSpawnArgs args)
        {
            throw new System.NotImplementedException();
        }

        public override void OnDespawned()
        {
            
        }
        
        protected override void ApplyArgs(CardSpawnArgs args)
        {
            _spawnArgs = args;
        }

        private void ChangeMainTextureOfMesh(MeshRenderer meshRenderer, Sprite sprite)
        {
            var mat = meshRenderer.materials[MATERIAL_INDEX_INCLUDING_MAIN_TEX];
            mat.SetTexture(MAIN_TEX_PROPERTY, sprite.texture);
        }
    }
}
