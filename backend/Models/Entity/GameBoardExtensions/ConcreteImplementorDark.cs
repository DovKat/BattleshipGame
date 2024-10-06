using Shared;

namespace backend.Models.Entity.GameBoardExtensions
{
    public class ConcreteImplementorDark : ThemeImplementor
    {
        public override Color Background()
        {
            return Color.Black;
        }

        public override string Text()
        {
            return "Turn light theme";
        }
        public override Color TextColor()
        {
            return Color.White;
        }
        public override Color ButtonBackgroundColor()
        {
            return Color.Gray;
        }
    }
}
