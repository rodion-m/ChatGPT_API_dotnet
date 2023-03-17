using System.Runtime.CompilerServices;

namespace OpenAI;

public static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<string> ThrowOnCancellation(
        this IAsyncEnumerable<string> stream, 
        bool throwOnCancellation)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        var enumerator = stream.GetAsyncEnumerator();
        string? result = null;
        var hasResult = true;
        while (hasResult)
        {
            try
            {
                hasResult = await enumerator.MoveNextAsync().ConfigureAwait(false);
                result = hasResult ? enumerator.Current : null;
            }
            catch (OperationCanceledException)
            {
                if (throwOnCancellation) throw;
            }
            if (result != null)
                yield return result;
        }

        await enumerator.DisposeAsync().ConfigureAwait(false);
    }
}