using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Core;
using UnityEngine;
using Random = System.Random;

namespace ProjectABC.InGame
{
    public class InGamePlayer : IPlayer
    {
        public string Name { get; private set; }
        public async Task<RecruitCardsAction> RecruitCardsAsync(PlayerState myState, RecruitOnRound recruitOnRound)
        {
            var pairs = recruitOnRound.GetRecruitLevelAmountPairs();
            var (grade, amount) = pairs[0];
            if (pairs.Count > 1)
            {
                InGameController.Instance.OnStartRecruitLevelAmount(pairs);
                var taskWaitRecruit = WaitUntilRecruitLevelAmountAsync();
                await Task.WhenAll(taskWaitRecruit);

                var index = InGameController.Instance.RecruitLevelAmountIndex;
                (grade, amount) = pairs[index];
            }
            
            // TODO : Use CardPile Codes
            InGameController.Instance.OnStartDrawCard(myState, grade, amount);
            var taskWaitDrawCard = WaitUntilDrawCardAsync();
            await Task.WhenAll(taskWaitDrawCard);

            List<Card> cardsToDraw = InGameController.Instance.DrawCards;
            RecruitCardsAction action = new RecruitCardsAction(this, grade, cardsToDraw);
            return action;
        }

        public InGamePlayer(string name)
        {
            Name = name;
        }

        private async Task WaitUntilRecruitLevelAmountAsync()
        {
            while (InGameController.Instance.IsRecruitLevelAmountFinished == false)
            {
                await Awaitable.NextFrameAsync();
            }
        }

        private async Task WaitUntilDrawCardAsync()
        {
            while (InGameController.Instance.IsDrawCardFinished == false)
            {
                await Awaitable.NextFrameAsync();
            }
        }
    }
}