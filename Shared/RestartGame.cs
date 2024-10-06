using backend.Models.Entity.Ships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    [Serializable]
    public class RestartGame
    {
        public ShipType ShipType { get; set; }
        public int PlacedX { get; set; } 
        public int PlacedY { get; set;}
        public List<ShipCoordinate> Coordinates { get; set; }

        public int ID { get; set; }
    }
}
