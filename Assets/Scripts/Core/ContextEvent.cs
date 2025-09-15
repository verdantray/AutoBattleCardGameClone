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

    public class DeckConstructionConsoleEvent : ConsoleContextEventBase
    {
        public DeckConstructionConsoleEvent(IPlayer player, ClubType selectedClubTypeFlag, IEnumerable<Card> piles, IEnumerable<Card> startings)
        {
            StringBuilder stringBuilder = new StringBuilder();
            
            ClubType[] clubTypes = Enum.GetValues(typeof(ClubType)) as ClubType[];
            var selectedClubNames = clubTypes!
                .Where(element => selectedClubTypeFlag.HasFlag(element))
                .Select(element => element.ToString());
            
            stringBuilder.AppendLine($"다음과 같은 동아리들을 선택했습니다. '{string.Join(", ", selectedClubNames)}'");
            stringBuilder.AppendLine("선택한 동아리에 따라 다음 카드들이 카드 더미에 추가됩니다.");
            stringBuilder.AppendLine($"{string.Join('\n', piles)}");
            stringBuilder.AppendLine("선택한 동아리에 따라 다음 카드들이 초기 덱에 추가됩니다.");
            stringBuilder.AppendLine($"{string.Join('\n', startings)}");

            Message = $"플레이어 {player.Name} : {stringBuilder}";
        }
    }

    public class RecruitConsoleEvent : ConsoleContextEventBase
    {
        public RecruitConsoleEvent(IPlayer player, GradeType selectedGrade, List<Card> drawnCards)
        {
            StringBuilder stringBuilder = new StringBuilder();
            
            stringBuilder.AppendLine($"{selectedGrade} 학년의 부원 {drawnCards.Count}명을 영입합니다.");
            stringBuilder.AppendLine($"영입한 챌린저들:\n{string.Join('\n', drawnCards)}");

            Message = $"플레이어 {player.Name} : {stringBuilder}";
        }
    }

    public class DeleteCardsConsoleEvent : ConsoleContextEventBase
    {
        public DeleteCardsConsoleEvent(IPlayer player, List<Card> deletedCards)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"덱으로부터 {deletedCards.Count} 장의 카드를 게임에서 제거합니다.");
            stringBuilder.AppendLine($"제거한 카드들:\n{string.Join('\n', deletedCards)})");

            Message = $"플레이어 {player.Name} : {stringBuilder}";
        }
    }
}