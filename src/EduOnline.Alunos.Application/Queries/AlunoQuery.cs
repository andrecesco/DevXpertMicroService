using AutoMapper;
using EduOnline.Alunos.Application.Queries.Dtos;
using EduOnline.Alunos.Domain.Interfaces;
using EduOnline.Alunos.Domain.Models;
using System.Linq.Expressions;

namespace EduOnline.Alunos.Application.Queries;

public class AlunoQuery(IMapper mapper, IAlunoRepository repository) : IAlunoQuery
{
    public async Task<List<AlunoDto>> ObterTodos()
    {
        return mapper.Map<List<AlunoDto>>(await repository.ObterTodos());
    }

    public async Task<AlunoDto> ObterPorId(Guid id)
    {
        return mapper.Map<AlunoDto>(await repository.ObterPorId(id));
    }

    public async Task<List<AlunoDto>> BuscarAsync(Expression<Func<AlunoDto, bool>> predicate)
    {
        var entityPredicate = mapper.Map<Expression<Func<Aluno, bool>>>(predicate);
        var alunos = await repository.BuscarAsync(entityPredicate);
        return mapper.Map<List<AlunoDto>>(alunos);
    }

    public async Task<MatriculaDto> ObterMatriculaPorId(Guid id)
    {
        return mapper.Map<MatriculaDto>(await repository.ObterMatriculaPorId(id));
    }

    public async Task<List<MatriculaDto>> ObterMatriculasPorAlunoId(Guid alunoId)
    {
        return mapper.Map<List<MatriculaDto>>(await repository.ObterMatriculasPorAlunoId(alunoId));
    }

    public async Task<CertificadoDto> ObterCertificadoPorMatriculaId(Guid matriculaId)
    {
        return mapper.Map<CertificadoDto>(await repository.ObterCertificadoPorMatriculaId(matriculaId));
    }
}
