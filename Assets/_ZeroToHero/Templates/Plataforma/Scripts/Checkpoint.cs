using UnityEngine;
using UnityEngine.Events;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.Componentes;
using ZeroToHero.Core.Gerenciadores;

namespace ZeroToHero.Cenas.Plataforma
{
    /// <summary>
    /// Ponto de checkpoint. Ao ser tocado pelo jogador, salva a posição de respawn.
    /// Conecte o evento OnCheckpoint para tocar som, animação, etc.
    /// </summary>
    public class Checkpoint : MonoBehaviour
    {
        [Header("Configuração")]
        [SerializeField, Tooltip("Tag do jogador.")]
        private string tagJogador = "Jogador";

        [SerializeField, Tooltip("Se ativado, o checkpoint só pode ser usado uma vez.")]
        private bool usarUmaVez = false;

        [SerializeField, Tooltip("Se ativado, cura o jogador ao ativar o checkpoint.")]
        private bool curarJogador = true;

        [Header("Visual")]
        [SerializeField, Tooltip("Cor quando o checkpoint está ativo.")]
        private Color corAtivo = Color.green;

        [SerializeField, Tooltip("Cor quando o checkpoint está desativado.")]
        private Color corDesativado = Color.gray;

        [Header("Eventos")]
        [SerializeField, Tooltip("Disparado quando o checkpoint é ativado.")]
        private UnityEvent onCheckpoint;

        private bool _foiUsado = false;
        private SpriteRenderer _sr;

        private void Start()
        {
            _sr = GetComponent<SpriteRenderer>();
            AtualizarCor();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_foiUsado && usarUmaVez) return;

            if (!other.CompareTag(tagJogador)) return;

            JogadorPlataforma jogador = other.GetComponent<JogadorPlataforma>();
            if (jogador != null)
            {
                jogador.SalvarCheckpoint(transform.position);
            }

            if (curarJogador)
            {
                ComponenteVida vida = other.GetComponent<ComponenteVida>();
                if (vida != null)
                {
                    vida.Curar(vida.VidaMaxima);
                }
            }

            _foiUsado = true;
            AtualizarCor();
            onCheckpoint?.Invoke();

            if (GerenciadorHUD.Instancia != null)
            {
                GerenciadorHUD.Instancia.MostrarMensagem("Checkpoint!");
            }
        }

        private void AtualizarCor()
        {
            if (_sr != null)
            {
                _sr.color = _foiUsado ? corDesativado : corAtivo;
            }
        }
    }
}
