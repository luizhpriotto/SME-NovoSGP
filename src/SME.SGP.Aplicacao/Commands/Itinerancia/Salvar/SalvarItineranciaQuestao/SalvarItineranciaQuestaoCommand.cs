﻿using FluentValidation;
using MediatR;
using SME.SGP.Infra;

namespace SME.SGP.Aplicacao
{
    public class SalvarItineranciaQuestaoCommand : IRequest<AuditoriaDto>
    {
        public SalvarItineranciaQuestaoCommand(long questaoId, long itineranciaId, string resposta,long? arquivoId)
        {
            QuestaoId = questaoId;
            ItineranciaId = itineranciaId;
            Resposta = resposta;
            ArquivoId = arquivoId;
        }

        public long QuestaoId { get; set; }
        public long? ArquivoId { get; set; }
        public long ItineranciaId { get; set; }
        public string Resposta { get; set; }
    }

    public class SalvarItineranciaQuestaoCommandValidator : AbstractValidator<SalvarItineranciaQuestaoCommand>
    {
        public SalvarItineranciaQuestaoCommandValidator()
        {
            RuleFor(x => x.QuestaoId)
                   .GreaterThan(0)
                   .WithMessage("O id da questão do aluno da itinerância deve ser informado!");
            RuleFor(x => x.ItineranciaId)
                   .GreaterThan(0)
                   .WithMessage("O id da itinerância deve ser informado!");
        }
    }
}
