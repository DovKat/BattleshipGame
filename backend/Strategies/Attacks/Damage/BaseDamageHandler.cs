using backend.Models.Entity;
using Shared;

namespace backend.Strategies.Attacks.Damage
{
    public abstract class BaseDamageHandler
    {
        protected BaseDamageHandler? _next = null;

        public void SetNext(BaseDamageHandler next)
        {
            _next = next;
        }

        protected abstract int CalculateDamage(GameBoard gameBoard, int damageSum);
        public virtual int GetDamage(GameBoard gameBoard, int damageSum)
        {
            if (_next == null)
            {
                return damageSum;
            }
            else
            {
                return _next.CalculateDamage(gameBoard, damageSum);
            }
        }
    }
}
