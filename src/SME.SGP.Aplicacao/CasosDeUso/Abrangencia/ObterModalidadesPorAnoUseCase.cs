﻿using MediatR;
using SME.SGP.Aplicacao.Interfaces;
using SME.SGP.Infra;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public class ObterModalidadesPorAnoUseCase : IObterModalidadesPorAnoUseCase
    {
        private readonly IMediator mediator;

        public ObterModalidadesPorAnoUseCase(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task<IEnumerable<EnumeradoRetornoDto>> Executar(int anoLetivo, bool consideraHistorico, bool consideraNovasModalidades)
        {
            var login = await mediator
                .Send(ObterLoginAtualQuery.Instance);

            var perfil = await mediator
                .Send(ObterPerfilAtualQuery.Instance);

            var modalidadesQueSeraoIgnoradas = await mediator
                .Send(new ObterNovasModalidadesPorAnoQuery(anoLetivo, consideraNovasModalidades));

            return await mediator
                .Send(new ObterModalidadesPorAnoQuery(anoLetivo, consideraHistorico, login, perfil, modalidadesQueSeraoIgnoradas));
        }
    }
}