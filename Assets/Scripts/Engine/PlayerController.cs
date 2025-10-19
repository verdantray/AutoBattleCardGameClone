using System.Threading.Tasks;
using ProjectABC.Core;
using UnityEngine;

namespace ProjectABC.Engine
{
    public sealed class PlayerController : MonoBehaviour, IPlayer
    {
        [SerializeField] private string temporaryName;
        
        public string Name => temporaryName;

        public Task<IPlayerAction<IContextEvent>> DeckConstructAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<IPlayerAction<IContextEvent>> RecruitCardsAsync(PlayerState myState, RecruitOnRound recruitOnRound)
        {
            throw new System.NotImplementedException();
        }

        public Task<IPlayerAction<IContextEvent>> DeleteCardsAsync(PlayerState myState)
        {
            throw new System.NotImplementedException();
        }

        public Task WaitUntilConfirmToProceed()
        {
            throw new System.NotImplementedException();
        }
    }
}
