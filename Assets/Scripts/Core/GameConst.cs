
namespace ProjectABC.Core
{
    public static class GameConst
    {
        public static class Address
        {
            public const string GAME_DATA_ASSET = "GameDataAsset";
            public const string LOCALIZATION_DATA_ASSET = "LocalizationDataAsset";
        }
        
        public static class AssetPath
        {
            public const string CARD_BACK_PATH = "material_card_back";
            public const string CARD_FRONT_FALLBACK_PATH = "material_card_fallback";

        }

        public static class SceneName
        {
            public const string INITIALIZER = "Initializer";
            public const string IN_GAME = "InGame";
        }

        public static class GameOption
        {
            public const int MAX_MATCHING_PLAYERS = 8;
            public const int MAX_ROUND = 7;
            public const int SELECT_CLUB_TYPES_AMOUNT = 6;
            public const string DEFAULT_CLUB_TYPE = "Council";
            public const int MULLIGAN_DEFAULT_AMOUNT = 2;
            public const int RECRUIT_HAND_AMOUNT = 5;
            public const int DEFAULT_INFIRMARY_SLOT_LIMIT = 6;
        }

        public static class CardEffect
        {
            public const string EFFECT_ID = "card_effect_id";
            public const string EFFECT_APPLY_TRIGGERS_KEY = "apply_triggers";
            public const string EFFECT_CANCEL_TRIGGERS_KEY = "cancel_triggers";
            public const string EFFECT_DESC_KEY = "desc_key";

            public const string FAIL_REASON_NO_MEET_CONDITION = "desc_effect_fail_no_meet_condition";
            public const string FAIL_REASON_NO_DECK_REMAINS = "desc_effect_fail_no_deck_remains";
            public const string FAIL_REASON_NO_CARD_PILE_REMAINS = "desc_effect_fail_no_opponent_deck_remains";
            public const string FAIL_REASON_NO_OPPONENT_CARD_PILE_REMAINS = "desc_effect_fail_no_opponent_card_pile_remains";
            public const string FAIL_REASON_NO_INFIRMARY_REMAINS = "desc_effect_fail_no_infirmary_remains";
            public const string FAIL_REASON_NO_OPPONENT_INFIRMARY_REMAINS = "desc_effect_fail_no_opponent_infirmary_remains";
        }

        public static class Localization
        {
            public const string MATCHING_KEY = "key";
            public const string INNER_MATCHING_PATTERN = "%localization_key:(?<key>[^%]+)%";
        }
    }
}
