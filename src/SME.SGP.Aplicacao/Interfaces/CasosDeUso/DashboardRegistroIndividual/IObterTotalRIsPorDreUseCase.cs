﻿using SME.SGP.Infra;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Aplicacao
{
    public interface IObterTotalRIsPorDreUseCase
    {
        Task<IEnumerable<GraficoBaseDto>> Executar(FiltroDashboardTotalRIsPorDreDTO filtro);
    }
}
