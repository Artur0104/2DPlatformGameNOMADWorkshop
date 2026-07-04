using UnityEngine;
using UnityEngine.Events;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.Gerenciadores;

namespace ZeroToHero.Cenas.Plataforma
{
    /// <summary>
    /// Portal de fim de fase: ao ser tocado pelo jogador, dispara a vitória.
    /// Coloque em um GameObject com Collider2D (IsTrigger = true).
    /// </summary>
    public class PortalFimFase : MonoBehaviour
    {
        [Header("Configuração")]
        [SerializeField, Tooltip("Tag do jogador que ativa o portal.")]
        private string tagJogador = "Jogador";

        [SerializeField, Tooltip("Efeito de partícula ao ativar o portal (opcional).")]
        private GameObject efeitoAtivacao;

        [Header("Eventos")]
        [SerializeField, Tooltip("Disparado quando o portal é ativado.")]
        private UnityEvent onAtivar;

        private bool _ativado = false;

        private void OnEnable()
        {
            _ativado = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_ativado) return;
            if (!other.CompareTag(tagJogador)) return;

            _ativado = true;
            onAtivar?.Invoke();

            if (efeitoAtivacao != null)
            {
                Instantiate(efeitoAtivacao, transform.position, Quaternion.identity);
            }

            if (GerenciadorJogo.Instancia != null)
            {
                GerenciadorJogo.Instancia.VitoriaJogo();
            }

            if (GerenciadorHUD.Instancia != null)
            {
                GerenciadorHUD.Instancia.MostrarMensagem("Fase Completa!");
                GerenciadorHUD.Instancia.MostrarTelaVitoria();
            }
        }
    }
}
