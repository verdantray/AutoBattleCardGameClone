using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectABC.Core
{
    public class RoundPairMap
    {
        private readonly RoundPairs[] _roundPairMap;

        public RoundPairMap(int playerAmount, int totalRounds = GameConst.GameOption.MAX_ROUND)
        {
            _roundPairMap = new RoundPairs[totalRounds];

            List<int> playerIndexes = Enumerable.Range(0, playerAmount).ToList();
            int pairAmount = playerAmount / 2;

            for (int r = 0; r < totalRounds; r++)
            {
                Span<RoundPair> roundPairs = new RoundPair[pairAmount];
                
                for (int i = 0; i < pairAmount; i++)
                {
                    int a = playerIndexes[i];
                    int b = playerIndexes[playerAmount - 1 - i];

                    roundPairs[i] = b > a
                        ? new RoundPair(a, b)
                        : new RoundPair(b, a);
                }

                _roundPairMap[r] = new RoundPairs(roundPairs.ToArray());
                
                int head = playerIndexes[0];
                var tail = playerIndexes.Skip(1).ToList();
                
                if (tail.Count > 0)
                {
                    // move last element to first of tail
                    tail.Insert(0, tail[^1]);
                    tail.RemoveAt(tail.Count - 1);
                }
                    
                tail.Insert(0, head);
                playerIndexes = tail;
            }
        }

        public RoundPairs GetRoundPairs(int round)
        {
            return _roundPairMap[round - 1];
        }
    }

    public readonly struct RoundPairs
    {
        private readonly RoundPair[] _roundPairs;

        public RoundPairs(RoundPair[] roundPairs)
        {
            _roundPairs = roundPairs;
        }

        public List<(T a, T b)> GetMatchingPlayerPairs<T>(IList<T> playerReferences)
        {
            List<(T a, T b)> list = new List<(T a, T b)>();

            foreach (RoundPair roundPair in _roundPairs)
            {
                var (a, b) = roundPair;
                
                T playerAState = playerReferences[a];
                T playerBState = playerReferences[b];
                
                list.Add((playerAState, playerBState));
            }

            return list;
        }
    }
    
    public readonly struct RoundPair
    {
        public readonly int A;
        public readonly int B;

        public RoundPair(int a, int b)
        {
            A = a;
            B = b;
        }

        public void Deconstruct(out int a, out int b)
        {
            a = A;
            b = B;
        }
    }
}