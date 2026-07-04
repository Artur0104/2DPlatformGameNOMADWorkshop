using UnityEngine;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.Componentes;
using ZeroToHero.Core.Interfaces;

namespace ZeroToHero.Cenas.Plataforma
{
    /// <summary>
    /// Zona que causa dano a entidades que ficam dentro dela (ex: espinhos, lava).
    /// Respeita a invencibilidade da entidade (ComponenteVida).
    /// </summary>
    public class ZonaDano : MonoBehaviour
    {
        [Header("Configuração de Dano")]
        [SerializeField, Tooltip("Dano por toque (aplicado uma vez a cada intervalo).")]
        private float danoPorToque = 1f;

        [SerializeField, Tooltip("Força de empurrão aplicada ao receber dano.")]
        private float forcaEmpurrao = 5f;

        [SerializeField, Tooltip("Intervalo mínimo entre aplicações de dano (segundos).")]
        private float intervaloSegundos = 0.6f;

        [Header("Alvos")]
        [SerializeField, Tooltip("Tag das entidades que recebem dano.")]
        private string tagAlvo = "Jogador";

        private float _timer = 0f;

        private void Update()
        {
            if (_timer > 0f)
                _timer -= Time.deltaTime;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.CompareTag(tagAlvo)) return;
            if (_timer > 0f) return;

            IDanificavel danificavel = other.GetComponent<IDanificavel>();
            if (danificavel == null) return;

            // Respeita invencibilidade via ComponenteVida
            ComponenteVida vida = other.GetComponent<ComponenteVida>();
            if (vida != null && (vida.Invencivel || !vida.EstaViva))
            {
                _timer = 0.15f;
                return;
            }

            danificavel.ReceberDano(danoPorToque);
            _timer = intervaloSegundos;

            if (forcaEmpurrao > 0f)
            {
                Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 direcao = (other.transform.position - transform.position).normalized;
                    rb.AddForce(direcao * forcaEmpurrao, ForceMode2D.Impulse);
                }
            }
        }
    }
}
