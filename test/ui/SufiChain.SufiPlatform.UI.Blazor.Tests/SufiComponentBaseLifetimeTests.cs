using System.Reflection;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.UI.Blazor.Tests;

public class SufiComponentBaseLifetimeTests
{
    [Fact]
    public void ComponentCancellationToken_StaysReadableAndCanceled_AfterDispose()
    {
        var probe = new LifetimeProbe();

        probe.Token.IsCancellationRequested.ShouldBeFalse();

        probe.DisposeProbe();
        probe.Token.IsCancellationRequested.ShouldBeTrue();

        probe.DisposeProbe();
        probe.Token.IsCancellationRequested.ShouldBeTrue();
    }

    [Fact]
    public void ComponentCancellationToken_DoesNotThrow_WhenSourceWasAlreadyDisposed()
    {
        var probe = new LifetimeProbe();
        var source = (CancellationTokenSource)typeof(SufiComponentBase)
            .GetField("_cts", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(probe)!;

        source.Dispose();

        var token = probe.Token;

        token.IsCancellationRequested.ShouldBeTrue();
    }

    [Fact]
    public async Task DisposeCancellationSource_IsIdempotent_AfterCancelAndAfterPriorDispose()
    {
        var source = new CancellationTokenSource();
        LifetimeProbe.DisposeSource(source);
        LifetimeProbe.DisposeSource(source);
        await LifetimeProbe.DisposeSourceAsync(source);
        source.IsCancellationRequested.ShouldBeTrue();

        var alreadyDisposed = new CancellationTokenSource();
        alreadyDisposed.Dispose();
        LifetimeProbe.DisposeSource(alreadyDisposed);
        await LifetimeProbe.DisposeSourceAsync(alreadyDisposed);
    }

    private sealed class LifetimeProbe : SufiComponentBase
    {
        public CancellationToken Token => ComponentCancellationToken;

        public void DisposeProbe() => ((IDisposable)this).Dispose();

        public static void DisposeSource(CancellationTokenSource? source) =>
            DisposeCancellationSource(source);

        public static ValueTask DisposeSourceAsync(CancellationTokenSource? source) =>
            DisposeCancellationSourceAsync(source);
    }
}
