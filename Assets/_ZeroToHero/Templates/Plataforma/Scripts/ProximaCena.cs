using UnityEngine;
using ZeroToHero.Core.Gerenciadores;

namespace ZeroToHero.Cenas.Plataforma
{
    public class PortalTransicao : MonoBehaviour
    {
        [SerializeField] private string nomeDaProximaCena;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Jogador"))
            {
                GerenciadorCena.Instancia.CarregarCena(nomeDaProximaCena);
            }
        }
    }
}