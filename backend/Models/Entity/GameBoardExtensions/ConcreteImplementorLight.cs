using Shared;

namespace backend.Models.Entity.GameBoardExtensions
{
    public class ConcreteImplementorLight : ThemeImplementor
    {
        public override Color Background()
        {
            return Color.White;
        }

        public override string Text()
        {
            return "Turn dark theme";
        }

        public override Color TextColor()
        {
            return Color.Black;
        }
        public override Color ButtonBackgroundColor()
        {
            return Color.White;
        }
    }
}
