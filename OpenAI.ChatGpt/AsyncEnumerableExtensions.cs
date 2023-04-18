namespace OpenAI.ChatGpt;

[Fody.ConfigureAwait(false)]
public static class AsyncEnumerableExtensions
{
    internal static async IAsyncEnumerable<T> ConfigureExceptions<T>(
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
                hasResult = await enumerator.MoveNextAsync();
                result = hasResult ? enumerator.Current : null;
            }
            catch (OperationCanceledException)
            {
                if (throwOnCancellation)
                {
                    await enumerator.DisposeAsync();
                    throw;
                }
            }
            if (result != null)
            {
                yield return result;
            }
        }

        await enumerator.DisposeAsync();
    }
}