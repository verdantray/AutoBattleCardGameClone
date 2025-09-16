using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Data;
using UnityEngine;

namespace ProjectABC.InGame
{
    public class InGameController : MonoBehaviour
    {
        public static InGameController Instance;
        
        public bool IsRecruitLevelAmountFinished => _isRecruitLevelAmountFinished;
        private bool _isRecruitLevelAmountFinished = false;
        
        public int RecruitLevelAmountIndex => _recruitLevelAmountIndex;
        private int _recruitLevelAmountIndex = -1;
        
        public bool IsDrawCardFinished => _isDrawCardFinished;
        private bool _isDrawCardFinished = false;
        
        public List<Card> DrawCards => _drawCards;
        private readonly List<Card> _drawCards = new();
        
        public bool IsBattleFinished => _isBattleFinished;
        private bool _isBattleFinished = false;
        
        [SerializeField]
        private GameBoardController _gameBoardController;

        private IPlayer _player;
        private IPlayer _enemy;
        private int _roundCount;

        private void Awake()
        {
            Instance = this;
        }
        private void Start()
        {
            UIManager.Instance.DEBUGLayoutInGame.OnFinishRecruitLevelAmount += OnFinishRecruitLevelAmount;
            UIManager.Instance.DEBUGLayoutInGame.OnFinishDrawCard += OnFinishDrawCard;
        }
        private void OnDestroy()
        {
            
        }
        
        public void ClearGame()
        {
            _roundCount = 0;
        }

        public void OnStartRecruitLevelAmount(IReadOnlyList<Tuple<GradeType, int>> pair)
        {
            _recruitLevelAmountIndex = -1;
            _isRecruitLevelAmountFinished = false;
            _isBattleFinished = false;
            UIManager.Instance.DEBUGLayoutInGame.OnStartRecruitLevelAmount(pair);
        }
        public void OnFinishRecruitLevelAmount(int index)
        {
            _recruitLevelAmountIndex = index;
            _isRecruitLevelAmountFinished = true;
        }

        public void OnStartDrawCard(PlayerState state, GradeType gradeType, int amount)
        {
            DrawCards.Clear();
            _isDrawCardFinished = false;
            UIManager.Instance.DEBUGLayoutInGame.OnStartDrawCard(state, gradeType, amount);
        }

        public void OnFinishDrawCard(List<Card> drawCards)
        {
            DrawCards.AddRange(drawCards);
            _isDrawCardFinished = true;
        }

        public void OnFinishBattle()
        {
            _isBattleFinished = true;
        }

        public void OnEvent()
        {
            
        }

        
        
        private async Task OnMatchStart(MatchStartEvent matchStartEvent)
        {
            if (matchStartEvent.Snapshot.TryGetMatchSideSnapshot(_player, out var snapshot))
            {
                _roundCount += 1;
                
                MatchState matchState = snapshot.State;
                _gameBoardController.OnFinishTurn(matchState);
                
                _gameBoardController.SetCardBackVisibility(MatchState.Attacking, true);
                _gameBoardController.SetCardBackVisibility(MatchState.Defending, true);

                UIManager.Instance.DEBUGLayoutInGame.OnStartRound(_roundCount, _player.Name, _enemy.Name);
                
                await Awaitable.WaitForSecondsAsync(2f);
            }
        }
        private async Task OnDrawCard(DrawCardEvent drawCardEvent)
        {
            var drawPlayer = drawCardEvent.DrawPlayer;
            var drawCard = drawCardEvent.DrawCard;
            var playerSnapshot = drawCardEvent.Snapshot.MatchSideSnapShots[drawPlayer];
            
            if (drawPlayer == _player)
            {
                _gameBoardController.OnDrawCard(drawCard, MatchState.Attacking);
                if (playerSnapshot.Deck.Count == 0)
                    _gameBoardController.SetCardBackVisibility(MatchState.Attacking, false);
            }
            else if (drawPlayer == _enemy)
            {
                _gameBoardController.OnDrawCard(drawCard, MatchState.Defending);
                if (playerSnapshot.Deck.Count == 0)
                    _gameBoardController.SetCardBackVisibility(MatchState.Defending, false);
            }
            
            await Awaitable.WaitForSecondsAsync(1f);
        }
        private async Task OnSwitchPosition(SwitchPositionEvent switchPositionEvent)
        {
            if (switchPositionEvent.Snapshot.TryGetMatchSideSnapshot(_player, out var snapshot))
            {
                MatchState matchState = snapshot.State;
                _gameBoardController.OnFinishTurn(matchState);
                
                await Awaitable.WaitForSecondsAsync(0.25f);
            }
        }
        private async Task OnTryPutCardInfirmary(TryPutCardInfirmaryEvent tryPutCardInfirmaryEvent)
        {
            if (tryPutCardInfirmaryEvent.Snapshot.TryGetMatchSideSnapshot(_player, out var snapshot))
            {
                MatchState matchState = snapshot.State;
                _gameBoardController.OnTryPutCardInfirmary(matchState);
                
                await Awaitable.WaitForSecondsAsync(0.5f);
            }
        }
        private async Task OnMatchFinish(MatchFinishEvent matchFinishEvent)
        {
            if (matchFinishEvent.WinningPlayer == _player)
            {
                UIManager.Instance.DEBUGLayoutInGame.OnEndRound(MatchState.Attacking, matchFinishEvent.Reason);
            }
            else if (matchFinishEvent.WinningPlayer == _enemy)
            {
                UIManager.Instance.DEBUGLayoutInGame.OnEndRound(MatchState.Defending, matchFinishEvent.Reason);
            }
            _gameBoardController.OnEndRound();
            await Awaitable.WaitForSecondsAsync(2f);
        }
    }
}