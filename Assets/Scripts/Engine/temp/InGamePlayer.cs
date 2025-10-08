using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectABC.Core;
using UnityEngine;

namespace ProjectABC.Engine
{
    public class InGamePlayer : IPlayer
    {
        public string Name { get; private set; }
        public async Task<RecruitCardsAction> RecruitCardsAsync(PlayerState myState, RecruitOnRound recruitOnRound)
        {
            var pairs = recruitOnRound.GetRecruitLevelAmountPairs();
            
            InGameController.Instance.OnStartRecruitLevelAmount(pairs);
            var taskWaitRecruit = WaitUntilAsync(() => InGameController.Instance.IsRecruitLevelAmountFinished);
            await Task.WhenAll(taskWaitRecruit);

            var index = InGameController.Instance.RecruitLevelAmountIndex;
            var (grade, amount) = pairs[index];
            
            // TODO : Use CardPile Codes
            InGameController.Instance.OnStartDrawCard(myState, grade, amount);
            var taskWaitDrawCard = WaitUntilAsync(() => InGameController.Instance.IsDrawCardFinished);
            await Task.WhenAll(taskWaitDrawCard);

            List<Card> cardsToDraw = InGameController.Instance.DrawCards;
            RecruitCardsAction action = new RecruitCardsAction(this, grade, cardsToDraw);
            return action;
        }

        public Task<DeleteCardsAction> DeleteCardsAsync(PlayerState myState)
        {
            throw new NotImplementedException();
        }

        public async Task WaitUntilConfirmToProceed()
        {
            while (InGameController.Instance.IsBattleFinished == false)
            {
                await Awaitable.NextFrameAsync();
            }
        }

        public InGamePlayer(string name)
        {
            Name = name;
        }

        private async Task WaitUntilAsync(Func<bool> waitUntil)
        {
            while (!waitUntil())
            {
                await Awaitable.NextFrameAsync();
            }
        }
    }
}