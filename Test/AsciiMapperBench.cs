using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using RinkuLib.Tools;

namespace Test;

//[MemoryDiagnoser]
//[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class AsciiMapperBenchmark {
    // ============================================================
    //  Test Key Sets
    // ============================================================

    [Params(8, 9)]
    public int KeysInd { get; set; }
    public static string[][] KeySets2 = [

    // 1. Same prefix, only last char changes (max collision on prefix logic)
    [
        "aaaaaaaaa0","aaaaaaaaa1","aaaaaaaaa2","aaaaaaaaa3","aaaaaaaaa4",
        "aaaaaaaaa5","aaaaaaaaa6","aaaaaaaaa7","aaaaaaaaa8","aaaaaaaaa9",
        "aaaaaaaaaA","aaaaaaaaaB","aaaaaaaaaC","aaaaaaaaaD","aaaaaaaaaE",
    ],

    // 2. Same length, tiny variations in middle character
    [
        "xxxxxAxxxxx",
        "xxxxxBxxxxx",
        "xxxxxCxxxxx",
        "xxxxxDxxxxx",
        "xxxxxExxxxx",
        "xxxxxFxxxxx",
        "xxxxxGxxxxx",
        "xxxxxHxxxxx",
        "xxxxxIxxxxx",
        "xxxxxJxxxxx",
    ],

    // 3. Prefix explosion – deep branching
    [
        "a", "aa", "aaa", "aaaa", "aaaaa", "aaaaaa",
        "aab", "aabb", "aabbb", "aabbbb", "aabbbbb",
        "aac", "aacd", "aacde", "aacdef", "aacdefg",
    ],

    // 4. Max ASCII spread, same length
    [
        "!a0~", "#B1`", "$c2_", "%D3^", "&e4]", "*F5[",
        "(g6)", ")H7(", "-i8=", "+J9+", "/k:/", "0L0?",
        "1m1>", "2N2<", "3o3;", "4P4,", "5q5.", "6R6-",
    ],

    // 5. Symbol-heavy, similar shapes
    [
        "!@#$", "!@#%", "!@#^", "!@#&", "!@#*", "!@#(",
        "@!#$", "@!#%", "@!#^", "@!#&", "@!#*", "@!#(",
    ],

    // 6. Repeated long prefix, large variety after
    [
        "prefix-aaaaaaaaaaa-00",
        "prefix-aaaaaaaaaaa-01",
        "prefix-aaaaaaaaaaa-02",
        "prefix-aaaaaaaaaaa-03",
        "prefix-aaaaaaaaaaa-04",
        "prefix-aaaaaaaaaaa-05",
        "prefix-aaaaaaaaaaa-06",
        "prefix-aaaaaaaaaaa-07",
        "prefix-aaaaaaaaaaa-08",
        "prefix-aaaaaaaaaaa-09",
        "prefix-aaaaaaaaaaa-0A",
        "prefix-aaaaaaaaaaa-0B",
        "prefix-aaaaaaaaaaa-0C",
        "prefix-aaaaaaaaaaa-0D",
        "prefix-aaaaaaaaaaa-0E",
        "prefix-aaaaaaaaaaa-0F",
    ],

    // 7. High-entropy pseudo-random ASCII (same length)
    [
        "A9f$-pQ@", "b_1!Z%t+", "Qm8@=c#^", "z4X*~R3!",
        "T5-v]a$2", "K@9{f>_!", "m1#q|L8&", "N0`Hs6+?",
        "w2E_}7F$", "J3^C!x9*"
    ],

    // 8. High-entropy variable length
    [
        "A$",
        "A$@",
        "A$@1",
        "A$@1f",
        "A$@1fZ",
        "A$@1fZ?",
        "A$@1fZ?3",
        "A$@1fZ?3%",
        "A$@1fZ?3%p",
        "A$@1fZ?3%p0"
    ],

    // 9. Numeric explosion – same signs but different lengths
    [
        "1","11","111","1111","11111","111111","1111111",
        "2","22","222","2222","22222","222222","2222222",
    ],
    //10
    [
        "name",
        "mode",
        "count"
    ],

    // 11 keys
    [
        "open",
        "close",
        "reload",
        "force",
        "reset"
    ],

    // 12 keys
    [
        "host",
        "user",
        "token",
        "path",
        "agent",
        "proto",
        "debug",
        "level"
    ],

    // 13 keys
    [
        "alpha",
        "bravo",
        "charlie",
        "delta",
        "echo",
        "foxtrot",
        "golf",
        "hotel",
        "india",
        "juliet",
        "kilo",
        "lima"
    ],

    // 14 keys
    [
        "one","two","three","four","five","six","seven","eight","nine","ten",
        "red","blue","green","yellow","black","white","brown","orange","pink","gray"
    ],

    // 15 keys
    [
        "id","code","type","flag","mode","user","auth","cfg",
        "pos","neg","min","max","low","high","sum","avg",
        "port","host","path","root","temp","data","file","size",
        "row","col","left","right","top","bottom","meta","info"
    ],

    // 16 keys
    [
        "itemA","itemB","itemC","itemD","itemE","itemF","itemG","itemH","itemI","itemJ",
        "tag1","tag2","tag3","tag4","tag5","tag6","tag7","tag8","tag9","tag10",
        "first","second","third","fourth","fifth","alpha1","alpha2","alpha3","alpha4","alpha5",
        "keyA","keyB","keyC","keyD","keyE","modeA","modeB","modeC","modeD","modeE",
        "flagA","flagB","flagC","flagD","flagE","valA","valB","valC","valD","valE"
    ],

    // 17 keys
    [
        "note","time","date","zone","lang","unit","rate","step","phase","level","score","count","limit","range","state",
        "icon","theme","style","color","shade","tone","hue","sat","bright","dim",
        "input","output","start","stop","pause","resume","next","prev","enter","exit","load","save","sync","apply","clear",
        "fast","slow","medium","small","large","tiny","huge","short","long","full","half","empty",
        "server","client","remote","local","proxy","cache","buffer","stream","pipe","node",
        "north","south","east","west","center","upper","lower"
    ],

    // 18 keys
    [
        "key01","key02","key03","key04","key05","key06","key07","key08","key09","key10",
        "key11","key12","key13","key14","key15","key16","key17","key18","key19","key20",
        "key21","key22","key23","key24","key25","key26","key27","key28","key29","key30",
        "alphaA","alphaB","alphaC","alphaD","alphaE","alphaF","alphaG","alphaH","alphaI","alphaJ",
        "betaA","betaB","betaC","betaD","betaE","betaF","betaG","betaH","betaI","betaJ",
        "cfgA","cfgB","cfgC","cfgD","cfgE","cfgF","cfgG","cfgH","cfgI","cfgJ",
        "optA","optB","optC","optD","optE","optF","optG","optH","optI","optJ",
        "mode01","mode02","mode03","mode04","mode05","mode06","mode07","mode08","mode09","mode10",
        "unit01","unit02","unit03","unit04","unit05","unit06","unit07","unit08","unit09","unit10"
    ],
];

    private static string[] EnsureCaseInsensitiveUnique(string[] keys) {
        var hs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var list = new List<string>(keys.Length);

        foreach (var k in keys)
            if (hs.Add(k))
                list.Add(k);

        return [.. list];
    }
    public static readonly string[] Keys_Symbolic = [
    "[abc]",
    "{abc}",
    "(test)",
    "<html>",
    "@key#value$",
    "H[6l",
    "H[5l",
    "|PIPE|",
    "~tilde~",
    "_underscore_"
];
    public static readonly string[] Keys_CasePatterns = [
    "abcde",
    "ABCDE",
    "AbCdE",
    "zzzZZZ",
    "HELLOworld",
    "mixedCASE",
    "xXxXxXxX",
    "UPPER_lower",
];
    public static string[] Keys_LongStrings(int count) {
        return Keys_RandomAscii(count, 256);
    }
    public static readonly string[] Keys_Simple = [
    "A", "B", "C", "Z",
    "AA", "AB", "AZ",
    "AAA", "ABC", "AZZ",
    "hello", "world", "test", "H[6l", "H[5l"
];
    public static string[] Keys_PrefixClustered(int count) {
        var arr = new string[count];
        for (int i = 0; i < count; i++)
            arr[i] = "PREFIX_DATA_XXXXXXXX_" + i.ToString("X4");
        return arr;
    }
    public static string[] Keys_CollisionPattern(int count) {
        var arr = new string[count];
        for (int i = 0; i < count; i++) {
            int v = i & 0x1F; // 32-variant cycle
            arr[i] = new string([
            (char)('A' + v),
            (char)('a' + v),
            (char)('A' + (v ^ 7)),
            (char)('a' + (v ^ 13)),
        ]) + "_END_" + (i >> 5);
        }
        return arr;
    }
    public static string[] Keys_RandomAscii(int count, int maxLen) {
        var rnd = new Random(12345);
        var arr = new string[count];

        for (int i = 0; i < count; i++) {
            int len = rnd.Next(1, maxLen + 1);
            char[] c = new char[len];
            for (int j = 0; j < len; j++)
                c[j] = (char)rnd.Next(32, 127);
            arr[i] = new string(c);
        }
        return arr;
    }
    public static string[] Keys_FixedLength(int count, int len) {
        var rnd = new Random(999);
        var arr = new string[count];

        for (int i = 0; i < count; i++) {
            char[] c = new char[len];
            for (int j = 0; j < len; j++)
                c[j] = (char)rnd.Next(32, 127);
            arr[i] = new string(c);
        }

        return arr;
    }
    private static string[] GenerateRandomAscii(int count, int minLen, int maxLen) {
        var r = new Random(1234);
        var arr = new string[count];
        for (int i = 0; i < count; i++) {
            int len = r.Next(minLen, maxLen + 1);
            var chars = new char[len];
            for (int j = 0; j < len; j++)
                chars[j] = (char)r.Next(32, 127);
            arr[i] = new string(chars);
        }
        return arr;
    }
    private static string MixCase(string s) {
        var r = new Random(s.GetHashCode());
        var c = s.ToCharArray();
        for (int i = 0; i < c.Length; i++)
            c[i] = r.Next(2) == 0 ? char.ToLowerInvariant(c[i]) : char.ToUpperInvariant(c[i]);
        return new string(c);
    }

    public static string[][] KeySets = [
        EnsureCaseInsensitiveUnique(
            [.. Enumerable.Range(0, 200).Select(i => "K" + i.ToString())]
        ),

        EnsureCaseInsensitiveUnique(
            GenerateRandomAscii(200, 1, 32)
        ),

        EnsureCaseInsensitiveUnique(
            GenerateRandomAscii(200, 64, 256)
        ),

        EnsureCaseInsensitiveUnique(
            [.. Enumerable.Range(0, 200).Select(i => "prefix_" + i.ToString("0000"))]
        ),

        EnsureCaseInsensitiveUnique(
            [.. Enumerable.Range(0, 200)
                .Select(i => "Key_" + i.ToString("X").PadLeft(4, '0'))
                .Select(MixCase)]
        ),

        EnsureCaseInsensitiveUnique(Keys_Simple),
        EnsureCaseInsensitiveUnique(Keys_PrefixClustered(512)),
        EnsureCaseInsensitiveUnique(Keys_CollisionPattern(512)),
        EnsureCaseInsensitiveUnique(Keys_RandomAscii(1024, 40)),
        EnsureCaseInsensitiveUnique(Keys_FixedLength(1024, 16)),
        EnsureCaseInsensitiveUnique(Keys_CasePatterns),
        EnsureCaseInsensitiveUnique(Keys_Symbolic),
        EnsureCaseInsensitiveUnique(Keys_LongStrings(256))
    ];

    // ============================================================
    //  Test Objects
    // ============================================================

    private Mapper mapper;
    private Dictionary<string, int> dict;
    private string wrongKey;

    [GlobalSetup]
    public void Setup() {
        // build mapper
        var keys = KeySets[KeysInd];
        mapper = Mapper.GetMapper(keys);

        // build dictionary (case-insensitive)
        dict = new Dictionary<string, int>(keys.Length, StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < keys.Length; i++)
            dict[keys[i]] = i;

        // pick one known-invalid key
        wrongKey = "invalid_key_!@#";
    }

    // ============================================================
    //  Mapper / Dictionary Creation
    // ============================================================

    //[BenchmarkCategory("Creation"), Benchmark(Baseline = true)]
    public Dictionary<string, int> MakeDictionary() {
        var keys = KeySets[KeysInd];
        var d = new Dictionary<string, int>(keys.Length, StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < keys.Length; i++)
            d[keys[i]] = i;
        return d;
    }

    //[BenchmarkCategory("Creation"), Benchmark]
    public Mapper MakeMapper() {
        var keys = KeySets[KeysInd];
        return Mapper.GetMapper(keys);
    }

    // ============================================================
    //  Valid Lookup
    // ============================================================

    [Benchmark(Baseline = true)]
    public int Dict_Valid() {
        var keys = KeySets[KeysInd];
        int result = 0;
        foreach (var k in keys) {
            dict.TryGetValue(k, out int v);
            result ^= v;
        }
        return result;
    }

    [Benchmark]
    public int Mapper_Valid() {
        var keys = KeySets[KeysInd];
        int result = 0;
        foreach (var k in keys) {
            result ^= mapper.GetIndex(k);
        }
        return result;
    }

    // ============================================================
    //  Invalid Lookup
    // ============================================================

    //[BenchmarkCategory("Invalid Lookup"), Benchmark(Baseline = true)]
    public bool Dict_Invalid() {
        return dict.TryGetValue(wrongKey, out _);
    }

    //[BenchmarkCategory("Invalid Lookup"), Benchmark]
    public bool Mapper_Invalid() {
        return mapper.GetIndex(wrongKey) >= 0;
    }
}
