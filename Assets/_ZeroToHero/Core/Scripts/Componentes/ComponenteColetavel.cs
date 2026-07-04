using UnityEngine;
using UnityEngine.Events;
using ZeroToHero.Core.Interfaces;
using ZeroToHero.Core.Gerenciadores;

namespace ZeroToHero.Core.Componentes
{
    /// <summary>
    /// Componente de item coletável.
    /// Ao ser tocado pelo jogador, executa um efeito (moeda, vida, poder).
    /// Funciona com Trigger Collider 2D.
    /// </summary>
    public class ComponenteColetavel : MonoBehaviour, IColetavel
    {
        [Header("Configuração")]
        [SerializeField, Tooltip("Tipo do coletável.")]
        private TipoColetavel tipo = TipoColetavel.Moeda;

        [SerializeField, Tooltip("Valor do coletável. Moeda: quantidade. Vida: HP. Poder: índice da habilidade.")]
        private float valor = 1f;

        [SerializeField, Tooltip("Tag do objeto que pode coletar este item.")]
        private string tagColetor = "Jogador";

        [SerializeField, Tooltip("Se ativado, o item é destruído ao ser coletado.")]
        private bool destruirAoColetar = true;

        [Header("Efeitos Visuais")]
        [SerializeField, Tooltip("Prefab de efeito ao ser coletado (opcional).")]
        private GameObject efeitoColeta;

        [SerializeField, Tooltip("Som ao ser coletado (opcional).")]
        private AudioClip somColeta;

        [Header("Eventos")]
        [SerializeField, Tooltip("Disparado quando o item é coletado. Parâmetro: coletor.")]
        private UnityEvent<GameObject> onColetado;

        public TipoColetavel GetTipo() => tipo;

        /// <summary>
        /// Chamado quando o item é coletado.
        /// Aplica o efeito baseado no tipo (moeda, vida, poder).
        /// Conecte o evento OnColetado para adicionar comportamentos extras.
        /// </summary>
        /// <param name="coletor">GameObject que coletou o item.</param>
        public virtual void Coletar(GameObject coletor)
        {
            AplicarEfeito(coletor);
            onColetado?.Invoke(coletor);

            if (efeitoColeta != null)
            {
                Instantiate(efeitoColeta, transform.position, Quaternion.identity);
            }

            if (somColeta != null)
            {
                AudioSource.PlayClipAtPoint(somColeta, transform.position);
            }

            if (destruirAoColetar)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Aplica o efeito do item no coletor.
        /// Sobrescreva para adicionar comportamentos customizados.
        /// </summary>
        public virtual void AplicarEfeito(GameObject coletor)
        {
            switch (tipo)
            {
                case TipoColetavel.Moeda:
                    if (GerenciadorHUD.Instancia != null)
                    {
                        GerenciadorHUD.Instancia.AdicionarMoedas((int)valor);
                    }
                    break;

                case TipoColetavel.Vida:
                    ComponenteVida vida = coletor.GetComponent<ComponenteVida>();
                    if (vida != null)
                    {
                        vida.Curar(valor);
                    }
                    break;

                case TipoColetavel.Poder:
                    ComponenteHabilidade[] habilidades = coletor.GetComponents<ComponenteHabilidade>();
                    int indice = (int)valor;
                    if (habilidades != null && indice >= 0 && indice < habilidades.Length)
                    {
                        habilidades[indice].Desbloquear();
                    }
                    break;

                case TipoColetavel.Chave:
                    Debug.Log($"[ComponenteColetavel] Chave coletada por {coletor.name}. Implemente a lógica de chave no seu jogo.");
                    break;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(tagColetor))
            {
                Coletar(other.gameObject);
            }
        }
    }
}
