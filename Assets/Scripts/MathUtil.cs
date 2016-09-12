using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathUtil
{
    public struct Vector2I
    {
        public int x;
        public int y;

        public Vector2I(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        static public Vector2I operator +(Vector2I first, Vector2I second)
        {
            return new Vector2I(first.x + second.x, first.y + second.y);
        }
        static public Vector2I operator -(Vector2I first, Vector2I second)
        {
            return new Vector2I(first.x - second.x, first.y - second.y);
        }
    }

    public struct Vector3I
    {
        public int x;
        public int y;
        public int z;

        public Vector3I(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        static public Vector3I operator +(Vector3I first, Vector3I second)
        {
            return new Vector3I(first.x + second.x, first.y + second.y, first.z /* do not Add Z, it stays! */);
        }

        public Vector2I ToVector2I()
        {
            return new Vector2I(x, y);
        }
    }
}


