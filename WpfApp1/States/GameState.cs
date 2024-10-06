namespace WpfApp1.States
{
    public abstract class GameState
    {
        protected GameState? _nextState;
        public GameState()
        {
            _nextState = null;
        }
        public GameState(GameState nextState)
        {
            _nextState = nextState;
        }
        public abstract State getName();
        public abstract string getStateInfo();
        public abstract GameState getNextState();
        public void updateNextState(GameState nextState)
        {
            _nextState = nextState;
        }
    }
}
