using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using RinkuLib.Queries;

namespace RinkuLib.Commands;
public static class CommandExtensions {
    extension(DbCommand cmd) {
        public int Execute(bool disposeCommand = true) {
            var cnn = cmd.Connection ?? throw new Exception("no connections was set with the command");
            var wasClosed = cnn.State != ConnectionState.Open;
            try {
                if (wasClosed)
                    cnn.Open();
                return cmd.ExecuteNonQuery();
            }
            finally {
                if (disposeCommand) {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
                if (wasClosed)
                    cnn.Close();
            }
        }
        public async Task<int> ExecuteAsync(bool disposeCommand = true, CancellationToken ct = default) {
            var cnn = cmd.Connection ?? throw new Exception("no connections was set with the command");
            var wasClosed = cnn.State != ConnectionState.Open;
            try {
                if (wasClosed)
                    await cnn.OpenAsync(ct).ConfigureAwait(false);
                return await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
            finally {
                if (disposeCommand) {
                    cmd.Parameters.Clear();
                    await cmd.DisposeAsync().ConfigureAwait(false);
                }
                if (wasClosed)
                    await cnn.CloseAsync().ConfigureAwait(false);
            }
        }

        public T? QuerySingle<TParser, T>(TParser parser, bool disposeCommand = true) where TParser : IParser<T> {
            var cnn = cmd.Connection ?? throw new Exception("no connections was set with the command");
            var wasClosed = cnn.State != ConnectionState.Open;
            DbDataReader? reader = null;
            try {
                var behavior = parser.DefaultBehavior | CommandBehavior.SingleRow;
                if (wasClosed) {
                    cnn.Open();
                    behavior |= CommandBehavior.CloseConnection;
                    wasClosed = false;
                }
                reader = cmd.ExecuteReader(behavior);
                parser.Prepare(reader, cmd);
                if (!reader.Read())
                    return default;
                return parser.Parse(reader);
            }
            finally {
                if (reader is not null) {
                    if (!reader.IsClosed) {
                        try { cmd.Cancel(); }
                        catch { }
                    }
                    reader.Dispose();
                }
                if (disposeCommand) {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
                if (wasClosed)
                    cnn.Close();
            }
        }
        public IEnumerable<T> QueryMultiple<TParser, T>(TParser parser, bool disposeCommand = true) where TParser : IParser<T> {
            var cnn = cmd.Connection ?? throw new Exception("no connections was set with the command");
            var wasClosed = cnn.State != ConnectionState.Open;
            DbDataReader? reader = null;
            try {
                var behavior = parser.DefaultBehavior;
                if (wasClosed) {
                    cnn.Open();
                    behavior |= CommandBehavior.CloseConnection;
                    wasClosed = false;
                }
                reader = cmd.ExecuteReader(behavior);
                parser.Prepare(reader, cmd);
                while (reader.Read())
                    yield return parser.Parse(reader);
                while (reader.NextResult()) { }
            }
            finally {
                if (reader is not null) {
                    if (!reader.IsClosed) {
                        try { cmd.Cancel(); }
                        catch { }
                    }
                    reader.Dispose();
                }
                if (disposeCommand) {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
                if (wasClosed)
                    cnn.Close();
            }
        }
        public async Task<T?> QuerySingleAsync<TParser, T>(TParser parser, bool disposeCommand = true, CancellationToken ct = default) where TParser : IParser<T> {
            var cnn = cmd.Connection ?? throw new Exception("no connections was set with the command");
            var wasClosed = cnn.State != ConnectionState.Open;
            DbDataReader? reader = null;
            try {
                var behavior = parser.DefaultBehavior | CommandBehavior.SingleRow;
                if (wasClosed) {
                    await cnn.OpenAsync(ct).ConfigureAwait(false);
                    behavior |= CommandBehavior.CloseConnection;
                    wasClosed = false;
                }
                reader = await cmd.ExecuteReaderAsync(behavior, ct).ConfigureAwait(false);
                parser.Prepare(reader, cmd);
                if (!await reader.ReadAsync(ct).ConfigureAwait(false))
                    return default;
                return parser.Parse(reader);
            }
            finally {
                if (reader is not null) {
                    if (!reader.IsClosed) {
                        try { cmd.Cancel(); }
                        catch { }
                    }
                    await reader.DisposeAsync().ConfigureAwait(false);
                }
                if (disposeCommand) {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
                if (wasClosed)
                    await cnn.CloseAsync().ConfigureAwait(false);
            }
        }
        public async IAsyncEnumerable<T> QueryMultipleAsync<TParser, T>(TParser parser, bool disposeCommand = true, [EnumeratorCancellation] CancellationToken ct = default) where TParser : IParser<T> {
            var cnn = cmd.Connection ?? throw new Exception("no connections was set with the command");
            var wasClosed = cnn.State != ConnectionState.Open;
            DbDataReader? reader = null;
            try {
                var behavior = parser.DefaultBehavior;
                if (wasClosed) {
                    await cnn.OpenAsync(ct).ConfigureAwait(false);
                    behavior |= CommandBehavior.CloseConnection;
                    wasClosed = false;
                }
                reader = await cmd.ExecuteReaderAsync(behavior, ct).ConfigureAwait(false);
                parser.Prepare(reader, cmd);
                while (await reader.ReadAsync(ct).ConfigureAwait(false))
                    yield return parser.Parse(reader);
                while (await reader.NextResultAsync(ct).ConfigureAwait(false)) { }
            }
            finally {
                if (reader is not null) {
                    if (!reader.IsClosed) {
                        try { cmd.Cancel(); }
                        catch { }
                    }
                    await reader.DisposeAsync().ConfigureAwait(false);
                }
                if (disposeCommand) {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
                if (wasClosed)
                    await cnn.CloseAsync().ConfigureAwait(false);
            }
        }
        public async Task<T?> QuerySingleParseAsync<TParser, T>(TParser parser, bool disposeCommand = true, CancellationToken ct = default) where TParser : IAsyncParser<T> {
            var cnn = cmd.Connection ?? throw new Exception("no connections was set with the command");
            var wasClosed = cnn.State != ConnectionState.Open;
            DbDataReader? reader = null;
            try {
                var behavior = parser.DefaultBehavior | CommandBehavior.SingleRow;
                if (wasClosed) {
                    await cnn.OpenAsync(ct).ConfigureAwait(false);
                    behavior |= CommandBehavior.CloseConnection;
                    wasClosed = false;
                }
                reader = await cmd.ExecuteReaderAsync(behavior, ct).ConfigureAwait(false);
                await parser.Prepare(reader, cmd).ConfigureAwait(false);
                if (!await reader.ReadAsync(ct).ConfigureAwait(false))
                    return default;
                return await parser.Parse(reader).ConfigureAwait(false);
            }
            finally {
                if (reader is not null) {
                    if (!reader.IsClosed) {
                        try { cmd.Cancel(); }
                        catch { }
                    }
                    await reader.DisposeAsync().ConfigureAwait(false);
                }
                if (disposeCommand) {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
                if (wasClosed)
                    await cnn.CloseAsync().ConfigureAwait(false);
            }
        }
        public async IAsyncEnumerable<T> QueryMultipleParseAsync<TParser, T>(TParser parser, bool disposeCommand = true, [EnumeratorCancellation] CancellationToken ct = default) where TParser : IAsyncParser<T> {
            var cnn = cmd.Connection ?? throw new Exception("no connections was set with the command");
            var wasClosed = cnn.State != ConnectionState.Open;
            DbDataReader? reader = null;
            try {
                var behavior = parser.DefaultBehavior;
                if (wasClosed) {
                    await cnn.OpenAsync(ct).ConfigureAwait(false);
                    behavior |= CommandBehavior.CloseConnection;
                    wasClosed = false;
                }
                reader = await cmd.ExecuteReaderAsync(behavior, ct).ConfigureAwait(false);
                await parser.Prepare(reader, cmd).ConfigureAwait(false);
                while (await reader.ReadAsync(ct).ConfigureAwait(false))
                    yield return await parser.Parse(reader).ConfigureAwait(false);
                while (await reader.NextResultAsync(ct).ConfigureAwait(false)) { }
            }
            finally {
                if (reader is not null) {
                    if (!reader.IsClosed) {
                        try { cmd.Cancel(); }
                        catch { }
                    }
                    await reader.DisposeAsync().ConfigureAwait(false);
                }
                if (disposeCommand) {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
                if (wasClosed)
                    await cnn.CloseAsync().ConfigureAwait(false);
            }
        }

        public T? QuerySingle<T>(Func<DbDataReader, T> readerFunc, CommandBehavior defaultBehavior = default, bool disposeCommand = true) {
            var cnn = cmd.Connection ?? throw new Exception("no connections was set with the command");
            var wasClosed = cnn.State != ConnectionState.Open;
            DbDataReader? reader = null;
            try {
                defaultBehavior |= CommandBehavior.SingleRow;
                if (wasClosed) {
                    cnn.Open();
                    defaultBehavior |= CommandBehavior.CloseConnection;
                    wasClosed = false;
                }
                reader = cmd.ExecuteReader(defaultBehavior);
                if (!reader.Read())
                    return default;
                return readerFunc(reader);
            }
            finally {
                if (reader is not null) {
                    if (!reader.IsClosed) {
                        try { cmd.Cancel(); }
                        catch { }
                    }
                    reader.Dispose();
                }
                if (disposeCommand) {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
                if (wasClosed)
                    cnn.Close();
            }
        }
        public IEnumerable<T> QueryMultiple<T>(Func<DbDataReader, T> readerFunc, CommandBehavior defaultBehavior = default, bool disposeCommand = true) {
            var cnn = cmd.Connection ?? throw new Exception("no connections was set with the command");
            var wasClosed = cnn.State != ConnectionState.Open;
            DbDataReader? reader = null;
            try {
                if (wasClosed) {
                    cnn.Open();
                    defaultBehavior |= CommandBehavior.CloseConnection;
                    wasClosed = false;
                }
                reader = cmd.ExecuteReader(defaultBehavior);
                while (reader.Read())
                    yield return readerFunc(reader);
                while (reader.NextResult()) { }
            }
            finally {
                if (reader is not null) {
                    if (!reader.IsClosed) {
                        try { cmd.Cancel(); }
                        catch { }
                    }
                    reader.Dispose();
                }
                if (disposeCommand) {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
                if (wasClosed)
                    cnn.Close();
            }
        }
        public async Task<T?> QuerySingleAsync<T>(Func<DbDataReader, T> readerFunc, CommandBehavior defaultBehavior = default, bool disposeCommand = true, CancellationToken ct = default) {
            var cnn = cmd.Connection ?? throw new Exception("no connections was set with the command");
            var wasClosed = cnn.State != ConnectionState.Open;
            DbDataReader? reader = null;
            try {
                defaultBehavior |= CommandBehavior.SingleRow;
                if (wasClosed) {
                    await cnn.OpenAsync(ct).ConfigureAwait(false);
                    defaultBehavior |= CommandBehavior.CloseConnection;
                    wasClosed = false;
                }
                reader = await cmd.ExecuteReaderAsync(defaultBehavior, ct).ConfigureAwait(false);
                if (!await reader.ReadAsync(ct).ConfigureAwait(false))
                    return default;
                return readerFunc(reader);
            }
            finally {
                if (reader is not null) {
                    if (!reader.IsClosed) {
                        try { cmd.Cancel(); }
                        catch { }
                    }
                    await reader.DisposeAsync().ConfigureAwait(false);
                }
                if (disposeCommand) {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
                if (wasClosed)
                    await cnn.CloseAsync().ConfigureAwait(false);
            }
        }
        public async IAsyncEnumerable<T> QueryMultipleAsync<T>(Func<DbDataReader, T> readerFunc, CommandBehavior defaultBehavior = default, bool disposeCommand = true, [EnumeratorCancellation] CancellationToken ct = default) {
            var cnn = cmd.Connection ?? throw new Exception("no connections was set with the command");
            var wasClosed = cnn.State != ConnectionState.Open;
            DbDataReader? reader = null;
            try {
                if (wasClosed) {
                    await cnn.OpenAsync(ct).ConfigureAwait(false);
                    defaultBehavior |= CommandBehavior.CloseConnection;
                    wasClosed = false;
                }
                reader = await cmd.ExecuteReaderAsync(defaultBehavior, ct).ConfigureAwait(false);
                while (await reader.ReadAsync(ct).ConfigureAwait(false))
                    yield return readerFunc(reader);
                while (await reader.NextResultAsync(ct).ConfigureAwait(false)) { }
            }
            finally {
                if (reader is not null) {
                    if (!reader.IsClosed) {
                        try { cmd.Cancel(); }
                        catch { }
                    }
                    await reader.DisposeAsync().ConfigureAwait(false);
                }
                if (disposeCommand) {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
                if (wasClosed)
                    await cnn.CloseAsync().ConfigureAwait(false);
            }
        }
        public async Task<T?> QuerySingleParseAsync<T>(Func<DbDataReader, Task<T>> readerFunc, CommandBehavior defaultBehavior = default, bool disposeCommand = true, CancellationToken ct = default) {
            var cnn = cmd.Connection ?? throw new Exception("no connections was set with the command");
            var wasClosed = cnn.State != ConnectionState.Open;
            DbDataReader? reader = null;
            try {
                defaultBehavior |= CommandBehavior.SingleRow;
                if (wasClosed) {
                    await cnn.OpenAsync(ct).ConfigureAwait(false);
                    defaultBehavior |= CommandBehavior.CloseConnection;
                    wasClosed = false;
                }
                reader = await cmd.ExecuteReaderAsync(defaultBehavior, ct).ConfigureAwait(false);
                if (!await reader.ReadAsync(ct).ConfigureAwait(false))
                    return default;
                return await readerFunc(reader).ConfigureAwait(false);
            }
            finally {
                if (reader is not null) {
                    if (!reader.IsClosed) {
                        try { cmd.Cancel(); }
                        catch { }
                    }
                    await reader.DisposeAsync().ConfigureAwait(false);
                }
                if (disposeCommand) {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
                if (wasClosed)
                    await cnn.CloseAsync().ConfigureAwait(false);
            }
        }
        public async IAsyncEnumerable<T> QueryMultipleParseAsync<T>(Func<DbDataReader, Task<T>> readerFunc, CommandBehavior defaultBehavior = default, bool disposeCommand = true, [EnumeratorCancellation] CancellationToken ct = default) {
            var cnn = cmd.Connection ?? throw new Exception("no connections was set with the command");
            var wasClosed = cnn.State != ConnectionState.Open;
            DbDataReader? reader = null;
            try {
                if (wasClosed) {
                    await cnn.OpenAsync(ct).ConfigureAwait(false);
                    defaultBehavior |= CommandBehavior.CloseConnection;
                    wasClosed = false;
                }
                reader = await cmd.ExecuteReaderAsync(defaultBehavior, ct).ConfigureAwait(false);
                while (await reader.ReadAsync(ct).ConfigureAwait(false))
                    yield return await readerFunc(reader).ConfigureAwait(false);
                while (await reader.NextResultAsync(ct).ConfigureAwait(false)) { }
            }
            finally {
                if (reader is not null) {
                    if (!reader.IsClosed) {
                        try { cmd.Cancel(); }
                        catch { }
                    }
                    await reader.DisposeAsync().ConfigureAwait(false);
                }
                if (disposeCommand) {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
                if (wasClosed)
                    await cnn.CloseAsync().ConfigureAwait(false);
            }
        }
    }
    extension(IDbCommand cmd) {
        public int ExecuteQuery(bool disposeCommand = true) {
            var cnn = cmd.Connection ?? throw new Exception("no connections was set with the command");
            var wasClosed = cnn.State != ConnectionState.Open;
            try {
                if (wasClosed)
                    cnn.Open();
                return cmd.ExecuteNonQuery();
            }
            finally {
                if (disposeCommand) {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
                if (wasClosed)
                    cnn.Close();
            }
        }
        public Task<int> ExecuteQueryAsync(bool disposeCommand = true, CancellationToken ct = default) {
            if (cmd is DbCommand c)
                return c.ExecuteAsync(disposeCommand, ct);
            return Task.FromResult(cmd.ExecuteQuery(disposeCommand));
        }

        public T? QuerySingle<TParser, T>(TParser parser, bool disposeCommand = true) where TParser : IParser<T> {
            if (cmd is DbCommand c)
                return c.QuerySingle<TParser, T>(parser, disposeCommand);
            return cmd.QuerySingleImpl<TParser, T>(parser, disposeCommand);
        }
        private T? QuerySingleImpl<TParser, T>(TParser parser, bool disposeCommand = true) where TParser : IParser<T> {
            var cnn = cmd.Connection ?? throw new Exception("no connections was set with the command");
            var wasClosed = cnn.State != ConnectionState.Open;
            DbDataReader? reader = null;
            try {
                var behavior = parser.DefaultBehavior | CommandBehavior.SingleRow;
                if (wasClosed) {
                    cnn.Open();
                    behavior |= CommandBehavior.CloseConnection;
                    wasClosed = false;
                }
                var r = cmd.ExecuteReader(behavior);
                reader = r is DbDataReader rd ? rd : new WrappedBasicReader(r);
                parser.Prepare(reader, cmd);
                if (!reader.Read())
                    return default;
                return parser.Parse(reader);
            }
            finally {
                if (reader is not null) {
                    if (!reader.IsClosed) {
                        try { cmd.Cancel(); }
                        catch { }
                    }
                    reader.Dispose();
                }
                if (disposeCommand) {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
                if (wasClosed)
                    cnn.Close();
            }
        }
        public IEnumerable<T> QueryMultiple<TParser, T>(TParser parser, bool disposeCommand = true) where TParser : IParser<T> {
            if (cmd is DbCommand c)
                return c.QueryMultiple<TParser, T>(parser, disposeCommand);
            return cmd.QueryMultipleImpl<TParser, T>(parser, disposeCommand);
        }
        private IEnumerable<T> QueryMultipleImpl<TParser, T>(TParser parser, bool disposeCommand = true) where TParser : IParser<T> {
            var cnn = cmd.Connection ?? throw new Exception("no connections was set with the command");
            var wasClosed = cnn.State != ConnectionState.Open;
            DbDataReader? reader = null;
            try {
                var behavior = parser.DefaultBehavior;
                if (wasClosed) {
                    cnn.Open();
                    behavior |= CommandBehavior.CloseConnection;
                    wasClosed = false;
                }
                var r = cmd.ExecuteReader(behavior);
                reader = r is DbDataReader rd ? rd : new WrappedBasicReader(r);
                parser.Prepare(reader, cmd);
                while (reader.Read())
                    yield return parser.Parse(reader);
                while (reader.NextResult()) { }
            }
            finally {
                if (reader is not null) {
                    if (!reader.IsClosed) {
                        try { cmd.Cancel(); }
                        catch { }
                    }
                    reader.Dispose();
                }
                if (disposeCommand) {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
                if (wasClosed)
                    cnn.Close();
            }
        }
        public Task<T?> QuerySingleAsync<TParser, T>(TParser parser, bool disposeCommand = true, CancellationToken ct = default) where TParser : IParser<T> {
            if (cmd is DbCommand c)
                return c.QuerySingleAsync<TParser, T>(parser, disposeCommand, ct);
            return Task.FromResult(cmd.QuerySingleImpl<TParser, T>(parser, disposeCommand));
        }
        public IAsyncEnumerable<T> QueryMultipleAsync<TParser, T>(TParser parser, bool disposeCommand = true, CancellationToken ct = default) where TParser : IParser<T> {
            if (cmd is DbCommand c)
                return c.QueryMultipleAsync<TParser, T>(parser, disposeCommand, ct);
            return cmd.QueryMultipleImpl<TParser, T>(parser, disposeCommand).ToAsyncEnumerable();
        }
        public Task<T?> QuerySingleParseAsync<TParser, T>(TParser parser, bool disposeCommand = true, CancellationToken ct = default) where TParser : IAsyncParser<T> {
            if (cmd is DbCommand c)
                return c.QuerySingleParseAsync<TParser, T>(parser, disposeCommand, ct);
            return cmd.QuerySingleParseAsyncImpl<TParser, T>(parser, disposeCommand);
        }
        private async Task<T?> QuerySingleParseAsyncImpl<TParser, T>(TParser parser, bool disposeCommand = true) where TParser : IAsyncParser<T> {
            var cnn = cmd.Connection ?? throw new Exception("no connections was set with the command");
            var wasClosed = cnn.State != ConnectionState.Open;
            DbDataReader? reader = null;
            try {
                var behavior = parser.DefaultBehavior | CommandBehavior.SingleRow;
                if (wasClosed) {
                    cnn.Open();
                    behavior |= CommandBehavior.CloseConnection;
                    wasClosed = false;
                }
                var r = cmd.ExecuteReader(behavior);
                reader = r is DbDataReader rd ? rd : new WrappedBasicReader(r);
                await parser.Prepare(reader, cmd).ConfigureAwait(false);
                if (!reader.Read())
                    return default;
                return await parser.Parse(reader).ConfigureAwait(false);
            }
            finally {
                if (reader is not null) {
                    if (!reader.IsClosed) {
                        try { cmd.Cancel(); }
                        catch { }
                    }
                    reader.Dispose();
                }
                if (disposeCommand) {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
                if (wasClosed)
                    cnn.Close();
            }
        }
        public IAsyncEnumerable<T> QueryMultipleParseAsync<TParser, T>(TParser parser, bool disposeCommand = true, CancellationToken ct = default) where TParser : IAsyncParser<T> {
            if (cmd is DbCommand c)
                return c.QueryMultipleParseAsync<TParser, T>(parser, disposeCommand, ct);
            return cmd.QueryMultipleParseAsyncImpl<TParser, T>(parser, disposeCommand, ct);
        }
        private async IAsyncEnumerable<T> QueryMultipleParseAsyncImpl<TParser, T>(TParser parser, bool disposeCommand = true, [EnumeratorCancellation] CancellationToken _ = default) where TParser : IAsyncParser<T> {
            var cnn = cmd.Connection ?? throw new Exception("no connections was set with the command");
            var wasClosed = cnn.State != ConnectionState.Open;
            DbDataReader? reader = null;
            try {
                var behavior = parser.DefaultBehavior | CommandBehavior.SingleRow;
                if (wasClosed) {
                    cnn.Open();
                    behavior |= CommandBehavior.CloseConnection;
                    wasClosed = false;
                }
                var r = cmd.ExecuteReader(behavior);
                reader = r is DbDataReader rd ? rd : new WrappedBasicReader(r);
                await parser.Prepare(reader, cmd).ConfigureAwait(false);
                while (reader.Read())
                    yield return await parser.Parse(reader).ConfigureAwait(false);
                while (reader.NextResult()) { }
            }
            finally {
                if (reader is not null) {
                    if (!reader.IsClosed) {
                        try { cmd.Cancel(); }
                        catch { }
                    }
                    reader.Dispose();
                }
                if (disposeCommand) {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
                if (wasClosed)
                    cnn.Close();
            }
        }

        public T? QuerySingle<T>(Func<DbDataReader, T> readerFunc, CommandBehavior defaultBehavior = default, bool disposeCommand = true) {
            if (cmd is DbCommand c)
                return c.QuerySingle(readerFunc, defaultBehavior, disposeCommand);
            return cmd.QuerySingleImpl(readerFunc, defaultBehavior, disposeCommand);
        }
        private T? QuerySingleImpl<T>(Func<DbDataReader, T> readerFunc, CommandBehavior defaultBehavior = default, bool disposeCommand = true) {
            var cnn = cmd.Connection ?? throw new Exception("no connections was set with the command");
            var wasClosed = cnn.State != ConnectionState.Open;
            DbDataReader? reader = null;
            try {
                defaultBehavior |= CommandBehavior.SingleRow;
                if (wasClosed) {
                    cnn.Open();
                    defaultBehavior |= CommandBehavior.CloseConnection;
                    wasClosed = false;
                }
                var r = cmd.ExecuteReader(defaultBehavior);
                reader = r is DbDataReader rd ? rd : new WrappedBasicReader(r);
                if (!reader.Read())
                    return default;
                return readerFunc(reader);
            }
            finally {
                if (reader is not null) {
                    if (!reader.IsClosed) {
                        try { cmd.Cancel(); }
                        catch { }
                    }
                    reader.Dispose();
                }
                if (disposeCommand) {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
                if (wasClosed)
                    cnn.Close();
            }
        }
        public IEnumerable<T> QueryMultiple<T>(Func<DbDataReader, T> readerFunc, CommandBehavior defaultBehavior = default, bool disposeCommand = true) {
            if (cmd is DbCommand c)
                return c.QueryMultiple(readerFunc, defaultBehavior, disposeCommand);
            return cmd.QueryMultipleImpl(readerFunc, defaultBehavior, disposeCommand);
        }
        private IEnumerable<T> QueryMultipleImpl<T>(Func<DbDataReader, T> readerFunc, CommandBehavior defaultBehavior = default, bool disposeCommand = true) {
            var cnn = cmd.Connection ?? throw new Exception("no connections was set with the command");
            var wasClosed = cnn.State != ConnectionState.Open;
            DbDataReader? reader = null;
            try {
                if (wasClosed) {
                    cnn.Open();
                    defaultBehavior |= CommandBehavior.CloseConnection;
                    wasClosed = false;
                }
                var r = cmd.ExecuteReader(defaultBehavior);
                reader = r is DbDataReader rd ? rd : new WrappedBasicReader(r);
                while (reader.Read())
                    yield return readerFunc(reader);
                while (reader.NextResult()) { }
            }
            finally {
                if (reader is not null) {
                    if (!reader.IsClosed) {
                        try { cmd.Cancel(); }
                        catch { }
                    }
                    reader.Dispose();
                }
                if (disposeCommand) {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
                if (wasClosed)
                    cnn.Close();
            }
        }
        public Task<T?> QuerySingleAsync<T>(Func<DbDataReader, T> readerFunc, CommandBehavior defaultBehavior = default, bool disposeCommand = true, CancellationToken ct = default) {
            if (cmd is DbCommand c)
                return c.QuerySingleAsync(readerFunc, defaultBehavior, disposeCommand, ct);
            return Task.FromResult(cmd.QuerySingleImpl(readerFunc, defaultBehavior, disposeCommand));
        }
        public IAsyncEnumerable<T> QueryMultipleAsync<T>(Func<DbDataReader, T> readerFunc, CommandBehavior defaultBehavior = default, bool disposeCommand = true, CancellationToken ct = default) {
            if (cmd is DbCommand c)
                return c.QueryMultipleAsync(readerFunc, defaultBehavior, disposeCommand, ct);
            return cmd.QueryMultipleImpl(readerFunc, defaultBehavior, disposeCommand).ToAsyncEnumerable();
        }
        public Task<T?> QuerySingleAsync<T>(Func<DbDataReader, Task<T>> readerFunc, CommandBehavior defaultBehavior = default, bool disposeCommand = true, CancellationToken ct = default) {
            if (cmd is DbCommand c)
                return c.QuerySingleParseAsync(readerFunc, defaultBehavior, disposeCommand, ct);
            return cmd.QuerySingleAsyncImpl(readerFunc, defaultBehavior, disposeCommand);
        }
        private async Task<T?> QuerySingleAsyncImpl<T>(Func<DbDataReader, Task<T>> readerFunc, CommandBehavior defaultBehavior = default, bool disposeCommand = true) {
            var cnn = cmd.Connection ?? throw new Exception("no connections was set with the command");
            var wasClosed = cnn.State != ConnectionState.Open;
            DbDataReader? reader = null;
            try {
                defaultBehavior |= CommandBehavior.SingleRow;
                if (wasClosed) {
                    cnn.Open();
                    defaultBehavior |= CommandBehavior.CloseConnection;
                    wasClosed = false;
                }
                var r = cmd.ExecuteReader(defaultBehavior);
                reader = r is DbDataReader rd ? rd : new WrappedBasicReader(r);
                if (!reader.Read())
                    return default;
                return await readerFunc(reader).ConfigureAwait(false);
            }
            finally {
                if (reader is not null) {
                    if (!reader.IsClosed) {
                        try { cmd.Cancel(); }
                        catch { }
                    }
                    reader.Dispose();
                }
                if (disposeCommand) {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
                if (wasClosed)
                    cnn.Close();
            }
        }
        public IAsyncEnumerable<T> QueryMultipleAsync<T>(Func<DbDataReader, Task<T>> readerFunc, CommandBehavior defaultBehavior = default, bool disposeCommand = true, CancellationToken ct = default) {
            if (cmd is DbCommand c)
                return c.QueryMultipleParseAsync(readerFunc, defaultBehavior, disposeCommand, ct);
            return cmd.QueryMultipleAsyncImpl(readerFunc, defaultBehavior, disposeCommand, ct);
        }
        private async IAsyncEnumerable<T> QueryMultipleAsyncImpl<T>(Func<DbDataReader, Task<T>> readerFunc, CommandBehavior defaultBehavior = default, bool disposeCommand = true, [EnumeratorCancellation] CancellationToken _ = default) {
            var items = cmd.QueryMultipleImpl(readerFunc, defaultBehavior, disposeCommand);
            foreach (var item in items)
                yield return await item.ConfigureAwait(false);
        }
    }
}
