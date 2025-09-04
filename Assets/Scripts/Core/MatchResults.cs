using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProjectABC.Core
{
    public class MatchResults : IReadOnlyDictionary<int, List<MatchResult>>
    {
        private readonly Dictionary<int, List<MatchResult>> _roundResultMap;

        public MatchResults(int totalRounds = GameConst.GameOption.MAX_ROUND)
        {
            _roundResultMap = new Dictionary<int, List<MatchResult>>();

            for (int i = 1; i <= totalRounds; i++)
            {
                _roundResultMap.Add(i, new List<MatchResult>());
            }
        }
        
        public void AddMatchResult(MatchResult matchResult)
        {
            if (!_roundResultMap.TryGetValue(matchResult.Round, out var results))
            {
                throw new InvalidOperationException($"Round {matchResult.Round} not found");
            }
            
            results.Add(matchResult);
        }

        public MatchResult GetMatchResult(int round, IPlayer participant)
        {
            if (!_roundResultMap.TryGetValue(round, out List<MatchResult> results))
            {
                throw new InvalidOperationException($"Round {round} not found");
            }

            if (!results.Exists(result => result.IsParticipants(participant)))
            {
                throw new InvalidOperationException($"Player {participant.Name} not found from Round {round}");
            }
            
            return results.First(result => result.IsParticipants(participant));
        }
        
        #region inherits of IReadOnlyDictionary<int, List<MatchResult>>

        public IEnumerator<KeyValuePair<int, List<MatchResult>>> GetEnumerator() => _roundResultMap.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _roundResultMap.GetEnumerator();

        public int Count => _roundResultMap.Count;
        public bool ContainsKey(int key) => _roundResultMap.ContainsKey(key);

        public bool TryGetValue(int key, out List<MatchResult> value)
        {
            return _roundResultMap.TryGetValue(key, out value);
        }

        public List<MatchResult> this[int key] => _roundResultMap[key];

        public IEnumerable<int> Keys => _roundResultMap.Keys;
        public IEnumerable<List<MatchResult>> Values => _roundResultMap.Values;
        
        #endregion
    }

    public readonly struct MatchResult
    {
        public readonly int Round;
        public readonly IPlayer Winner;
        public readonly IPlayer Loser;
        public readonly MatchEndReason Reason;

        public MatchResult(int round, IPlayer winner, IPlayer loser, MatchEndReason reason)
        {
            Round = round;
            Winner = winner;
            Loser = loser;
            Reason = reason;
        }

        public bool IsParticipants(IPlayer player)
        {
            return Winner == player || Loser == player;
        }
    }
}
