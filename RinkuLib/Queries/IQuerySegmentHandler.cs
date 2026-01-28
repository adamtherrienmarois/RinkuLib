using RinkuLib.Tools;

namespace RinkuLib.Queries;
/// <summary>
/// Defines the contract for processing variable data into a SQL-ready string format.
/// </summary>
public interface IQuerySegmentHandler {
    /// <summary>
    /// A sentinel handler used during the factory phase to mark variables requiring special external resolution.
    /// Attempting to use this handler during assembly will throw a <see cref="NotImplementedException"/>.
    /// </summary>
    public static readonly IQuerySegmentHandler NotSet = new NotSetHandler();
    /// <summary>
    /// Transforms the provided <paramref name="value"/> and appends the result to the <see cref="ValueStringBuilder"/>.
    /// </summary>
    /// <param name="sb">The active string builder for query assembly.</param>
    /// <param name="value">The raw data value fetched from the external state array.</param>
    public void Handle(ref ValueStringBuilder sb, object value);
}
/// <summary>
/// A factory delegate responsible for instantiating a specific <see cref="IQuerySegmentHandler"/>.
/// </summary>
/// <param name="Name">
/// The full identifier name of the variable as it appears in the query (e.g., "Order_N"). 
/// This name is expected to include the underscore separator and the type-suffix character.
/// </param>
public delegate T HandlerGetter<out T>(string Name) where T : IQuerySegmentHandler;
/// <summary>
/// Internal sentinel implementation that prevents execution of uninitialized special handlers.
/// </summary>
internal class NotSetHandler() : IQuerySegmentHandler {
    public void Handle(ref ValueStringBuilder sb, object? value)
        => throw new NotImplementedException();
}