using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Application.Services.Transfers;
using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Interfaces;
using Moq;

namespace BitcoinPayments.UnitTests.Application.Services;

public sealed class TransferStrategyResolverTests
{
    private static TransferStrategyResolver CreateResolver(string mode, IServiceProvider sp)
    {
        var settings = new Mock<IBitcoinSettings>();
        settings.Setup(s => s.TransferMode).Returns(mode);
        return new TransferStrategyResolver(settings.Object, sp);
    }

    [Fact]
    public void Resolve_ShouldReturnInternal_WhenModeIsInternal()
    {
        var internalStrategy = new InternalLedgerStrategy(Mock.Of<ITransactionRepository>());
        var sp = new Mock<IServiceProvider>();
        sp.Setup(s => s.GetService(typeof(InternalLedgerStrategy))).Returns(internalStrategy);

        var resolver = CreateResolver("internal", sp.Object);
        var result = resolver.Resolve(new Wallet("A", "k", "testnet4"), new Wallet("B", "k", "testnet4"));

        Assert.Same(internalStrategy, result);
    }

    [Fact]
    public void Resolve_AutoMode_SameUser_ShouldReturnInternal()
    {
        var internalStrategy = new InternalLedgerStrategy(Mock.Of<ITransactionRepository>());
        var sp = new Mock<IServiceProvider>();
        sp.Setup(s => s.GetService(typeof(InternalLedgerStrategy))).Returns(internalStrategy);

        var resolver = CreateResolver("auto", sp.Object);
        var source = new Wallet("A", "k", "testnet4") { UserId = "user-1" };
        var dest = new Wallet("B", "k", "testnet4") { UserId = "user-1" };
        var result = resolver.Resolve(source, dest);

        Assert.Same(internalStrategy, result);
    }

    [Fact]
    public void Resolve_AutoMode_DifferentUser_ShouldReturnOnChain()
    {
        var onChainStrategy = new Mock<ITransferStrategy>();
        var sp = new Mock<IServiceProvider>();
        sp.Setup(s => s.GetService(typeof(OnChainTransferStrategy))).Returns(onChainStrategy.Object);

        var resolver = CreateResolver("auto", sp.Object);
        var source = new Wallet("A", "k", "testnet4") { UserId = "user-1" };
        var dest = new Wallet("B", "k", "testnet4") { UserId = "user-2" };
        var result = resolver.Resolve(source, dest);

        Assert.Same(onChainStrategy.Object, result);
    }
}
