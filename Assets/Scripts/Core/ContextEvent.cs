using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public interface IContextEvent
    {
        
    }
    
    /// <summary>
    /// base ContextEvent class for testing simulation
    /// </summary>
    public abstract class ConsoleContextEventBase : IContextEvent
    {
        protected string Message;

        public override string ToString()
        {
            return Message;
        }
    }

    public class CommonConsoleEvent : ConsoleContextEventBase
    {
        public CommonConsoleEvent(string message)
        {
            Message = message;
        }
    }

    public class CardPilesConstructionConsoleEvent : ConsoleContextEventBase
    {
        public CardPilesConstructionConsoleEvent(SetType selectedSetTypeFlag, IEnumerable<Card> cards)
        {
            StringBuilder stringBuilder = new StringBuilder();
            
            SetType[] setTypes = Enum.GetValues(typeof(SetType)) as SetType[];
            var selectedSetNames = setTypes!
                .Where(element => selectedSetTypeFlag.HasFlag(element))
                .Select(element => element.ToString());

            stringBuilder.AppendLine($"다음과 같은 카드 세트가 선택되었습니다. '{string.Join(", ", selectedSetNames)}'");
            stringBuilder.AppendLine("선택된 카드 세트에 따라 다음 카드들이 A, B, C 레벨 카드 더미에 추가됩니다.");
            stringBuilder.Append($"{string.Join('\n', cards)}");

            Message = stringBuilder.ToString();
        }
    }

    public class DeckConstructionConsoleEvent : ConsoleContextEventBase
    {
        public DeckConstructionConsoleEvent(IPlayer player, IEnumerable<Card> startingCards)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"초기 덱을 구성합니다.");
            stringBuilder.AppendLine($"영입한 챌린저들:\n{string.Join('\n', startingCards)}");

            Message = $"플레이어 {player.Name} : {stringBuilder}";
        }
    }

    public class RecruitConsoleEvent : ConsoleContextEventBase
    {
        public RecruitConsoleEvent(IPlayer player, LevelType selectedLevel, List<Card> drawnCards)
        {
            StringBuilder stringBuilder = new StringBuilder();
            
            stringBuilder.AppendLine($"{selectedLevel} 레벨의 챌린저 {drawnCards.Count}명을 영입합니다.");
            stringBuilder.AppendLine($"영입한 챌린저들:\n{string.Join('\n', drawnCards)}");

            Message = $"플레이어 {player.Name} : {stringBuilder}";
        }
    }
}