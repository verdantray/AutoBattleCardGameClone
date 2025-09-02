using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace ProjectABC.Core
{
    public class ScoreBoard : IReadOnlyDictionary<IPlayer, List<ScoreEntry>>
    {
        private readonly Dictionary<IPlayer, List<ScoreEntry>> _scoreEntries = new Dictionary<IPlayer, List<ScoreEntry>>();

        public ScoreBoard(IEnumerable<IPlayer> players)
        {
            foreach (IPlayer player in players)
            {
                _scoreEntries.Add(player, new List<ScoreEntry>());
            }
        }

        public int GetTotalWinPoints(IPlayer player)
        {
            return !_scoreEntries.TryGetValue(player, out var entries)
                ? throw new InvalidOperationException($"{player.Name} does not exist.")
                : entries.Sum(entry => entry.GainedWinPoints);
        }
        
        public void RegisterScoreEntry(IPlayer player, ScoreEntry entry)
        {
            if (!_scoreEntries.TryGetValue(player, out var scoreEntries))
            {
                throw new InvalidOperationException($"{player.Name} does not exist.");
            }
            
            scoreEntries.Add(entry);
        }

        #region inherits of IReadOnlyDictionary<IPlayer, List<ScoreEntry>>

        public IEnumerator<KeyValuePair<IPlayer, List<ScoreEntry>>> GetEnumerator()
        {
            return _scoreEntries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _scoreEntries.GetEnumerator();
        }

        public int Count => _scoreEntries.Count;
        public bool ContainsKey(IPlayer key)
        {
            return _scoreEntries.ContainsKey(key);
        }

        public bool TryGetValue(IPlayer key, out List<ScoreEntry> value)
        {
            return _scoreEntries.TryGetValue(key, out value);
        }

        public List<ScoreEntry> this[IPlayer key] => _scoreEntries[key];

        public IEnumerable<IPlayer> Keys => _scoreEntries.Keys;
        public IEnumerable<List<ScoreEntry>> Values => _scoreEntries.Values;

        #endregion
    }
    
    public readonly struct ScoreEntry
    {
        public enum ScoreReason
        {
            ScoreByMatchWin,
            ScoreByCardEffect,
            ScoreByArguments,
        }
        
        public readonly int GainedWinPoints;
        public readonly ScoreReason Reason;

        public ScoreEntry(int gainedWinPoints, ScoreReason reason)
        {
            GainedWinPoints = gainedWinPoints;
            Reason = reason;
        }
    }
}
