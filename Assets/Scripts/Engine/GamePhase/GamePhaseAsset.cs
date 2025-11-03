using System.Threading.Tasks;
using ProjectABC.Core;
using UnityEngine;

namespace ProjectABC.Engine
{
    public abstract class GamePhaseAsset : ScriptableObject, IGamePhase
    {
        public abstract Task ExecutePhaseAsync(SimulationContext simulationContext);
        
        public virtual void StopPhase()
        {
            if (PersistentWorldCameraPoints.HasInstance)
            {
                PersistentWorldCameraPoints.Instance.SwapPoint("Default");
            }
        }
    }
}
