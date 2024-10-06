namespace WpfApp1.States
{
    public class PlacingShipsState : GameState
    {
        public PlacingShipsState() : base()
        {
        }

        public PlacingShipsState(GameState nextState) : base(nextState)
        {
        }

        public override string getStateInfo()
        {
            return "It's time to place your ships";
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
            return State.PlacingShips;
        }
    }
}
