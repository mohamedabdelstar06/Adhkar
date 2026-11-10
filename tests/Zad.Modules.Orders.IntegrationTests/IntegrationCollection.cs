using Xunit;
using Zad.TestSupport.Collections;
using Zad.TestSupport.Databases;

namespace Zad.Modules.Orders.IntegrationTests;

[CollectionDefinition(IntegrationTestCollection.Name)]
public sealed class IntegrationCollection : ICollectionFixture<SqlServerFixture>;

