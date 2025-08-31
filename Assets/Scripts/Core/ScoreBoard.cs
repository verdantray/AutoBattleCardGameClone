using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace ProjectABC.Core
{
    public class ScoreBoard : IReadOnlyDictionary<IPlayer, RoundScore[]>
    {
        private readonly Dictionary<IPlayer, RoundScore[]> _roundScoreMap = new Dictionary<IPlayer, RoundScore[]>();

        public ScoreBoard(IEnumerable<IPlayer> players, int totalRounds = GameConst.GameOption.MAX_ROUND)
        {
            foreach (IPlayer player in players)
            {
                _roundScoreMap.Add(player, new RoundScore[totalRounds]);
            }
        }

        public int GetTotalWinPoints(IPlayer player, int round)
        {
            ReadOnlySpan<RoundScore> totalScoresUntilRounds = _roundScoreMap[player][..^round]
                .Where(element => element != null)
                .ToArray();
            
            int totalPoints = 0;
            
            foreach (RoundScore roundScore in totalScoresUntilRounds)
            {
                totalPoints += roundScore.WinPoints;
            }
            
            return totalPoints;
        }
        
        public void RegisterRoundScores(params RoundScore[] roundScores)
        {
            foreach (var roundScore in roundScores)
            {
                if (roundScore.Player == null)
                {
                    throw  new ArgumentException("Round scores player can't be null.");
                }
                
                if (!_roundScoreMap.TryGetValue(roundScore.Player, out var playersScores))
                {
                    throw new ArgumentException($"{roundScore.Player.Name} has not registered a Scoreboard.");
                }
                
                playersScores[roundScore.Round - 1] = roundScore;
            }
        }

        #region inherits of IReadOnlyDictionary<IPlayer, List<RoundScore>>

        public IEnumerator<KeyValuePair<IPlayer, RoundScore[]>> GetEnumerator()
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

        public bool TryGetValue(IPlayer key, out RoundScore[] value)
        {
            return _roundScoreMap.TryGetValue(key, out value);
        }

        public RoundScore[] this[IPlayer key] => _roundScoreMap[key];

        public IEnumerable<IPlayer> Keys => _roundScoreMap.Keys;
        public IEnumerable<RoundScore[]> Values => _roundScoreMap.Values;

        #endregion
    }

    public enum RoundResult
    {
        Lose,
        Win,
    }
    
    public record RoundScore
    {
        public readonly int Round;
        public readonly IPlayer Player;
        public readonly IPlayer Opponent;
        public readonly int WinPoints;
        public readonly RoundResult Result = RoundResult.Lose;

        public RoundScore(int round, IPlayer player, IPlayer opponent, int winPoints, RoundResult result)
        {
            Round = round;
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
