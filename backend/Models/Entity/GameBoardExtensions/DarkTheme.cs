using Shared;

namespace backend.Models.Entity.GameBoardExtensions
{
    public class DarkTheme : ThemeAbstraction
    {
        public DarkTheme(ThemeImplementor implementor) : base(implementor){ }
        public override Color Background()
        {
            return implementor.Background();
        }

        public override Color ButtonBackgroundColor()
        {
            return implementor.ButtonBackgroundColor();
        }

        public override string Text()
        {
            return implementor.Text();
        }

        public override Color TextColor()
        {
            return implementor.TextColor();
        }
    }
}
