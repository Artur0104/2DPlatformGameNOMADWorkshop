using UnityEngine;
using UnityEngine.UI;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.Gerenciadores;

namespace ZeroToHero.Cenas.Plataforma
{
    /// <summary>
    /// Controlador da tela de Game Over. Faz bind dos botões Menu e Reiniciar.
    /// Exibe o Panel quando o estado do jogo é GameOver.
    /// </summary>
    public class TelaGameOver : MonoBehaviour
    {
        [Header("Referências (opcionais, busca automática)")]
        [SerializeField, Tooltip("Botão 'Menu'.")]
        private Button btnMenu;

        [SerializeField, Tooltip("Botão 'Reiniciar'.")]
        private Button btnReiniciar;

        private void Awake()
        {
            if (btnMenu == null)
            {
                var t = transform.Find("Panel/Content/Btn_Menu");
                if (t != null) btnMenu = t.GetComponent<Button>();
            }
            if (btnReiniciar == null)
            {
                var t = transform.Find("Panel/Content/Btn_Reiniciar");
                if (t != null) btnReiniciar = t.GetComponent<Button>();
            }

            if (btnMenu != null)
                btnMenu.onClick.AddListener(IrParaMenu);

            if (btnReiniciar != null)
                btnReiniciar.onClick.AddListener(Reiniciar);
        }

        private void Update()
        {
            if (GerenciadorJogo.Instancia == null) return;

            bool deveMostrar = GerenciadorJogo.Instancia.EstadoAtual == EstadoJogo.GameOver;
            Transform painel = transform.Find("Panel");
            if (painel != null && painel.gameObject.activeSelf != deveMostrar)
                painel.gameObject.SetActive(deveMostrar);
        }

        public void IrParaMenu()
        {
            if (GerenciadorJogo.Instancia != null)
                GerenciadorJogo.Instancia.Retomar();

            if (GerenciadorCena.Instancia != null)
                GerenciadorCena.Instancia.CarregarCena("MenuPrincipal");
        }

        public void Reiniciar()
        {
            if (GerenciadorJogo.Instancia != null)
                GerenciadorJogo.Instancia.Retomar();

            if (GerenciadorCena.Instancia != null)
                GerenciadorCena.Instancia.RecarregarCena();
        }
    }
}
