using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectABC.Core
{
    public class RoundPairMap
    {
        private readonly RoundPairs[] roundPairMap;

        public RoundPairMap(int rounds, int playerAmount)
        {
            roundPairMap = new RoundPairs[rounds];

            List<int> playerIndexes = Enumerable.Range(0, playerAmount).ToList();
            int pairAmount = playerAmount / 2;

            for (int r = 0; r < rounds; r++)
            {
                Span<RoundPair> roundPairs = new RoundPair[pairAmount];
                
                for (int i = 0; i < pairAmount; i++)
                {
                    int a = playerIndexes[i];
                    int b = playerIndexes[playerAmount - 1 - i];
                
                    roundPairs[i] = new RoundPair(a, b);
                
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

                roundPairMap[r] = new RoundPairs(roundPairs.ToArray());
            }
        }

        public RoundPairs GetRoundPairs(int round)
        {
            return roundPairMap[round - 1];
        }
    }

    public readonly struct RoundPairs
    {
        private readonly RoundPair[] roundPairs;

        public RoundPairs(RoundPair[] roundPairs)
        {
            this.roundPairs = roundPairs;
        }

        public List<(PlayerState a, PlayerState b)> GetMatchingPlayerPairs(IList<PlayerState> playerStates)
        {
            List<(PlayerState a, PlayerState b)> list = new List<(PlayerState a, PlayerState b)>();

            foreach (RoundPair roundPair in roundPairs)
            {
                var (a, b) = roundPair;
                
                PlayerState playerAState = playerStates[a];
                PlayerState playerBState = playerStates[b];
                
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