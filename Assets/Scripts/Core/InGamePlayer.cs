using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Core;
using UnityEngine;

namespace ProjectABC.InGame
{
    public class InGamePlayer : IPlayer
    {
        public string Name { get; private set; }

        public InGamePlayer(string name)
        {
            Name = name;
        }
        
        public async Task<DrawCardsFromPilesAction> DrawCardsFromPilesAsync(int mulliganChances,
            RecruitOnRound recruitOnRound, LevelCardPiles levelCardPiles)
        {
            try
            {
                System.Random random = new System.Random();

                var pairs = recruitOnRound.GetRecruitLevelAmountPairs();
                var (level, amount) = pairs[0];
                if (pairs.Count > 1)
                {
                    InGameController.Instance.OnStartRecruitLevelAmount(pairs);
                    var task = WaitUntilRecruitLevelAmountAsync();
                    await Task.WhenAll(task);

                    var index = InGameController.Instance.RecruitLevelAmountIndex;
                    (level, amount) = pairs[index];
                }
                
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
            catch (Exception e)
            {
                Debug.LogError($"{nameof(DrawCardsFromPilesAsync)} exception : {e}");
                throw;
            }
        }

        private async Task WaitUntilRecruitLevelAmountAsync()
        {
            while (InGameController.Instance.IsRecruitLevelAmountFinished == false)
            {
                await Awaitable.NextFrameAsync();
            }
        }
    }
}