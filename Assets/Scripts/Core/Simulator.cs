using System;
using ProjectABC.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectABC.Core
{
    public class Simulator : MonoBehaviour
    {
        private async void Start()
        {
            try
            {
                var handle = Addressables.LoadAssetAsync<GameDataAsset>(GameConst.Address.GAME_DATA_ASSET);
                await handle.Task;
                
                Storage.CreateInstance(handle.Result);
                Addressables.Release(handle);

                IPlayer[] players = new IPlayer[8];

                for (int i = 0; i < players.Length; i++)
                {
                    players[i] = new ScriptedPlayer($"플레이어 {(char)('A' + i)}");
                }
                
                Simulation simulation = new Simulation(players);
                await simulation.RunAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Simulator has error occured : {e}");
                throw; // TODO 예외 처리
            }
        }
    }
}
