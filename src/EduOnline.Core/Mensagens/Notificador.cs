namespace EduOnline.Core.Mensagens;

public class Notificador : INotificador
{
    private readonly List<Notificacao> _notificacoes;

    public Notificador()
    {
        _notificacoes = [];
    }

    public void Handle(Notificacao notificacao)
    {
        _notificacoes.Add(notificacao);
    }

    public List<Notificacao> ObterNotificacoes()
    {
        return _notificacoes;
    }

    public bool TemNotificacao()
    {
        return _notificacoes.Count != 0;
    }
}
