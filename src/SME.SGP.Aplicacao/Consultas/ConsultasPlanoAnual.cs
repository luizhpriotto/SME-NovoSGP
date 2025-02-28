﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ConsultasPlanoAnual : IConsultasPlanoAnual
    {
        private readonly IConsultasObjetivoAprendizagem consultasObjetivoAprendizagem;
        private readonly IRepositorioComponenteCurricularJurema repositorioComponenteCurricular;
        private readonly IRepositorioPeriodoEscolarConsulta repositorioPeriodoEscolar;
        private readonly IRepositorioPlanoAnual repositorioPlanoAnual;
        private readonly IRepositorioTipoCalendarioConsulta repositorioTipoCalendario;
        private readonly IServicoUsuario servicoUsuario;
        private readonly IMediator mediator;

        public ConsultasPlanoAnual(IRepositorioPlanoAnual repositorioPlanoAnual,
                                   IConsultasObjetivoAprendizagem consultasObjetivoAprendizagem,
                                   IRepositorioPeriodoEscolarConsulta repositorioPeriodoEscolar,
                                   IRepositorioTipoCalendarioConsulta repositorioTipoCalendario,
                                   IRepositorioComponenteCurricularJurema repositorioComponenteCurricular,
                                   IServicoUsuario servicoUsuario,IMediator mediator)
        {
            this.repositorioPlanoAnual = repositorioPlanoAnual ?? throw new System.ArgumentNullException(nameof(repositorioPlanoAnual));
            this.consultasObjetivoAprendizagem = consultasObjetivoAprendizagem ?? throw new System.ArgumentNullException(nameof(consultasObjetivoAprendizagem));
            this.repositorioPeriodoEscolar = repositorioPeriodoEscolar ?? throw new ArgumentNullException(nameof(repositorioPeriodoEscolar));
            this.repositorioTipoCalendario = repositorioTipoCalendario ?? throw new ArgumentNullException(nameof(repositorioTipoCalendario));
            this.repositorioComponenteCurricular = repositorioComponenteCurricular ?? throw new ArgumentNullException(nameof(repositorioComponenteCurricular));
            this.servicoUsuario = servicoUsuario ?? throw new ArgumentNullException(nameof(servicoUsuario));
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<PlanoAnualCompletoDto> ObterBimestreExpandido(FiltroPlanoAnualBimestreExpandidoDto filtro)
        {
            var planoAnualLista = new List<PlanoAnualCompletoDto>();

            var bimestres = filtro.ModalidadePlanoAnual == Modalidade.EJA ? 2 : 4;

            var filtroPlanoAnualDto = ObtenhaFiltro(filtro.AnoLetivo, filtro.ComponenteCurricularEolId, filtro.EscolaId, filtro.TurmaId, 0);

            for (int i = 1; i <= bimestres; i++)
            {
                filtroPlanoAnualDto.Bimestre = i;

                planoAnualLista.Add(await ObterPorEscolaTurmaAnoEBimestre(filtroPlanoAnualDto));
            }

            var periodosEscolares = await ObterPeriodoEscolar(filtro.TurmaId, filtro.AnoLetivo);

            if (periodosEscolares.EhNulo())
                return null;

            var retorno = planoAnualLista.FirstOrDefault(x => VerificaSeBimestreEhExpandido(periodosEscolares, x.Bimestre));

            return retorno;
        }

        public long ObterIdPlanoAnualPorAnoEscolaBimestreETurma(int ano, string escolaId, long turmaId, int bimestre, long disciplinaId)
        {
            var plano = repositorioPlanoAnual.ObterPlanoAnualSimplificadoPorAnoEscolaBimestreETurma(ano, escolaId, turmaId, bimestre, disciplinaId);
            return plano.NaoEhNulo() ? plano.Id : 0;
        }

        public async Task<PlanoAnualObjetivosDisciplinaDto> ObterObjetivosEscolaTurmaDisciplina(FiltroPlanoAnualDisciplinaDto filtro)
        {
            var planoAnual = repositorioPlanoAnual.ObterPlanoObjetivosEscolaTurmaDisciplina(filtro.AnoLetivo,
                                                            filtro.EscolaId,
                                                            filtro.TurmaId,
                                                            filtro.Bimestre,
                                                            filtro.ComponenteCurricularEolId,
                                                            filtro.DisciplinaId);
            if (planoAnual.NaoEhNulo())
            {
                var objetivosAprendizagem = await consultasObjetivoAprendizagem.Listar();

                if (planoAnual.IdsObjetivosAprendizagem.EhNulo())
                    return planoAnual;

                foreach (var idObjetivo in planoAnual.IdsObjetivosAprendizagem)
                {
                    var objetivo = objetivosAprendizagem.FirstOrDefault(c => c.Id == idObjetivo);
                    if (objetivo.NaoEhNulo())
                    {
                        planoAnual.ObjetivosAprendizagem.Add(objetivo);
                    }
                }
            }
            return planoAnual;
        }

        public async Task<PlanoAnualCompletoDto> ObterPorEscolaTurmaAnoEBimestre(FiltroPlanoAnualDto filtroPlanoAnualDto, bool seNaoExistirRetornaNovo = true)
        {
            var planoAnual = repositorioPlanoAnual.ObterPlanoAnualCompletoPorAnoEscolaBimestreETurma(filtroPlanoAnualDto.AnoLetivo, filtroPlanoAnualDto.EscolaId, filtroPlanoAnualDto.TurmaId, filtroPlanoAnualDto.Bimestre, filtroPlanoAnualDto.ComponenteCurricularEolId);

            if (planoAnual.NaoEhNulo())
            {
                var objetivosAprendizagem = await consultasObjetivoAprendizagem.Listar();

                if (planoAnual.IdsObjetivosAprendizagem.EhNulo())
                    return planoAnual;

                foreach (var idObjetivo in planoAnual.IdsObjetivosAprendizagem)
                {
                    var objetivo = objetivosAprendizagem.FirstOrDefault(c => c.Id == idObjetivo);
                    if (objetivo.NaoEhNulo())
                    {
                        planoAnual.ObjetivosAprendizagem.Add(objetivo);
                    }
                }
            }
            else if (seNaoExistirRetornaNovo)
                planoAnual = await ObterNovoPlanoAnual(filtroPlanoAnualDto.TurmaId, filtroPlanoAnualDto.AnoLetivo, filtroPlanoAnualDto.EscolaId);

            return planoAnual;
        }

        public async Task<IEnumerable<PlanoAnualCompletoDto>> ObterPorUETurmaAnoEComponenteCurricular(string ueId, string turmaId, int anoLetivo, long componenteCurricularEolId)
        {
            var periodos = await ObterPeriodoEscolar(turmaId, anoLetivo);
            var dataAtual = DateTime.Now.Date;
            var listaPlanoAnual = repositorioPlanoAnual.ObterPlanoAnualCompletoPorAnoUEETurma(anoLetivo, ueId, turmaId, componenteCurricularEolId);
            var componentesCurricularesEol = repositorioComponenteCurricular.Listar();
            if (listaPlanoAnual.PossuiRegistros())
            {
                var objetivosAprendizagem = await consultasObjetivoAprendizagem.Listar();
                foreach (var planoAnual in listaPlanoAnual)
                {
                    var periodo = periodos.FirstOrDefault(c => c.Bimestre == planoAnual.Bimestre)
                                  ?? throw new NegocioException("Plano anual com data fora do período escolar. Contate o suporte.");
                    PreencherPlanoAnualObrigatorio(planoAnual, periodo, dataAtual);
                    AdicionarObjetivosAprendizagem(planoAnual, objetivosAprendizagem, componentesCurricularesEol);
                }
                if (listaPlanoAnual.Count() != periodos.Count())
                {
                    var periodosFaltantes = periodos.Where(c => !listaPlanoAnual.Any(p => p.Bimestre == c.Bimestre));
                    var planosFaltantes = ObterNovoPlanoAnualCompleto(turmaId, anoLetivo, ueId, periodosFaltantes, dataAtual).ToList();
                    planosFaltantes.AddRange(listaPlanoAnual);
                    listaPlanoAnual = planosFaltantes;
                }
            }
            else
                listaPlanoAnual = ObterNovoPlanoAnualCompleto(turmaId, anoLetivo, ueId, periodos, dataAtual);
            return listaPlanoAnual.OrderBy(c => c.Bimestre);
        }

        private void AdicionarObjetivosAprendizagem(PlanoAnualCompletoDto planoAnual,
                                                    IEnumerable<ObjetivoAprendizagemDto> objetivosAprendizagem,
                                                    IEnumerable<ComponenteCurricularJurema> componentesCurricularesEol)
        {
            foreach (var idObjetivo in (planoAnual.IdsObjetivosAprendizagem ?? Enumerable.Empty<long>()))
            {
                var objetivo = objetivosAprendizagem.FirstOrDefault(c => c.Id == idObjetivo);
                if (objetivo.NaoEhNulo())
                {
                    var componenteCurricularEol = componentesCurricularesEol.FirstOrDefault(c => c.CodigoJurema == objetivo.IdComponenteCurricular);
                    if (componenteCurricularEol.NaoEhNulo())
                        objetivo.ComponenteCurricularEolId = componenteCurricularEol.CodigoEOL;
                    planoAnual.ObjetivosAprendizagem.Add(objetivo);
                }
            }
        }
        private void PreencherPlanoAnualObrigatorio(PlanoAnualCompletoDto planoAnual, PeriodoEscolar periodo, DateTime dataAtual)
        {
            if (periodo.PeriodoFim.Local() >= dataAtual 
                && periodo.PeriodoInicio.Local() <= dataAtual)
                planoAnual.Obrigatorio = true;
        }
        public async Task<IEnumerable<TurmaParaCopiaPlanoAnualDto>> ObterTurmasParaCopia(int turmaId, long componenteCurricular, bool consideraHistorico)
        {
            var codigoRfUsuarioLogado = servicoUsuario.ObterRf();
            var turmasEOL = await mediator.Send(new ObterTurmasParaCopiaPlanoAnualQuery(codigoRfUsuarioLogado, componenteCurricular, turmaId));
            if (turmasEOL.NaoEhNulo() && turmasEOL.Any())
            {
                var idsTurmas = turmasEOL.Select(c => c.TurmaId.ToString());                
                turmasEOL = await mediator.Send(new ValidaSeTurmasPossuemPlanoAnualQuery(idsTurmas.ToArray(), consideraHistorico));
            }
            return turmasEOL;
        }

        public bool ValidarPlanoAnualExistente(FiltroPlanoAnualDto filtroPlanoAnualDto)
        {
            return repositorioPlanoAnual.ValidarPlanoExistentePorAnoEscolaTurmaEBimestre(filtroPlanoAnualDto.AnoLetivo, filtroPlanoAnualDto.EscolaId, filtroPlanoAnualDto.TurmaId, filtroPlanoAnualDto.Bimestre, filtroPlanoAnualDto.ComponenteCurricularEolId);
        }

        private static PlanoAnualCompletoDto ObterPlanoAnualPorBimestre(string turmaId, int anoLetivo, string ueId, int bimestre, bool obrigatorio)
        {
            return new PlanoAnualCompletoDto
            {
                Bimestre = bimestre,
                Migrado = false,
                EscolaId = ueId,
                TurmaId = turmaId,
                AnoLetivo = anoLetivo,
                Obrigatorio = obrigatorio
            };
        }

        private FiltroPlanoAnualDto ObtenhaFiltro(int anoLetivo, long componenteCurricularEolId, string escolaId, string turmaId, int bimestre)
        {
            return new FiltroPlanoAnualDto
            {
                AnoLetivo = anoLetivo,
                ComponenteCurricularEolId = componenteCurricularEolId,
                EscolaId = escolaId,
                TurmaId = turmaId,
                Bimestre = bimestre
            };
        }

        private async Task<PlanoAnualCompletoDto> ObterNovoPlanoAnual(string turmaId, int anoLetivo, string ueId)
        {
            var periodos = await ObterPeriodoEscolar(turmaId, anoLetivo);

            var periodo = periodos.FirstOrDefault(c => c.PeriodoFim >= DateTime.Now.Date && c.PeriodoInicio <= DateTime.Now.Date);

            return new PlanoAnualCompletoDto
            {
                Bimestre = periodo.Bimestre,
                Migrado = false,
                EscolaId = ueId,
                TurmaId = turmaId,
            };
        }

        private IEnumerable<PlanoAnualCompletoDto> ObterNovoPlanoAnualCompleto(string turmaId, int anoLetivo, string ueId, IEnumerable<PeriodoEscolar> periodos, DateTime dataAtual)
        {
            var listaPlanoAnual = new List<PlanoAnualCompletoDto>();
            foreach (var periodo in periodos)
            {
                var obrigatorio = false;
                if (periodo.PeriodoFim.Local() >= dataAtual && periodo.PeriodoInicio.Local() <= dataAtual)
                {
                    obrigatorio = true;
                }
                listaPlanoAnual.Add(ObterPlanoAnualPorBimestre(turmaId, anoLetivo, ueId, periodo.Bimestre, obrigatorio));
            }
            return listaPlanoAnual;
        }

        private async Task<IEnumerable<PeriodoEscolar>> ObterPeriodoEscolar(string turmaId, int anoLetivo)
        {
            var turma = await mediator.Send(new ObterTurmaPorCodigoQuery(turmaId));
            if (turma.EhNulo())
            {
                throw new NegocioException("Turma não encontrada.");
            }
            var tipoCalendario = await repositorioTipoCalendario.BuscarPorAnoLetivoEModalidade(anoLetivo, turma.ModalidadeCodigo.ObterModalidadeTipoCalendario(), turma.Semestre);
            if (tipoCalendario.EhNulo())
            {
                throw new NegocioException("Tipo de calendário não encontrado.");
            }

            var periodos = await repositorioPeriodoEscolar.ObterPorTipoCalendario(tipoCalendario.Id);
            if (periodos.EhNulo())
            {
                throw new NegocioException("Período escolar não encontrado.");
            }
            return periodos;
        }

        private bool VerificaSeBimestreEhExpandido(IEnumerable<PeriodoEscolar> periodosEscolares, int bimestre)
        {
            var periodo = periodosEscolares.FirstOrDefault(p => p.Bimestre == bimestre);

            if (periodo.EhNulo())
                return false;

            var dataAtual = DateTime.Now;

            return periodo.PeriodoInicio <= dataAtual && periodo.PeriodoFim >= dataAtual;
        }
    }
}