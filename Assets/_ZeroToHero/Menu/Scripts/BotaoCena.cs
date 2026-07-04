using UnityEngine;

namespace ZeroToHero.Menu
{
    /// <summary>
    /// Botão que carrega uma cena ao ser clicado.
    /// Simples: arraste para um botão, defina o nome da cena no Inspector.
    /// </summary>
    public class BotaoCena : MonoBehaviour
    {
        [Header("Configuração")]
        [SerializeField, Tooltip("Nome da cena a ser carregada (deve estar no Build Settings).")]
        private string nomeCena = "Plataforma";

        [SerializeField, Tooltip("Se ativado, usa GerenciadorCena (singleton). Senão, usa SceneManager diretamente.")]
        private bool usarGerenciador = true;

        /// <summary>
        /// Carrega a cena configurada.
        /// Conecte ao evento OnClick de um botão Unity UI.
        /// </summary>
        public void CarregarCena()
        {
            if (string.IsNullOrEmpty(nomeCena))
            {
                Debug.LogWarning("[BotaoCena] Nome da cena não definido.");
                return;
            }

            if (usarGerenciador && ZeroToHero.Core.Gerenciadores.GerenciadorCena.Instancia != null)
            {
                ZeroToHero.Core.Gerenciadores.GerenciadorCena.Instancia.CarregarCena(nomeCena);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nomeCena);
            }
        }

        /// <summary>
        /// Define o nome da cena em tempo de execução.
        /// </summary>
        public void DefinirCena(string nome)
        {
            nomeCena = nome;
        }
    }
}
