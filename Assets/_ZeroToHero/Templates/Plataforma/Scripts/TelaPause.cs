using UnityEngine;
using UnityEngine.UI;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.Ferramentas;
using ZeroToHero.Core.Gerenciadores;

namespace ZeroToHero.Cenas.Plataforma
{
    /// <summary>
    /// Controlador da tela de pausa. Conecta-se aos eventos do GerenciadorJogo
    /// e faz bind dos botões Retomar, Reiniciar e Menu.
    /// </summary>
    public class TelaPause : MonoBehaviour
    {
        [Header("Referências (opcionais, busca automática)")]
        [SerializeField, Tooltip("Botão 'Retomar'.")]
        private Button btnRetomar;

        [SerializeField, Tooltip("Botão 'Reiniciar'.")]
        private Button btnReiniciar;

        [SerializeField, Tooltip("Botão 'Menu'.")]
        private Button btnMenu;

        private void Awake()
        {
            if (btnRetomar == null)
            {
                var t = transform.Find("Panel/Content/Btn_Retomar");
                if (t != null) btnRetomar = t.GetComponent<Button>();
            }
            if (btnReiniciar == null)
            {
                var t = transform.Find("Panel/Content/Btn_Reiniciar");
                if (t != null) btnReiniciar = t.GetComponent<Button>();
            }
            if (btnMenu == null)
            {
                var t = transform.Find("Panel/Content/Btn_Menu");
                if (t != null) btnMenu = t.GetComponent<Button>();
            }

            if (btnRetomar != null)
                btnRetomar.onClick.AddListener(Retomar);

            if (btnReiniciar != null)
                btnReiniciar.onClick.AddListener(Reiniciar);

            if (btnMenu != null)
                btnMenu.onClick.AddListener(IrParaMenu);
        }

        private void Update()
        {
            if (GerenciadorJogo.Instancia == null) return;

            bool deveMostrar = GerenciadorJogo.Instancia.EstadoAtual == EstadoJogo.Pausado
                && !PainelAjusteBase.PausadoPeloOverlay;

            Transform painel = transform.Find("Panel");
            if (painel != null && painel.gameObject.activeSelf != deveMostrar)
                painel.gameObject.SetActive(deveMostrar);
        }

        public void Retomar()
        {
            if (GerenciadorJogo.Instancia != null)
                GerenciadorJogo.Instancia.Retomar();
        }

        public void Reiniciar()
        {
            if (GerenciadorCena.Instancia != null)
                GerenciadorCena.Instancia.RecarregarCena();
        }

        public void IrParaMenu()
        {
            if (GerenciadorJogo.Instancia != null)
                GerenciadorJogo.Instancia.Retomar();

            if (GerenciadorCena.Instancia != null)
                GerenciadorCena.Instancia.CarregarCena("MenuPrincipal");
        }
    }
}
