﻿using MediatR;
using Moq;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SME.SGP.Aplicacao.Teste.Queries
{
    public class ObterIndicativoPendenciasAulasPorTipoQueryHandlerTeste
    {

        private readonly ObterIndicativoPendenciasAulasPorTipoQueryHandler query;
        private readonly Mock<IRepositorioPendenciaAulaConsulta> repositorioPendenciaAula;
        private readonly Mock<IMediator> mediator;

        public ObterIndicativoPendenciasAulasPorTipoQueryHandlerTeste()
        {
            repositorioPendenciaAula = new Mock<IRepositorioPendenciaAulaConsulta>();
            mediator = new Mock<IMediator>();
            query = new ObterIndicativoPendenciasAulasPorTipoQueryHandler(repositorioPendenciaAula.Object);
        }

        [Fact]
        public async Task Deve_Verificar_Se_Nao_Ha_Pendencia_Diario_Bordo()
        {
            //Arrange

            var dados = new List<PossuiPendenciaDiarioBordoDto>();
            dados.Add(new PossuiPendenciaDiarioBordoDto()
            {
                TurmaId = "1",
                Bimestre = 1,
                AulaCJ = false
            });

            var aulas = new List<long>();
            aulas.Add(123);

            repositorioPendenciaAula.Setup(x => x.TrazerAulasComPendenciasDiarioBordo("512", "999", false, "", 2022))
                .ReturnsAsync(aulas);

            repositorioPendenciaAula.Setup(x => x.TurmasPendenciaDiarioBordo(aulas, "1", 1))
                .ReturnsAsync(dados);

            mediator.Setup(x => x.Send(It.IsAny<ObterIndicativoPendenciasAulasPorTipoQuery>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new Infra.PendenciaPaginaInicialListao() { PendenciaDiarioBordo = false });

            var turma = new Turma() { CodigoTurma = "1", ModalidadeCodigo = Modalidade.EducacaoInfantil };
            // Act
            var retornoConsulta = await query.Handle(new ObterIndicativoPendenciasAulasPorTipoQuery(turma, new Usuario(), "512", 2022, 1), new CancellationToken());

            // Assert
            Assert.NotNull(retornoConsulta);
            Assert.False(retornoConsulta.PendenciaDiarioBordo, "O usuário não possui pendência do diário para ser resolvida!");
        }
    }
}
