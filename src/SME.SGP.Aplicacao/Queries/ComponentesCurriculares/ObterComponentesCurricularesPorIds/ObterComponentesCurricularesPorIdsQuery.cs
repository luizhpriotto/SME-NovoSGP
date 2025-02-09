﻿using MediatR;
using SME.SGP.Infra;
using System.Collections.Generic;

namespace SME.SGP.Aplicacao
{
    public class ObterComponentesCurricularesPorIdsQuery : IRequest<IEnumerable<DisciplinaDto>>
    {
        public ObterComponentesCurricularesPorIdsQuery()
        { }

        public ObterComponentesCurricularesPorIdsQuery(long[] ids)
        {
            Ids = ids;
        }

        public long[] Ids { get; set; }
    }

}
