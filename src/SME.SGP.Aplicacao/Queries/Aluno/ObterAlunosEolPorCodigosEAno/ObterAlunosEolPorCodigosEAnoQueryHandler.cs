﻿using MediatR;
using Newtonsoft.Json;
using SME.SGP.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterAlunosEolPorCodigosEAnoQueryHandler : IRequestHandler<ObterAlunosEolPorCodigosEAnoQuery, IEnumerable<TurmasDoAlunoDto>>
    {
        private readonly IHttpClientFactory httpClientFactory;

        public ObterAlunosEolPorCodigosEAnoQueryHandler(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<IEnumerable<TurmasDoAlunoDto>> Handle(ObterAlunosEolPorCodigosEAnoQuery request, CancellationToken cancellationToken)
        {
            var alunos = Enumerable.Empty<TurmasDoAlunoDto>();

            var codigosAlunos = String.Join("&codigosAluno=", request.CodigosAluno);

            var httpClient = httpClientFactory.CreateClient(ServicosEolConstants.SERVICO);
            var resposta = await httpClient.GetAsync(string.Format(ServicosEolConstants.URL_ALUNOS_ANO_LETIVO_ALUNOS, request.AnoLetivo) + $"?codigosAluno={codigosAlunos}");
            if (resposta.IsSuccessStatusCode)
            {
                var json = await resposta.Content.ReadAsStringAsync();
                alunos = JsonConvert.DeserializeObject<List<TurmasDoAlunoDto>>(json);
            }
            return alunos;
        }
    }
}
