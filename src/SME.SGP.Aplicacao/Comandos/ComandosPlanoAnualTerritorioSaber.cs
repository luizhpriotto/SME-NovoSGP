﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SME.SGP.Dominio.Constantes.MensagensNegocio;
using SME.SGP.Infra.Utilitarios;

namespace SME.SGP.Aplicacao
{
    public class ComandosPlanoAnualTerritorioSaber : IComandosPlanoAnualTerritorioSaber
    {
        private readonly IRepositorioPlanoAnualTerritorioSaber repositorioPlanoAnualTerritorioSaber;
        private readonly IServicoUsuario servicoUsuario;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMediator mediator;
        private readonly IOptions<ConfiguracaoArmazenamentoOptions> configuracaoArmazenamentoOptions;

        public ComandosPlanoAnualTerritorioSaber(IRepositorioPlanoAnualTerritorioSaber repositorioPlanoAnualTerritorioSaber,
                                  IUnitOfWork unitOfWork,
                                  IServicoUsuario servicoUsuario, IMediator mediator,
                                  IOptions<ConfiguracaoArmazenamentoOptions> configuracaoArmazenamentoOptions)
        {
            this.repositorioPlanoAnualTerritorioSaber = repositorioPlanoAnualTerritorioSaber ?? throw new ArgumentNullException(nameof(repositorioPlanoAnualTerritorioSaber));
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            this.servicoUsuario = servicoUsuario ?? throw new ArgumentNullException(nameof(servicoUsuario));
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.configuracaoArmazenamentoOptions = configuracaoArmazenamentoOptions ?? throw new ArgumentNullException(nameof(configuracaoArmazenamentoOptions));
        }

        public async Task<IEnumerable<EntidadeBase>> Salvar(PlanoAnualTerritorioSaberDto planoAnualTerritorioSaberDto)
        {
            Validar(planoAnualTerritorioSaberDto);

            var listaAuditoria = new List<EntidadeBase>();

            unitOfWork.IniciarTransacao();
            try
            {
                var listaDescricao = new List<PlanoAnualTerritorioSaberResumidoDto>();
                var usuarioAtual = await servicoUsuario.ObterUsuarioLogado();

                if (string.IsNullOrWhiteSpace(usuarioAtual.CodigoRf))
                    throw new NegocioException("Não foi possível obter o RF do usuário.");

                foreach (var bimestrePlanoAnual in planoAnualTerritorioSaberDto.Bimestres)
                {
                    PlanoAnualTerritorioSaber planoAnualTerritorioSaber = await ObterPlanoAnualTerritorioSaberSimplificado(planoAnualTerritorioSaberDto, bimestrePlanoAnual.Bimestre.Value, usuarioAtual.EhProfessor() ? usuarioAtual.CodigoRf : null);
                    await ValidarPermissaoPersistenciaTurmaDisciplina(planoAnualTerritorioSaber, usuarioAtual, planoAnualTerritorioSaberDto.TurmaId.ToString(), planoAnualTerritorioSaberDto.TerritorioExperienciaId.ToString());
                    listaDescricao.Add(ObterPlanoAnualTerritorioSaberResumidoDto(planoAnualTerritorioSaber, bimestrePlanoAnual));
                    planoAnualTerritorioSaber = MapearParaDominio(planoAnualTerritorioSaberDto, planoAnualTerritorioSaber, bimestrePlanoAnual.Bimestre.Value, bimestrePlanoAnual.Desenvolvimento, bimestrePlanoAnual.Reflexao);
                    repositorioPlanoAnualTerritorioSaber.Salvar(planoAnualTerritorioSaber);
                    listaAuditoria.Add(planoAnualTerritorioSaber);
                }

                unitOfWork.PersistirTransacao();
                await MoverRemoverArquivosExcluidos(listaDescricao);
                return listaAuditoria;
            }
            catch
            {
                unitOfWork.Rollback();
                throw;
            }
        }

        private PlanoAnualTerritorioSaberResumidoDto ObterPlanoAnualTerritorioSaberResumidoDto(PlanoAnualTerritorioSaber planoAnualTerritorioSaber, BimestrePlanoAnualTerritorioSaberDto bimestrePlano)
        {
            return new PlanoAnualTerritorioSaberResumidoDto()
            {
                DesenvolvimentoNovo = bimestrePlano.Desenvolvimento,
                ReflexaoNovo = bimestrePlano.Reflexao,
                DesenvolvimentoAtual = planoAnualTerritorioSaber?.Desenvolvimento ?? string.Empty,
                ReflexaoAtual = planoAnualTerritorioSaber?.Reflexao ?? string.Empty
            };
        }

        private async Task ValidarPermissaoPersistenciaTurmaDisciplina(PlanoAnualTerritorioSaber planoAnualTerritorioSaber, Usuario usuario, string turmaId, string componenteCurricularId)
        {
            if (planoAnualTerritorioSaber.NaoEhNulo())
            {
                var podePersistirTurmaDisciplina = await servicoUsuario.PodePersistirTurmaDisciplina(usuario.CodigoRf, turmaId, componenteCurricularId, DateTime.Now);
                if (usuario.PerfilAtual == Perfis.PERFIL_PROFESSOR && !podePersistirTurmaDisciplina)
                    throw new NegocioException(MensagemNegocioComuns.Voce_nao_pode_fazer_alteracoes_ou_inclusoes_nesta_turma_componente_e_data);
            }

        }

        private async Task MoverRemoverArquivosExcluidos(List<PlanoAnualTerritorioSaberResumidoDto> planos)
        {
            foreach (var item in planos)
            {
                await MoverRemoverExcluidos(item.DesenvolvimentoNovo, item.DesenvolvimentoAtual);
                await MoverRemoverExcluidos(item.ReflexaoNovo, item.ReflexaoAtual);
            }
        }

        private async Task MoverRemoverExcluidos(string novo, string atual)
        {
            if (!string.IsNullOrEmpty(novo))
            {
                await mediator.Send(new MoverArquivosTemporariosCommand(TipoArquivo.TerritorioSaber, atual, novo));
            }
            if (!string.IsNullOrEmpty(atual))
            {
                await mediator.Send(new RemoverArquivosExcluidosCommand(atual, novo, TipoArquivo.TerritorioSaber.Name()));
            }
        }
        private void Validar(PlanoAnualTerritorioSaberDto planoAnualTerritorioSaberDto)
        {
            var bimestresDescricaoVazia = planoAnualTerritorioSaberDto.Bimestres.Where(b =>
                   string.IsNullOrEmpty(b.Desenvolvimento) && string.IsNullOrEmpty(b.Reflexao));

            if (bimestresDescricaoVazia.Any())
                throw new NegocioException($@"É necessário preencher o desenvolvimento e/ou reflexão do 
                                            {string.Join(", ", bimestresDescricaoVazia.Select(b => $"{b.Bimestre}º"))} bimestre");
        }

        private async Task<PlanoAnualTerritorioSaber> ObterPlanoAnualTerritorioSaberSimplificado(PlanoAnualTerritorioSaberDto planoAnualTerritorioSaberDto, int bimestre, string professor = null)
        {
            return await repositorioPlanoAnualTerritorioSaber.ObterPlanoAnualTerritorioSaberSimplificadoPorAnoEscolaBimestreETurma(planoAnualTerritorioSaberDto.AnoLetivo.Value,
                                                                                                      planoAnualTerritorioSaberDto.EscolaId,
                                                                                                      planoAnualTerritorioSaberDto.TurmaId.Value,
                                                                                                      bimestre,
                                                                                                      planoAnualTerritorioSaberDto.TerritorioExperienciaId,
                                                                                                      professor);
        }
        private PlanoAnualTerritorioSaber MapearParaDominio(PlanoAnualTerritorioSaberDto planoAnualTerritorioSaberDto, PlanoAnualTerritorioSaber planoAnualTerritorioSaber, int bimestre, string desenvolvimento, string reflexao)
        {
            if (planoAnualTerritorioSaber.EhNulo())
            {
                planoAnualTerritorioSaber = new PlanoAnualTerritorioSaber();
            }
            planoAnualTerritorioSaber.Ano = planoAnualTerritorioSaberDto.AnoLetivo.Value;
            planoAnualTerritorioSaber.Bimestre = bimestre;
            planoAnualTerritorioSaber.Reflexao = reflexao?.Replace(configuracaoArmazenamentoOptions.Value.BucketTemp, configuracaoArmazenamentoOptions.Value.BucketArquivos) ?? string.Empty;
            planoAnualTerritorioSaber.Desenvolvimento = desenvolvimento?.Replace(configuracaoArmazenamentoOptions.Value.BucketTemp, configuracaoArmazenamentoOptions.Value.BucketArquivos) ?? string.Empty;
            planoAnualTerritorioSaber.EscolaId = planoAnualTerritorioSaberDto.EscolaId;
            planoAnualTerritorioSaber.TurmaId = planoAnualTerritorioSaberDto.TurmaId.Value;
            planoAnualTerritorioSaber.TerritorioExperienciaId = planoAnualTerritorioSaberDto.TerritorioExperienciaId;
            return planoAnualTerritorioSaber;
        }
    }
}
