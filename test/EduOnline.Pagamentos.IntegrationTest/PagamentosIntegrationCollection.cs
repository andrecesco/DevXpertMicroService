namespace EduOnline.Pagamentos.IntegrationTest;

[CollectionDefinition(Name, DisableParallelization = true)]
public class PagamentosIntegrationCollection : ICollectionFixture<PagamentosApiTestFactory>
{
    public const string Name = "Pagamentos API Integration Collection";
}
