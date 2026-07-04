using UnityEngine;

namespace ZeroToHero.Core.Interfaces
{
    /// <summary>
    /// Interface para objetos com os quais o jogador pode interagir.
    /// Implemente em portas, alavancas, NPCs, baús, etc.
    /// </summary>
    public interface IInteragivel
    {
        /// <summary>
        /// Executa a interação com o objeto.
        /// </summary>
        /// <param name="interagente">GameObject que iniciou a interação.</param>
        void Interagir(GameObject interagente);

        /// <summary>
        /// Retorna a mensagem de dica exibida ao jogador quando próximo.
        /// Ex: "Pressione E para abrir", "Pressione E para falar".
        /// </summary>
        string GetMensagemInteracao();
    }
}
