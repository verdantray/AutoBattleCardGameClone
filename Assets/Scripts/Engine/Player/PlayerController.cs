using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Data;
using ProjectABC.Engine.UI;
using UnityEngine;

namespace ProjectABC.Engine
{
    public class LocalPlayerEntry : IPlayerEntry
    {
        // TODO : get player name from userData
        private const string LOCAL_PLAYER_NAME = "Chococornets";
        
        public IPlayer GetPlayer()
        {
            var player = Simulator.Model.player;
            player.SetName(LOCAL_PLAYER_NAME);

            return player;
        }
    }
    
    public sealed class PlayerController : MonoBehaviour, IPlayer
    {
        [SerializeField] private MatchEventRunner matchEventRunner;

        public string Name { get; private set; }
        
        public bool IsLocalPlayer => true;

        private readonly Dictionary<Type, IConfirmHandler> _confirmHandlers = new Dictionary<Type, IConfirmHandler>();

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
            _confirmHandlers.Add(typeof(DeckConstructionEvent), new DeckConstructionHandler());
            _confirmHandlers.Add(typeof(DeckConstructionOverviewEvent), new DeckConstructionOverviewHandler());
            _confirmHandlers.Add(typeof(PrepareRoundEvent), new PrepareRoundHandler());
            _confirmHandlers.Add(typeof(GainWinPointsEvent), new GainWinPointsHandler());
            _confirmHandlers.Add(typeof(MatchContextEvent), matchEventRunner);

            foreach (var confirmHandler in _confirmHandlers.Values)
            {
                confirmHandler.StartListening();
            }
        }

        private void UnregisterConfirmHandlers()
        {
            foreach (var confirmHandler in _confirmHandlers.Values)
            {
                confirmHandler.StopListening();
            }
            
            _confirmHandlers.Clear();
        }

        public void SetName(string playerName)
        {
            Name = playerName;
        }

        public async Task<IPlayerAction> DeckConstructAsync()
        {
            // TODO : get fixedClub / selectable club flags from Storage or others
            ClubType fixedClubFlag = ClubType.Council;
            ClubType selectableClubFlag = ClubType.Coastline
                                          | ClubType.Band
                                          | ClubType.GameDevelopment
                                          | ClubType.HauteCuisine
                                          | ClubType.Unregistered
                                          | ClubType.TraditionExperience;
            
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

        public Task WaitUntilConfirmToProceed(Type eventType)
        {
            if (!_confirmHandlers.TryGetValue(eventType, out IConfirmHandler handler))
            {
                // Debug.LogWarning($"{nameof(PlayerController)} : No confirm handlers for {eventType}");
                return Task.CompletedTask;
            }

            return handler.WaitUntilConfirmAsync();
        }
    }
}
