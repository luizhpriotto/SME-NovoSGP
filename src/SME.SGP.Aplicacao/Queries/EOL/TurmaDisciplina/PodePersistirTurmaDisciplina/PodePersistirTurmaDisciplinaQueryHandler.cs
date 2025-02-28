﻿using Elasticsearch.Net;
using MediatR;
using Newtonsoft.Json;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Enumerados;
using SME.SGP.Infra;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class PodePersistirTurmaDisciplinaQueryHandler : IRequestHandler<PodePersistirTurmaDisciplinaQuery, bool>
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IMediator mediator;

        public PodePersistirTurmaDisciplinaQueryHandler(IHttpClientFactory httpClientFactory, IMediator mediator)
        {
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
        public async Task<bool> Handle(PodePersistirTurmaDisciplinaQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var httpClient = httpClientFactory.CreateClient(ServicosEolConstants.SERVICO);
                var url = string.Format(ServicosEolConstants.URL_FUNCIONARIOS_TURMAS_DISCIPLINAS_ATRIBUICAO_VERIFICAR_DATA, request.CriadoRF, request.TurmaCodigo, request.ComponenteParaVerificarAtribuicao) + $"?dataConsultaTick={request.DataTick}";
           
                var resposta = await httpClient.GetAsync(url);
                if (resposta.IsSuccessStatusCode)
                {
                    var json = await resposta.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<bool>(json);
                }
                else
                {
                    string erro = $"Não foi possível validar a atribuição do professor no EOL - HttpCode {(int)resposta.StatusCode} - erro: {JsonConvert.SerializeObject(resposta.RequestMessage)}";
                    await mediator.Send(new SalvarLogViaRabbitCommand(erro, LogNivel.Negocio, LogContexto.Turma, string.Empty));
                    throw new NegocioException(erro);
                }
            }
            catch (Exception e)
            {
                var erro = $"Erro ao validar a atribuição do professor no EOL - Turma:{request.TurmaCodigo}, Professor:{request.CriadoRF}, Disciplina:{request.ComponenteParaVerificarAtribuicao} - Erro:{e.Message}";
                await mediator.Send(new SalvarLogViaRabbitCommand(erro, LogNivel.Negocio, LogContexto.Turma, e.Message));
                throw;
            }
        }
    }
}
