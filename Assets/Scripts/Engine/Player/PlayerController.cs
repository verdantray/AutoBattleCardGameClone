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

        public string Name => temporaryName;
        public bool IsLocalPlayer => true;

        private readonly List<IContextEventUIHandler> _contextEventUIHandlers = new List<IContextEventUIHandler>();

        private void Awake()
        {
            InitializeEventUIHandlers();
        }

        private void OnDestroy()
        {
            DisposeEventUIHandlers();
        }

        private void InitializeEventUIHandlers()
        {
            DisposeEventUIHandlers();
            _contextEventUIHandlers.Add(new DeckConstructionEventHandler());
        }

        private void DisposeEventUIHandlers()
        {
            foreach (var contextEventUIHandler in _contextEventUIHandlers)
            {
                contextEventUIHandler.Dispose();
            }

            _contextEventUIHandlers.Clear();
        }

        public async Task<IPlayerAction> DeckConstructAsync(ClubType fixedClubFlag, ClubType selectableClubFlag)
        {
            var selectClubsUI = UIManager.Instance.OpenUI<SelectClubsUI>();
            selectClubsUI.SelectClubsForDeckConstruction(fixedClubFlag, selectableClubFlag);

            await selectClubsUI.WaitUntilCloseAsync();

            IPlayerAction action = new DeckConstructAction(this, selectClubsUI.SelectedClubFlag);
            return action;
        }

        public Task<IPlayerAction> RecruitCardsAsync(PlayerState myState, RecruitOnRound recruitOnRound)
        {
            throw new System.NotImplementedException();
        }

        public Task<IPlayerAction> DeleteCardsAsync(PlayerState myState)
        {
            throw new System.NotImplementedException();
        }

        public Task WaitUntilConfirmToProceed()
        {
            var waitTasks = _contextEventUIHandlers
                .Where(handler => handler.IsWaitConfirm)
                .Select(handler => handler.WaitUntilConfirmAsync());

            return Task.WhenAll(waitTasks);
        }
    }
}
