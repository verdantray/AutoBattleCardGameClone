using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public class ScriptedPlayer : IPlayer
    {
        public string Name { get; private set; }
        public bool IsLocalPlayer => false;

        public ScriptedPlayer(string name)
        {
            Name = name;
        }


        public Task<IPlayerAction> DeckConstructAsync(ClubType fixedClubFlag, ClubType selectableClubFlag)
        {
            ClubType selectedSetFlag = ClubType.Council
                                       | ClubType.Coastline
                                       | ClubType.Band
                                       | ClubType.GameDevelopment
                                       | ClubType.HauteCuisine
                                       | ClubType.Unregistered
                                       | ClubType.TraditionExperience;

            IPlayerAction action = new DeckConstructAction(this, selectedSetFlag);
            Task<IPlayerAction> task = Task.FromResult(action);

            return task;
        }

        public Task<IPlayerAction> RecruitCardsAsync(PlayerState myState, RecruitOnRound recruitOnRound)
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

                foreach (var card in cardPool)
                {
                    cardPile.Add(card);
                }
            }

            myState.GradeCardPiles[level].Shuffle();
            
            IPlayerAction action = new RecruitCardsAction(this, level, cardsToDraw);
            Task<IPlayerAction> task = Task.FromResult(action);
            
            return task;
        }

        public Task<IPlayerAction> DeleteCardsAsync(PlayerState myState)
        {
            // TODO : use PCG32
            Random random = new Random();
            
            int deleteAmount = Enumerable.Range(0, myState.Deck.Count - 1).OrderBy(_ => random.Next()).First();
            List<Card> cardsToDelete = myState.Deck.OrderBy(_ => random.Next()).Take(deleteAmount).ToList();

            IPlayerAction action = new DeleteCardsAction(this, cardsToDelete);
            Task<IPlayerAction> task = Task.FromResult(action);
            
            return task;
        }

        public Task WaitUntilConfirmToProceed()
        {
            return Task.CompletedTask;
        }
    }
}
