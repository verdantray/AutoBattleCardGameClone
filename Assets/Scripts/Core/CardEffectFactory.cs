using System;
using System.Collections.Generic;
using ProjectABC.Data;


namespace ProjectABC.Core
{
    public static class CardEffectFactory
    {
        public static CardEffect GetCardEffect(Card card, string cardEffectId)
        {
            var cardEffectData = Storage.Instance.CardEffectData.GetValueOrDefault(cardEffectId);
            
            return cardEffectData.GetCardEffectFromData(card);
        }

        private static CardEffect GetCardEffectFromData(this CardEffectData data, Card card)
        {
            if (data == null)
            {
                return new EmptyCardEffect(card, null);
            }

            switch (data.id)
            {
                case "effect_elite":
                    return new EliteCardEffect(card, data);
                case "effect_move_specific_grade_cards_from_piles_to_deck":
                    return new MoveSpecificGradeCardsFromPilesToDeck(card, data);
                case "effect_power_up_self_as_both_side_field_amount":
                    return new PowerUpSelfAsBothSideFieldAmount(card, data);
                case "effect_power_up_self_as_each_opponent_belong_clubs_from_infirmary":
                    return new PowerUpSelfAsEachOpponentBelongClubsFromInfirmary(card, data);
                case "effect_give_power_up_to_belong_clubs_from_infirmary":
                    return new GivePowerUpToBelongClubsFromInfirmary(card, data);
                case "effect_power_up_self_as_each_opponent_won_count":
                    return new PowerUpSelfAsEachOpponentWonCount(card, data);
                case "effect_power_up_self_as_each_won_count":
                    return new PowerUpSelfAsEachWonCount(card, data);
                case "effect_power_up_self_belong_clubs_from_infirmary":
                    return new PowerUpSelfBelongClubsFromInfirmary(card, data);
                case "effect_move_deck_to_infirmary":
                    return new MoveDeckToInfirmary(card, data);
                case "effect_gain_win_points_with_clubs_from_infirmary":
                    return new GainWinPointsWithClubsFromInfirmary(card, data);
                case "effect_gain_win_points_on_success_attack":
                    return new GainWinPointsOnSuccessAttack(card, data);
                case "effect_power_up_self_by_last_match":
                    return new PowerUpSelfByLastMatch(card, data);
                case "effect_gain_win_points_by_last_match":
                    return new GainWinPointsByLastMatch(card, data);
                case "effect_power_up_self_as_each_card_types_belong_clubs":
                    return new PowerUpSelfAsEachCardTypesBelongClubs(card, data);
                case "effect_move_random_cards_to_field":
                    return new MoveRandomCardsToField(card, data);
                case "effect_give_power_up_to_specific_power_from_infirmary":
                    return new GivePowerUpToSpecificPowerFromInfirmary(card, data);
                case "effect_replace_movement_infirmary_to_deck":
                    return new ReplaceMovementInfirmaryToDeck(card, data);
                case "effect_shuffle_lowest_power_cards_to_top_of_deck":
                    return new ShuffleLowestPowerCardsToTopOfDeck(card, data);
                case "effect_power_up_self_by_deck_stock":
                    return new PowerUpSelfByDeckStock(card, data);
                case "effect_give_power_down_while_state_from_infirmary":
                    return new GivePowerDownWhileStateFromInfirmary(card, data);
                case "effect_power_up_self_while_match_state":
                    return new PowerUpSelfWhileMatchState(card, data);
                case "effect_power_up_self_if_attack_alone":
                    return new PowerUpSelfIfAttackAlone(card, data);
                case "effect_power_up_self_as_each_infirmary_slots":
                    return new PowerUpSelfAsEachInfirmarySlots(card, data);
                case "effect_move_opponent_cards_deck_to_infirmary":
                    return new MoveOpponentCardsDeckToInfirmary(card, data);
                case "effect_power_up_self_as_each_clubs_from_infirmary":
                    return new PowerUpSelfAsEachClubsFromInfirmary(card, data);
                case "effect_give_disabler_both_side_for_infirmary":
                    return new GiveDisablerBothSideForInfirmary(card, data);
                case "effect_move_random_cards_belong_clubs_from_infirmary_to_deck":
                    return new MoveRandomCardsBelongClubsFromInfirmaryToDeck(card, data);
                case "effect_power_up_self_as_each_round":
                    return new PowerUpSelfAsEachRound(card, data);
                case "effect_power_up_self_as_each_cards_from_infirmary":
                    return new PowerUpSelfAsEachCardsFromInfirmary(card, data);
                case "effect_move_low_base_power_cards_from_infirmary_to_deck":
                    return new MoveLowBasePowerCardsFromInfirmaryToDeck(card, data);
                case "effect_power_up_self_by_opponent_deck_stock":
                    return new PowerUpSelfByOpponentDeckStock(card, data);
                case "effect_power_up_self_as_each_grade_of_opponent_field_last":
                    return new PowerUpSelfAsEachGradeOfOpponentFieldLast(card, data);
                case "effect_power_up_self_as_each_belong_clubs_from_infirmary":
                    return new PowerUpSelfAsEachBelongClubsFromInfirmary(card, data);
                case "effect_gain_win_points_on_recruit":
                    return new GainWinPointsOnRecruit(card, data);
                
                default:
                    throw new ArgumentException($"{data.id} is not valid");
            }
        }
    }
}