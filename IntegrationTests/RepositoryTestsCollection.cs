namespace IntegrationTests;

[CollectionDefinition(nameof(RepositoryTestsCollection))]
public sealed class RepositoryTestsCollection : ICollectionFixture<PostgresTestFixture>
{
}