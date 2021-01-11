namespace LifeSim.Assets
{
    public class BinPacker
    {
        private class Node
        {
            public uint x;
            public uint y;
            public uint width;
            public uint height;
            public Node? down = null;
            public Node? right = null;

            public Node(uint x, uint y, uint width, uint height)
            {
                this.x = x;
                this.y = y;
                this.width = width;
                this.height = height;
            }

            public void Split(uint width, uint height) 
            {
                this.down  = new Node( this.x        , this.y + height, this.width        , this.height - height );
                this.right = new Node( this.x + width, this.y         , this.width - width, height               );
            }

            public Node? Find(uint width, uint height) 
            {
                if (this.right != null) {
                    return this.right.Find(width, height) ?? this.down?.Find(width, height);
                } else if ((width <= this.width) && (height <= this.height)) {
                    return this;
                } else {
                    return null;
                }
            }

        }

        public class BinRect<T>
        {
            public readonly uint width;
            public readonly uint height;
            public readonly T element;
            public readonly uint area;

            public BinRect(uint width, uint height, T element)
            {
                this.width = width;
                this.height = height;
                this.element = element;
                this.area = width * height;
            }
        }

        public struct Result<T>
        {
            public RectInt rect;
            public T element;

            public Result(RectInt rect, T element)
            {
                this.rect = rect;
                this.element = element;
            }
        }

        private Node _root;

        public BinPacker(uint atlasSize)
        {
            this._root = new Node(0, 0, atlasSize, atlasSize);
        }

        public Result<T>[] Fit<T>(BinRect<T>[] rects)
        {
            Result<T>[] results = new Result<T>[rects.Length];

            System.Array.Sort(rects, (BinRect<T> a, BinRect<T> b) => ((int) b.area - (int) a.area));

            int i = 0;
            foreach (var rect in rects) {
                Node? node = this._root.Find(rect.width, rect.height);
                if (node != null) {
                    node.Split(rect.width, rect.height);
                    results[i++] = new Result<T>(new RectInt((int) node.x, (int) node.y, (int) node.width, (int) node.height), rect.element);
                } else {
                    throw new System.Exception("Cannot fit the rectangles in the atlas");
                }
            }

            return results;
        }
    }
}