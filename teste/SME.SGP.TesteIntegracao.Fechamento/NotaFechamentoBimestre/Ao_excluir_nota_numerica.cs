﻿using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;
using SME.SGP.Aplicacao;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using SME.SGP.TesteIntegracao.Fechamento.ConselhoDeClasse.ServicosFakes;
using SME.SGP.TesteIntegracao.ServicosFakes;
using SME.SGP.TesteIntegracao.Setup;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SME.SGP.TesteIntegracao.NotaFechamentoBimestre
{
    public class Ao_excluir_nota_numerica : NotaFechamentoBimestreTesteBase
    {
        public Ao_excluir_nota_numerica(CollectionFixture collectionFixture) : base(collectionFixture)
        {
        }

        protected override void RegistrarFakes(IServiceCollection services)
        {
            base.RegistrarFakes(services);

            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterUsuarioPossuiPermissaoNaTurmaEDisciplinaQuery, bool>),
                typeof(ObterUsuarioPossuiPermissaoNaTurmaEDisciplinaQueryHandlerComPermissaoFake), ServiceLifetime.Scoped));
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterTurmaItinerarioEnsinoMedioQuery, IEnumerable<TurmaItinerarioEnsinoMedioDto>>), typeof(ObterTurmaItinerarioEnsinoMedioQueryHandlerFake), ServiceLifetime.Scoped));
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterTurmaCodigosAlunoPorAnoLetivoAlunoTipoTurmaQuery, string[]>), typeof(ObterTurmaCodigosAlunoPorAnoLetivoAlunoTipoTurmaQueryHandlerFake), ServiceLifetime.Scoped));
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterInfoComponentesCurricularesESPorTurmasCodigoQuery, IEnumerable<InfoComponenteCurricular>>), typeof(ObterInfoComponentesCurricularesESPorTurmasCodigoQueryHandlerFake), ServiceLifetime.Scoped));
        }

        [Fact(DisplayName = "Fechamento Bimestre - Deve excluir nota numérica lançada pelo Professor Titular em ano atual")]
        public async Task Deve_permitir_excluir_nota_titular_fundamental()
        {
            var filtroNotaFechamento = ObterFiltroFechamentoNotaDto(ObterPerfilProfessor(), ANO_7);

            await CriarDadosBase(filtroNotaFechamento);

            var dto = ObterListaFechamentoTurma(ObterListaDeFechamentoNumerica(COMPONENTE_CURRICULAR_PORTUGUES_ID_138), COMPONENTE_CURRICULAR_PORTUGUES_ID_138);
            var retorno = await ExecutarTesteComValidacaoNota(dto, TipoNota.Nota);
            var fechamentoDto = dto.FirstOrDefault();

            retorno.ShouldNotBeNull();

            fechamentoDto.Id = retorno.FirstOrDefault().Id;

            foreach (var fechamentoAluno in fechamentoDto.NotaConceitoAlunos)
                fechamentoAluno.Nota = null;

            await ExecutarTesteComValidacaoNota(dto, TipoNota.Nota);
            
            var historicoNotas = ObterTodos<HistoricoNota>();
            historicoNotas.Count.ShouldBe(8);
            
            var historicoNotasNotaFechamentos = ObterTodos<HistoricoNotaFechamento>();
            historicoNotasNotaFechamentos.Count.ShouldBe(8);
            
            historicoNotas.Count(w=> !w.NotaAnterior.HasValue).ShouldBe(4);
            historicoNotas.Count(w=> w.NotaAnterior.HasValue).ShouldBe(4);
            
            historicoNotas.Count(w=> !w.NotaAnterior.HasValue).ShouldBe(4);
            historicoNotas.Count(w=> w.NotaAnterior.HasValue).ShouldBe(4);
            
            historicoNotas.Any(w=> w.Id == 1 && !w.NotaAnterior.HasValue && w.NotaNova == NOTA_6).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 2 && !w.NotaAnterior.HasValue && w.NotaNova == NOTA_7).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 3 && !w.NotaAnterior.HasValue && w.NotaNova == NOTA_8).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 4 && !w.NotaAnterior.HasValue && w.NotaNova == NOTA_9).ShouldBeTrue();
            
            historicoNotas.Any(w=> w.Id == 5 && w.NotaAnterior == NOTA_6 && !w.NotaNova.HasValue).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 6 && w.NotaAnterior == NOTA_7 && !w.NotaNova.HasValue).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 7 && w.NotaAnterior == NOTA_8 && !w.NotaNova.HasValue).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 8 && w.NotaAnterior == NOTA_9 && !w.NotaNova.HasValue).ShouldBeTrue();
        }

        [Fact(DisplayName = "Fechamento Bimestre - Deve excluir nota numérica lançada pelo CP em ano atual")]
        public async Task Deve_permitir_excluir_nota_cp_fundamental()
        {
            var filtroNotaFechamento = ObterFiltroFechamentoNotaDto(ObterPerfilCP(), ANO_7);

            await CriarDadosBase(filtroNotaFechamento);

            var dto = ObterListaFechamentoTurma(ObterListaDeFechamentoNumerica(COMPONENTE_CURRICULAR_PORTUGUES_ID_138), COMPONENTE_CURRICULAR_PORTUGUES_ID_138);
            var retorno = await ExecutarTesteComValidacaoNota(dto, TipoNota.Nota);
            var fechamentoDto = dto.FirstOrDefault();

            retorno.ShouldNotBeNull();

            fechamentoDto.Id = retorno.FirstOrDefault().Id;

            foreach (var fechamentoAluno in fechamentoDto.NotaConceitoAlunos)
                fechamentoAluno.Nota = null;

            await ExecutarTesteComValidacaoNota(dto, TipoNota.Nota);
            
            var historicoNotas = ObterTodos<HistoricoNota>();
            historicoNotas.Count.ShouldBe(8);
            
            var historicoNotasNotaFechamentos = ObterTodos<HistoricoNotaFechamento>();
            historicoNotasNotaFechamentos.Count.ShouldBe(8);
            
            historicoNotas.Count(w=> !w.NotaAnterior.HasValue).ShouldBe(4);
            historicoNotas.Count(w=> w.NotaAnterior.HasValue).ShouldBe(4);
            
            historicoNotas.Count(w=> !w.NotaAnterior.HasValue).ShouldBe(4);
            historicoNotas.Count(w=> w.NotaAnterior.HasValue).ShouldBe(4);
            
            historicoNotas.Any(w=> w.Id == 1 && !w.NotaAnterior.HasValue && w.NotaNova == NOTA_6).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 2 && !w.NotaAnterior.HasValue && w.NotaNova == NOTA_7).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 3 && !w.NotaAnterior.HasValue && w.NotaNova == NOTA_8).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 4 && !w.NotaAnterior.HasValue && w.NotaNova == NOTA_9).ShouldBeTrue();
            
            historicoNotas.Any(w=> w.Id == 5 && w.NotaAnterior == NOTA_6 && !w.NotaNova.HasValue).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 6 && w.NotaAnterior == NOTA_7 && !w.NotaNova.HasValue).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 7 && w.NotaAnterior == NOTA_8 && !w.NotaNova.HasValue).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 8 && w.NotaAnterior == NOTA_9 && !w.NotaNova.HasValue).ShouldBeTrue();
        }

        [Fact(DisplayName = "Fechamento Bimestre - Deve excluir nota numérica lançada pelo DIRETOR em ano atual")]
        public async Task Deve_permitir_excluir_nota_diretor_fundamental()
        {
            var filtroNotaFechamento = ObterFiltroFechamentoNotaDto(ObterPerfilDiretor(), ANO_7);

            await CriarDadosBase(filtroNotaFechamento);

            var dto = ObterListaFechamentoTurma(ObterListaDeFechamentoNumerica(COMPONENTE_CURRICULAR_PORTUGUES_ID_138), COMPONENTE_CURRICULAR_PORTUGUES_ID_138);
            var retorno = await ExecutarTesteComValidacaoNota(dto, TipoNota.Nota);
            var fechamentoDto = dto.FirstOrDefault();

            retorno.ShouldNotBeNull();

            fechamentoDto.Id = retorno.FirstOrDefault().Id;

            foreach (var fechamentoAluno in fechamentoDto.NotaConceitoAlunos)
                fechamentoAluno.Nota = null;

            await ExecutarTesteComValidacaoNota(dto, TipoNota.Nota);
            
            var historicoNotas = ObterTodos<HistoricoNota>();
            historicoNotas.Count.ShouldBe(8);
            
            var historicoNotasNotaFechamentos = ObterTodos<HistoricoNotaFechamento>();
            historicoNotasNotaFechamentos.Count.ShouldBe(8);
            
            historicoNotas.Count(w=> !w.NotaAnterior.HasValue).ShouldBe(4);
            historicoNotas.Count(w=> w.NotaAnterior.HasValue).ShouldBe(4);
            
            historicoNotas.Count(w=> !w.NotaAnterior.HasValue).ShouldBe(4);
            historicoNotas.Count(w=> w.NotaAnterior.HasValue).ShouldBe(4);
            
            historicoNotas.Any(w=> w.Id == 1 && !w.NotaAnterior.HasValue && w.NotaNova == NOTA_6).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 2 && !w.NotaAnterior.HasValue && w.NotaNova == NOTA_7).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 3 && !w.NotaAnterior.HasValue && w.NotaNova == NOTA_8).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 4 && !w.NotaAnterior.HasValue && w.NotaNova == NOTA_9).ShouldBeTrue();
            
            historicoNotas.Any(w=> w.Id == 5 && w.NotaAnterior == NOTA_6 && !w.NotaNova.HasValue).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 6 && w.NotaAnterior == NOTA_7 && !w.NotaNova.HasValue).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 7 && w.NotaAnterior == NOTA_8 && !w.NotaNova.HasValue).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 8 && w.NotaAnterior == NOTA_9 && !w.NotaNova.HasValue).ShouldBeTrue();
        }

        [Fact(DisplayName = "Fechamento Bimestre - Deve excluir nota numérica lançada Professor Regente em ano atual")]
        public async Task Deve_permitir_excluir_nota_titular_regencia_classe_fundamental()
        {
            var filtroNotaFechamento = ObterFiltroFechamentoNotaDto(ObterPerfilProfessor(), ANO_7);

            await CriarDadosBase(filtroNotaFechamento);

            var dto = ObterListaFechamentoTurma(ObterListaDeFechamentoNumerica(COMPONENTE_REGENCIA_CLASSE_FUND_I_5H_ID_1105), COMPONENTE_REGENCIA_CLASSE_FUND_I_5H_ID_1105);
            var retorno = await ExecutarTesteComValidacaoNota(dto, TipoNota.Nota);
            var fechamentoDto = dto.FirstOrDefault();

            retorno.ShouldNotBeNull();

            fechamentoDto.Id = retorno.FirstOrDefault().Id;

            foreach (var fechamentoAluno in fechamentoDto.NotaConceitoAlunos)
                fechamentoAluno.Nota = null;

            await ExecutarTesteComValidacaoNota(dto, TipoNota.Nota);
            
            var historicoNotas = ObterTodos<HistoricoNota>();
            historicoNotas.Count.ShouldBe(8);
            
            var historicoNotasNotaFechamentos = ObterTodos<HistoricoNotaFechamento>();
            historicoNotasNotaFechamentos.Count.ShouldBe(8);
            
            historicoNotas.Count(w=> !w.NotaAnterior.HasValue).ShouldBe(4);
            historicoNotas.Count(w=> w.NotaAnterior.HasValue).ShouldBe(4);
            
            historicoNotas.Count(w=> !w.NotaAnterior.HasValue).ShouldBe(4);
            historicoNotas.Count(w=> w.NotaAnterior.HasValue).ShouldBe(4);
            
            historicoNotas.Any(w=> w.Id == 1 && !w.NotaAnterior.HasValue && w.NotaNova == NOTA_6).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 2 && !w.NotaAnterior.HasValue && w.NotaNova == NOTA_7).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 3 && !w.NotaAnterior.HasValue && w.NotaNova == NOTA_8).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 4 && !w.NotaAnterior.HasValue && w.NotaNova == NOTA_9).ShouldBeTrue();
            
            historicoNotas.Any(w=> w.Id == 5 && w.NotaAnterior == NOTA_6 && !w.NotaNova.HasValue).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 6 && w.NotaAnterior == NOTA_7 && !w.NotaNova.HasValue).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 7 && w.NotaAnterior == NOTA_8 && !w.NotaNova.HasValue).ShouldBeTrue();
            historicoNotas.Any(w=> w.Id == 8 && w.NotaAnterior == NOTA_9 && !w.NotaNova.HasValue).ShouldBeTrue();
        } 

        private List<FechamentoNotaDto> ObterListaDeFechamentoNumerica(long disciplina)
        {
            return new List<FechamentoNotaDto>()
            {
                ObterNotaNumerica(CODIGO_ALUNO_1, disciplina, (long)NOTA_6),
                ObterNotaNumerica(CODIGO_ALUNO_2, disciplina, (long)NOTA_7),
                ObterNotaNumerica(CODIGO_ALUNO_3, disciplina, (long)NOTA_8),
                ObterNotaNumerica(CODIGO_ALUNO_4, disciplina, (long)NOTA_9),
            };
        }
    }
}