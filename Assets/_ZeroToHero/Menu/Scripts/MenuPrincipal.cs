using UnityEngine;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.Gerenciadores;

namespace ZeroToHero.Menu
{
    public class MenuPrincipal : MonoBehaviour
    {
        [Header("Cenas")]
        [SerializeField, Tooltip("Nome da cena de Plataforma.")]
        private string cenaPlataforma = "Plataforma";

        [SerializeField, Tooltip("Nome da cena de Tower Defense.")]
        private string cenaTowerDefense = "TowerDefense";

        [SerializeField, Tooltip("Nome da cena de Infinite Runner.")]
        private string cenaInfiniteRunner = "InfiniteRunner";

        private void Start()
        {
            if (InputReader.Instancia != null)
                InputReader.Instancia.HabilitarUI();
            if (GerenciadorJogo.Instancia != null)
                GerenciadorJogo.Instancia.MudarEstado(EstadoJogo.Menu);
        }

        public void AbrirPlataforma() { CarregarCena(cenaPlataforma); }
        public void AbrirTowerDefense() { CarregarCena(cenaTowerDefense); }
        public void AbrirInfiniteRunner() { CarregarCena(cenaInfiniteRunner); }

        public void SairDoJogo()
        {
            if (GerenciadorCena.Instancia != null) GerenciadorCena.Instancia.SairDoJogo();
            else Application.Quit();
        }

        private void CarregarCena(string nomeCena)
        {
            if (GerenciadorCena.Instancia != null) GerenciadorCena.Instancia.CarregarCena(nomeCena);
            else UnityEngine.SceneManagement.SceneManager.LoadScene(nomeCena);
        }
    }
}
