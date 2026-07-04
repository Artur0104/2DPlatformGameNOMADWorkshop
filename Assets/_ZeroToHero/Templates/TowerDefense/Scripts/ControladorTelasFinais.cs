using UnityEngine;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.Gerenciadores;

namespace ZeroToHero.Cenas.TowerDefense
{
    /// <summary>
    /// Controlador para os botões das telas finais (Vitória, Game Over, Pause)
    /// e para ativação/desativação do menu de pause.
    /// </summary>
    public class ControladorTelasFinais : MonoBehaviour
    {
        private bool _inscrito;

        private void Start()
        {
            InscreverEventosPause();
        }

        private void OnEnable()
        {
            InscreverEventosPause();
        }

        private GameObject _pauseGo;

        private void InscreverEventosPause()
        {
            if (_inscrito) return;
            if (GerenciadorJogo.Instancia == null) return;

            _pauseGo = BuscarNaCena("HUD_Pause");
            if (_pauseGo != null)
                _pauseGo.SetActive(false);

            GerenciadorJogo.Instancia.onJogoPausado.AddListener(MostrarPause);
            GerenciadorJogo.Instancia.onJogoRetomado.AddListener(EsconderPause);
            _inscrito = true;
        }

        private void MostrarPause()
        {
            if (_pauseGo != null)
                _pauseGo.SetActive(true);
        }

        private void EsconderPause()
        {
            if (_pauseGo != null)
                _pauseGo.SetActive(false);
        }

        /// <summary>
        /// Reinicia a fase atual, destruindo singletons persistentes
        /// para garantir estado limpo.
        /// </summary>
        public void Reiniciar()
        {
            Time.timeScale = 1f;
            DestruirSingleton(GerenciadorJogo.Instancia);
            DestruirSingleton(GerenciadorHUD.Instancia);
            DestruirSingleton(GerenciadorCena.Instancia);

            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// Volta para o menu principal, destruindo singletons persistentes.
        /// </summary>
        private const string CENA_MENU = "MenuPrincipal";

        public void IrParaMenu()
        {
            Time.timeScale = 1f;
            DestruirSingleton(GerenciadorJogo.Instancia);
            DestruirSingleton(GerenciadorHUD.Instancia);
            DestruirSingleton(GerenciadorCena.Instancia);

            UnityEngine.SceneManagement.SceneManager.LoadScene(CENA_MENU);
        }

        /// <summary>
        /// Retoma o jogo da pausa.
        /// </summary>
        public void Retomar()
        {
            if (GerenciadorJogo.Instancia != null)
                GerenciadorJogo.Instancia.Retomar();
        }

        private void DestruirSingleton(MonoBehaviour singleton)
        {
            if (singleton != null)
                Destroy(singleton.gameObject);
        }

        private GameObject BuscarNaCena(string nome)
        {
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == nome)
                    return roots[i];
            }
            return null;
        }

    }
}
