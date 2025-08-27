using System.Collections.Generic;
using System.Linq;

namespace ProjectABC.Core
{
    public enum MatchState
    {
        Attacking,
        Defending,
    }
    
    public class MatchSide
    {
        public readonly CardPile Hands = new CardPile();
        public readonly Infirmary Infirmary = new Infirmary();

        public readonly List<Card> Field = new List<Card>();
        public readonly IPlayer Player;
        
        public MatchState State { get; private set; } = MatchState.Attacking;
        
        private bool IsAttacking => State == MatchState.Attacking;
        
        public MatchSide(PlayerState playerState)
        {
            Player = playerState.Player;
            
            Hands.AddRange(playerState.Deck);
            Hands.Shuffle();
        }

        public void SetMatchState(MatchState state) => State = state;

        public bool TryDraw()
        {
            bool isSuccessToDraw = Hands.TryDraw(out Card drawn);
            if (isSuccessToDraw)
            {
                // TODO: call draw effect after implements card abilities
                Field.Add(drawn);
            }

            return isSuccessToDraw;
        }

        public bool TryPutCardFieldToInfirmary(out int remainSlots)
        {
            bool isSuccessToPut = Infirmary.TryPut(Field, out remainSlots);
            Field.Clear();
            
            if (isSuccessToPut)
            {
                // TODO : call put to bench effect after implements card abilities
            }

            return isSuccessToPut;
        }

        public int GetEffectivePower()
        {
            if (Field.Count == 0)
            {
                return 0;
            }

            var effectiveCards = IsAttacking ? Field : Field.GetRange(Field.Count - 1, 1);

            return effectiveCards.Sum(card => card.Power);
        }
    }
}
