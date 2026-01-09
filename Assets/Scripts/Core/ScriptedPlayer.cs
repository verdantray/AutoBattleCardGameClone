using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    [Serializable]
    public record ScriptedPlayerEntry : IPlayerEntry
    {
        public string name;
        public ClubType deckConstructClubFlag;
        public CardDeletionRange cardDeletionRange;

        public IPlayer GetPlayer()
        {
            return new ScriptedPlayer(name, deckConstructClubFlag, cardDeletionRange);
        }
    }

    [Serializable]
    public record CardDeletionRange
    {
        public ushort min;
        public ushort max;

        public ushort GetDeletionAmount(int totalCardsAmount)
        {
            // TODO: use PCG32
            Random random = new Random();
            ushort rangeCount = (ushort)(Math.Min(max, totalCardsAmount) - min);

            return (ushort)Enumerable
                .Range(min, rangeCount)
                .OrderBy(_ => random.Next())
                .First();
        }
    }
    
    public class ScriptedPlayer : IPlayer
    {
        public string Name { get; }
        public bool IsLocalPlayer => false;

        private readonly ClubType _selectedClubFlag;
        private readonly CardDeletionRange _cardDeletionRange;

        public ScriptedPlayer(string name, ClubType selectedClubFlag, CardDeletionRange cardDeletionRange)
        {
            Name = name;
            _selectedClubFlag = selectedClubFlag;
            _cardDeletionRange = cardDeletionRange;
        }
        
        public Task<IPlayerAction> DeckConstructAsync()
        {
            IPlayerAction action = new DeckConstructAction(this, _selectedClubFlag);
            Task<IPlayerAction> task = Task.FromResult(action);

            return task;
        }

        public Task<IPlayerAction> RecruitCardsAsync(PlayerState myState, RecruitOnRound recruitOnRound)
        {
            // temporary method
            var cardIdQueue = myState.GradeCardPiles[GradeType.Third];
            bool isDrawableRound = recruitOnRound.Round is 3 or 6 or 8;
            
            List<string> drawnCardIds = cardIdQueue.DequeueCardIds(isDrawableRound ? 1 : 0);
            
            IPlayerAction action = new RecruitCardsAction(this, GradeType.Third, drawnCardIds);
            Task<IPlayerAction> task = Task.FromResult(action);
            return task;

            // // TODO : use PCG32
            // Random random = new Random();
            // var (level, amount) = recruitOnRound.GetRecruitGradeAmountPairs()
            //     .OrderBy(_ => random.Next())
            //     .First();
            //
            // List<string> cardIdsToDraw = new List<string>();
            //     
            // RerollChance rerollChance = myState.RerollChance;
            //
            // while (cardIdsToDraw.Count < amount)
            // {
            //     var cardIdQueue = myState.GradeCardPiles[level];
            //     int drawSize = GameConst.GameOption.RECRUIT_HAND_AMOUNT - cardIdsToDraw.Count;
            //     List<string> cardIdPool = rerollChance.GetRerollCardIds(cardIdQueue, drawSize);
            //
            //     bool isLastChance = rerollChance.RemainRerollChance == 0;
            //     int remainAmount = amount - cardIdsToDraw.Count;
            //         
            //     int drawAmount = isLastChance
            //         ? remainAmount
            //         : Enumerable.Range(0, remainAmount + 1)
            //             .OrderBy(_ => random.Next())
            //             .First();
            //
            //     cardIdsToDraw.AddRange(cardIdPool.Take(drawAmount));
            //     cardIdPool.RemoveRange(0, drawAmount);
            //
            //     cardIdQueue.EnqueueCardIds(cardIdPool);
            // }
            //
            // rerollChance.Reset();
            //
            // IPlayerAction action = new RecruitCardsAction(this, level, cardIdsToDraw);
            // Task<IPlayerAction> task = Task.FromResult(action);
            //
            // return task;
        }

        public Task<IPlayerAction> DeleteCardsAsync(PlayerState myState)
        {
            int deleteAmount = _cardDeletionRange.GetDeletionAmount(myState.IncludeCardIds.Count);
            
            // TODO : use PCG32
            Random random = new Random();
            List<string> cardIdsToDelete = myState.IncludeCardIds.OrderBy(_ => random.Next()).Take(deleteAmount).ToList();

            IPlayerAction action = new DeleteCardsAction(this, cardIdsToDelete);
            Task<IPlayerAction> task = Task.FromResult(action);
            
            return task;
        }

        public Task WaitUntilConfirmToProceed(Type eventType)
        {
            return Task.CompletedTask;
        }
    }
}
