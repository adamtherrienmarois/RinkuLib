using BenchmarkDotNet.Attributes;

namespace Test;

[MemoryDiagnoser]
public unsafe class ArrayCheckBench {
    private const int N = 10_000_000;

    private int[] _ints = null!;
    private long[] _longs = null!;
    private object[] _objects = null!;
    private int[] _order = null!;

    [GlobalSetup]
    public void Setup() {
        _ints = new int[N];
        _longs = new long[N];
        _objects = new object[N];
        _order = new int[N];

        for (int i = 0; i < N; i++) {
            _order[i] = i;
            if ((i & 1) == 0) {
                _ints[i] = 1;
                _longs[i] = 1;
                _objects[i] = new object();
            }
        }

        // shuffle access order (Fisher–Yates)
        var rng = new Random(123);
        for (int i = N - 1; i > 0; i--) {
            int j = rng.Next(i + 1);
            (_order[i], _order[j]) = (_order[j], _order[i]);
        }
    }

    // -------- int --------

    [Benchmark]
    public int Int_Safe_Random() {
        int sum = 0;
        var arr = _ints;
        var idx = _order;
        for (int i = 0; i < idx.Length; i++)
            if (arr[idx[i]] != 0)
                sum++;
        return sum;
    }

    [Benchmark]
    public int Int_Unsafe_Random() {
        int sum = 0;
        var idx = _order;
        fixed (int* p = _ints) {
            for (int i = 0; i < idx.Length; i++)
                if (p[idx[i]] != 0)
                    sum++;
        }
        return sum;
    }

    // -------- long --------

    [Benchmark]
    public int Long_Safe_Random() {
        int sum = 0;
        var arr = _longs;
        var idx = _order;
        for (int i = 0; i < idx.Length; i++)
            if (arr[idx[i]] != 0)
                sum++;
        return sum;
    }

    [Benchmark]
    public int Long_Unsafe_Random() {
        int sum = 0;
        var idx = _order;
        fixed (long* p = _longs) {
            for (int i = 0; i < idx.Length; i++)
                if (p[idx[i]] != 0)
                    sum++;
        }
        return sum;
    }

    // -------- object --------

    [Benchmark]
    public int Object_Safe_Random() {
        int sum = 0;
        var arr = _objects;
        var idx = _order;
        for (int i = 0; i < idx.Length; i++)
            if (arr[idx[i]] != null)
                sum++;
        return sum;
    }

    [Benchmark]
    public int Object_Unsafe_Random() {
        int sum = 0;
        var idx = _order;
        fixed (object* p = _objects) {
            for (int i = 0; i < idx.Length; i++)
                if (p[idx[i]] != null)
                    sum++;
        }
        return sum;
    }
}