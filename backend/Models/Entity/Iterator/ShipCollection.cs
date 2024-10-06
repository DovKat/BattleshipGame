using backend.Models.Entity.Ships;
using System.Collections;

namespace backend.Models.Entity.Iterator
{
    public class ShipCollection : IteratorAggregate
    {
        List<Ship> _collection = new List<Ship>();

        bool _direction = false;

        public void ReverseDirection()
        {
            _direction = !_direction;
        }

        public List<Ship> getItems()
        {
            return _collection;
        }

        public void AddItem(Ship item)
        {
            this._collection.Add(item);
        }

        public override IEnumerator GetEnumerator()
        {
            return new ShipIterator(this, _direction);
        }
    }
}
