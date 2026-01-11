using System;

#if UNITY_EDITOR

using UnityEngine;

#endif


namespace ProjectABC.Data
{
    [Serializable]
    public record CardEffectData : ILocalFieldUpdatable
    {
        public string id;
        public JsonObject json;

        public static implicit operator JsonObject(CardEffectData data) => data.json;

#if UNITY_EDITOR
        public void UpdateFields(TextAsset textAsset)
        {
            id = textAsset.name;
            json = textAsset.text.ParseToJsonValue().obj;
        }
#endif
    }
}