using System.Linq;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    /// <summary>
    /// 덱에서 가장 낮은 파워 카드들을 덱 맨 위로 정렬
    /// </summary>
    public class LowestPowerCardsToTopOfDeck : CardEffect
    {
        private readonly int _cardsAmount;

        public LowestPowerCardsToTopOfDeck(Card card, JsonObject json) : base(card, json)
        {
            foreach (var field in json.fields.Where(field => field.key == "cards_amount"))
            {
                _cardsAmount = field.value.intValue;
                return;
            }
        }
        
        public override bool TryApplyEffect(MatchSide mySide, out IMatchEvent matchEvent)
        {
            if (mySide.Field.Count < _cardsAmount)
            {
                string failedMessage = $"{CallCard.Name}의 효과를 발동하려 했으나 덱의 남은 카드가 {_cardsAmount} 미만이라 무시됨.\n"
                                       + $"카드 효과 : {Description}";

                matchEvent = new CommonMatchConsoleEvent(failedMessage);
                return false;
            }
            
            var toMove = mySide.Field.OrderBy(card => card.BasePower).Take(_cardsAmount).ToList();
            foreach (var card in toMove)
            {
                mySide.Field.Remove(card);
            }
            
            mySide.Field.InsertRange(0, toMove);

            string successMessage = $"{CallCard.Name}의 효과로 덱에서 가장 파워가 낮은 카드 {_cardsAmount}장을 덱 맨 위로 정렬\n"
                                    + $"카드 효과 : {Description}\n"
                                    + $"적용된 카드들 :\n{string.Join('\n', toMove)}";
            
            matchEvent = new CommonMatchConsoleEvent(successMessage);
            return true;
        }

        protected override string GetDescription()
        {
            // TODO : use localization
            return DescriptionKey;
        }
    }
}
