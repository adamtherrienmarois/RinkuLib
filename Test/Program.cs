using System.Data;
using System.Data.Common;
using BenchmarkDotNet.Running;
using Dapper;
using Microsoft.Data.SqlClient;
using RinkuLib.Commands;
using RinkuLib.Queries;
using RinkuLib.Tools;
using Test;
var cnnStr = "Data Source=localhost;Initial Catalog=Miliem;Persist Security Info=True;User ID=sa;Password=allo123; TrustServerCertificate=true;";
/*
var testQuery = "SELECT ID, MR FROM Batches WHERE MR LIKE CONCAT('%', @MR, '%') OR ID = @ID";
using var cnn1 = new SqlConnection(cnnStr);
using var cmd1 = cnn1.CreateCommand();
cmd1.CommandText = testQuery;
var p1 = cmd1.CreateParameter();
p1.ParameterName = "@MR";
p1.Value = "MR24087";
var p2 = cmd1.CreateParameter();
p2.ParameterName = "@ID";
p2.Value = 162L;

cmd1.Parameters.Add(p1);
cmd1.Parameters.Add(p2);
cnn1.Open();
var rd1 = cmd1.ExecuteReader();
while (rd1.Read()) {
    Console.WriteLine(rd1.GetInt32(0));
    Console.WriteLine(rd1.GetString(1));
}
rd1.Dispose();
var parameters1 = cmd1.Parameters;
for (int i = 0; i < parameters1.Count; i++) {
    var pT = parameters1[i];
    Console.WriteLine(pT.ToString());
}
return;*/
var queryString = @"SELECT b.ID, sr.Quarantine, b.SourceID, b.BoxTypeID, bt.Name AS BoxTypeValue, sr.Name AS BatchNB, b.ProductID, CONCAT(pr.Code, ':', pr.Name) AS ProductValue, b.Date, b.Gross, b.Tare, (b.Gross - b.Tare) AS Net, b.UserID, CASE WHEN b.UserID IS NULL THEN NULL ELSE CONCAT(u.Firstname, ' ', u.Lastname) END AS UserValue, b.StatusID, CASE WHEN b.StatusID IS NULL THEN NULL ELSE CONCAT(s.Name, ' (', sg.Name, ')') END AS StatusValue, sg.Color AS StatusColor, b.StationID, st.Name AS StationValue, 
Count(*) AS Count&, Sum(b.Gross - b.Tare) AS Sum 
FROM Boxes b INNER JOIN vwSources sr ON sr.BoxTypeID = b.BoxTypeID AND sr.SourceID = b.SourceID LEFT JOIN Stations st ON st.ID = b.StationID INNER JOIN BoxTypes bt ON bt.ID = b.BoxTypeID INNER JOIN Products pr ON pr.ID = b.ProductID LEFT JOIN Statuses s ON s.ID = b.StatusID /*StatusValue|StatusColor|@Status*/LEFT JOIN StatusGroups sg ON sg.ID = s.StatusGroupID /*UserValue*/LEFT JOIN Users u ON u.ID = b.UserID 
WHERE sr.Valid = 1 AND b.Valid = ?@IsValid AND /*NoStatus*/b.StatusID IS NULL AND b.SourceID = ?@SourceID AND b.BoxTypeID = ?@BoxTypeID AND b.ID = ?@ID AND b.Date >= ?@DateMin AND b.Date <= ?@DateMax AND sr.Name LIKE CONCAT('%', ?@BatchNB, '%') AND st.Name LIKE CONCAT('%', ?@Station, '%') AND pr.Code LIKE ?@Product &OR pr.Name LIKE @Product AND CONCAT(s.Name, ' (', sg.Name, ')') LIKE CONCAT('%', ?@Status, '%') 
ORDER BY ?@Order_N @Dir_R /*@Order*/OFFSET ?@Offset_N ROWS FETCH NEXT @PageSize_N ROWS ONLY";
//KeyValuePair<string, string?>
var query = QueryCommand<BoxItem>.New(queryString, true);

//QueryFactory.SegmentQuery(queryString);
var builder = query.StartBuilder();
using var cnn = new SqlConnection(cnnStr);
/*using var cmdtest = cnn.CreateCommand();
cnn.Open();
cmdtest.CommandText = "SELECT StatusID FROM Boxes WHERE ID = 34124";
using (var readerTest = cmdtest.ExecuteReader(CommandBehavior.SingleRow)) { 
    readerTest.Read();
    var statusid = readerTest.GetInt32(0);
}*/
builder.Use("ID");
builder.Use("Quarantine");
builder.Use("SourceID");
builder.Use("BoxTypeID");
builder.Use("BoxTypeValue");
builder.Use("BatchNB");
builder.Use("ProductID");
builder.Use("ProductValue");
builder.Use("Date");
builder.Use("Gross");
builder.Use("Tare");
builder.Use("Net");
builder.Use("UserID");
builder.Use("UserValue");
builder.Use("StatusID");
builder.Use("StatusValue");
builder.Use("StatusColor");
builder.Use("StationID");
builder.Use("StationValue");
builder.Use("@IsValid", true);
//builder.Use("NoStatus");
//builder.Use("@SourceID", 1);
//builder.Use("@BoxTypeID", 1);
builder.Use("@ID", 4);//34119
//builder.Use("@DateMin", DateTime.Now);
//builder.Use("@DateMax", DateTime.Now);
//builder.Use("@BatchNB", "Test");
//builder.Use("@Station", "Test");
//builder.Use("@Product", "Test");
//builder.Use("@Status", "Con");
builder.Use("@Order", builder.GetRelativeIndex("StationID") + 1);
builder.Use("@Dir", "");
builder.Use("@Offset", 0);
builder.Use("@PageSize", 25);

var boxes = builder.QueryMultiple(cnn, out var cmd/*, r => {
    var id = r.GetFieldValue<int?>(14);
    Reference<int, string> status = !id.HasValue ? default : new(id.Value, r.GetFieldValue<string>(15));
    return new BoxItem() {
        Status = status
    };
}*/);
foreach (var box in boxes) {
    Console.WriteLine(box);
}
builder.ResetSelects();
builder.Use("Count");
//builder.Use("Sum");
builder.Remove("@Order");
cmd.CommandText = builder.GetQueryText();
var (count, sum) = await cmd.QuerySingleAsync(r => new ValueTuple<int, decimal>(r.GetInt32(0), r.GetDecimal(1)), CommandBehavior.SequentialAccess);
Console.WriteLine(count);
Console.WriteLine(sum);
cmd.Dispose();
/*
using var reader = builder.ExecuteReader(cnn);
while (reader.Read()) {
    Console.WriteLine($"{reader.GetValue(1)} | {reader.GetValue(2)}");
}*/
//Console.WriteLine(builder.GetQuery());
//BenchmarkRunner.Run<AsciiMapperBenchmark>();
/*
| Method         | Keys         | Mean           | Error          | StdDev          | Median         | Ratio     | RatioSD  | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------- |------------- |---------------:|---------------:|----------------:|---------------:|----------:|---------:|-------:|-------:|----------:|------------:|
| MakeDictionary | String[6]    |      57.448 ns |      0.3302 ns |       0.2927 ns |      57.545 ns |      1.00 |     0.01 | 0.0523 |      - |     328 B |        1.00 |
| MakeDictionary | String[10]   |      83.993 ns |      0.5878 ns |       0.5499 ns |      84.269 ns |      1.46 |     0.01 | 0.0701 |      - |     440 B |        1.34 |
| MakeDictionary | String[15]   |     108.972 ns |      0.7219 ns |       0.6753 ns |     109.153 ns |      1.90 |     0.01 | 0.0969 | 0.0454 |     608 B |        1.85 |
| MakeDictionary | String[200]  |   1,127.530 ns |     10.4512 ns |       9.7760 ns |   1,126.457 ns |     19.63 |     0.19 | 1.0872 | 0.0267 |    6824 B |       20.80 |
| MakeDictionary | String[200]  |   1,643.401 ns |     11.0751 ns |       9.8178 ns |   1,645.134 ns |     28.61 |     0.22 | 1.0872 | 0.0267 |    6824 B |       20.80 |
| MakeDictionary | String[200]  |   1,937.597 ns |     11.9790 ns |      11.2051 ns |   1,942.534 ns |     33.73 |     0.25 | 1.0872 | 0.0267 |    6824 B |       20.80 |
| MakeDictionary | String[200]  |   2,190.989 ns |      8.3980 ns |       7.8555 ns |   2,189.452 ns |     38.14 |     0.23 | 1.0872 | 0.0267 |    6824 B |       20.80 |
| MakeDictionary | String[512]  |   4,713.743 ns |     45.2959 ns |      42.3698 ns |   4,734.800 ns |     82.05 |     0.82 | 2.3346 | 0.1221 |   14720 B |       44.88 |
| MakeDictionary | String[512]  |   7,476.063 ns |     31.9255 ns |      28.3012 ns |   7,470.003 ns |    130.14 |     0.80 | 2.3422 | 0.1221 |   14720 B |       44.88 |
| MakeDictionary | String[1024] |  12,426.441 ns |    137.6709 ns |     128.7774 ns |  12,436.832 ns |    216.31 |     2.42 | 4.9133 | 0.4883 |   31016 B |       94.56 |
| MakeDictionary | String[1019] |  13,239.739 ns |    122.3229 ns |      95.5017 ns |  13,251.656 ns |    230.47 |     1.96 | 4.9133 | 0.4883 |   31016 B |       94.56 |
| MakeDictionary | String[200]  |  14,312.208 ns |     85.4783 ns |      79.9565 ns |  14,333.643 ns |    249.14 |     1.82 | 1.0834 | 0.0153 |    6824 B |       20.80 |
| MakeDictionary | String[256]  |  15,235.259 ns |     51.9189 ns |      48.5649 ns |  15,234.250 ns |    265.21 |     1.54 | 1.3123 | 0.0305 |    8336 B |       25.41 |
| MakeMapper     | String[200]  |  17,679.803 ns |    178.0666 ns |     166.5636 ns |  17,715.765 ns |    307.76 |     3.19 | 0.7629 | 0.0153 |    4192 B |       12.78 |
| MakeMapper     | String[200]  |  18,852.539 ns |    160.8407 ns |     125.5739 ns |  18,846.895 ns |    328.17 |     2.65 | 0.7324 | 0.1526 |    4136 B |       12.61 |
| MakeMapper     | String[200]  |  20,119.973 ns |    172.5206 ns |     161.3759 ns |  20,111.191 ns |    350.24 |     3.22 | 0.8850 | 0.0305 |    5032 B |       15.34 |
| MakeMapper     | String[200]  |  39,998.380 ns |    245.6781 ns |     205.1524 ns |  39,941.367 ns |    696.27 |     4.86 | 1.7090 | 0.0610 |   10160 B |       30.98 |
| MakeMapper     | String[512]  |  62,700.151 ns |    265.1779 ns |     235.0733 ns |  62,721.082 ns |  1,091.45 |     6.68 | 1.9531 | 0.1221 |   10848 B |       33.07 |
| MakeMapper     | String[10]   |  82,646.030 ns | 39,292.9389 ns | 115,856.1254 ns |   2,539.669 ns |  1,438.66 | 2,007.40 | 0.1297 |      - |     728 B |        2.22 |
| MakeMapper     | String[512]  | 185,974.870 ns |  1,927.2864 ns |   1,609.3714 ns | 186,555.054 ns |  3,237.35 |    31.37 | 4.1504 | 0.2441 |   24992 B |       76.20 |
| MakeMapper     | String[256]  | 198,311.502 ns | 11,509.9156 ns |  31,508.2107 ns | 184,535.901 ns |  3,452.10 |   545.81 | 1.2207 |      - |    8072 B |       24.61 |
| MakeMapper     | String[200]  | 210,772.418 ns | 14,935.6176 ns |  42,612.1605 ns | 186,492.517 ns |  3,669.01 |   738.32 | 0.9766 |      - |    6912 B |       21.07 |
| MakeMapper     | String[6]    | 216,301.759 ns | 21,188.7357 ns |  62,475.4698 ns | 215,490.032 ns |  3,765.26 | 1,082.65 | 0.0610 | 0.0038 |     280 B |        0.85 |
| MakeMapper     | String[15]   | 343,932.484 ns | 32,945.1503 ns |  97,139.5259 ns | 343,128.046 ns |  5,986.99 | 1,683.36 | 0.1450 | 0.0076 |     736 B |        2.24 |
| MakeMapper     | String[1019] | 548,554.806 ns |  4,072.4675 ns |   3,610.1363 ns | 548,377.344 ns |  9,548.94 |    76.87 | 3.9063 | 0.9766 |   27248 B |       83.07 |
| MakeMapper     | String[1024] | 578,971.243 ns |  5,530.5210 ns |   5,173.2528 ns | 578,194.824 ns | 10,078.41 |   100.41 | 3.9063 |      - |   24256 B |       73.95 |
|                |              |                |                |                 |                |           |          |        |        |           |             |
| Mapper_Invalid | String[256]  |       1.056 ns |      0.0152 ns |       0.0143 ns |       1.055 ns |      0.15 |     0.00 |      - |      - |         - |          NA |
| Mapper_Invalid | String[200]  |       1.850 ns |      0.0088 ns |       0.0074 ns |       1.850 ns |      0.26 |     0.00 |      - |      - |         - |          NA |
| Mapper_Invalid | String[10]   |       1.887 ns |      0.0131 ns |       0.0122 ns |       1.886 ns |      0.26 |     0.00 |      - |      - |         - |          NA |
| Mapper_Invalid | String[1024] |       1.888 ns |      0.0146 ns |       0.0137 ns |       1.889 ns |      0.26 |     0.00 |      - |      - |         - |          NA |
| Mapper_Invalid | String[15]   |       1.892 ns |      0.0173 ns |       0.0162 ns |       1.890 ns |      0.26 |     0.00 |      - |      - |         - |          NA |
| Mapper_Invalid | String[6]    |       1.897 ns |      0.0126 ns |       0.0118 ns |       1.899 ns |      0.26 |     0.00 |      - |      - |         - |          NA |
| Mapper_Invalid | String[1019] |       1.970 ns |      0.0136 ns |       0.0127 ns |       1.971 ns |      0.27 |     0.00 |      - |      - |         - |          NA |
| Mapper_Invalid | String[200]  |       2.014 ns |      0.0196 ns |       0.0183 ns |       2.016 ns |      0.28 |     0.00 |      - |      - |         - |          NA |
| Mapper_Invalid | String[200]  |       2.029 ns |      0.0212 ns |       0.0198 ns |       2.038 ns |      0.28 |     0.00 |      - |      - |         - |          NA |
| Mapper_Invalid | String[200]  |       2.464 ns |      0.0290 ns |       0.0257 ns |       2.469 ns |      0.34 |     0.00 |      - |      - |         - |          NA |
| Mapper_Invalid | String[200]  |       2.551 ns |      0.0336 ns |       0.0298 ns |       2.564 ns |      0.36 |     0.00 |      - |      - |         - |          NA |
| Mapper_Invalid | String[512]  |       2.568 ns |      0.0120 ns |       0.0112 ns |       2.568 ns |      0.36 |     0.00 |      - |      - |         - |          NA |
| Mapper_Invalid | String[512]  |       2.577 ns |      0.0145 ns |       0.0136 ns |       2.579 ns |      0.36 |     0.00 |      - |      - |         - |          NA |
| Dict_Invalid   | String[10]   |       7.181 ns |      0.0617 ns |       0.0577 ns |       7.190 ns |      1.00 |     0.01 |      - |      - |         - |          NA |
| Dict_Invalid   | String[1024] |       7.227 ns |      0.0637 ns |       0.0565 ns |       7.243 ns |      1.01 |     0.01 |      - |      - |         - |          NA |
| Dict_Invalid   | String[15]   |       7.239 ns |      0.0207 ns |       0.0184 ns |       7.240 ns |      1.01 |     0.01 |      - |      - |         - |          NA |
| Dict_Invalid   | String[6]    |       7.244 ns |      0.0223 ns |       0.0198 ns |       7.248 ns |      1.01 |     0.01 |      - |      - |         - |          NA |
| Dict_Invalid   | String[512]  |       7.252 ns |      0.0202 ns |       0.0189 ns |       7.258 ns |      1.01 |     0.01 |      - |      - |         - |          NA |
| Dict_Invalid   | String[200]  |       7.253 ns |      0.0306 ns |       0.0286 ns |       7.249 ns |      1.01 |     0.01 |      - |      - |         - |          NA |
| Dict_Invalid   | String[512]  |       7.255 ns |      0.0198 ns |       0.0185 ns |       7.262 ns |      1.01 |     0.01 |      - |      - |         - |          NA |
| Dict_Invalid   | String[200]  |       8.173 ns |      0.0222 ns |       0.0197 ns |       8.165 ns |      1.14 |     0.01 |      - |      - |         - |          NA |
| Dict_Invalid   | String[200]  |       8.186 ns |      0.0229 ns |       0.0214 ns |       8.187 ns |      1.14 |     0.01 |      - |      - |         - |          NA |
| Dict_Invalid   | String[200]  |       8.254 ns |      0.0491 ns |       0.0459 ns |       8.250 ns |      1.15 |     0.01 |      - |      - |         - |          NA |
| Dict_Invalid   | String[256]  |       8.918 ns |      0.0273 ns |       0.0256 ns |       8.913 ns |      1.24 |     0.01 |      - |      - |         - |          NA |
| Dict_Invalid   | String[1019] |       9.130 ns |      0.0711 ns |       0.0666 ns |       9.139 ns |      1.27 |     0.01 |      - |      - |         - |          NA |
| Dict_Invalid   | String[200]  |      10.250 ns |      0.0338 ns |       0.0300 ns |      10.253 ns |      1.43 |     0.01 |      - |      - |         - |          NA |
|                |              |                |                |                 |                |           |          |        |        |           |             |
| Mapper_Valid   | String[6]    |      10.148 ns |      0.0790 ns |       0.0739 ns |      10.187 ns |      0.25 |     0.00 |      - |      - |         - |          NA |
| Mapper_Valid   | String[10]   |      19.517 ns |      0.1696 ns |       0.1586 ns |      19.541 ns |      0.48 |     0.01 |      - |      - |         - |          NA |
| Mapper_Valid   | String[15]   |      29.892 ns |      0.3333 ns |       0.3118 ns |      29.923 ns |      0.73 |     0.01 |      - |      - |         - |          NA |
| Dict_Valid     | String[6]    |      41.007 ns |      0.4215 ns |       0.3942 ns |      41.117 ns |      1.00 |     0.01 |      - |      - |         - |          NA |
| Dict_Valid     | String[10]   |      64.644 ns |      0.3725 ns |       0.3111 ns |      64.770 ns |      1.58 |     0.02 |      - |      - |         - |          NA |
| Dict_Valid     | String[15]   |      81.982 ns |      0.5828 ns |       0.5452 ns |      81.962 ns |      2.00 |     0.02 |      - |      - |         - |          NA |
| Mapper_Valid   | String[200]  |     371.180 ns |      2.2845 ns |       2.1369 ns |     371.775 ns |      9.05 |     0.10 |      - |      - |         - |          NA |
| Mapper_Valid   | String[200]  |     411.936 ns |      4.3164 ns |       4.0376 ns |     413.251 ns |     10.05 |     0.13 |      - |      - |         - |          NA |
| Mapper_Valid   | String[256]  |     469.648 ns |      3.1284 ns |       2.9264 ns |     471.192 ns |     11.45 |     0.13 |      - |      - |         - |          NA |
| Mapper_Valid   | String[200]  |     494.081 ns |      1.8909 ns |       1.7688 ns |     494.309 ns |     12.05 |     0.12 |      - |      - |         - |          NA |
| Mapper_Valid   | String[200]  |     500.343 ns |      3.0286 ns |       2.8329 ns |     499.681 ns |     12.20 |     0.13 |      - |      - |         - |          NA |
| Mapper_Valid   | String[200]  |     665.088 ns |      3.2944 ns |       3.0816 ns |     664.022 ns |     16.22 |     0.17 |      - |      - |         - |          NA |
| Dict_Valid     | String[200]  |     846.595 ns |      3.6683 ns |       3.4314 ns |     846.699 ns |     20.65 |     0.21 |      - |      - |         - |          NA |
| Mapper_Valid   | String[512]  |   1,288.379 ns |     10.7333 ns |      10.0399 ns |   1,288.462 ns |     31.42 |     0.38 |      - |      - |         - |          NA |
| Dict_Valid     | String[200]  |   1,312.142 ns |      5.6616 ns |       5.2959 ns |   1,314.315 ns |     32.00 |     0.32 |      - |      - |         - |          NA |
| Dict_Valid     | String[200]  |   1,587.744 ns |     15.8420 ns |      14.8186 ns |   1,591.383 ns |     38.72 |     0.50 |      - |      - |         - |          NA |
| Mapper_Valid   | String[512]  |   1,705.264 ns |      9.4984 ns |       8.4201 ns |   1,703.024 ns |     41.59 |     0.44 |      - |      - |         - |          NA |
| Dict_Valid     | String[200]  |   2,046.474 ns |      8.0248 ns |       7.5064 ns |   2,046.244 ns |     49.91 |     0.50 |      - |      - |         - |          NA |
| Mapper_Valid   | String[1019] |   2,240.879 ns |     12.2237 ns |      11.4341 ns |   2,243.580 ns |     54.65 |     0.58 |      - |      - |         - |          NA |
| Mapper_Valid   | String[1024] |   2,880.273 ns |     20.2539 ns |      17.9546 ns |   2,878.673 ns |     70.25 |     0.78 |      - |      - |         - |          NA |
| Dict_Valid     | String[512]  |   4,147.265 ns |     34.4116 ns |      30.5050 ns |   4,146.888 ns |    101.15 |     1.19 |      - |      - |         - |          NA |
| Dict_Valid     | String[512]  |   7,068.297 ns |     27.1989 ns |      25.4419 ns |   7,062.661 ns |    172.38 |     1.72 |      - |      - |         - |          NA |
| Dict_Valid     | String[1024] |  10,374.604 ns |     59.6590 ns |      55.8051 ns |  10,391.765 ns |    253.02 |     2.70 |      - |      - |         - |          NA |
| Dict_Valid     | String[1019] |  12,074.352 ns |    101.5828 ns |      95.0207 ns |  12,076.320 ns |    294.47 |     3.55 |      - |      - |         - |          NA |
| Dict_Valid     | String[200]  |  14,027.331 ns |     49.9078 ns |      46.6838 ns |  14,019.231 ns |    342.10 |     3.38 |      - |      - |         - |          NA |
| Dict_Valid     | String[256]  |  14,976.554 ns |    122.6019 ns |     114.6819 ns |  14,956.994 ns |    365.25 |     4.35 |      - |      - |         - |          NA |
*/
/*
| Method       | KeysInd | Mean       | Error     | StdDev     | Median     | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------- |-------- |-----------:|----------:|-----------:|-----------:|------:|--------:|----------:|------------:|
| Dict_Valid   | 0       | 101.593 ns | 0.2776 ns |  0.2597 ns | 101.652 ns |  1.00 |    0.00 |         - |          NA |
| Mapper_Valid | 0       |  26.916 ns | 0.5431 ns |  0.7062 ns |  27.402 ns |  0.26 |    0.01 |         - |          NA |
|              |         |            |           |            |            |       |         |           |             |
| Dict_Valid   | 1       |  85.491 ns | 1.7005 ns |  2.3276 ns |  86.173 ns |  1.00 |    0.04 |         - |          NA |
| Mapper_Valid | 1       |  18.691 ns | 0.2352 ns |  0.2200 ns |  18.699 ns |  0.22 |    0.01 |         - |          NA |
|              |         |            |           |            |            |       |         |           |             |
| Dict_Valid   | 2       | 118.468 ns | 2.3939 ns |  4.7253 ns | 118.243 ns |  1.00 |    0.06 |         - |          NA |
| Mapper_Valid | 2       |  38.875 ns | 0.7682 ns |  0.9434 ns |  39.037 ns |  0.33 |    0.02 |         - |          NA |
|              |         |            |           |            |            |       |         |           |             |
| Dict_Valid   | 3       |  98.104 ns | 5.8194 ns | 15.8320 ns | 104.512 ns |  1.03 |    0.25 |         - |          NA |
| Mapper_Valid | 3       |  32.270 ns | 0.4877 ns |  0.4562 ns |  32.417 ns |  0.34 |    0.06 |         - |          NA |
|              |         |            |           |            |            |       |         |           |             |
| Dict_Valid   | 4       |  51.232 ns | 0.1776 ns |  0.1387 ns |  51.238 ns |  1.00 |    0.00 |         - |          NA |
| Mapper_Valid | 4       |  28.774 ns | 0.4103 ns |  0.3838 ns |  28.687 ns |  0.56 |    0.01 |         - |          NA |
|              |         |            |           |            |            |       |         |           |             |
| Dict_Valid   | 5       | 202.932 ns | 1.7407 ns |  1.4535 ns | 203.265 ns |  1.00 |    0.01 |         - |          NA |
| Mapper_Valid | 5       |  28.861 ns | 0.3228 ns |  0.2862 ns |  28.754 ns |  0.14 |    0.00 |         - |          NA |
|              |         |            |           |            |            |       |         |           |             |
| Dict_Valid   | 6       |  64.872 ns | 0.9213 ns |  0.8617 ns |  65.062 ns |  1.00 |    0.02 |         - |          NA |
| Mapper_Valid | 6       |  18.538 ns | 0.2756 ns |  0.2578 ns |  18.536 ns |  0.29 |    0.01 |         - |          NA |
|              |         |            |           |            |            |       |         |           |             |
| Dict_Valid   | 7       |  68.638 ns | 0.6252 ns |  0.5848 ns |  68.845 ns |  1.00 |    0.01 |         - |          NA |
| Mapper_Valid | 7       |  16.060 ns | 0.1895 ns |  0.1773 ns |  16.054 ns |  0.23 |    0.00 |         - |          NA |
|              |         |            |           |            |            |       |         |           |             |
| Dict_Valid   | 8       |  76.092 ns | 0.5863 ns |  0.5484 ns |  76.079 ns |  1.00 |    0.01 |         - |          NA |
| Mapper_Valid | 8       |  25.568 ns | 0.3420 ns |  0.3199 ns |  25.477 ns |  0.34 |    0.00 |         - |          NA |
|              |         |            |           |            |            |       |         |           |             |
| Dict_Valid   | 9       |  14.773 ns | 0.1558 ns |  0.1457 ns |  14.771 ns |  1.00 |    0.01 |         - |          NA |
| Mapper_Valid | 9       |   5.928 ns | 0.0889 ns |  0.0832 ns |   5.953 ns |  0.40 |    0.01 |         - |          NA |
|              |         |            |           |            |            |       |         |           |             |
| Dict_Valid   | 10      |  24.644 ns | 0.3418 ns |  0.3197 ns |  24.695 ns |  1.00 |    0.02 |         - |          NA |
| Mapper_Valid | 10      |   8.690 ns | 0.0696 ns |  0.0617 ns |   8.694 ns |  0.35 |    0.01 |         - |          NA |
|              |         |            |           |            |            |       |         |           |             |
| Dict_Valid   | 11      |  39.794 ns | 0.3079 ns |  0.2729 ns |  39.803 ns |  1.00 |    0.01 |         - |          NA |
| Mapper_Valid | 11      |  15.049 ns | 0.2171 ns |  0.2030 ns |  15.083 ns |  0.38 |    0.01 |         - |          NA |
|              |         |            |           |            |            |       |         |           |             |
| Dict_Valid   | 12      |  72.641 ns | 0.7048 ns |  0.6593 ns |  72.541 ns |  1.00 |    0.01 |         - |          NA |
| Mapper_Valid | 12      |  21.834 ns | 0.2815 ns |  0.2633 ns |  21.829 ns |  0.30 |    0.00 |         - |          NA |
|              |         |            |           |            |            |       |         |           |             |
| Dict_Valid   | 13      |  95.790 ns | 1.0004 ns |  0.9358 ns |  96.047 ns |  1.00 |    0.01 |         - |          NA |
| Mapper_Valid | 13      |  37.088 ns | 0.3817 ns |  0.3383 ns |  36.993 ns |  0.39 |    0.01 |         - |          NA |
|              |         |            |           |            |            |       |         |           |             |
| Dict_Valid   | 14      | 170.106 ns | 1.4375 ns |  1.3446 ns | 169.880 ns |  1.00 |    0.01 |         - |          NA |
| Mapper_Valid | 14      |  64.471 ns | 1.0157 ns |  0.9501 ns |  64.717 ns |  0.38 |    0.01 |         - |          NA |
|              |         |            |           |            |            |       |         |           |             |
| Dict_Valid   | 15      | 269.972 ns | 5.4212 ns |  6.6578 ns | 268.382 ns |  1.00 |    0.03 |         - |          NA |
| Mapper_Valid | 15      | 138.008 ns | 0.0620 ns |  0.0518 ns | 138.024 ns |  0.51 |    0.01 |         - |          NA |
|              |         |            |           |            |            |       |         |           |             |
| Dict_Valid   | 16      | 328.677 ns | 3.1104 ns |  2.9095 ns | 328.699 ns |  1.00 |    0.01 |         - |          NA |
| Mapper_Valid | 16      | 154.074 ns | 1.5540 ns |  1.4536 ns | 154.084 ns |  0.47 |    0.01 |         - |          NA |
|              |         |            |           |            |            |       |         |           |             |
| Dict_Valid   | 17      | 446.091 ns | 6.2841 ns |  5.8781 ns | 445.645 ns |  1.00 |    0.02 |         - |          NA |
| Mapper_Valid | 17      | 197.197 ns | 2.6319 ns |  2.4619 ns | 197.039 ns |  0.44 |    0.01 |         - |          NA |
*/
/*

| Method       | KeysInd | Mean          | Error       | StdDev      | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------- |-------- |--------------:|------------:|------------:|------:|--------:|----------:|------------:|
| Dict_Valid   | 0       |  1,327.027 ns |  35.5376 ns | 104.2257 ns |  1.01 |    0.13 |         - |          NA |
| Mapper_Valid | 0       |    627.644 ns |   0.9423 ns |   0.8814 ns |  0.48 |    0.05 |         - |          NA |
|              |         |               |             |             |       |         |           |             |
| Dict_Valid   | 1       |  2,263.638 ns |  46.7022 ns | 137.7025 ns |  1.00 |    0.09 |         - |          NA |
| Mapper_Valid | 1       |    566.423 ns |   3.1525 ns |   2.9488 ns |  0.25 |    0.02 |         - |          NA |
|              |         |               |             |             |       |         |           |             |
| Dict_Valid   | 2       | 14,641.686 ns | 258.9844 ns | 354.5008 ns |  1.00 |    0.03 |         - |          NA |
| Mapper_Valid | 2       |    347.577 ns |   6.5570 ns |   6.4399 ns |  0.02 |    0.00 |         - |          NA |
|              |         |               |             |             |       |         |           |             |
| Dict_Valid   | 3       |  1,692.217 ns |   9.1543 ns |   8.5629 ns |  1.00 |    0.01 |         - |          NA |
| Mapper_Valid | 3       |    607.533 ns |   4.7903 ns |   4.4809 ns |  0.36 |    0.00 |         - |          NA |
|              |         |               |             |             |       |         |           |             |
| Dict_Valid   | 4       |  1,296.135 ns |   9.6136 ns |   8.9925 ns |  1.00 |    0.01 |         - |          NA |
| Mapper_Valid | 4       |    474.481 ns |   9.0196 ns |  10.7372 ns |  0.37 |    0.01 |         - |          NA |
|              |         |               |             |             |       |         |           |             |
| Dict_Valid   | 5       |     86.220 ns |   1.7331 ns |   1.7022 ns |  1.00 |    0.03 |         - |          NA |
| Mapper_Valid | 5       |     34.845 ns |   1.0415 ns |   3.0708 ns |  0.40 |    0.04 |         - |          NA |
|              |         |               |             |             |       |         |           |             |
| Dict_Valid   | 6       | 10,394.531 ns | 207.9140 ns | 222.4657 ns |  1.00 |    0.03 |         - |          NA |
| Mapper_Valid | 6       |  1,581.981 ns |   9.3569 ns |  16.6319 ns |  0.15 |    0.00 |         - |          NA |
|              |         |               |             |             |       |         |           |             |
| Dict_Valid   | 7       |  4,264.030 ns |  34.1371 ns |  28.5060 ns |  1.00 |    0.01 |         - |          NA |
| Mapper_Valid | 7       |  1,194.173 ns |   9.6077 ns |   8.9870 ns |  0.28 |    0.00 |         - |          NA |
|              |         |               |             |             |       |         |           |             |
| Dict_Valid   | 8       | 12,559.868 ns |  43.7210 ns |  40.8966 ns |  1.00 |    0.00 |         - |          NA |
| Mapper_Valid | 8       |            NA |          NA |          NA |     ? |       ? |        NA |           ? |
|              |         |               |             |             |       |         |           |             |
| Dict_Valid   | 9       | 10,739.449 ns |  32.3368 ns |  28.6658 ns |  1.00 |    0.00 |         - |          NA |
| Mapper_Valid | 9       |            NA |          NA |          NA |     ? |       ? |        NA |           ? |
|              |         |               |             |             |       |         |           |             |
| Dict_Valid   | 10      |     43.873 ns |   0.6478 ns |   0.6060 ns |  1.00 |    0.02 |         - |          NA |
| Mapper_Valid | 10      |      9.598 ns |   0.2088 ns |   0.2234 ns |  0.22 |    0.01 |         - |          NA |
|              |         |               |             |             |       |         |           |             |
| Dict_Valid   | 11      |     68.179 ns |   0.8900 ns |   0.8325 ns |  1.00 |    0.02 |         - |          NA |
| Mapper_Valid | 11      |     17.972 ns |   0.3595 ns |   0.3692 ns |  0.26 |    0.01 |         - |          NA |
|              |         |               |             |             |       |         |           |             |
| Dict_Valid   | 12      | 15,805.904 ns | 267.3048 ns | 262.5290 ns |  1.00 |    0.02 |         - |          NA |
| Mapper_Valid | 12      |    452.250 ns |   6.4448 ns |   6.0285 ns |  0.03 |    0.00 |         - |          NA |
 */