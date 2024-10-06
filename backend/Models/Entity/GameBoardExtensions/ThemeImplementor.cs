using Shared;

namespace backend.Models.Entity.GameBoardExtensions
{
    public abstract class ThemeImplementor
    {
        public abstract Color Background();

        public abstract string Text();

        public abstract Color TextColor();

        public abstract Color ButtonBackgroundColor();
    }
}
