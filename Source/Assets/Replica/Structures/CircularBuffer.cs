namespace Replica.Structures {
    public class CircularBuffer<T> {
        int _first;

        int _count;

        T[] _elements;

        public CircularBuffer(int capacity) => _elements = new T[capacity];

        public int Capacity => _elements.Length;

        public int Count => _count;

        public T[] GetArray() => _elements;

        public int HeadIndex => _first;

        public void Add(T item) {
            var index = (_first + _count) % _elements.Length;
            _elements[index] = item;

            if (_count == _elements.Length) {
                _first = (_first + 1) % _elements.Length;
            } else {
                ++_count;
            }
        }

        public void Clear() {
            _first = 0;
            _count = 0;
        }

        public T this[int i] {
            get => _elements[(_first + i) % _elements.Length];
            set => _elements[(_first + i) % _elements.Length] = value;
        }

        public void Reset(int headIndex, int count) {
            _first = headIndex;
            _count = count;
        }
    }
}