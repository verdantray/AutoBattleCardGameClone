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

        public IPlayer GetPlayer()
        {
            return new ScriptedPlayer(name, deckConstructClubFlag);
        }
    }
    
    public class ScriptedPlayer : IPlayer
    {
        public string Name { get; }
        public bool IsLocalPlayer => false;

        private readonly ClubType _selectedClubFlag;

        public ScriptedPlayer(string name, ClubType selectedClubFlag)
        {
            Name = name;
            _selectedClubFlag = selectedClubFlag;
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

        public Task<IPlayerAction> DismissCardsAsync(PlayerState myState, DismissOnRound dismissOnRound)
        {
            // temporary
            List<string> cardIdsToDelete = new List<string>();
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
