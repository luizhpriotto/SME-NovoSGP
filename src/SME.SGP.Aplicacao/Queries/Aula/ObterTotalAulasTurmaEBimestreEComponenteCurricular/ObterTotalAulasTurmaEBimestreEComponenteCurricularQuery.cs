﻿using FluentValidation;
using MediatR;
using SME.SGP.Infra.Dtos;
using System;
using System.Collections.Generic;

namespace SME.SGP.Aplicacao
{
    public class ObterTotalAulasTurmaEBimestreEComponenteCurricularQuery : IRequest<IEnumerable<TurmaComponenteQntAulasDto>>
    {
        public ObterTotalAulasTurmaEBimestreEComponenteCurricularQuery(string[] turmasCodigo, long tipoCalendarioId, string[] componentesCurricularesId, int[] bimestres, DateTime? dataMatriculaAluno = null, DateTime? dataSituacaoAluno = null)
        {
            TurmasCodigo = turmasCodigo;
            TipoCalendarioId = tipoCalendarioId;
            ComponentesCurricularesId = componentesCurricularesId;
            Bimestres = bimestres;
            DataMatriculaAluno = dataMatriculaAluno;
            DataSituacaoAluno = dataSituacaoAluno;
        }

        public string[] TurmasCodigo { get; set; }
        public long TipoCalendarioId { get; set; }
        public string[] ComponentesCurricularesId { get; set; }
        public int[] Bimestres { get; set; }
        public DateTime? DataMatriculaAluno { get; set; }
        public DateTime? DataSituacaoAluno { get; set; }
    }

    public class ObterTotalAulasTurmaEBimestreEComponenteCurricularQueryValidator : AbstractValidator<ObterTotalAulasTurmaEBimestreEComponenteCurricularQuery>
    {
        public ObterTotalAulasTurmaEBimestreEComponenteCurricularQueryValidator()
        {
            RuleFor(x => x.TurmasCodigo)
                .NotEmpty()
                .WithMessage("A turma deve ser informada.");

            RuleFor(x => x.TipoCalendarioId)
                .NotEmpty()
                .WithMessage("O tipo de calendário deve ser informado.");
        }
    }
}