using System.Data;
using System.Data.Common;
using RinkuLib.Queries;

namespace RinkuLib.Commands;
public static class QueryBuilderExtensions {

    extension(QueryBuilder builder) {
        public T? QuerySingle<T>(DbConnection cnn, DbTransaction? transaction = null, int? timeout = null)
            => builder.GetCommandAndInfo<T>(cnn, transaction, timeout, out var cache, out var parser, out var behavior)
                .QuerySingle(parser, cache, behavior, true);
        public IEnumerable<T> QueryMultiple<T>(DbConnection cnn, DbTransaction? transaction = null, int? timeout = null)
            => builder.GetCommandAndInfo<T>(cnn, transaction, timeout, out var cache, out var parser, out var behavior)
                .QueryMultiple(parser, cache, behavior, true);

        public Task<T?> QuerySingleAsync<T>(DbConnection cnn, DbTransaction? transaction = null, int? timeout = null, CancellationToken ct = default)
            => builder.GetCommandAndInfo<T>(cnn, transaction, timeout, out var cache, out var parser, out var behavior)
                .QuerySingleAsync(parser, cache, behavior, true, ct);
        public IAsyncEnumerable<T> QueryMultipleAsync<T>(DbConnection cnn, DbTransaction? transaction = null, int? timeout = null, CancellationToken ct = default)
            => builder.GetCommandAndInfo<T>(cnn, transaction, timeout, out var cache, out var parser, out var behavior)
                .QueryMultipleAsync(parser, cache, behavior, true, ct);

        public T? QuerySingle<T>(IDbConnection cnn, IDbTransaction? transaction = null, int? timeout = null)
            => builder.GetCommandAndInfo<T>(cnn, transaction, timeout, out var cache, out var parser, out var behavior)
                .QuerySingle(parser, cache, behavior, true);
        public IEnumerable<T> QueryMultiple<T>(IDbConnection cnn, IDbTransaction? transaction = null, int? timeout = null)
            => builder.GetCommandAndInfo<T>(cnn, transaction, timeout, out var cache, out var parser, out var behavior)
                .QueryMultiple(parser, cache, behavior, true);

        public Task<T?> QuerySingleAsync<T>(IDbConnection cnn, IDbTransaction? transaction = null, int? timeout = null, CancellationToken ct = default)
            => builder.GetCommandAndInfo<T>(cnn, transaction, timeout, out var cache, out var parser, out var behavior)
                .QuerySingleAsync(parser, cache, behavior, true, ct);
        public IAsyncEnumerable<T> QueryMultipleAsync<T>(IDbConnection cnn, IDbTransaction? transaction = null, int? timeout = null, CancellationToken ct = default)
            => builder.GetCommandAndInfo<T>(cnn, transaction, timeout, out var cache, out var parser, out var behavior)
                .QueryMultipleAsync(parser, cache, behavior, true, ct);



    }
    extension<TBuilder>(TBuilder builder) where TBuilder : ICommandBuilder {
        public T? QuerySingle<T>(DbConnection cnn, Func<DbDataReader, T> func, CommandBehavior defaultBehavior = default, DbTransaction? transaction = null, int? timeout = null)
            => builder.GetCommandAndInfo<T>(cnn, transaction, timeout, out var cache, out _, out _)
                .QuerySingle(func, cache, defaultBehavior, true);
        public IEnumerable<T> QueryMultiple<T>(DbConnection cnn, Func<DbDataReader, T> func, CommandBehavior defaultBehavior = default, DbTransaction? transaction = null, int? timeout = null)
            => builder.GetCommandAndInfo<T>(cnn, transaction, timeout, out var cache, out _, out _)
                .QueryMultiple(func, cache, defaultBehavior, true);

        public Task<T?> QuerySingleAsync<T>(DbConnection cnn, Func<DbDataReader, T> func, CommandBehavior defaultBehavior = default, DbTransaction? transaction = null, int? timeout = null, CancellationToken ct = default)
            => builder.GetCommandAndInfo<T>(cnn, transaction, timeout, out var cache, out _, out _)
                .QuerySingleAsync(func, cache, defaultBehavior, true, ct);
        public IAsyncEnumerable<T> QueryMultipleAsync<T>(DbConnection cnn, Func<DbDataReader, T> func, CommandBehavior defaultBehavior = default, DbTransaction? transaction = null, int? timeout = null, CancellationToken ct = default)
            => builder.GetCommandAndInfo<T>(cnn, transaction, timeout, out var cache, out _, out _)
                .QueryMultipleAsync(func, cache, defaultBehavior, true, ct);



        public T? QuerySingle<T>(IDbConnection cnn, Func<DbDataReader, T> func, CommandBehavior defaultBehavior = default, IDbTransaction? transaction = null, int? timeout = null)
            => builder.GetCommandAndInfo<T>(cnn, transaction, timeout, out var cache, out _, out _)
                .QuerySingle(func, cache, defaultBehavior, true);
        public IEnumerable<T> QueryMultiple<T>(IDbConnection cnn, Func<DbDataReader, T> func, CommandBehavior defaultBehavior = default, IDbTransaction? transaction = null, int? timeout = null)
            => builder.GetCommandAndInfo<T>(cnn, transaction, timeout, out var cache, out _, out _)
                .QueryMultiple(func, cache, defaultBehavior, true);

        public Task<T?> QuerySingleAsync<T>(IDbConnection cnn, Func<DbDataReader, T> func, CommandBehavior defaultBehavior = default, IDbTransaction? transaction = null, int? timeout = null, CancellationToken ct = default)
            => builder.GetCommandAndInfo<T>(cnn, transaction, timeout, out var cache, out _, out _)
                .QuerySingleAsync(func, cache, defaultBehavior, true, ct);
        public IAsyncEnumerable<T> QueryMultipleAsync<T>(IDbConnection cnn, Func<DbDataReader, T> func, CommandBehavior defaultBehavior = default, IDbTransaction? transaction = null, int? timeout = null, CancellationToken ct = default)
            => builder.GetCommandAndInfo<T>(cnn, transaction, timeout, out var cache, out _, out _)
                .QueryMultipleAsync(func, cache, defaultBehavior, true, ct);

        public int ExecuteQuery(DbConnection cnn, DbTransaction? transaction = null, int? timeout = null)
            => builder.GetCommandAndCache(cnn, transaction, timeout, out var cache)
                .ExecuteQuery(cache, true);
        public Task<int> ExecuteQueryAsync(DbConnection cnn, DbTransaction? transaction = null, int? timeout = null, CancellationToken ct = default)
            => builder.GetCommandAndCache(cnn, transaction, timeout, out var cache)
                .ExecuteQueryAsync(cache, true, ct);
    }
}
