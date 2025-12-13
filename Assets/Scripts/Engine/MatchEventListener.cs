using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Utils;
using UnityEngine;

namespace ProjectABC.Engine
{
    public sealed class MatchEventListener : MonoBehaviour, IContextEventListener<MatchContextEvent>
    {
        private readonly List<IMatchEvent> _matchEvents = new List<IMatchEvent>();

        private readonly Dictionary<Type, IMatchEventProcessor> _matchEventProcessors = new()
        {
            { typeof(MatchStartEvent), new MatchStartProcessor() },
        };

        private CancellationTokenSource _cts;
        private int _eventIndex = 0;
        
        private void Awake()
        {
            this.StartListening();
        }

        private void OnDestroy()
        {
            this.StopListening();
        }

        public async Task RunMatchAsync()
        {
            _cts ??= new CancellationTokenSource();

            try
            {
                while (!_cts.IsCancellationRequested && _eventIndex < _matchEvents.Count)
                {
                    var matchEvent = _matchEvents[_eventIndex];
                    if (!_matchEventProcessors.TryGetValue(matchEvent.GetType(), out var matchEventProcessor))
                    {
                        throw new KeyNotFoundException($"No IMatchEventProcessor for type {matchEvent.GetType()}");
                    }
                    
                    await matchEventProcessor.ProcessEventAsync(matchEvent, _cts.Token);
                    _eventIndex++;
                }
            }
            catch (OperationCanceledException) when (_cts.IsCancellationRequested)
            {
                // pass
            }
            catch (Exception e)
            {
                Debug.LogError($"{nameof(MatchEventListener)} has thrown an exception: {e.Message}");
                throw;
            }
        }

        public void Pause()
        {
            RequestCancel();
            
            var currentEvent = _matchEvents[_eventIndex];
            if (!_matchEventProcessors.TryGetValue(currentEvent.GetType(), out var matchEventProcessor))
            {
                throw new KeyNotFoundException($"No IMatchEventProcessor for type {currentEvent.GetType()}");
            }
            
            matchEventProcessor.SkipEvent(currentEvent);
        }

        public void Stop()
        {
            RequestCancel();
        }

        private void RequestCancel()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public void OnEvent(MatchContextEvent contextEvent)
        {
            var participants = new[] { contextEvent.Result.Winner, contextEvent.Result.Loser };
            if (!participants.Contains(Simulator.Model.player))
            {
                return;
            }
            
            _matchEvents.AddRange(contextEvent.MatchEvents);
            _eventIndex = 0;
            
            RunMatchAsync().Forget();
        }
    }
}
