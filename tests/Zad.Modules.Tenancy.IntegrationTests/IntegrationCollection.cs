using Xunit;
using Zad.TestSupport.Collections;
using Zad.TestSupport.Databases;

namespace Zad.Modules.Tenancy.IntegrationTests;

[CollectionDefinition(IntegrationTestCollection.Name)]
public sealed class IntegrationCollection : ICollectionFixture<SqlServerFixture>;

