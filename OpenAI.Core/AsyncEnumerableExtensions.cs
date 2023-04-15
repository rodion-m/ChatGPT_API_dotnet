namespace OpenAI;

public static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<T> ThrowOnCancellation<T>(
        this IAsyncEnumerable<T> stream, 
        bool throwOnCancellation) where T: class
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        var enumerator = stream.GetAsyncEnumerator();
        T? result = null;
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
                if (throwOnCancellation)
                {
                    await enumerator.DisposeAsync().ConfigureAwait(false);
                    throw;
                }
            }
            if (result != null)
                yield return result;
        }

        await enumerator.DisposeAsync().ConfigureAwait(false);
    }
}