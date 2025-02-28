﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SME.SGP.Api.Filtros;
using SME.SGP.Aplicacao;
using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Aplicacao.Interfaces.CasosDeUso;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Infra;
using SME.SGP.Infra.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Api.Controllers
{
    [ApiController]
    [Route("api/v1/encaminhamento-naapa")]
    [Authorize("Bearer")]
    public class EncaminhamentoNAAPAController : ControllerBase
    {
        [HttpPost("salvar")]
        [ProducesResponseType(typeof(IEnumerable<ResultadoEncaminhamentoNAAPADto>), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.NAAPA_I, Policy = "Bearer")]
        public async Task<IActionResult> RegistrarEncaminhamento([FromBody] EncaminhamentoNAAPADto encaminhamentoNAAPADto, [FromServices] IRegistrarEncaminhamentoNAAPAUseCase registrarEncaminhamentoNAAPAUseCase)
        {
            return Ok(await registrarEncaminhamentoNAAPAUseCase.Executar(encaminhamentoNAAPADto));
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginacaoResultadoDto<EncaminhamentoNAAPAResumoDto>), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.NAAPA_C, Policy = "Bearer")]
        public async Task<IActionResult> ObterEncaminhamentosNAAPA([FromQuery] FiltroEncaminhamentoNAAPADto filtro,
            [FromServices] IObterEncaminhamentoNAAPAUseCase useCase)
        {
            return Ok(await useCase.Executar(filtro));
        }

        [HttpGet("secoes")]
        [ProducesResponseType(typeof(IEnumerable<SecaoQuestionarioDto>), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.NAAPA_C, Policy = "Bearer")]
        public async Task<IActionResult> ObterSecoesDeEncaminhamentoNAAPA([FromQuery] FiltroSecoesDeEncaminhamento filtro,
            [FromServices] IObterSecoesEncaminhamentosSecaoNAAPAUseCase obterSecoesDeEncaminhamentoNAAPAUseCase)
        {
            return Ok(await obterSecoesDeEncaminhamentoNAAPAUseCase.Executar(filtro));
        }

        [HttpGet("{encaminhamentoNAAPAId}/secoes-itinerancia")]
        [ProducesResponseType(typeof(PaginacaoResultadoDto<SecaoQuestionarioDto>), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.NAAPA_C, Policy = "Bearer")]
        public async Task<IActionResult> ObterSecoesItineranciaDeEncaminhamentoNAAPA(long encaminhamentoNAAPAId,
            [FromServices] IObterSecoesItineranciaDeEncaminhamentoNAAPAUseCase obterSecoesItineranciaDeEncaminhamentoNAAPAUseCase)
        {
            return Ok(await obterSecoesItineranciaDeEncaminhamentoNAAPAUseCase.Executar(encaminhamentoNAAPAId));
        }

        [HttpGet("questionario")]
        [ProducesResponseType(typeof(IEnumerable<QuestaoDto>), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.NAAPA_C, Policy = "Bearer")]
        public async Task<IActionResult> ObterQuestionario([FromQuery] long questionarioId, [FromQuery] long? encaminhamentoId, [FromQuery] string codigoAluno, [FromQuery] string codigoTurma, [FromServices] IObterQuestionarioEncaminhamentoNAAPAUseCase useCase)
        {
            return Ok(await useCase.Executar(questionarioId, encaminhamentoId, codigoAluno, codigoTurma));
        }
        
        [HttpGet("situacoes")]
        [ProducesResponseType(typeof(IEnumerable<EnumeradoRetornoDto>), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.NAAPA_C, Policy = "Bearer")]
        public IActionResult ObterSituacoes()
        {
            var lista = EnumExtensao.ListarDto<SituacaoNAAPA>().OrderBy(situacao => situacao.Descricao);

            return Ok(lista);
        }

        [HttpGet("prioridades")]
        [ProducesResponseType(typeof(IEnumerable<PrioridadeEncaminhamentoNAAPADto>), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.NAAPA_C, Policy = "Bearer")]
        public async Task<IActionResult> ObterPrioridades([FromServices] IObterPrioridadeEncaminhamentoNAAPAUseCase useCase)
        {
            return Ok(await useCase.Executar());
        }

        [HttpDelete("arquivo")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.NAAPA_E, Policy = "Bearer")]
        public async Task<IActionResult> ExcluirArquivo([FromQuery] Guid arquivoCodigo, [FromServices] IExcluirArquivoNAAPAUseCase useCase)
        {
            return Ok(await useCase.Executar(arquivoCodigo));
        }

        [HttpPost("upload")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.NAAPA_I, Policy = "Bearer")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromServices] IUploadDeArquivoUseCase useCase)
        {
            if (file.Length > 0)
                return Ok(await useCase.Executar(file, Dominio.TipoArquivo.EncaminhamentoNAAPA));

            return BadRequest();
        }

        [HttpDelete("{encaminhamentoNAAPAId}")]
        [ProducesResponseType(typeof(EncaminhamentoNAAPADto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.NAAPA_E, Policy = "Bearer")]
        public async Task<IActionResult> ExcluirEncaminhamento(long encaminhamentoNAAPAId, [FromServices] IExcluirEncaminhamentoNAAPAUseCase useCase)
        {
            return Ok(await useCase.Executar(encaminhamentoNAAPAId));
        }

        [HttpGet("{encaminhamentoId}")]
        [ProducesResponseType(typeof(EncaminhamentoNAAPARespostaDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.NAAPA_C, Policy = "Bearer")]
        public async Task<IActionResult> ObterEncaminhamento(long encaminhamentoId, [FromServices] IObterEncaminhamentoNAAPAPorIdUseCase useCase)
        {
            return Ok(await useCase.Executar(encaminhamentoId));
        }

        [HttpGet("questionarioItinerario")]
        [ProducesResponseType(typeof(IEnumerable<QuestaoDto>), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.NAAPA_C, Policy = "Bearer")]
        public async Task<IActionResult> ObterQuestionarioItinerario([FromQuery] long questionarioId, [FromQuery] long? encaminhamentoSecaoId, [FromServices] IObterQuestionarioItinerarioEncaminhamentoNAAPAUseCase useCase)
        {
            return Ok(await useCase.Executar(questionarioId, encaminhamentoSecaoId));
        }

        [HttpDelete("{encaminhamentoNAAPAId}/secoes-itinerancia/{secaoItineranciaId}")]
        [ProducesResponseType(typeof(EncaminhamentoNAAPADto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.NAAPA_E, Policy = "Bearer")]
        public async Task<IActionResult> ExcluirSecaoItinerancia(long encaminhamentoNAAPAId, long secaoItineranciaId, [FromServices] IExcluirSecaoItineranciaEncaminhamentoNAAPAUseCase useCase)
        {
            return Ok(await useCase.Executar(encaminhamentoNAAPAId, secaoItineranciaId));
        }

        [HttpPost("salvarItinerario")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.NAAPA_I, Policy = "Bearer")]
        public async Task<IActionResult> RegistrarEncaminhamentoItinerario([FromBody] EncaminhamentoNAAPAItineranciaDto encaminhamentoNAAPAItineranciaDto, [FromServices] IRegistrarEncaminhamentoItinerarioNAAPAUseCase registrarEncaminhamentoItinerarioNAAPAUseCase)
        {
            return Ok(await registrarEncaminhamentoItinerarioNAAPAUseCase.Executar(encaminhamentoNAAPAItineranciaDto));
        }

        [HttpGet("{encaminhamentoNAAPAId}/situacao")]
        [ProducesResponseType(typeof(SituacaoDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.NAAPA_C, Policy = "Bearer")]
        public async Task<IActionResult> ObterSituacao(long encaminhamentoNAAPAId, [FromServices] IObterSituacaoEncaminhamentoNAAPAUseCase useCase)
        {
            return Ok(await useCase.Executar(encaminhamentoNAAPAId));
        }

        [HttpPost("encerrar")]
        [ProducesResponseType(typeof(RetornoBaseDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.NAAPA_A, Policy = "Bearer")]
        public async Task<IActionResult> EncerrarEncaminhamento([FromBody] EncerramentoEncaminhamentoNAAPADto parametros, [FromServices] IEncerrarEncaminhamentoNAAPAUseCase useCase)
        {
            return Ok(await useCase.Executar(parametros.EncaminhamentoId, parametros.MotivoEncerramento));
        }

        [HttpPost("reabrir/{encaminhamentoNAAPAId}")]
        [ProducesResponseType(typeof(RetornoBaseDto), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.NAAPA_A, Policy = "Bearer")]
        public async Task<IActionResult> ReabrirEncaminhamento(long encaminhamentoNAAPAId, [FromServices] IReabrirEncaminhamentoNAAPAUseCase useCase)
        {
            return Ok(await useCase.Executar(encaminhamentoNAAPAId));
        }

        [HttpGet("fluxos-alerta")]
        [ProducesResponseType(typeof(IEnumerable<OpcaoRespostaSimplesDto>), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.NAAPA_C, Policy = "Bearer")]
        public async Task<IActionResult> ObterFluxosAlerta([FromServices] IObterOpcoesRespostaFluxoAlertaEncaminhamentosNAAPAUseCase useCase) => Ok(await useCase.Executar());

        [HttpGet("portas-entrada")]
        [ProducesResponseType(typeof(IEnumerable<OpcaoRespostaSimplesDto>), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.NAAPA_C, Policy = "Bearer")]
        public async Task<IActionResult> ObterPortasEntrada([FromServices] IObterOpcoesRespostaPortaEntradaEncaminhamentosNAAPAUseCase useCase) => Ok(await useCase.Executar());

        [HttpPost("imprimir-detalhado")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.NAAPA_C, Policy = "Bearer")]
        public async Task<IActionResult> ImprimirDetalhado([FromBody] FiltroRelatorioEncaminhamentoNaapaDetalhadoDto filtro, [FromServices] IRelatorioEncaminhamentoNaapaDetalhadoUseCase detalhadoUseCase)
        {
            return Ok(await detalhadoUseCase.Executar(filtro));
        }

        [HttpGet("{encaminhamentoNAAPAId}/observacoes")]
        [ProducesResponseType(typeof(PaginacaoResultadoDto<EncaminhamentoNAAPAObservacoesDto>), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.NAAPA_C, Policy = "Bearer")]
        public async Task<IActionResult> ObterObservacoes(long encaminhamentoNAAPAId,
            [FromServices] IObterObservacoesDeEncaminhamentoNAAPAUseCase obterObservacoesDeEncaminhamentoNAAPAUseCase)
        {
            return Ok(await obterObservacoesDeEncaminhamentoNAAPAUseCase.Executar(encaminhamentoNAAPAId));
        }

        [HttpPost("salvar-observacao")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> SalvarObservacao([FromBody]EncaminhamentoNAAPAObservacaoSalvarDto filtro,[FromServices]ISalvarObservacoesDeEncaminhamentoNAAPAUseCase useCase)
        {
            return Ok(await useCase.Executar(filtro));
        }

        [HttpDelete("excluir-observacao/{observacaoId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> ExcluirObservacao(long observacaoId, [FromServices] IExcluirObservacoesDeEncaminhamentoNAAPAUseCase useCase)
        {
            return Ok(await useCase.Executar(observacaoId));
        }

        [HttpGet("{encaminhamentoNAAPAId}/historico-alteracoes")]
        [ProducesResponseType(typeof(PaginacaoResultadoDto<EncaminhamentoNAAPAObservacoesDto>), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.NAAPA_C, Policy = "Bearer")]
        public async Task<IActionResult> ObterHistoricoDeAlteracoes(long encaminhamentoNAAPAId,
            [FromServices] IObterHistoricosDeAlteracoesApresentacaoEncaminhamentoNAAPAUseCase useCase)
        {
            return Ok(await useCase.Executar(encaminhamentoNAAPAId));
        }

        [HttpGet("aluno/{codigoAluno}/existe-encaminhamento-ativo")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        [Permissao(Permissao.NAAPA_C, Policy = "Bearer")]
        public async Task<IActionResult> ExisteEncaminhamentoAtivoParaAluno(string codigoAluno, [FromServices] IExisteEncaminhamentoNAAPAAtivoParaAlunoUseCase useCase)
        {
            return Ok(await useCase.Executar(codigoAluno));
        }
    }
}