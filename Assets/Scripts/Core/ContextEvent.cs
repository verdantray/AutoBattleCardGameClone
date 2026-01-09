using System.Collections.Generic;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public interface IContextEvent
    {
        
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

    public class DeckConstructionOverviewEvent : IContextEvent
    {
        
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
}