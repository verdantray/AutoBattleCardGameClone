using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Engine.UI;
using ProjectABC.Utils;
using UnityEngine;

namespace ProjectABC.Engine
{
    public sealed class MatchEventRunner : MonoBehaviour, IConfirmHandler<MatchContextEvent>
    {
        private readonly List<IMatchEvent> _matchEvents = new List<IMatchEvent>();
        private readonly Dictionary<Type, IMatchEventProcessor> _matchEventProcessors =
            new Dictionary<Type, IMatchEventProcessor>();

        private CancellationTokenSource _cts;
        private int _eventIndex = 0;

        private void Awake()
        {
            RegisterMatchEventProcessors();
        }

        private void OnDestroy()
        {
            UnregisterMatchEventProcessors();
        }

        private void RegisterMatchEventProcessors()
        {
            _matchEventProcessors.Add(typeof(MatchStartEvent), new MatchStartProcessor(this));
            _matchEventProcessors.Add(typeof(DrawCardToFieldEvent), new DrawCardToFieldProcessor());
            _matchEventProcessors.Add(typeof(SuccessAttackEvent), new SuccessAttackProcessor());
            _matchEventProcessors.Add(typeof(SendToInfirmaryEvent), new SendToInfirmaryProcessor());
            _matchEventProcessors.Add(typeof(SwitchPositionEvent), new SwitchPositionProcessor());
            _matchEventProcessors.Add(typeof(MoveCardByEffectEvent), new MoveCardByEffectProcessor());
            _matchEventProcessors.Add(typeof(ShuffleDeckEvent), new ShuffleDeckProcessor());
            _matchEventProcessors.Add(typeof(GainWinPointsByCardEffectEvent), new GainWinPointsByEffectProcessor());
            _matchEventProcessors.Add(typeof(FailToActivateCardEffectEvent), new FailToActivateCardEffectProcessor());
            _matchEventProcessors.Add(typeof(ActiveBuffEvent), new ActiveBuffProcessor());
            _matchEventProcessors.Add(typeof(InactiveBuffEvent), new InactiveBuffProcessor());
            _matchEventProcessors.Add(typeof(MatchFinishEvent), new MatchFinishProcessor());
        }

        private void UnregisterMatchEventProcessors()
        {
            _matchEventProcessors.Clear();
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
                _matchEvents.Clear();
                _eventIndex = 0;
                
                _cts.Dispose();
                _cts = null;
            }
        }

        public void StartListening()
        {
            this.StartListening<MatchContextEvent>();
        }

        public void StopListening()
        {
            this.StopListening<MatchContextEvent>();
        }

        public async Task WaitUntilConfirmAsync()
        {
            MatchResultUI matchResultUI = UIManager.Instance.GetUI<MatchResultUI>();
            
            while (true)
            {
                if (matchResultUI.gameObject.activeInHierarchy)
                {
                    break;
                }

                
                await Task.Yield();
            }

            await matchResultUI.WaitUntilCloseAsync();
        }

        public void SkipToLastMatchEvent()
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
