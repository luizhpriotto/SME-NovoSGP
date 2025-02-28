﻿using Dapper;
using SME.SGP.Dominio;
using SME.SGP.Dominio.Interfaces;
using SME.SGP.Infra;
using SME.SGP.Infra.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Dados.Repositorios
{
    public class RepositorioCartaIntencoesObservacao : RepositorioBase<CartaIntencoesObservacao>, IRepositorioCartaIntencoesObservacao
    {
        public RepositorioCartaIntencoesObservacao(ISgpContext conexao, IServicoAuditoria servicoAuditoria) : base(conexao, servicoAuditoria)
        { }

        public async Task<IEnumerable<CartaIntencoesObservacaoDto>> ListarPorTurmaEComponenteCurricularAsync(long turmaId, long componenteCurricularId, long usuarioLogadoId)
        {
            var sql = @"select
                            id,
                            observacao,
                            (usuario_id = @usuarioLogadoId) as Proprietario,
                            criado_em as CriadoEm,
                            criado_por as CriadoPor,
                            criado_rf as CriadoRf,
                            alterado_em as AlteradoEm,
                            alterado_por as AlteradoPor,
                            alterado_rf as AlteradoRf
                        from
                            carta_intencoes_observacao
                        where
                            turma_id = @turmaId and
                            componente_curricular_id = @componenteCurricularId and                    
                            not excluido 
                        order by criado_em desc";

            return await database.Conexao.QueryAsync<CartaIntencoesObservacaoDto>(sql, new { turmaId, componenteCurricularId, usuarioLogadoId });
        }

        public async Task<CartaIntencoesObservacaoDto> ObterCartaIntencoesObservacaoPorObservacaoId(long observacaoId)
        {
            const string sql = @" select cio.observacao, u.rf_codigo as UsuarioCodigoRfCartaIntencaoObs, u.nome as UsuarioNomeCartaIntencaoObs
                                    from carta_intencoes_observacao cio  
                                    inner join usuario u on u.rf_codigo = cio.criado_rf     
                                    where cio.id = @observacaoId";

            return await database.Conexao.QuerySingleOrDefaultAsync<CartaIntencoesObservacaoDto>(sql, new { observacaoId });
        }
    }
}
