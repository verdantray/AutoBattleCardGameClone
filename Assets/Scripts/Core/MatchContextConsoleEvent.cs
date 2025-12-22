using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ProjectABC.Core
{
    public enum MatchEndReason
    {
        EndByEmptyDeck,
        EndByFullOfInfirmary
    }
    
    public class MatchContextConsoleEvent : IMatchContextEvent
    {
        public readonly int Round;
        public MatchResult Result { get; private set; }
        public bool MatchFinished { get; private set; } = false;
        public List<IMatchEvent> MatchEvents { get; } = new List<IMatchEvent>();

        private MatchContextConsoleEvent(int round)
        {
            Round = round;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            
            foreach (var matchEvent in MatchEvents)
            {
                switch (matchEvent)
                {
                    case MatchFinishMessageEvent matchFinishMessage:
                        stringBuilder.AppendLine($"{matchFinishMessage.Message}\n");
                        break;
                    case SwitchPositionMessageEvent switchPositionMessage:
                        stringBuilder.AppendLine($"{switchPositionMessage.Message}\n");
                        break;
                    case FailToApplyCardEffectEvent failToApplyCardEffect:
                        stringBuilder.AppendLine($"{failToApplyCardEffect.Message}\n");
                        break;
                    case GainWinPointsByCardEffectMessageEvent gainWinPointsByCardEffect:
                        stringBuilder.AppendLine($"{gainWinPointsByCardEffect.Message}\n");
                        break;
                    case CommonMatchMessageEvent commonMessage:
                        stringBuilder.AppendLine($"{commonMessage.Message}\n");
                        break;
                }
            }

            return stringBuilder.ToString();
        }

        public List<string> GetMatchContextByTurn()
        {
            List<string> matchContextByTurn = new List<string>();
            StringBuilder stringBuilder = new StringBuilder();
            
            foreach (var matchEvent in MatchEvents)
            {
                switch (matchEvent)
                {
                    case MatchFinishMessageEvent matchFinishMessage:
                        stringBuilder.AppendLine($"{matchFinishMessage.Message}\n");
                        matchContextByTurn.Add(stringBuilder.ToString());
                        stringBuilder.Clear();
                        break;
                    case SwitchPositionMessageEvent switchPositionMessage:
                        stringBuilder.AppendLine($"{switchPositionMessage.Message}\n");
                        matchContextByTurn.Add(stringBuilder.ToString());
                        stringBuilder.Clear();
                        break;
                    case FailToApplyCardEffectEvent failToApplyCardEffect:
                        stringBuilder.AppendLine($"{failToApplyCardEffect.Message}\n");
                        break;
                    case GainWinPointsByCardEffectMessageEvent gainWinPointsByCardEffect:
                        stringBuilder.AppendLine($"{gainWinPointsByCardEffect.Message}\n");
                        break;
                    case CommonMatchMessageEvent commonMessage:
                        stringBuilder.AppendLine($"{commonMessage.Message}\n");
                        break;
                }
            }

            return matchContextByTurn;
        }
        
        public void SetResult(IPlayer winPlayer, IPlayer losePlayer, MatchEndReason reason)
        {
            Result = new MatchResult(Round, winPlayer, losePlayer, reason);
            MatchFinished = true;
        }

        public static MatchContextConsoleEvent RunMatch(GameState currentState, params PlayerState[] playerStates)
        {
            MatchContextConsoleEvent matchContextEvent = new MatchContextConsoleEvent(currentState.Round);

            var (defender, attacker, reason) = IMatchContextEvent.GetMatchSidesOnStart(currentState, playerStates);
            defender.SetMatchState(MatchPosition.Defending);
            attacker.SetMatchState(MatchPosition.Attacking);

            string matchStartMessage = $"매치 시작 : {defender.Player.Name} vs {attacker.Player.Name}";
            CommonMatchMessageEvent matchStartEvent = new CommonMatchMessageEvent(matchStartMessage);
            matchStartEvent.RegisterEvent(matchContextEvent);

            if (!defender.TryDraw(out Card drawnByDefender))
            {
                MatchFinishMessageEvent finishByEmptyDeck = new MatchFinishMessageEvent(
                    attacker.Player,
                    defender.Player,
                    MatchEndReason.EndByEmptyDeck
                );
                
                finishByEmptyDeck.RegisterEvent(matchContextEvent);
                return matchContextEvent;
            }

            string defenderDrawMessage = $"{defender.Player.Name}가 카드를 드로우 함.\n{drawnByDefender}";
            CommonMatchMessageEvent defenderDrawEvent = new CommonMatchMessageEvent(defenderDrawMessage);
            defenderDrawEvent.RegisterEvent(matchContextEvent);

            CardEffectArgs defenderDrawnEffectArgs = new CardEffectArgs(
                EffectTriggerEvent.OnEnterFieldAsDefender,
                defender,
                attacker,
                currentState
            );
            
            drawnByDefender.CardEffect.CheckApplyEffect(defenderDrawnEffectArgs, matchContextEvent);
            if (matchContextEvent.MatchFinished)
            {
                return matchContextEvent;
            }
            
            IMatchContextEvent.CheckApplyBuffs(currentState, matchContextEvent, defender, attacker);

            while (true)
            {
                while (attacker.GetEffectivePower(defender, currentState) < defender.GetEffectivePower(attacker, currentState))
                {
                    if (!attacker.TryDraw(out Card drawnByAttacker))
                    {
                        MatchFinishMessageEvent finishByEmptyDeck = new MatchFinishMessageEvent(
                            defender.Player,
                            attacker.Player,
                            MatchEndReason.EndByEmptyDeck
                        );
                
                        finishByEmptyDeck.RegisterEvent(matchContextEvent);
                        return matchContextEvent;
                    }
                    
                    string attackerDrawnMessage = $"{attacker.Player.Name}가 카드를 드로우 함.\n{drawnByAttacker}";
                    CommonMatchMessageEvent attackerDrawnEvent = new CommonMatchMessageEvent(attackerDrawnMessage);
                    attackerDrawnEvent.RegisterEvent(matchContextEvent);
                    
                    CardEffectArgs attackerDrawnEffectArgs = new CardEffectArgs(
                        EffectTriggerEvent.OnEnterFieldAsAttacker,
                        attacker,
                        defender,
                        currentState
                    );
                    
                    drawnByAttacker.CardEffect.CheckApplyEffect(attackerDrawnEffectArgs, matchContextEvent);
                    if (matchContextEvent.MatchFinished)
                    {
                        return matchContextEvent;
                    }
            
                    IMatchContextEvent.CheckApplyBuffs(currentState, matchContextEvent, defender, attacker);
                }

                string successAttackMessage = $"{attacker.Player.Name}의 공격이 성공하였습니다.\n"
                                              + $"{attacker.Player.Name} : {attacker.GetEffectivePower(defender, currentState)} vs "
                                              + $"{defender.Player.Name} : {defender.GetEffectivePower(attacker, currentState)}";
                
                CommonMatchMessageEvent successAttackEvent = new CommonMatchMessageEvent(successAttackMessage);
                successAttackEvent.RegisterEvent(matchContextEvent);

                #region Put cards of defender from field to infirmary

                CardEffectArgs leaveFieldEffectArgs = new CardEffectArgs(
                    EffectTriggerEvent.OnLeaveField,
                    defender,
                    attacker,
                    currentState
                );
                
                CardEffectArgs enterInfirmaryEffectArgs = new CardEffectArgs(
                    EffectTriggerEvent.OnEnterInfirmary,
                    defender,
                    attacker,
                    currentState
                );

                while (defender.Field.Count > 0)
                {
                    Card cardToMove = defender.Field[^1];
                    defender.Field.Remove(cardToMove);

                    bool isMovementReplaced = cardToMove.CardEffect.TryReplaceMovement(leaveFieldEffectArgs, matchContextEvent);
                    
                    // replace movement instead put to infirmary
                    if (isMovementReplaced)
                    {
                        IMatchContextEvent.CheckApplyBuffs(currentState, matchContextEvent, defender, attacker);
                        continue;
                    }
                    
                    defender.Infirmary.PutCard(cardToMove, out var location);
                    
                    string putCardToInfirmaryMessage = $"{defender.Player.Name}가 카드를 양호실에 넣음. \n{cardToMove}";
                    CommonMatchMessageEvent putCardToInfirmaryEvent = new CommonMatchMessageEvent(putCardToInfirmaryMessage);
                    putCardToInfirmaryEvent.RegisterEvent(matchContextEvent);

                    if (!defender.Infirmary.IsSlotRemains)
                    {
                        MatchFinishMessageEvent finishByFullOfInfirmary = new MatchFinishMessageEvent(
                            attacker.Player,
                            defender.Player,
                            MatchEndReason.EndByFullOfInfirmary
                        );
                
                        finishByFullOfInfirmary.RegisterEvent(matchContextEvent);
                        return matchContextEvent;
                    }
                    
                    cardToMove.CardEffect.CheckApplyEffect(enterInfirmaryEffectArgs, matchContextEvent);
                    if (matchContextEvent.MatchFinished)
                    {
                        return matchContextEvent;
                    }
                }

                #endregion
                
                IMatchContextEvent.CheckApplyBuffs(currentState, matchContextEvent, defender, attacker);
                
                // changes position between two players
                (defender, attacker) = (attacker, defender);
                defender.SetMatchState(MatchPosition.Defending);
                attacker.SetMatchState(MatchPosition.Attacking);

                #region Trigger defender's cards on field

                CardEffectArgs remainFieldEffectArgs = new CardEffectArgs(
                    EffectTriggerEvent.OnRemainField,
                    defender,
                    attacker,
                    currentState
                );
                
                foreach (var cardOnField in defender.Field.SkipLast(1))
                {
                    cardOnField.CardEffect.CheckApplyEffect(remainFieldEffectArgs, matchContextEvent);
                }
                
                CardEffectArgs switchToDefendEffectArgs = new CardEffectArgs(
                    EffectTriggerEvent.OnSwitchToDefend,
                    defender,
                    attacker,
                    currentState
                );
                
                defender.Field[^1].CardEffect.CheckApplyEffect(switchToDefendEffectArgs, matchContextEvent);

                #endregion

                SwitchPositionMessageEvent switchPositionEvent = new SwitchPositionMessageEvent(defender.Player, attacker.Player);
                switchPositionEvent.RegisterEvent(matchContextEvent);
                
                IMatchContextEvent.CheckApplyBuffs(currentState, matchContextEvent, defender, attacker);
            }
        }
    }
}