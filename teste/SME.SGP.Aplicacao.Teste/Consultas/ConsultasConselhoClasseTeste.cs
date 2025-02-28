﻿using MediatR;
using Moq;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SME.SGP.Aplicacao.Teste.Consultas
{
    public class ConsultasConselhoClasseTeste
    {
        private readonly ConsultasConselhoClasse consultasConselhoClasse;
        private readonly Mock<IRepositorioConselhoClasseConsulta> repositorioConselhoClasseConsulta;
        private readonly Mock<IRepositorioPeriodoEscolarConsulta> repositorioPeriodoEscolar;
        private readonly Mock<IRepositorioConselhoClasseAlunoConsulta> repositorioConselhoClasseAluno;
        private readonly Mock<IRepositorioParametrosSistemaConsulta> repositorioParametrosSistema;
        private readonly Mock<IRepositorioTipoCalendarioConsulta> repositorioTipoCalendario;
        private readonly Mock<IRepositorioFechamentoTurmaConsulta> repositorioFechamentoTurma;
        private readonly Mock<IConsultasTurma> consultasTurma;
        private readonly Mock<IConsultasPeriodoFechamento> consultasPeriodoFechamento;
        private readonly Mock<IConsultasFechamentoTurma> consultasFechamentoTurma;
        private readonly Mock<IServicoDeNotasConceitos> servicoDeNotasConceitos;
        private readonly Mock<IRepositorioConselhoClasseConsolidado> repositorioConselhoClasseConsolidado;
        private readonly Mock<IMediator> mediator;

        public ConsultasConselhoClasseTeste()
        {
            repositorioConselhoClasseConsulta = new Mock<IRepositorioConselhoClasseConsulta>();
            repositorioPeriodoEscolar = new Mock<IRepositorioPeriodoEscolarConsulta>();
            repositorioConselhoClasseAluno = new Mock<IRepositorioConselhoClasseAlunoConsulta>();
            repositorioParametrosSistema = new Mock<IRepositorioParametrosSistemaConsulta>();
            repositorioTipoCalendario = new Mock<IRepositorioTipoCalendarioConsulta>();
            repositorioFechamentoTurma = new Mock<IRepositorioFechamentoTurmaConsulta>();
            consultasTurma = new Mock<IConsultasTurma>();
            consultasPeriodoFechamento = new Mock<IConsultasPeriodoFechamento>();
            consultasFechamentoTurma = new Mock<IConsultasFechamentoTurma>();
            servicoDeNotasConceitos = new Mock<IServicoDeNotasConceitos>();
            repositorioConselhoClasseConsolidado = new Mock<IRepositorioConselhoClasseConsolidado>();
            mediator = new Mock<IMediator>();
            consultasConselhoClasse = new ConsultasConselhoClasse(repositorioConselhoClasseConsulta.Object,
                                                                  repositorioConselhoClasseAluno.Object,
                                                                  repositorioPeriodoEscolar.Object,
                                                                  repositorioParametrosSistema.Object,
                                                                  repositorioTipoCalendario.Object,
                                                                  repositorioFechamentoTurma.Object,                                                                  
                                                                  consultasTurma.Object,
                                                                  consultasPeriodoFechamento.Object,
                                                                  consultasFechamentoTurma.Object,
                                                                  mediator.Object);
        }

        [Fact]
        public Task DeveObterResultado()
        {
            consultasTurma.Setup(t => t.ObterComUeDrePorCodigo(It.IsAny<string>())).Returns(Task.FromResult(ObterTurma()));
            consultasFechamentoTurma.Setup(f => f.ObterPorTurmaCodigoBimestreAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(ObterFechamentoTurma()));
            mediator.Setup(a => a.Send(It.IsAny<ObterUltimoPeriodoEscolarPorAnoModalidadeSemestreQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(ObterPeriodoEscolar());
            servicoDeNotasConceitos.Setup(tn => tn.ObterNotaTipo(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<bool>())).Returns(Task.FromResult(new NotaTipoValor()));
            repositorioParametrosSistema.Setup(m => m.ObterValorPorTipoEAno(It.IsAny<TipoParametroSistema>(),null)).Returns(Task.FromResult("10"));
            repositorioConselhoClasseAluno.Setup(c => c.ObterPorConselhoClasseAlunoCodigoAsync(It.IsAny<long>(), It.IsAny<string>())).Returns(Task.FromResult(new ConselhoClasseAluno()));
            Assert.NotNull(consultasConselhoClasse.ObterConselhoClasseTurma("", "", 0, true, false));

            return Task.CompletedTask;
        }

        [Fact]
        public async Task DeveTerErroAoObterTurma()
        {
            await Assert.ThrowsAsync<NegocioException>(() => consultasConselhoClasse.ObterConselhoClasseTurma("", "", 0, true, false));
        }

        [Fact]
        public async Task DeveTerErroFechamentoNaoLocalizadoAnoAtual()
        {
            var turma = ObterTurma();
            turma.AnoLetivo = DateTime.Today.Year;
            consultasTurma.Setup(t => t.ObterComUeDrePorCodigo(It.IsAny<string>())).Returns(Task.FromResult(turma));
            consultasFechamentoTurma.Setup(f => f.ObterPorTurmaCodigoBimestreAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(ObterFechamentoTurma()));
            await Assert.ThrowsAsync<NegocioException>(() => consultasConselhoClasse.ObterConselhoClasseTurma("", "", 0, true, false));
        }

        [Fact]
        public async Task ErroObterPeriodoUltimoBimestre()
        {
            consultasTurma.Setup(t => t.ObterComUeDrePorCodigo(It.IsAny<string>())).Returns(Task.FromResult(ObterTurma()));
            consultasFechamentoTurma.Setup(f => f.ObterPorTurmaCodigoBimestreAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(ObterFechamentoTurma()));
            await Assert.ThrowsAsync<NegocioException>(() => consultasConselhoClasse.ObterConselhoClasseTurma("", "", 0, true, false));
        }

        [Fact]
        public async Task ErroObterTipoNota()
        {
            consultasTurma.Setup(t => t.ObterComUeDrePorCodigo(It.IsAny<string>())).Returns(Task.FromResult(ObterTurma()));
            consultasFechamentoTurma.Setup(f => f.ObterPorTurmaCodigoBimestreAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(ObterFechamentoTurma()));
            mediator.Setup(a => a.Send(It.IsAny<ObterUltimoPeriodoEscolarPorAnoModalidadeSemestreQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(ObterPeriodoEscolar());
            await Assert.ThrowsAsync<NegocioException>(() => consultasConselhoClasse.ObterConselhoClasseTurma("", "", 0, true, false));
        }

        [Fact]
        public async Task DeveTerErroTipoCalendarioAnoAnterior()
        {
            var turma = ObterTurma();
            turma.AnoLetivo = DateTime.Today.Year-1;
            consultasTurma.Setup(t => t.ObterComUeDrePorCodigo(It.IsAny<string>())).Returns(Task.FromResult(turma));
            consultasFechamentoTurma.Setup(f => f.ObterPorTurmaCodigoBimestreAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(ObterFechamentoTurma()));
            await Assert.ThrowsAsync<NegocioException>(() => consultasConselhoClasse.ObterConselhoClasseTurma("", "", 0, true, false));
        }

        [Fact]
        public async Task DeveTerErroPeriodoEscolarAnoAnterior()
        {
            var turma = ObterTurma();
            turma.AnoLetivo = DateTime.Today.Year - 1;
            consultasTurma.Setup(t => t.ObterComUeDrePorCodigo(It.IsAny<string>())).Returns(Task.FromResult(turma));
            consultasFechamentoTurma.Setup(f => f.ObterPorTurmaCodigoBimestreAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(ObterFechamentoTurma()));
            repositorioTipoCalendario.Setup(t => t.BuscarPorAnoLetivoEModalidade(It.IsAny<int>(), It.IsAny<ModalidadeTipoCalendario>(), It.IsAny<int>())).Returns(Task.FromResult(new TipoCalendario()));
            await Assert.ThrowsAsync<NegocioException>(() => consultasConselhoClasse.ObterConselhoClasseTurma("", "", 0, true, false));
        }

        [Fact]
        public Task DeveObterResultadoAnoAnterior()
        {
            var turma = ObterTurma();
            turma.AnoLetivo = DateTime.Today.Year - 1;
            consultasTurma.Setup(t => t.ObterComUeDrePorCodigo(It.IsAny<string>())).Returns(Task.FromResult(turma));
            consultasFechamentoTurma.Setup(f => f.ObterPorTurmaCodigoBimestreAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(ObterFechamentoTurma()));
            repositorioTipoCalendario.Setup(t => t.BuscarPorAnoLetivoEModalidade(It.IsAny<int>(), It.IsAny<ModalidadeTipoCalendario>(), It.IsAny<int>())).Returns(Task.FromResult(new TipoCalendario()));
            repositorioPeriodoEscolar.Setup(p => p.ObterPorTipoCalendarioEBimestreAsync(It.IsAny<long>(), It.IsAny<int>())).Returns(Task.FromResult(new PeriodoEscolar()));
            mediator.Setup(a => a.Send(It.IsAny<ObterUltimoPeriodoEscolarPorAnoModalidadeSemestreQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(ObterPeriodoEscolar());
            servicoDeNotasConceitos.Setup(tn => tn.ObterNotaTipo(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<bool>())).Returns(Task.FromResult(new NotaTipoValor()));
            repositorioParametrosSistema.Setup(m => m.ObterValorPorTipoEAno(It.IsAny<TipoParametroSistema>(), null)).Returns(Task.FromResult("10"));
            repositorioConselhoClasseAluno.Setup(c => c.ObterPorConselhoClasseAlunoCodigoAsync(It.IsAny<long>(), It.IsAny<string>())).Returns(Task.FromResult(new ConselhoClasseAluno()));
            Assert.NotNull(consultasConselhoClasse.ObterConselhoClasseTurma("", "", 0, true, false));

            return Task.CompletedTask;
        }

        private Turma ObterTurma()
        {
            var turma = new Turma();
            return turma;
        }

        private FechamentoTurma ObterFechamentoTurma()
        {
            return new FechamentoTurma()
            {

            };
        }

        private ConselhoClasse ObterConselhoClasse()
        {
            return new ConselhoClasse() { };
        }

        private PeriodoEscolar ObterPeriodoEscolar()
        {
            return new PeriodoEscolar() { };
        }

        private PeriodoFechamentoBimestre ObterPeriodoFechamentoBimestre()
        {
            return new PeriodoFechamentoBimestre(0, ObterPeriodoEscolar(), null, null) { };
        }

    }
}
