namespace OakERP.Tests.Integration.TestSetup;

[CollectionDefinition("TransactionalDB")]
public class TransactionalDbCollection : ICollectionFixture<TransactionalDbFixture>
{ }

[CollectionDefinition("PersistentDB")]
public class PersistentDbCollection : ICollectionFixture<PersistentDbFixture>
{ }