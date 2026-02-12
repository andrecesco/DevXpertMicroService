using AutoMapper;
using EduOnline.Alunos.Application.Queries.Dtos;
using EduOnline.Alunos.Domain.Enumeradores;
using EduOnline.Alunos.Domain.Models;
using EduOnline.Alunos.Domain.ValueObjects;
using EduOnline.Core.DomainObjects;

namespace EduOnline.Alunos.Application.Automapper;

public class ModelToDto : Profile
{
    public ModelToDto()
    {
        CreateMap<Aluno, AlunoDto>();
        CreateMap<Matricula, MatriculaDto>()
            .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.StatusId))
            .ForMember(dest => dest.StatusNome, opt => opt.MapFrom(src => Enumerador.GetById<MatriculaStatus>(src.StatusId)))
            .ForMember(dest => dest.PagamentoStatusId, opt => opt.MapFrom(src => src.PagamentoStatusId))
            .ForMember(dest => dest.PagamentoStatusNome, opt => opt.MapFrom(src => Enumerador.GetById<PagamentoStatus>(src.PagamentoStatusId)));
        CreateMap<HistoricoAprendizagem, HistoricoAprendizagemDto>();
        CreateMap<Certificado, CertificadoDto>();
    }
}
