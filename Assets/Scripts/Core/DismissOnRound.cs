using System.Linq;

namespace ProjectABC.Core
{
    public class DismissOnRound
    {
        private readonly int _round;
        private readonly int _amount;

        public DismissOnRound(int round)
        {
            _round = round;

            var dismissData = Storage.Instance.DismissData.FirstOrDefault(data => data.round == round);
            _amount = dismissData?.amount ?? 0;
        }

        public int GetDismissAmount() => _amount;
    }
}
