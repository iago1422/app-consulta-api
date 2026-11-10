using System;
using System.Threading.Tasks;

namespace Spark.Domain.Services
{
    /// <summary>Custos vindos da configuração (painel), aplicados pela API.</summary>
    public interface ICustoConsulta
    {
        /// <summary>Créditos do tipo; null se o tipo não está configurado (ex.: "VIDEO" de versões antigas).</summary>
        Task<int?> CreditosDoTipo(string tipo);

        /// <summary>Créditos da consulta cuja entrada na fila tem id = referencia; null se não for uma.</summary>
        Task<int?> CreditosDaFila(string referencia);

        /// <summary>Saldo do usuário dono da ficha; 0 se não tem conta de crédito.</summary>
        Task<double> SaldoDoDonoDaFicha(Guid fichaId);
    }
}
