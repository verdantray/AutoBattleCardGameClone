using System;
using System.Collections.Generic;
using System.Linq;
using ProjectABC.Data;

namespace ProjectABC.Core
{
    public class RecruitOnRound
    {
        // TODO : remove Round member
        public readonly int Round;
        private readonly List<Tuple<GradeType, int>> _recruitLevelAndAmounts;
        
        public RecruitOnRound(int round)
        {
            Round = round;
            _recruitLevelAndAmounts = Storage.Instance.RecruitData
                .Where(data => data.round == round)
                .Select(ElementSelector)
                .ToList();
        }

        private static Tuple<GradeType, int> ElementSelector(RecruitData recruitData)
        {
            return new Tuple<GradeType, int>(recruitData.recruitGradeType, recruitData.amount);
        }

        public IReadOnlyList<Tuple<GradeType, int>> GetRecruitGradeAmountPairs()
        {
            return _recruitLevelAndAmounts;
        }
    }
}
