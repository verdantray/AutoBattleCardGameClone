using System.Threading.Tasks;
using ProjectABC.Core;
using UnityEngine;

namespace ProjectABC.Engine
{
    public abstract class GamePhaseAsset : ScriptableObject, IGamePhase
    {
        [SerializeField] private GamePhase gamePhase;

        public GamePhase Phase => gamePhase;
        
        public abstract Task ExecutePhaseAsync(SimulationContext simulationContext);
        
        public virtual void StopPhase()
        {
            
        }
    }
}
