using backend.Models.Entity.Ships;
using System.Collections;

namespace backend.Models.Entity.Iterator
{
    class GameCollection : IteratorAggregate
    {
        List<Game> _collection = new List<Game>();

        bool _direction = false;

        public void ReverseDirection()
        {
            _direction = !_direction;
        }

        public List<Game> getItems()
        {
            return _collection;
        }

        public void AddItem(Game item)
        {
            this._collection.Add(item);
        }

        public override IEnumerator GetEnumerator()
        {
            return new GameIterator(this, _direction);
        }
    }
}
