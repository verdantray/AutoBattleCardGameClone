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
    public sealed class MatchEventRunner : MonoBehaviour, IConfirmHandler<MatchContextEvent>
    {
        public bool IsWaitConfirm { get; private set; }
        
        private readonly List<IMatchEvent> _matchEvents = new List<IMatchEvent>();

        private readonly Dictionary<Type, IMatchEventProcessor> _matchEventProcessors = new()
        {
            { typeof(MatchStartEvent), new MatchStartProcessor() },
            { typeof(DrawCardToFieldEvent), new DrawCardToFieldProcessor() },
            { typeof(SuccessAttackEvent), new SuccessAttackProcessor() },
            { typeof(SendToInfirmaryEvent), new SendToInfirmaryProcessor() },
            { typeof(SwitchPositionEvent), new SwitchPositionProcessor() },
            { typeof(MoveCardByEffectEvent), new MoveCardByEffectProcessor() },
            { typeof(ShuffleDeckEvent), new ShuffleDeckProcessor() },
            { typeof(GainWinPointsByCardEffectEvent), new GainWinPointsByEffectProcessor() },
            { typeof(FailToActivateCardEffectEvent), new FailToActivateCardEffectProcessor() },
            { typeof(ActiveBuffEvent), new ActiveBuffProcessor() },
            { typeof(InactiveBuffEvent), new InactiveBuffProcessor() },
        };

        private CancellationTokenSource _cts;
        private Task _currentRunningMatchTask;
        
        private int _eventIndex = 0;

        private void Awake()
        {
            this.StartListening();
        }

        private void OnDestroy()
        {
            this.StopListening();
        }

        private async Task RunMatchAsync()
        {
            try
            {
                _cts ??= new CancellationTokenSource();
                
                while (!_cts.IsCancellationRequested && _eventIndex < _matchEvents.Count)
                {
                    var matchEvent = _matchEvents[_eventIndex];
                    _eventIndex++;
                    
                    if (!_matchEventProcessors.TryGetValue(matchEvent.GetType(), out var matchEventProcessor))
                    {
                        throw new KeyNotFoundException($"No IMatchEventProcessor for type {matchEvent.GetType()}");
                        // Debug.LogWarning($"No IMatchEventProcessor for type {matchEvent.GetType()}");
                        // continue;
                    }

                    await matchEventProcessor.ProcessEventAsync(matchEvent, _cts.Token);
                }

                IsWaitConfirm = false;
            }
            catch (OperationCanceledException) when (_cts.IsCancellationRequested)
            {
                // pass
            }
            catch (Exception e)
            {
                Debug.LogError($"{nameof(MatchEventRunner)} has thrown an exception: {e}");
                throw;
            }
            finally
            {
                _currentRunningMatchTask = null;
            }
        }

        public async Task WaitUntilConfirmAsync()
        {
            while (IsWaitConfirm)
            {
                await Task.Yield();
            }
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
            
            IsWaitConfirm = true;
            
            _currentRunningMatchTask = RunMatchAsync();
            _currentRunningMatchTask.Forget();
        }
    }
}
