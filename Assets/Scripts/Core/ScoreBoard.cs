using System.Collections;
using System.Collections.Generic;


namespace ProjectABC.Core
{
    public class ScoreBoard : IReadOnlyDictionary<IPlayer, List<RoundScore>>
    {
        private readonly Dictionary<IPlayer, List<RoundScore>> _roundScoreMap = new Dictionary<IPlayer, List<RoundScore>>();

        public void RegisterRoundScores(params RoundScore[] roundScores)
        {
            foreach (RoundScore roundScore in roundScores)
            {
                if (!_roundScoreMap.ContainsKey(roundScore.Player))
                {
                    _roundScoreMap[roundScore.Player] = new List<RoundScore>();
                }
                
                _roundScoreMap[roundScore.Player].Add(roundScore);
            }
        }

        public bool TryGetTotalScores(IPlayer player, out IReadOnlyList<RoundScore> scores)
        {
            bool result = _roundScoreMap.TryGetValue(player, out var list);
            scores = result ? list.AsReadOnly() : null;

            return result;
        }

        #region inherits of IReadOnlyDictionary<IPlayer, List<RoundScore>>

        public IEnumerator<KeyValuePair<IPlayer, List<RoundScore>>> GetEnumerator()
        {
            return _roundScoreMap.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _roundScoreMap.GetEnumerator();
        }

        public int Count => _roundScoreMap.Count;
        public bool ContainsKey(IPlayer key)
        {
            return _roundScoreMap.ContainsKey(key);
        }

        public bool TryGetValue(IPlayer key, out List<RoundScore> value)
        {
            return _roundScoreMap.TryGetValue(key, out value);
        }

        public List<RoundScore> this[IPlayer key] => _roundScoreMap[key];

        public IEnumerable<IPlayer> Keys => _roundScoreMap.Keys;
        public IEnumerable<List<RoundScore>> Values => _roundScoreMap.Values;

        #endregion
    }

    public enum RoundResult
    {
        Win,
        Lose,
    }
    
    public record RoundScore
    {
        public readonly IPlayer Player;
        public readonly IPlayer Opponent;
        public readonly int WinPoints;
        public readonly RoundResult Result;

        public RoundScore(IPlayer player, IPlayer opponent, int winPoints, RoundResult result)
        {
            Player = player;
            Opponent = opponent;
            WinPoints = winPoints;
            Result = result;
        }

        public void Deconstruct(out IPlayer player, out IPlayer opponent, out int winPoints, out RoundResult result)
        {
            player = Player;
            opponent = Opponent;
            winPoints = WinPoints;
            result = Result;
        }
    }
}
