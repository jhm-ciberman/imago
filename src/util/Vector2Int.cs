using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace LifeSim
{
    public struct Vector2Int : IEquatable<Vector2Int>
    {
        public int x;
        public int y;

        public static Vector2Int one => new Vector2Int(1, 1);
        public static Vector2Int zero => new Vector2Int(0, 0);

        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2Int(uint x, uint y)
        {
            this.x = (int) x;
            this.y = (int) y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int operator+(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x + b.x, a.y + b.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int operator-(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x - b.x, a.y - b.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int operator*(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x * b.x, a.y * b.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int operator/(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x / b.x, a.y / b.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int operator*(Vector2Int a, int b)
        {
            return new Vector2Int(a.x * b, a.y * b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int operator/(Vector2Int a, int b)
        {
            return new Vector2Int(a.x / b, a.y / b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int operator*(Vector2Int a, uint b)
        {
            return new Vector2Int(a.x * (int) b, a.y * (int) b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int operator/(Vector2Int a, uint b)
        {
            return new Vector2Int(a.x / (int) b, a.y / (int) b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator==(Vector2Int lhs, Vector2Int rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator!=(Vector2Int lhs, Vector2Int rhs)
        {
            return !(lhs == rhs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector2(Vector2Int a)
        {
            return new Vector2(a.x, a.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? other)
        {
            if (!(other is Vector2Int)) return false;

            return this.Equals((Vector2Int)other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector2Int other)
        {
            return this.x == other.x && this.y == other.y;
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ (this.y.GetHashCode() << 16);
        }

        public override string? ToString()
        {
            return "(" + this.x + ", " + this.y + ")";
        }
    }
}