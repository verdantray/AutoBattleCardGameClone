using System;
using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Utils;
using UnityEngine;

namespace ProjectABC.Engine
{
    public sealed class MatchSimulationTimeScaler : Singleton<MatchSimulationTimeScaler>
    {
        private float _timescale = 1.0f;
        public bool Paused { get; private set; } = false;

        public void SetTimescale(float timescale) => _timescale = MathF.Max(0.0f, timescale);
        public void SetPause(bool pause) => Paused = pause;

        public float GetScaledTime(float simulationTime)
        {
            return Timescale <= 0.0f
                ? float.PositiveInfinity
                : simulationTime / Timescale;
        }

        public static MatchSimulationTimeScaler CreateInstance() => CreateInstanceInternal();

        public static float Timescale
        {
            get
            {
                MatchSimulationTimeScaler timeScaler = CreateInstance();
                
                // Debug.Log($"scale : {timeScaler._timescale} / pause : {timeScaler.Paused}");
                

                return timeScaler.Paused
                    ? 0.0f
                    : timeScaler._timescale;
            }
        }

        public static async Task WaitScaledTimeAsync(float simulationTime, CancellationToken token = default)
        {
            if (simulationTime <= 0.0f)
            {
                return;
            }

            float remaining = simulationTime;
            while (remaining > 0.0f)
            {
                token.ThrowIfCancellationRequested();
                await Task.Yield();

                float scale = Timescale;
                if (scale <= 0.0f)
                {
                    continue;
                }

                remaining -= Time.deltaTime * scale;
            }
        }
    }

    [Serializable]
    public struct ScaledTime
    {
        public float time;

        public ScaledTime(float time)
        {
            this.time = time;
        }
        
        public static implicit operator ScaledTime(float time) => new ScaledTime(time);

        public static implicit operator float(ScaledTime scaledTime)
        {
            MatchSimulationTimeScaler timeScaler = MatchSimulationTimeScaler.CreateInstance();
            float value = timeScaler.GetScaledTime(scaledTime.time);

            return value;
        }

        public Task WaitScaledTimeAsync(CancellationToken token = default)
        {
            return MatchSimulationTimeScaler.WaitScaledTimeAsync(time, token);
        }
    }
}
