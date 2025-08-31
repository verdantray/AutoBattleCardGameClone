using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectABC.Core
{
    public class ScriptedPlayer : IPlayer
    {
        public string Name { get; private set; }

        public ScriptedPlayer(string name)
        {
            Name = name;
        }
        
        public Task<RecruitCardsAction> RecruitCardsAsync(PlayerState myState, RecruitOnRound recruitOnRound)
        {
            // TODO : use PCG32
            Random random = new Random();
            var (level, amount) = recruitOnRound.GetRecruitLevelAmountPairs()
                .OrderBy(_ => random.Next())
                .First();
            
            List<Card> cardsToDraw = new List<Card>();
                
            int remainMulliganChances = myState.MulliganChances;

            while (cardsToDraw.Count < amount)
            {
                remainMulliganChances--;

                var cardPile = myState.GradeCardPiles[level];
                int handSize = GameConst.GameOption.RECRUIT_HAND_AMOUNT - cardsToDraw.Count;
                List<Card> cardPool = cardPile.DrawCards(handSize);

                bool isLastChance = remainMulliganChances == 0;
                int remainAmount = amount - cardsToDraw.Count;
                    
                int drawAmount = isLastChance
                    ? remainAmount
                    : Enumerable.Range(0, remainAmount + 1)
                        .OrderBy(_ => random.Next())
                        .First();

                cardsToDraw.AddRange(cardPool.Take(drawAmount));
                cardPool.RemoveRange(0, drawAmount);

                cardPile.AddRange(cardPool);
            }

            myState.GradeCardPiles[level].Shuffle();
            
            RecruitCardsAction action = new RecruitCardsAction(this, level, cardsToDraw);
            Task<RecruitCardsAction> task = Task.FromResult(action);
            
            return task;
        }

        public Task WaitUntilConfirmToProceed()
        {
            return Task.CompletedTask;
        }
    }
}
