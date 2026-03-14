namespace BitcoinPayments.EndToEndTests;

public sealed class PaymentFlowTests
{
    [Fact(Skip = "Requires PostgreSQL and bitcoind regtest from docker-compose.yml.")]
    public void FullLifecycle_FlowRequiresExternalInfrastructure()
    {
    }
}
