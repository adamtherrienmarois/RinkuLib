using System.Data;
using System.Data.Common;
using RinkuLib.Queries;

namespace RinkuLib.Commands;
public readonly struct ParserCacher<T>(QueryCommand QueryCommand, Func<DbDataReader, T> Func, CommandBehavior DefaultBehavior) : IParser<T> {
    private readonly QueryCommand QueryCommand = QueryCommand;
    private readonly Func<DbDataReader, T> Func = Func;
    public CommandBehavior DefaultBehavior { get; } = DefaultBehavior;
    public readonly T Parse(DbDataReader reader) => Func(reader);
    public void Prepare(DbDataReader reader, IDbCommand cmd) => QueryCommand.UpdateCache(cmd);
}
public struct ParserGetFunc<T>(QueryCommand<T> QueryCommand, object?[] Variables) : IParser<T> {
    public readonly QueryCommand<T> QueryCommand = QueryCommand;
    public readonly object?[] Variables = Variables;
    public readonly CommandBehavior DefaultBehavior => default;
    private Func<DbDataReader, T> Func = null!;
    public readonly T Parse(DbDataReader reader) => Func(reader);
    public void Prepare(DbDataReader reader, IDbCommand cmd)
        => Func = QueryCommand.GetFunc(Variables, reader);
}
public struct ParserAll<T>(QueryCommand<T> QueryCommand, object?[] Variables) : IParser<T> {
    public readonly QueryCommand<T> QueryCommand = QueryCommand;
    public readonly object?[] Variables = Variables;
    public readonly CommandBehavior DefaultBehavior => default;
    private Func<DbDataReader, T> Func = null!;
    public readonly T Parse(DbDataReader reader) => Func(reader);
    public void Prepare(DbDataReader reader, IDbCommand cmd) {
        QueryCommand.UpdateCache(cmd);
        Func = QueryCommand.GetFunc(Variables, reader);
    }
}
public unsafe interface IDispatcher<T, TRet> {
    public abstract static delegate* managed<DbCommand, Func<DbDataReader, T>, CommandBehavior, bool, TRet> Direct { get; }
    public abstract static delegate* managed<DbCommand, ParserCacher<T>, bool, TRet> Cacher { get; }
    public abstract static delegate* managed<DbCommand, ParserGetFunc<T>, bool, TRet> GetFunc { get; }
    public abstract static delegate* managed<DbCommand, ParserAll<T>, bool, TRet> All { get; }
}
public unsafe interface IDispatcherAsync<T, TRet> {
    public abstract static delegate* managed<DbCommand, Func<DbDataReader, T>, CommandBehavior, bool, CancellationToken, TRet> DirectAsync { get; }
    public abstract static delegate* managed<DbCommand, ParserCacher<T>, bool, CancellationToken, TRet> CacherAsync { get; }
    public abstract static delegate* managed<DbCommand, ParserGetFunc<T>, bool, CancellationToken, TRet> GetFuncAsync { get; }
    public abstract static delegate* managed<DbCommand, ParserAll<T>, bool, CancellationToken, TRet> AllAsync { get; }
}
public readonly unsafe struct QuerySingleVTable<T> : IDispatcher<T, T?>, IDispatcherAsync<T, Task<T?>> {
    public static delegate* managed<DbCommand, Func<DbDataReader, T>, CommandBehavior, bool, T?> Direct => &CommandExtensions.QuerySingle;
    public static delegate* managed<DbCommand, ParserCacher<T>, bool, T?> Cacher => &CommandExtensions.QuerySingle<ParserCacher<T>, T>;
    public static delegate* managed<DbCommand, ParserGetFunc<T>, bool, T?> GetFunc => &CommandExtensions.QuerySingle<ParserGetFunc<T>, T>;
    public static delegate* managed<DbCommand, ParserAll<T>, bool, T?> All => &CommandExtensions.QuerySingle<ParserAll<T>, T>;
    public static delegate* managed<DbCommand, Func<DbDataReader, T>, CommandBehavior, bool, CancellationToken, Task<T?>> DirectAsync => &CommandExtensions.QuerySingleAsync;
    public static delegate* managed<DbCommand, ParserCacher<T>, bool, CancellationToken, Task<T?>> CacherAsync => &CommandExtensions.QuerySingleAsync<ParserCacher<T>, T>;
    public static delegate* managed<DbCommand, ParserGetFunc<T>, bool, CancellationToken, Task<T?>> GetFuncAsync => &CommandExtensions.QuerySingleAsync<ParserGetFunc<T>, T>;
    public static delegate* managed<DbCommand, ParserAll<T>, bool, CancellationToken, Task<T?>> AllAsync => &CommandExtensions.QuerySingleAsync<ParserAll<T>, T>;
}
public readonly unsafe struct QueryMultipleVTable<T> : IDispatcher<T, IEnumerable<T>>, IDispatcherAsync<T, IAsyncEnumerable<T>> {
    public static delegate* managed<DbCommand, Func<DbDataReader, T>, CommandBehavior, bool, IEnumerable<T>> Direct => &CommandExtensions.QueryMultiple;
    public static delegate* managed<DbCommand, ParserCacher<T>, bool, IEnumerable<T>> Cacher => &CommandExtensions.QueryMultiple<ParserCacher<T>, T>;
    public static delegate* managed<DbCommand, ParserGetFunc<T>, bool, IEnumerable<T>> GetFunc => &CommandExtensions.QueryMultiple<ParserGetFunc<T>, T>;
    public static delegate* managed<DbCommand, ParserAll<T>, bool, IEnumerable<T>> All => &CommandExtensions.QueryMultiple<ParserAll<T>, T>;
    public static delegate* managed<DbCommand, Func<DbDataReader, T>, CommandBehavior, bool, CancellationToken, IAsyncEnumerable<T>> DirectAsync => &CommandExtensions.QueryMultipleAsync;
    public static delegate* managed<DbCommand, ParserCacher<T>, bool, CancellationToken, IAsyncEnumerable<T>> CacherAsync => &CommandExtensions.QueryMultipleAsync<ParserCacher<T>, T>;
    public static delegate* managed<DbCommand, ParserGetFunc<T>, bool, CancellationToken, IAsyncEnumerable<T>> GetFuncAsync => &CommandExtensions.QueryMultipleAsync<ParserGetFunc<T>, T>;
    public static delegate* managed<DbCommand, ParserAll<T>, bool, CancellationToken, IAsyncEnumerable<T>> AllAsync => &CommandExtensions.QueryMultipleAsync<ParserAll<T>, T>;
}
public static class QueryBuilderExtensions {
    public static QueryBuilder<QueryCommand<T>> StartBuilder<T>(this QueryCommand<T> command)
        => new(command);
    public static QueryBuilderCommand<QueryCommand<T>, TCmd> StartBuilder<T, TCmd>(this QueryCommand<T> command, TCmd cmd) where TCmd : IDbCommand
        => new(command, cmd);
    public static DbCommand GetCommand<T>(this QueryBuilder<T> builder, DbConnection cnn, DbTransaction? transaction = null, int? timeout = null) where T : QueryCommand {
        var cmd = cnn.CreateCommand();
        if (transaction is not null)
            cmd.Transaction = transaction;
        if (timeout.HasValue)
            cmd.CommandTimeout = timeout.Value;
        builder.QueryCommand.SetCommand(cmd, builder.Variables);
        return cmd;
    }
    public static IDbCommand GetCommand<T>(this QueryBuilder<T> builder, IDbConnection cnn, IDbTransaction? transaction = null, int? timeout = null) where T : QueryCommand {
        var cmd = cnn.CreateCommand();
        if (transaction is not null)
            cmd.Transaction = transaction;
        if (timeout.HasValue)
            cmd.CommandTimeout = timeout.Value;
        if (cmd is DbCommand c)
            builder.QueryCommand.SetCommand(c, builder.Variables);
        else
            builder.QueryCommand.SetCommand(cmd, builder.Variables);
        return cmd;
    }
    private static unsafe TRet Query<TDispatcher, T, TRet>(QueryCommand queryCommand, object?[] variables, DbCommand cmd, Func<DbDataReader, T> func, CommandBehavior defaultBehavior, DbTransaction? transaction, int? timeout, bool disposeCommand)
        where TDispatcher : IDispatcher<T, TRet> {
        if (transaction is not null)
            cmd.Transaction = transaction;
        if (timeout.HasValue)
            cmd.CommandTimeout = timeout.Value;
        queryCommand.SetCommand(cmd, variables);
        var needToCache = queryCommand.Parameters.NeedToCache(variables);
        if (needToCache)
            return TDispatcher.Cacher(cmd, new(queryCommand, func, defaultBehavior), disposeCommand);
        return TDispatcher.Direct(cmd, func, defaultBehavior, disposeCommand);
    }
    private static unsafe TRet QueryAsync<TDispatcher, T, TRet>(QueryCommand queryCommand, object?[] variables, DbCommand cmd, Func<DbDataReader, T> func, CommandBehavior defaultBehavior, DbTransaction? transaction, int? timeout, bool disposeCommand, CancellationToken ct)
        where TDispatcher : IDispatcherAsync<T, TRet> {
        if (transaction is not null)
            cmd.Transaction = transaction;
        if (timeout.HasValue)
            cmd.CommandTimeout = timeout.Value;
        queryCommand.SetCommand(cmd, variables);
        var needToCache = queryCommand.Parameters.NeedToCache(variables);
        if (needToCache)
            return TDispatcher.CacherAsync(cmd, new(queryCommand, func, defaultBehavior), disposeCommand, ct);
        return TDispatcher.DirectAsync(cmd, func, defaultBehavior, disposeCommand, ct);
    }
    private static unsafe TRet Query<TDispatcher, T, TRet>(QueryCommand<T> queryCommand, object?[] variables, DbCommand cmd, DbTransaction? transaction, int? timeout, bool disposeCommand) where TDispatcher : IDispatcher<T, TRet> {
        if (transaction is not null)
            cmd.Transaction = transaction;
        if (timeout.HasValue)
            cmd.CommandTimeout = timeout.Value;
        queryCommand.SetCommand(cmd, variables);
        var needToCache = queryCommand.Parameters.NeedToCache(variables);
        var func = queryCommand.GetFunc(variables, out var behavior);
        if (!needToCache && func is not null)
            return TDispatcher.Direct(cmd, func, behavior, disposeCommand);
        if (func is not null)
            return TDispatcher.Cacher(cmd, new ParserCacher<T>(queryCommand, func, behavior), disposeCommand);
        if (!needToCache)
            return TDispatcher.GetFunc(cmd, new ParserGetFunc<T>(queryCommand, variables), disposeCommand);
        return TDispatcher.All(cmd, new ParserAll<T>(queryCommand, variables), disposeCommand);
    }
    private static unsafe TRet QueryAsync<TDispatcher, T, TRet>(QueryCommand<T> queryCommand, object?[] variables, DbCommand cmd, DbTransaction? transaction, int? timeout, bool disposeCommand, CancellationToken ct) where TDispatcher : IDispatcherAsync<T, TRet> {
        if (transaction is not null)
            cmd.Transaction = transaction;
        if (timeout.HasValue)
            cmd.CommandTimeout = timeout.Value;
        queryCommand.SetCommand(cmd, variables);
        var needToCache = queryCommand.Parameters.NeedToCache(variables);
        var func = queryCommand.GetFunc(variables, out var behavior);
        if (!needToCache && func is not null)
            return TDispatcher.DirectAsync(cmd, func, behavior, disposeCommand, ct);
        if (func is not null)
            return TDispatcher.CacherAsync(cmd, new ParserCacher<T>(queryCommand, func, behavior), disposeCommand, ct);
        if (!needToCache)
            return TDispatcher.GetFuncAsync(cmd, new ParserGetFunc<T>(queryCommand, variables), disposeCommand, ct);
        return TDispatcher.AllAsync(cmd, new ParserAll<T>(queryCommand, variables), disposeCommand, ct);
    }
    private static int Execute(QueryCommand queryCommand, object?[] variables, DbCommand cmd, DbTransaction? transaction, int? timeout, bool disposeCommand) {
        if (transaction is not null)
            cmd.Transaction = transaction;
        if (timeout.HasValue)
            cmd.CommandTimeout = timeout.Value;
        queryCommand.SetCommand(cmd, variables);
        return cmd.Execute(disposeCommand);
    }
    private static Task<int> ExecuteAsync(QueryCommand queryCommand, object?[] variables, DbCommand cmd, DbTransaction? transaction, int? timeout, bool disposeCommand, CancellationToken ct) {
        if (transaction is not null)
            cmd.Transaction = transaction;
        if (timeout.HasValue)
            cmd.CommandTimeout = timeout.Value;
        queryCommand.SetCommand(cmd, variables);
        return cmd.ExecuteAsync(disposeCommand, ct);
    }
    extension<T>(QueryBuilder<QueryCommand<T>> builder) {

        public T? QuerySingle(DbConnection cnn, DbTransaction? transaction = null, int? timeout = null)
            => Query<QuerySingleVTable<T>, T, T?>(builder.QueryCommand, builder.Variables, cnn.CreateCommand(), transaction, timeout, true);
        public T? QuerySingle(DbConnection cnn, out DbCommand cmd, DbTransaction? transaction = null, int? timeout = null)
            => Query<QuerySingleVTable<T>, T, T?>(builder.QueryCommand, builder.Variables, cmd = cnn.CreateCommand(), transaction, timeout, false);
        public IEnumerable<T> QueryMultiple(DbConnection cnn, DbTransaction? transaction = null, int? timeout = null)
            => Query<QueryMultipleVTable<T>, T, IEnumerable<T>>(builder.QueryCommand, builder.Variables, cnn.CreateCommand(), transaction, timeout, true);
        public IEnumerable<T> QueryMultiple(DbConnection cnn, out DbCommand cmd, DbTransaction? transaction = null, int? timeout = null)
            => Query<QueryMultipleVTable<T>, T, IEnumerable<T>>(builder.QueryCommand, builder.Variables, cmd = cnn.CreateCommand(), transaction, timeout, false);

        public Task<T?> QuerySingleAsync(DbConnection cnn, DbTransaction? transaction = null, int? timeout = null, CancellationToken ct = default)
            => QueryAsync<QuerySingleVTable<T>, T, Task<T?>>(builder.QueryCommand, builder.Variables, cnn.CreateCommand(), transaction, timeout, true, ct);
        public Task<T?> QuerySingleAsync(DbConnection cnn, out DbCommand cmd, DbTransaction? transaction = null, int? timeout = null, CancellationToken ct = default)
            => QueryAsync<QuerySingleVTable<T>, T, Task<T?>>(builder.QueryCommand, builder.Variables, cmd = cnn.CreateCommand(), transaction, timeout, false, ct);
        public IAsyncEnumerable<T> QueryMultipleAsync(DbConnection cnn, DbTransaction? transaction = null, int? timeout = null, CancellationToken ct = default)
            => QueryAsync<QueryMultipleVTable<T>, T, IAsyncEnumerable<T>>(builder.QueryCommand, builder.Variables, cnn.CreateCommand(), transaction, timeout, true, ct);
        public IAsyncEnumerable<T> QueryMultipleAsync(DbConnection cnn, out DbCommand cmd, DbTransaction? transaction = null, int? timeout = null, CancellationToken ct = default)
            => QueryAsync<QueryMultipleVTable<T>, T, IAsyncEnumerable<T>>(builder.QueryCommand, builder.Variables, cmd = cnn.CreateCommand(), transaction, timeout, false, ct);
    }
    extension(QueryBuilder<QueryCommand> builder) {
        public int Execute(DbConnection cnn, DbTransaction? transaction = null, int? timeout = null)
            => Execute(builder.QueryCommand, builder.Variables, cnn.CreateCommand(), transaction, timeout, true);
        public int Execute(DbConnection cnn, out DbCommand cmd, DbTransaction? transaction = null, int? timeout = null)
            => Execute(builder.QueryCommand, builder.Variables, cmd = cnn.CreateCommand(), transaction, timeout, false);
        public Task<int> ExecuteAsync(DbConnection cnn, DbTransaction? transaction = null, int? timeout = null, CancellationToken ct = default)
            => ExecuteAsync(builder.QueryCommand, builder.Variables, cnn.CreateCommand(), transaction, timeout, true, ct);
        public Task<int> ExecuteAsync(DbConnection cnn, out DbCommand cmd, DbTransaction? transaction = null, int? timeout = null, CancellationToken ct = default)
            => ExecuteAsync(builder.QueryCommand, builder.Variables, cmd = cnn.CreateCommand(), transaction, timeout, false, ct);
    }
    extension<TQuery>(QueryBuilder<TQuery> builder) where TQuery : QueryCommand {

        public T? QuerySingle<T>(DbConnection cnn, Func<DbDataReader, T> func, CommandBehavior defaultBehavior = default, DbTransaction? transaction = null, int? timeout = null)
            => Query<QuerySingleVTable<T>, T, T?>(builder.QueryCommand, builder.Variables, cnn.CreateCommand(), func, defaultBehavior, transaction, timeout, true);
        public T? QuerySingle<T>(DbConnection cnn, out DbCommand cmd, Func<DbDataReader, T> func, CommandBehavior defaultBehavior = default, DbTransaction? transaction = null, int? timeout = null)
            => Query<QuerySingleVTable<T>, T, T?>(builder.QueryCommand, builder.Variables, cmd = cnn.CreateCommand(), func, defaultBehavior, transaction, timeout, false);
        public IEnumerable<T> QueryMultiple<T>(DbConnection cnn, Func<DbDataReader, T> func, CommandBehavior defaultBehavior = default, DbTransaction? transaction = null, int? timeout = null)
            => Query<QueryMultipleVTable<T>, T, IEnumerable<T>>(builder.QueryCommand, builder.Variables, cnn.CreateCommand(), func, defaultBehavior, transaction, timeout, true);
        public IEnumerable<T> QueryMultiple<T>(DbConnection cnn, out DbCommand cmd, Func<DbDataReader, T> func, CommandBehavior defaultBehavior = default, DbTransaction? transaction = null, int? timeout = null)
            => Query<QueryMultipleVTable<T>, T, IEnumerable<T>>(builder.QueryCommand, builder.Variables, cmd = cnn.CreateCommand(), func, defaultBehavior, transaction, timeout, false);

        public Task<T?> QuerySingleAsync<T>(DbConnection cnn, Func<DbDataReader, T> func, CommandBehavior defaultBehavior = default, DbTransaction? transaction = null, int? timeout = null, CancellationToken ct = default)
            => QueryAsync<QuerySingleVTable<T>, T, Task<T?>>(builder.QueryCommand, builder.Variables, cnn.CreateCommand(), func, defaultBehavior, transaction, timeout, true, ct);
        public Task<T?> QuerySingleAsync<T>(DbConnection cnn, out DbCommand cmd, Func<DbDataReader, T> func, CommandBehavior defaultBehavior = default, DbTransaction? transaction = null, int? timeout = null, CancellationToken ct = default)
            => QueryAsync<QuerySingleVTable<T>, T, Task<T?>>(builder.QueryCommand, builder.Variables, cmd = cnn.CreateCommand(), func, defaultBehavior, transaction, timeout, false, ct);
        public IAsyncEnumerable<T> QueryMultipleAsync<T>(DbConnection cnn, Func<DbDataReader, T> func, CommandBehavior defaultBehavior = default, DbTransaction? transaction = null, int? timeout = null, CancellationToken ct = default)
            => QueryAsync<QueryMultipleVTable<T>, T, IAsyncEnumerable<T>>(builder.QueryCommand, builder.Variables, cnn.CreateCommand(), func, defaultBehavior, transaction, timeout, true, ct);
        public IAsyncEnumerable<T> QueryMultipleAsync<T>(DbConnection cnn, out DbCommand cmd, Func<DbDataReader, T> func, CommandBehavior defaultBehavior = default, DbTransaction? transaction = null, int? timeout = null, CancellationToken ct = default)
            => QueryAsync<QueryMultipleVTable<T>, T, IAsyncEnumerable<T>>(builder.QueryCommand, builder.Variables, cmd = cnn.CreateCommand(), func, defaultBehavior, transaction, timeout, false, ct);

        public int Execute(DbConnection cnn, DbTransaction? transaction = null, int? timeout = null)
            => Execute(builder.QueryCommand, builder.Variables, cnn.CreateCommand(), transaction, timeout, true);
        public int Execute(DbConnection cnn, out DbCommand cmd, DbTransaction? transaction = null, int? timeout = null)
            => Execute(builder.QueryCommand, builder.Variables, cmd = cnn.CreateCommand(), transaction, timeout, false);
        public Task<int> ExecuteAsync(DbConnection cnn, DbTransaction? transaction = null, int? timeout = null, CancellationToken ct = default)
            => ExecuteAsync(builder.QueryCommand, builder.Variables, cnn.CreateCommand(), transaction, timeout, true, ct);
        public Task<int> ExecuteAsync(DbConnection cnn, out DbCommand cmd, DbTransaction? transaction = null, int? timeout = null, CancellationToken ct = default)
            => ExecuteAsync(builder.QueryCommand, builder.Variables, cmd = cnn.CreateCommand(), transaction, timeout, false, ct);
    }
}
