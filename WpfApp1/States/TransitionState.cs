namespace WpfApp1.States
{
    class TransitionState : GameState
    {
        public TransitionState() : base()
        {
        }

        public TransitionState(GameState nextState) : base(nextState)
        {
        }

        public override string getStateInfo()
        {
            return "Preparing..";
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
            return State.Transition;
        }
    }
}
