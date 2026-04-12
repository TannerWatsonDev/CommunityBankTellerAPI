namespace CommunityBankTellerAPI.Tests.Integration
{
    // defines a named collection that shares a single CustomWebApplicationFactory instance across all test classes that belong to it, preventing duplicate migrations
    [CollectionDefinition("Integration Tests")]
    public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
    {
    }
}