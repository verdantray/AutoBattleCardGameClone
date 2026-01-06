using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Data;
using ProjectABC.Engine.UI;
using UnityEngine;

namespace ProjectABC.Engine
{
    public sealed class PlayerController : MonoBehaviour, IPlayer
    {
        [SerializeField] private string temporaryName;
        [SerializeField] private MatchEventRunner matchEventRunner;

        public string Name => temporaryName;
        public bool IsLocalPlayer => true;

        private readonly Dictionary<GamePhase, List<IConfirmHandler>> _confirmHandlers = new();

        private void Awake()
        {
            RegisterConfirmHandlers();
        }

        private void OnDestroy()
        {
            UnregisterConfirmHandlers();
        }

        private void RegisterConfirmHandlers()
        {
            _confirmHandlers.Add(GamePhase.DeckConstruction, new List<IConfirmHandler> { new DeckConstructionConfirmHandler() });
            _confirmHandlers.Add(GamePhase.Preparation, new List<IConfirmHandler> { new PrepareRoundConfirmHandler() });
            _confirmHandlers.Add(GamePhase.Match, new List<IConfirmHandler> { matchEventRunner });
        }

        private void UnregisterConfirmHandlers()
        {
            _confirmHandlers.Clear();
        }

        public async Task<IPlayerAction> DeckConstructAsync(ClubType fixedClubFlag, ClubType selectableClubFlag)
        {
            var selectClubsUI = UIManager.Instance.OpenUI<SelectClubsUI>();
            selectClubsUI.SelectClubsForDeckConstruction(fixedClubFlag, selectableClubFlag);

            await selectClubsUI.WaitUntilCloseAsync();

            IPlayerAction action = new DeckConstructAction(this, selectClubsUI.SelectedClubFlag);
            return action;
        }

        public async Task<IPlayerAction> RecruitCardsAsync(PlayerState myState, RecruitOnRound recruitOnRound)
        {
            var gradeAmountPairs =  recruitOnRound.GetRecruitGradeAmountPairs();
            var selectCardPoolUI = UIManager.Instance.OpenUI<SelectRecruitCardPoolUI>();
            selectCardPoolUI.SetRecruitCardPools(gradeAmountPairs);

            await selectCardPoolUI.WaitUntilCloseAsync();

            int selectedPoolIndex = selectCardPoolUI.FocusIndex;
            var (grade, amount) = gradeAmountPairs[selectedPoolIndex];
            
            var recruitCardUI = UIManager.Instance.OpenUI<RecruitCardUI>();
            var param = new RecruitCardUI.RecruitCardParam(amount, myState.GradeCardPiles[grade], myState.RerollChance);
            
            recruitCardUI.SetRecruit(param);
            var cards = await recruitCardUI.GetRecruitCardsAsync();
            
            IPlayerAction action = new RecruitCardsAction(this, grade, cards);
            return action;
        }

        public Task<IPlayerAction> DeleteCardsAsync(PlayerState myState)
        {
            throw new System.NotImplementedException();
        }

        public Task WaitUntilConfirmToProceed(GamePhase phase)
        {
            if (!_confirmHandlers.TryGetValue(phase, out List<IConfirmHandler> handlers))
            {
                Debug.LogWarning($"{nameof(PlayerController)} : No confirm handlers for {phase}");
                return Task.CompletedTask;
            }
            
            var waitTasks = handlers
                .Select(handler => handler.WaitUntilConfirmAsync());
            
            return Task.WhenAll(waitTasks);
        }
    }
}
