namespace EduOnline.Core.Data.EventSourcing;

public class StoredEvent(Guid id, string tipo, DateTime dataOcorrencia, string dados)
{
    public Guid Id { get; private set; } = id;

    public string Tipo { get; private set; } = tipo;

    public DateTime DataOcorrencia { get; set; } = dataOcorrencia;

    public string Dados { get; private set; } = dados;
}
