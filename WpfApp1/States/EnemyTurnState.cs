namespace WpfApp1.States
{
    public class EnemyTurnState : GameState
    {
        public EnemyTurnState() : base()
        {
        }

        public EnemyTurnState(GameState nextState) : base(nextState)
        {
        }

        public override string getStateInfo()
        {
            return "Be prepared; it's the enemy's turn.";
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
            return State.EnemyTurn;
        }
    }
}
