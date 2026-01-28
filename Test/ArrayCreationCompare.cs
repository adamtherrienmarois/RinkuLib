using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Test;
[MemoryDiagnoser] // Crucial: shows how much memory you save
public class PoolVsNewBenchmark {
    [Params(0, 1, 2, 3, 4, 5, 6, 7, 8, 10, 11, 14, 15, 18, 20, 24, 25, 32, 50, 64, 128, 256, 1024)]
    public int Size;

    [Benchmark(Baseline = true)]
    public int NewArray() {
        var array = new object?[Size];
        var length = array.Length;
        return length;
    }

    [Benchmark]
    public int ArrayPoolShared() {
        var array = ArrayPool<object?>.Shared.Rent(Size);
        var length = array.Length;
        Array.Clear(array, 0, length);
        ArrayPool<object?>.Shared.Return(array);
        return length;
    }
}