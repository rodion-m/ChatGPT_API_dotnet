namespace OpenAI.ChatGpt.Extensions;

[Fody.ConfigureAwait(false)]
internal static class AsyncEnumerableExtensions
{
    internal static async IAsyncEnumerable<T> ConfigureExceptions<T>(
        this IAsyncEnumerable<T> stream, 
        bool throwOnCancellation) where T: class
    {
        ArgumentNullException.ThrowIfNull(stream);
        await using var enumerator = stream.GetAsyncEnumerator();
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
    }
    
    internal static async IAsyncEnumerable<T> ConfigureExceptions<T>(
        this IAsyncEnumerable<T> stream,
        bool throwOnCancellation, 
        Action<Exception>? onExceptionBeforeThrowing) where T: class
    {
        ArgumentNullException.ThrowIfNull(stream);
        IAsyncEnumerator<T> enumerator;
        try
        {
            enumerator = stream.GetAsyncEnumerator();
        }
        catch (Exception e)
        {
            onExceptionBeforeThrowing?.Invoke(e);
            throw;
        }
        T? result = null;
        var hasResult = true;
        while (hasResult)
        {
            try
            {
                hasResult = await enumerator.MoveNextAsync();
                result = hasResult ? enumerator.Current : null;
            }
            catch (OperationCanceledException e)
            {
                await DisposeAsyncSafe();
                onExceptionBeforeThrowing?.Invoke(e);
                if (throwOnCancellation)
                {
                    throw;
                }
            }
            if (result != null)
            {
                yield return result;
            }
        }

        await DisposeAsyncSafe();
        yield break;

        async Task DisposeAsyncSafe()
        {
            try
            {
                await enumerator.DisposeAsync();
            }
            catch (Exception e)
            {
                onExceptionBeforeThrowing?.Invoke(e);
                throw;
            }
        }
    }
}