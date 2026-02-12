using Bogus;
using EduOnline.Conteudos.Domain.Enumeradores;
using EduOnline.Conteudos.Domain.Services;
using EduOnline.Core.DomainObjects;
using EduOnline.Core.Mensagens;
using Moq;
using Moq.AutoMock;

namespace EduOnline.Conteudos.Domain.UnitTest;

public class CursoServiceTest
{
    private readonly AutoMocker _mocker;
    private readonly Mock<INotificador> _notificador;
    private readonly Mock<ICursoRepository> _repository;

    public CursoServiceTest()
    {
        _mocker = new AutoMocker();
        _notificador = _mocker.GetMock<INotificador>();
        _repository = _mocker.GetMock<ICursoRepository>();
    }

    private static Curso GerarCurso()
    {
        var niveis = Enumerador.GetAll<Nivel>().Select(a => a.Id);

        var conteudoProgramatico = new Faker<ConteudoProgramatico>()
            .RuleFor(c => c.Tema, f => f.Lorem.Paragraph())
            .RuleFor(c => c.NivelId, f => f.PickRandom(niveis))
            .RuleFor(c => c.CargaHoraria, f => f.Random.Int(0, 1000));

        var cursoFaker = new Faker<Curso>("pt_BR")
            .RuleFor(c => c.Nome, f => f.Company.CompanyName())
            .RuleFor(c => c.Autor, f => f.Person.FullName)
            .RuleFor(c => c.Validade, f => f.Date.FutureDateOnly())
            .RuleFor(c => c.ConteudoProgramatico, f => conteudoProgramatico.Generate());

        return cursoFaker.Generate();
    }

    [Fact(DisplayName = "Adicionar Curso deve retornar sucesso")]
    [Trait("Categoria", "CursoService")]
    public async Task CursoService_Adicionar_DeveRetornarSucesso()
    {
        // Arrange
        var curso = GerarCurso();
        _mocker.GetMock<ICursoRepository>().Setup(r => r.UnitOfWork.Commit()).ReturnsAsync(true);
        var cursoService = new CursoService(_notificador.Object, _repository.Object);

        // Act
        await cursoService.Adicionar(curso);

        // Assert
        _mocker.GetMock<ICursoRepository>().Verify(r => r.Adicionar(It.IsAny<Curso>()), Times.Once);
        _mocker.GetMock<ICursoRepository>().Verify(r => r.UnitOfWork.Commit(), Times.Once);
    }

    [Fact(DisplayName = "Adicionar Curso sem preencher os campos")]
    [Trait("Categoria", "CursoService")]
    public async Task CursoService_Adicionar_DeveRetornarErrosDeCamposObrigatórios()
    {
        // Arrange
        var curso = new Curso
        {
            ConteudoProgramatico = new ConteudoProgramatico()
        };

        _mocker.GetMock<ICursoRepository>().Setup(r => r.UnitOfWork.Commit()).ReturnsAsync(true);
        var cursoService = new CursoService(_notificador.Object, _repository.Object);

        // Act
        await cursoService.Adicionar(curso);

        // Assert
        _mocker.GetMock<INotificador>().Verify(n => n.Handle(It.IsAny<Notificacao>()), Times.Exactly(7));
        _mocker.GetMock<ICursoRepository>().Verify(r => r.Adicionar(It.IsAny<Curso>()), Times.Never);
        _mocker.GetMock<ICursoRepository>().Verify(r => r.UnitOfWork.Commit(), Times.Never);
    }

    [Fact(DisplayName = "Adicionar Curso com valores inválidos")]
    [Trait("Categoria", "CursoService")]
    public async Task CursoService_Adicionar_DeveRetornarErrosDeCamposInválidos()
    {
        // Arrange
        var conteudoProgramatico = new Faker<ConteudoProgramatico>()
            .RuleFor(c => c.Tema, f => f.Lorem.Letter(2049))
            .RuleFor(c => c.NivelId, f => f.Random.Int(4, 10))
            .RuleFor(c => c.CargaHoraria, f => f.Random.Int(1001, 2000));

        var cursoFaker = new Faker<Curso>("pt_BR")
            .RuleFor(c => c.Nome, f => f.Lorem.Letter(101))
            .RuleFor(c => c.Autor, f => f.Lorem.Letter(101))
            .RuleFor(c => c.Validade, f => f.Date.PastDateOnly())
            .RuleFor(c => c.ConteudoProgramatico, f => conteudoProgramatico.Generate());

        var curso = cursoFaker.Generate();

        _mocker.GetMock<ICursoRepository>().Setup(r => r.UnitOfWork.Commit()).ReturnsAsync(true);
        var cursoService = new CursoService(_notificador.Object, _repository.Object);

        // Act
        await cursoService.Adicionar(curso);

        // Assert
        _mocker.GetMock<INotificador>().Verify(n => n.Handle(It.IsAny<Notificacao>()), Times.Exactly(6));
        _mocker.GetMock<ICursoRepository>().Verify(r => r.Adicionar(It.IsAny<Curso>()), Times.Never);
        _mocker.GetMock<ICursoRepository>().Verify(r => r.UnitOfWork.Commit(), Times.Never);
    }

    [Fact(DisplayName = "Adicionar Curso deve retornar erro de Curso Existente")]
    [Trait("Categoria", "CursoService")]
    public async Task CursoService_Adicionar_DeveRetornarCursoExistente()
    {
        // Arrange
        var curso = GerarCurso();
        _mocker.GetMock<ICursoRepository>().Setup(r => r.UnitOfWork.Commit()).ReturnsAsync(true);
        _mocker.GetMock<ICursoRepository>().Setup(r => r.BuscarAsync(c => c.Nome == curso.Nome))
            .ReturnsAsync([GerarCurso()]);
        var cursoService = new CursoService(_notificador.Object, _repository.Object);

        // Act
        await cursoService.Adicionar(curso);

        // Assert
        _mocker.GetMock<INotificador>().Verify(n => n.Handle(It.IsAny<Notificacao>()), Times.Exactly(1));
        _mocker.GetMock<ICursoRepository>().Verify(r => r.Adicionar(It.IsAny<Curso>()), Times.Never);
        _mocker.GetMock<ICursoRepository>().Verify(r => r.UnitOfWork.Commit(), Times.Never);
    }

    [Fact(DisplayName = "Adicionar Curso deve retornar uma mensagem de erro do banco")]
    [Trait("Categoria", "CursoService")]
    public async Task CursoService_Adicionar_DeveRetornarErroAoSalvarNoBanco()
    {
        // Arrange
        var curso = GerarCurso();
        _mocker.GetMock<ICursoRepository>().Setup(r => r.UnitOfWork.Commit()).ReturnsAsync(false);
        var cursoService = new CursoService(_notificador.Object, _repository.Object);

        // Act
        await cursoService.Adicionar(curso);

        // Assert
        _mocker.GetMock<INotificador>().Verify(n => n.Handle(It.IsAny<Notificacao>()), Times.Exactly(1));
        _mocker.GetMock<ICursoRepository>().Verify(r => r.Adicionar(It.IsAny<Curso>()), Times.Once);
        _mocker.GetMock<ICursoRepository>().Verify(r => r.UnitOfWork.Commit(), Times.Once);

    }

    [Fact(DisplayName = "Adicionar Aula a um Curso existente")]
    [Trait("Categoria", "CursoService")]
    public async Task CursoService_AdicionarAula_DeveRetornarSucesso()
    {
        // Arrange
        var curso = GerarCurso();
        var aula = GerarAula(curso.Id);
        _mocker.GetMock<ICursoRepository>().Setup(r => r.ObterPorIdAsync(curso.Id)).ReturnsAsync(curso);
        _mocker.GetMock<ICursoRepository>().Setup(r => r.UnitOfWork.Commit()).ReturnsAsync(true);
        var cursoService = new CursoService(_notificador.Object, _repository.Object);

        // Act
        await cursoService.AdicionarAula(curso.Id, aula);

        // Assert
        _mocker.GetMock<ICursoRepository>().Verify(r => r.AdicionarAula(It.IsAny<Aula>()), Times.Once);
        _mocker.GetMock<ICursoRepository>().Verify(r => r.UnitOfWork.Commit(), Times.Once);
    }

    [Fact(DisplayName = "Adicionar Aula a um Curso sem preencher os campos")]
    [Trait("Categoria", "CursoService")]
    public async Task CursoService_AdicionarAula_DeveRetornarErrosDeCamposNaoPreenchidos()
    {
        // Arrange
        var curso = GerarCurso();
        var aula = new Aula();
        _mocker.GetMock<ICursoRepository>().Setup(r => r.ObterPorIdAsync(curso.Id)).ReturnsAsync(curso);
        _mocker.GetMock<ICursoRepository>().Setup(r => r.UnitOfWork.Commit()).ReturnsAsync(true);
        var cursoService = new CursoService(_notificador.Object, _repository.Object);

        // Act
        await cursoService.AdicionarAula(curso.Id, aula);

        // Assert
        Assert.Equivalent(0, curso.Aulas?.Count);
        _mocker.GetMock<INotificador>().Verify(n => n.Handle(It.IsAny<Notificacao>()), Times.Exactly(3));
        _mocker.GetMock<ICursoRepository>().Verify(r => r.Atualizar(It.IsAny<Curso>()), Times.Never);
        _mocker.GetMock<ICursoRepository>().Verify(r => r.UnitOfWork.Commit(), Times.Never);
    }

    [Fact(DisplayName = "Adicionar Aula a um Curso com os campos inválidos")]
    [Trait("Categoria", "CursoService")]
    public async Task CursoService_AdicionarAula_DeveRetornarErrosDeCamposInválidos()
    {
        // Arrange
        var curso = GerarCurso();
        var aula = new Aula
        {
            CursoId = curso.Id,
            Titulo = new string('A', 101),
            LinkMaterial = new string('A', 2049)
        };
        _mocker.GetMock<ICursoRepository>().Setup(r => r.ObterPorIdAsync(curso.Id)).ReturnsAsync(curso);
        _mocker.GetMock<ICursoRepository>().Setup(r => r.UnitOfWork.Commit()).ReturnsAsync(true);
        var cursoService = new CursoService(_notificador.Object, _repository.Object);

        // Act
        await cursoService.AdicionarAula(curso.Id, aula);

        // Assert
        Assert.Equivalent(0, curso.Aulas?.Count);
        _mocker.GetMock<INotificador>().Verify(n => n.Handle(It.IsAny<Notificacao>()), Times.Exactly(2));
        _mocker.GetMock<ICursoRepository>().Verify(r => r.Atualizar(It.IsAny<Curso>()), Times.Never);
        _mocker.GetMock<ICursoRepository>().Verify(r => r.UnitOfWork.Commit(), Times.Never);
    }

    [Fact(DisplayName = "Adicionar Aula a um Curso inexistente")]
    [Trait("Categoria", "CursoService")]
    public async Task CursoService_AdicionarAula_DeveRetornarCursoInexistente()
    {
        // Arrange
        var curso = GerarCurso();
        var aula = GerarAula(curso.Id);
        _mocker.GetMock<ICursoRepository>().Setup(r => r.ObterPorIdAsync(curso.Id)).ReturnsAsync((Curso)null);
        _mocker.GetMock<ICursoRepository>().Setup(r => r.UnitOfWork.Commit()).ReturnsAsync(true);
        var cursoService = new CursoService(_notificador.Object, _repository.Object);

        // Act
        await cursoService.AdicionarAula(curso.Id, aula);

        // Assert
        _mocker.GetMock<INotificador>().Verify(n => n.Handle(It.IsAny<Notificacao>()), Times.Exactly(1));
        _mocker.GetMock<ICursoRepository>().Verify(r => r.Atualizar(It.IsAny<Curso>()), Times.Never);
        _mocker.GetMock<ICursoRepository>().Verify(r => r.UnitOfWork.Commit(), Times.Never);
    }

    [Fact(DisplayName = "Adicionar Aula a deve garantir a unicidade da aula")]
    [Trait("Categoria", "CursoService")]
    public async Task CursoService_AdicionarAula_DeveRetornarAulaComMesmoTitulo()
    {
        // Arrange
        var curso = GerarCurso();
        var aula1 = GerarAula(curso.Id);
        curso.Aulas.Add(aula1);
        var aula2 = GerarAula(curso.Id);
        aula2.Titulo = aula1.Titulo;
        _mocker.GetMock<ICursoRepository>().Setup(r => r.ObterPorIdAsync(curso.Id)).ReturnsAsync(curso);
        _mocker.GetMock<ICursoRepository>().Setup(r => r.UnitOfWork.Commit()).ReturnsAsync(true);
        var cursoService = new CursoService(_notificador.Object, _repository.Object);

        // Act
        await cursoService.AdicionarAula(curso.Id, aula2);

        // Assert
        _mocker.GetMock<INotificador>().Verify(n => n.Handle(It.IsAny<Notificacao>()), Times.Exactly(1));
        _mocker.GetMock<ICursoRepository>().Verify(r => r.Atualizar(It.IsAny<Curso>()), Times.Never);
        _mocker.GetMock<ICursoRepository>().Verify(r => r.UnitOfWork.Commit(), Times.Never);
    }

    [Fact(DisplayName = "Adicionar Curso deve retornar uma mensagem de erro do banco")]
    [Trait("Categoria", "CursoService")]
    public async Task CursoService_AdicionarAula_DeveRetornarErroAoSalvarNoBanco()
    {
        // Arrange
        var curso = GerarCurso();
        var aula = GerarAula(curso.Id);
        _mocker.GetMock<ICursoRepository>().Setup(r => r.ObterPorIdAsync(curso.Id)).ReturnsAsync(curso);
        _mocker.GetMock<ICursoRepository>().Setup(r => r.UnitOfWork.Commit()).ReturnsAsync(false);
        var cursoService = new CursoService(_notificador.Object, _repository.Object);

        // Act
        await cursoService.AdicionarAula(curso.Id, aula);

        // Assert
        _mocker.GetMock<ICursoRepository>().Verify(r => r.AdicionarAula(It.IsAny<Aula>()), Times.Once);
        _mocker.GetMock<ICursoRepository>().Verify(r => r.UnitOfWork.Commit(), Times.Once);
    }

    [Fact(DisplayName = "Atualizar Aula deve retornar erro de campos inválidos")]
    [Trait("Categoria", "AtualizarAula")]
    public async Task CursoService_AtualizarAula_DeveRetornarErroDeCamposInvalidos()
    {
        // Arrange
        var curso = GerarCurso();
        var aula1 = GerarAula(curso.Id);
        curso.Aulas.Add(aula1);
        var aula2 = new Aula
        {
            CursoId = curso.Id,
            Titulo = new string('A', 101),
            LinkMaterial = new string('A', 2049)
        };
        _repository.Setup(r => r.ObterPorIdAsync(curso.Id)).ReturnsAsync(curso);
        _repository.Setup(r => r.UnitOfWork.Commit()).ReturnsAsync(true);

        var cursoService = new CursoService(_notificador.Object, _repository.Object);

        // Act
        await cursoService.AtualizarAula(curso.Id, aula2);

        // Assert
        _mocker.GetMock<INotificador>().Verify(n => n.Handle(It.IsAny<Notificacao>()), Times.Exactly(2));
        _mocker.GetMock<ICursoRepository>().Verify(r => r.Atualizar(It.IsAny<Curso>()), Times.Never);
    }

    [Fact(DisplayName = "Atualizar Aula deve retornar erro curso não encontrado")]
    [Trait("Categoria", "AtualizarAula")]
    public async Task CursoService_AtualizarAula_DeveRetornarErroDeCursoNaoEncontrado()
    {
        // Arrange
        var curso = GerarCurso();
        var aula1 = GerarAula(curso.Id);
        curso.Aulas.Add(aula1);
        var aula2 = GerarAula(curso.Id);
        _repository.Setup(r => r.ObterPorIdAsync(curso.Id)).ReturnsAsync((Curso)null);
        _repository.Setup(r => r.UnitOfWork.Commit()).ReturnsAsync(true);

        var cursoService = new CursoService(_notificador.Object, _repository.Object);

        // Act
        await cursoService.AtualizarAula(curso.Id, aula2);

        // Assert
        _mocker.GetMock<INotificador>().Verify(n => n.Handle(It.IsAny<Notificacao>()), Times.Exactly(1));
        _notificador.Verify(n => n.Handle(It.Is<Notificacao>(nt => nt.Mensagem == "Curso não encontrado.")), Times.Once);
        _mocker.GetMock<ICursoRepository>().Verify(r => r.Atualizar(It.IsAny<Curso>()), Times.Never);
    }

    [Fact(DisplayName = "Atualizar Aula deve retornar erro Aula não encontrada neste curso.")]
    [Trait("Categoria", "AtualizarAula")]
    public async Task CursoService_AtualizarAula_DeveRetornarErroAulaNaoEncontrada()
    {
        // Arrange
        var curso = GerarCurso();
        var aula1 = GerarAula(curso.Id);
        curso.Aulas.Add(aula1);
        var aula2 = GerarAula(curso.Id);
        _repository.Setup(r => r.ObterPorIdAsync(curso.Id)).ReturnsAsync(curso);
        _repository.Setup(r => r.UnitOfWork.Commit()).ReturnsAsync(true);

        var cursoService = new CursoService(_notificador.Object, _repository.Object);

        // Act
        await cursoService.AtualizarAula(curso.Id, aula2);

        // Assert
        _mocker.GetMock<INotificador>().Verify(n => n.Handle(It.IsAny<Notificacao>()), Times.Exactly(1));
        _notificador.Verify(n => n.Handle(It.Is<Notificacao>(nt => nt.Mensagem == "Aula não encontrada neste curso.")), Times.Once);
        _mocker.GetMock<ICursoRepository>().Verify(r => r.Atualizar(It.IsAny<Curso>()), Times.Never);
    }

    [Fact(DisplayName = "Atualizar Aula deve retornar erro Já existe uma aula com este nome neste curso.")]
    [Trait("Categoria", "AtualizarAula")]
    public async Task CursoService_AtualizarAula_DeveRetornarErroDeAulaExistenteComMesmoTitulo()
    {
        // Arrange
        var curso = GerarCurso();
        var aula1 = GerarAula(curso.Id);
        curso.Aulas.Add(aula1);
        var aula2 = GerarAula(curso.Id);
        aula2.Titulo = aula1.Titulo;
        curso.Aulas.Add(aula2);
        _repository.Setup(r => r.ObterPorIdAsync(curso.Id)).ReturnsAsync(curso);
        _repository.Setup(r => r.UnitOfWork.Commit()).ReturnsAsync(true);

        var cursoService = new CursoService(_notificador.Object, _repository.Object);

        // Act
        await cursoService.AtualizarAula(curso.Id, aula2);

        // Assert
        _mocker.GetMock<INotificador>().Verify(n => n.Handle(It.IsAny<Notificacao>()), Times.Exactly(1));
        _notificador.Verify(n => n.Handle(It.Is<Notificacao>(nt => nt.Mensagem == "Já existe uma aula com este Titulo neste curso.")), Times.Once);
        _mocker.GetMock<ICursoRepository>().Verify(r => r.Atualizar(It.IsAny<Curso>()), Times.Never);
    }

    [Fact(DisplayName = "Atualizar Aula deve retornar sucesso")]
    [Trait("Categoria", "AtualizarAula")]
    public async Task CursoService_AtualizarAula_DeveRetornarSucesso()
    {
        // Arrange
        var curso = GerarCurso();
        var aula1 = GerarAula(curso.Id);
        curso.Aulas.Add(aula1);
        var aula2 = GerarAula(curso.Id);
        aula2.Id = aula1.Id;
        _repository.Setup(r => r.ObterPorIdAsync(curso.Id)).ReturnsAsync(curso);
        _repository.Setup(r => r.UnitOfWork.Commit()).ReturnsAsync(true);

        var cursoService = new CursoService(_notificador.Object, _repository.Object);

        // Act
        await cursoService.AtualizarAula(curso.Id, aula2);

        // Assert
        _mocker.GetMock<INotificador>().Verify(n => n.Handle(It.IsAny<Notificacao>()), Times.Exactly(0));
        _mocker.GetMock<ICursoRepository>().Verify(r => r.AtualizarAula(It.IsAny<Aula>()), Times.Once);
        _mocker.GetMock<ICursoRepository>().Verify(r => r.UnitOfWork.Commit(), Times.Once);
    }

    //[Fact(DisplayName = "Remover Aula deve retornar erro curso não encontrado")]
    //[Trait("Categoria", "RemoverAula")]
    //public async Task CursoService_RemoverAula_DeveRetornarErroDeCursoNaoEncontrado()
    //{
    //    // Arrange
    //    var curso = GerarCurso();
    //    var aula1 = GerarAula(curso.Id);
    //    curso.Aulas.Add(aula1);
    //    _repository.Setup(r => r.ObterPorIdAsync(curso.Id)).ReturnsAsync((Curso)null);
    //    _repository.Setup(r => r.UnitOfWork.Commit()).ReturnsAsync(true);

    //    var cursoService = new CursoService(_notificador.Object, _repository.Object);

    //    // Act
    //    await cursoService.RemoverAula(curso.Id, aula1.Id);

    //    // Assert
    //    _mocker.GetMock<INotificador>().Verify(n => n.Handle(It.IsAny<Notificacao>()), Times.Exactly(1));
    //    _notificador.Verify(n => n.Handle(It.Is<Notificacao>(nt => nt.Mensagem == "Curso não encontrado.")), Times.Once);
    //    _mocker.GetMock<ICursoRepository>().Verify(r => r.Atualizar(It.IsAny<Curso>()), Times.Never);
    //}

    //[Fact(DisplayName = "Remover Aula deve retornar erro Aula não encontrada neste curso.")]
    //[Trait("Categoria", "RemoverAula")]
    //public async Task CursoService_RemoverAula_DeveRetornarErroAulaNaoEncontrada()
    //{
    //    // Arrange
    //    var curso = GerarCurso();
    //    var aula1 = GerarAula(curso.Id);
    //    curso.Aulas.Add(aula1);
    //    _repository.Setup(r => r.ObterPorIdAsync(curso.Id)).ReturnsAsync(curso);
    //    _repository.Setup(r => r.UnitOfWork.Commit()).ReturnsAsync(true);

    //    var cursoService = new CursoService(_notificador.Object, _repository.Object);

    //    // Act
    //    await cursoService.RemoverAula(curso.Id, Guid.NewGuid());

    //    // Assert
    //    _mocker.GetMock<INotificador>().Verify(n => n.Handle(It.IsAny<Notificacao>()), Times.Exactly(1));
    //    _notificador.Verify(n => n.Handle(It.Is<Notificacao>(nt => nt.Mensagem == "Aula não encontrada neste curso.")), Times.Once);
    //    _mocker.GetMock<ICursoRepository>().Verify(r => r.Atualizar(It.IsAny<Curso>()), Times.Never);
    //}

    //[Fact(DisplayName = "Remover Aula deve retornar sucesso")]
    //[Trait("Categoria", "RemoverAula")]
    //public async Task CursoService_RemoverAula_DeveRetornarSucesso()
    //{
    //    // Arrange
    //    var curso = GerarCurso();
    //    var aula1 = GerarAula(curso.Id);
    //    curso.Aulas.Add(aula1);
    //    _repository.Setup(r => r.ObterPorIdAsync(curso.Id)).ReturnsAsync(curso);
    //    _repository.Setup(r => r.UnitOfWork.Commit()).ReturnsAsync(true);

    //    var cursoService = new CursoService(_notificador.Object, _repository.Object);

    //    // Act
    //    await cursoService.RemoverAula(curso.Id, aula1.Id);

    //    // Assert
    //    _mocker.GetMock<INotificador>().Verify(n => n.Handle(It.IsAny<Notificacao>()), Times.Exactly(0));
    //    _mocker.GetMock<ICursoRepository>().Verify(r => r.Atualizar(It.IsAny<Curso>()), Times.Once);
    //    _mocker.GetMock<ICursoRepository>().Verify(r => r.UnitOfWork.Commit(), Times.Once);
    //}

    private static Aula GerarAula(Guid cursoId)
    {
        var aulaFaker = new Faker<Aula>("pt_BR")
            .RuleFor(a => a.CursoId, cursoId)
            .RuleFor(a => a.Titulo, f => f.Lorem.Sentence(5))
            .RuleFor(a => a.LinkMaterial, f => f.Internet.Url());

        return aulaFaker.Generate();
    }

    [Fact(DisplayName = "Atualizar Curso com os campos inválidos")]
    [Trait("Categoria", "CursoService")]
    public async Task CursoSerive_Atualizar_DeveRetornarErroDeCamposInvalidos()
    {
        // Arrange
        var curso = new Curso
        {
            Nome = new string('A', 101),
            Autor = new string('A', 101),
            Validade = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
            ConteudoProgramatico = new ConteudoProgramatico
            {
                Tema = new string('A', 2049),
                NivelId = 4,
                CargaHoraria = 1001
            }
        };
        var cursoService = new CursoService(_notificador.Object, _repository.Object);

        // Act
        await cursoService.Atualizar(curso);

        // Assert
        _mocker.GetMock<INotificador>().Verify(n => n.Handle(It.IsAny<Notificacao>()), Times.Exactly(6));
        _mocker.GetMock<ICursoRepository>().Verify(r => r.Adicionar(It.IsAny<Curso>()), Times.Never);
    }

    [Fact(DisplayName = "Atualizar Curso deve garantir a unicidade do curso por nome")]
    [Trait("Categoria", "CursoService")]
    public async Task CursoSerive_Atualizar_DeveRetornarErroDeCursoComTituloIgual()
    {
        // Arrange
        var curso = GerarCurso();
        _repository.Setup(r => r.ObterPorIdAsync(curso.Id))
            .ReturnsAsync(curso);
        _repository.Setup(r => r.BuscarAsync(c => c.Nome == curso.Nome))
            .ReturnsAsync([GerarCurso()]);
        _repository.Setup(r => r.UnitOfWork.Commit()).ReturnsAsync(true);
        var cursoService = new CursoService(_notificador.Object, _repository.Object);

        // Act
        await cursoService.Atualizar(curso);

        // Assert
        _mocker.GetMock<INotificador>().Verify(n => n.Handle(It.IsAny<Notificacao>()), Times.Exactly(1));
        _mocker.GetMock<ICursoRepository>().Verify(r => r.UnitOfWork.Commit(), Times.Never);
        _notificador.Verify(n => n.Handle(It.Is<Notificacao>(nt => nt.Mensagem == "Já existe um curso cadastrado com este nome.")), Times.Once);
    }

    [Fact(DisplayName = "Atualizar Curso deve atualizar com sucesso")]
    [Trait("Categoria", "CursoService")]
    public async Task CursoSerive_Atualizar_DeveRetornarSucesso()
    {
        // Arrange
        var curso = GerarCurso();
        _mocker.GetMock<ICursoRepository>().Setup(r => r.ObterPorIdAsync(curso.Id))
            .ReturnsAsync(curso);
        _mocker.GetMock<ICursoRepository>().Setup(r => r.BuscarAsync(c => c.Nome == curso.Nome))
            .ReturnsAsync([]);
        _mocker.GetMock<ICursoRepository>().Setup(r => r.UnitOfWork.Commit()).ReturnsAsync(true);
        var cursoService = new CursoService(_notificador.Object, _repository.Object);

        // Act
        await cursoService.Atualizar(curso);

        // Assert
        _mocker.GetMock<INotificador>().Verify(n => n.Handle(It.IsAny<Notificacao>()), Times.Exactly(0));
        _mocker.GetMock<ICursoRepository>().Verify(r => r.Atualizar(It.IsAny<Curso>()), Times.Once);
        _mocker.GetMock<ICursoRepository>().Verify(r => r.UnitOfWork.Commit(), Times.Once);
    }

    //[Fact(DisplayName = "Remover Curso deve validar se existe")]
    //[Trait("Categoria", "CursoService")]
    //public async Task CursoSerive_Remover_DeveRetornarErroDeCursoNaoExiste()
    //{
    //    // Arrange
    //    _repository.Setup(r => r.Remover(It.IsAny<Guid>())).Throws(new DomainException("Curso não encontrado."));
    //    _repository.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>()))
    //        .ReturnsAsync((Curso)null);
    //    _repository.Setup(r => r.UnitOfWork.Commit()).ReturnsAsync(true);
    //    var cursoService = new CursoService(_notificador.Object, _repository.Object);

    //    // Act
    //    var ex = await Assert.ThrowsAsync<DomainException>(() => cursoService.Inativar(Guid.NewGuid()));

    //    // Assert
    //    Assert.Equal("Curso não encontrado.", ex.Message);
    //}

    //[Fact(DisplayName = "Remover Curso deve remover com sucesso")]
    //[Trait("Categoria", "CursoService")]
    //public async Task CursoSerive_Remover_DeveRetornarRemoverComSucesso()
    //{
    //    // Arrange
    //    var curso = GerarCurso();
    //    _repository.Setup(r => r.ObterPorIdAsync(curso.Id))
    //        .ReturnsAsync(curso);
    //    _repository.Setup(r => r.UnitOfWork.Commit()).ReturnsAsync(true);
    //    var cursoService = new CursoService(_notificador.Object, _repository.Object);

    //    // Act
    //    await cursoService.Inativar(curso.Id);

    //    // Assert
    //    _mocker.GetMock<INotificador>().Verify(n => n.Handle(It.IsAny<Notificacao>()), Times.Exactly(0));
    //    _mocker.GetMock<ICursoRepository>().Verify(r => r.Remover(It.IsAny<Guid>()), Times.Once);
    //    _mocker.GetMock<ICursoRepository>().Verify(r => r.UnitOfWork.Commit(), Times.Once);
    //}
}
