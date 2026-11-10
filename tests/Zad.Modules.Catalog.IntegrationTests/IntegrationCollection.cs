using Xunit;
using Zad.TestSupport.Collections;
using Zad.TestSupport.Databases;

namespace Zad.Modules.Catalog.IntegrationTests;

[CollectionDefinition(IntegrationTestCollection.Name)]
public sealed class IntegrationCollection : ICollectionFixture<SqlServerFixture>;

