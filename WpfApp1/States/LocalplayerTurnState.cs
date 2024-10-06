namespace WpfApp1.States
{
    public class LocalplayerTurnState : GameState
    {
        public LocalplayerTurnState() : base()
        {
        }

        public LocalplayerTurnState(GameState nextState) : base(nextState)
        {
        }

        public override string getStateInfo()
        {
            return "Don't be shy, it's your turn!";
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
            return State.LocalPlayerTurn;
        }
    }
}
