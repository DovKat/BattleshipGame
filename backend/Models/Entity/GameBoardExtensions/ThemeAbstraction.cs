using Shared;

namespace backend.Models.Entity.GameBoardExtensions
{
    public abstract class ThemeAbstraction
    {
        protected ThemeImplementor implementor;

        public ThemeAbstraction(ThemeImplementor implementor)
        {
            this.implementor = implementor;
        }


        public abstract Color Background();
        public abstract string Text();
        public abstract Color TextColor();
        public abstract Color ButtonBackgroundColor();





        //public ThemeImplementor Implementor
        //{
        //    get { return implementor; }
        //    set { implementor = value; }
        //}

        //public virtual Color Background()
        //{
        //    return implementor.Background();
        //}
        //public virtual string Text()
        //{
        //    return implementor.Text();
        //}
        //public virtual Color TextColor()
        //{
        //    return implementor.TextColor();
        //}
        //public virtual Color ButtonBackgroundColor()
        //{
        //    return implementor.ButtonBackgroundColor();
        //}
    }
}
