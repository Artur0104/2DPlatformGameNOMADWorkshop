using UnityEngine;

namespace ZeroToHero.Core.Interfaces
{
    /// <summary>
    /// Tipos de itens coletáveis suportados pelo template.
    /// </summary>
    public enum TipoColetavel
    {
        Moeda,
        Vida,
        Poder,
        Chave
    }

    /// <summary>
    /// Interface para itens coletáveis no jogo.
    /// Implemente em moedas, power-ups, itens de vida, etc.
    /// </summary>
    public interface IColetavel
    {
        /// <summary>
        /// Executa a coleta do item pelo objeto especificado.
        /// </summary>
        /// <param name="coletor">GameObject que coletou o item.</param>
        void Coletar(GameObject coletor);

        /// <summary>
        /// Retorna o tipo deste coletável.
        /// </summary>
        TipoColetavel GetTipo();
    }
}
