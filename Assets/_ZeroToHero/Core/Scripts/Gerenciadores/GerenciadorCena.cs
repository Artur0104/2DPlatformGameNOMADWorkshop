using UnityEngine;
using UnityEngine.SceneManagement;
using ZeroToHero.Core.Base;

namespace ZeroToHero.Core.Gerenciadores
{
    /// <summary>
    /// Gerenciador de cenas (Singleton).
    /// Fornece métodos para transição e carregamento de cenas.
    /// Acesse de qualquer lugar: GerenciadorCena.Instancia
    /// </summary>
    public class GerenciadorCena : GerenciadorBase<GerenciadorCena>
    {
        /// <summary>
        /// Carrega uma cena pelo nome.
        /// </summary>
        /// <param name="nomeCena">Nome da cena (deve estar no Build Settings).</param>
        public void CarregarCena(string nomeCena)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(nomeCena);
            if (GerenciadorJogo.Instancia != null)
                GerenciadorJogo.Instancia.IniciarJogo();
        }

        /// <summary>
        /// Recarrega a cena atual.
        /// </summary>
        public void RecarregarCena()
        {
            CarregarCena(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// Sai do jogo (funciona em builds, não no Editor).
        /// </summary>
        public void SairDoJogo()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
