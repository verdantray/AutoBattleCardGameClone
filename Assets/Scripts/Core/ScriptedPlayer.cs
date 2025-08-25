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

        public async Task<DrawCardsFromPilesAction> DrawCardsFromPilesAsync(int mulliganChances, RecruitOnRound recruitOnRound, LevelCardPiles levelCardPiles)
        {
            System.Random random = new System.Random();
            var (level, amount) = recruitOnRound.GetRecruitLevelAmountPairs()
                .OrderBy(_ => random.Next())
                .First();

            List<Card> cardsToDraw = new List<Card>();
                
            int remainMulliganChances = mulliganChances;

            while (cardsToDraw.Count < amount)
            {
                remainMulliganChances--;

                var cardPile = levelCardPiles[level];
                int handSize = GameConst.GameOption.RECRUIT_HAND_AMOUNT - cardsToDraw.Count;
                List<Card> cardPool = await cardPile.DrawCardsAsync(handSize);

                bool isLastChance = remainMulliganChances == 0;
                int remainAmount = amount - cardsToDraw.Count;
                    
                int drawAmount = isLastChance
                    ? remainAmount
                    : Enumerable.Range(0, remainAmount + 1)
                        .OrderBy(_ => random.Next())
                        .First();

                cardsToDraw.AddRange(cardPool.Take(drawAmount));
                cardPool.RemoveRange(0, drawAmount);

                await cardPile.AddRangeAsync(cardPool);
            }

            DrawCardsFromPilesAction action = new DrawCardsFromPilesAction(this, level, cardsToDraw);
                
            return action;
        }
    }
}
