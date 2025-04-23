namespace OakERP.Tests.Integration.TestSetup;

public class TransactionalDbFixture : IntegrationTestBase
{
    protected override bool UseTransaction => true;
}