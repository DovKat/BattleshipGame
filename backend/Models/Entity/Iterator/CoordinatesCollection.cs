using backend.Models.Entity.Ships;
using System.Collections;

namespace backend.Models.Entity.Iterator
{
    public class ShipCoordinatesCollection : IteratorAggregate
    {
        List<ShipCoordinate> _collection = new List<ShipCoordinate>();

        bool _direction = false;

        public void ReverseDirection()
        {
            _direction = !_direction;
        }

        public List<ShipCoordinate> getItems()
        {
            return _collection;
        }

        public void AddItem(ShipCoordinate item)
        {
            this._collection.Add(item);
        }

        public override IEnumerator GetEnumerator()
        {
            return new ShipCoordinatesIterator(this, _direction);
        }
    }
}
