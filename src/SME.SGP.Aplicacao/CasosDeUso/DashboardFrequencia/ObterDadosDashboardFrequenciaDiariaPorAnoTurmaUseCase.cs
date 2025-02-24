﻿using MediatR;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Infra;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterDadosDashboardFrequenciaDiariaPorAnoTurmaUseCase : AbstractUseCase, IObterDadosDashboardFrequenciaDiariaPorAnoTurmaUseCase
    {
        public ObterDadosDashboardFrequenciaDiariaPorAnoTurmaUseCase(IMediator mediator) : base(mediator)
        {
        }

        public async Task<GraficoFrequenciaAlunoDto> Executar(FrequenciasConsolidadacaoPorTurmaEAnoDto frequenciaDto)
        {
            var dadosFrequenciaAlunos = Enumerable.Empty<FrequenciaAlunoDashboardDto>();
            
            dadosFrequenciaAlunos = await mediator.Send(new ObterDadosDashboardFrequenciaDiariaPorAnoTurmaQuery(frequenciaDto));

            if (dadosFrequenciaAlunos.EhNulo() || !dadosFrequenciaAlunos.Any())
                return null;
            
            return MapearParaDto(dadosFrequenciaAlunos, frequenciaDto.Modalidade);
        }

        private GraficoFrequenciaAlunoDto MapearParaDto(IEnumerable<FrequenciaAlunoDashboardDto> frequenciasAlunos, int modalidade)
        {
            var dadosFrequenciaDashboard = new List<DadosRetornoFrequenciaAlunoDashboardDto>();

            foreach (var frequenciasGroup in frequenciasAlunos.GroupBy(f => f.Descricao))
            {
                var frequencia = frequenciasGroup.FirstOrDefault();
                var totalPresentes = frequenciasGroup.Select(f => f.Presentes).Sum();
                var totalAusentes = frequenciasGroup.Select(f => f.Ausentes).Sum();
                var totalRemotos = frequenciasGroup.Select(f => f.Remotos).Sum();

                dadosFrequenciaDashboard.Add(new DadosRetornoFrequenciaAlunoDashboardDto()
                {
                    Descricao = TipoFrequenciaDashboard.Presentes.Name(),
                    TurmaAno = frequencia.Descricao,
                    Quantidade = totalPresentes
                });

                dadosFrequenciaDashboard.Add(new DadosRetornoFrequenciaAlunoDashboardDto()
                {
                    Descricao = TipoFrequenciaDashboard.Remotos.Name(),
                    TurmaAno = frequencia.Descricao,
                    Quantidade = totalRemotos
                });

                dadosFrequenciaDashboard.Add(new DadosRetornoFrequenciaAlunoDashboardDto()
                {
                    Descricao = TipoFrequenciaDashboard.Ausentes.Name(),
                    TurmaAno = frequencia.Descricao,
                    Quantidade = totalAusentes
                });

                dadosFrequenciaDashboard.Add(new DadosRetornoFrequenciaAlunoDashboardDto()
                {
                    Descricao = modalidade == (int)Modalidade.EducacaoInfantil ? TipoFrequenciaDashboard.TotalCriancas.Name() : TipoFrequenciaDashboard.TotalEstudantes.Name(),
                    TurmaAno = frequencia.Descricao,
                    Quantidade = totalPresentes + totalAusentes + totalRemotos
                });
            }
            
            var dadosTotal = new TotalFrequenciaEAulasPorPeriodoDto()
            {
                TotalAulas = frequenciasAlunos.Select(a => a.TotalAulas).Sum(),
                TotalFrequencias = frequenciasAlunos.Select(a => a.TotalFrequencias).Sum(),
            };
            var totalFrequencia = dadosTotal.NaoEhNulo() ? dadosTotal.TotalFrequenciaFormatado : "";

            return new GraficoFrequenciaAlunoDto()
            {
                TagTotalFrequencia = totalFrequencia,
                TotalFrequenciaFormatado = totalFrequencia,
                DadosFrequenciaDashboard = dadosFrequenciaDashboard
            };
        }
    }
}
