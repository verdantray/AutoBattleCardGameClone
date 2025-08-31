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
        public readonly CardPile Deck = new CardPile();
        public readonly List<Card> Field = new List<Card>();
        public readonly Infirmary Infirmary = new Infirmary();
        
        public readonly IPlayer Player;
        public readonly PlayerState PlayerState;
        
        public MatchState State { get; private set; } = MatchState.Attacking;
        public int GainedWinPointsOnMatch { get; private set; } = 0;
        
        private bool IsAttacking => State == MatchState.Attacking;
        
        public MatchSide(PlayerState playerState)
        {
            Player = playerState.Player;
            PlayerState = playerState;
            
            Deck.AddRange(playerState.Deck);
            Deck.Shuffle();
        }

        public void SetMatchState(MatchState state) => State = state;
        public void AddGainWinPoints(int gainWinPoints) => GainedWinPointsOnMatch += gainWinPoints;

        public bool TryDraw(out Card drawn)
        {
            bool isSuccessToDraw = Deck.TryDraw(out drawn);
            if (isSuccessToDraw)
            {
                // TODO: call draw effect after implements card abilities
                Field.Add(drawn);
            }

            return isSuccessToDraw;
        }

        public void PutCardsToInfirmary(out List<Card> cardsToInfirmary)
        {
            cardsToInfirmary = new List<Card>(Field);
            Field.Clear();
            
            Infirmary.PutCards(cardsToInfirmary);
        }

        public int GetEffectivePower()
        {
            if (Field.Count == 0)
            {
                return 0;
            }

            int effectivePower = IsAttacking
                ? Field.Sum(card => card.Power)
                : Field[^1].Power;
            
            return effectivePower;
        }
    }
}
