namespace WpfApp1.States
{
    public class MatchmakingState : GameState
    {
        public MatchmakingState() : base()
        {
        }

        public MatchmakingState(GameState nextState) : base(nextState)
        {
        }

        public override string getStateInfo()
        {
            return "Waiting for other players to join";
        }

        public override GameState getNextState()
        {
            if (_nextState == null)
            {
                return this;
            }
            return _nextState;
        }

        public override State getName()
        {
            return State.Matchmaking;
        }
    }
}
