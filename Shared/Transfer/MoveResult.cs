using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Transfer
{
    public class MoveResult
    {
        public int X { get; set; }
        public int Y { get; set; } 
        public bool IsHit { get; set; }

        public MoveResult(int x, int y, bool isHit)
        {
            X = x;
            Y = y;
            IsHit = isHit;
        }
    }
}
