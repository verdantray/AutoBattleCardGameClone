using System.Collections.Generic;
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

    public class DeckConstructionEvent : IContextEvent
    {
        public readonly IPlayer Player;
        public readonly ClubType SelectedClubFlag;
        
        public DeckConstructionEvent(IPlayer player, ClubType selectedClubFlag)
        {
            Player = player;
            SelectedClubFlag = selectedClubFlag;
        }
    }

    public class PrepareRoundEvent : IContextEvent
    {
        public readonly int Round;
        
        public PrepareRoundEvent(int round)
        {
            Round = round;
        }
    }

    public class RecruitCardsEvent : IContextEvent
    {
        public readonly IPlayer Player;
        public readonly GradeType  SelectedGrade;
        public readonly List<string> DrawnCardIds;
        
        public RecruitCardsEvent(IPlayer player, GradeType selectedGrade, List<string> drawnCardIds)
        {
            Player = player;
            SelectedGrade = selectedGrade;
            DrawnCardIds = drawnCardIds;
        }
    }

    public class DeleteCardsEvent : IContextEvent
    {
        public readonly IPlayer Player;
        public readonly List<string> DeleteCardIds;
        
        public DeleteCardsEvent(IPlayer player, List<string> deleteCardIds)
        {
            Player = player;
            DeleteCardIds = deleteCardIds;
        }
    }

    public class GainWinPointsOnRecruitEvent : IContextEvent
    {
        public readonly IPlayer Player;
        public readonly string ActivatedCardId;
        public readonly int GainPoints;
        public readonly int TotalPoints;

        public GainWinPointsOnRecruitEvent(IPlayer player, string activatedCardId, int gainPoints, int totalPoints)
        {
            Player = player;
            ActivatedCardId = activatedCardId;
            GainPoints = gainPoints;
            TotalPoints = totalPoints;
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
            stringBuilder.AppendLine($"제거한 카드들:\n{string.Join('\n', deletedCards)}");

            Message = $"플레이어 {player.Name} : {stringBuilder}";
        }
    }
}