using UnityEngine;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.Componentes;
using ZeroToHero.Core.Gerenciadores;

namespace ZeroToHero.Cenas.TowerDefense
{
    /// <summary>
    /// Base do jogador no Tower Defense.
    /// Possui vida e, se destruída, causa Game Over.
    /// </summary>
    public class Base : EntidadeBase
    {
        [Header("Atributos da Base")]
        [SerializeField, Tooltip("Vida máxima da base.")]
        private float vidaMaximaBase = 100f;

        [SerializeField, Tooltip("Moedas iniciais do jogador.")]
        private int moedasIniciais = 200;

        protected override void AoIniciar()
        {
            base.AoIniciar();

            if (vida == null)
            {
                vida = gameObject.AddComponent<ComponenteVida>();
            }

            vida.DefinirVidaMaxima(vidaMaximaBase);
            vida.onMorte.AddListener(AoDestruirBase);

            if (GerenciadorHUD.Instancia != null)
            {
                GerenciadorHUD.Instancia.AtualizarVida(vida.VidaAtual, vida.VidaMaxima);
                GerenciadorHUD.Instancia.AdicionarMoedas(moedasIniciais);
            }
        }

        /// <summary>
        /// Aplica dano à base. Chamado por inimigos que chegam ao fim do caminho.
        /// </summary>
        public void ReceberDanoBase(float dano)
        {
            if (vida != null)
            {
                vida.ReceberDano(dano);

                if (GerenciadorHUD.Instancia != null)
                {
                    GerenciadorHUD.Instancia.AtualizarVida(vida.VidaAtual, vida.VidaMaxima);
                }
            }
        }

        /// <summary>
        /// Chamado quando a base é destruída.
        /// </summary>
        private void AoDestruirBase()
        {
            if (efeito != null)
            {
                efeito.EmitirParticula(transform.position);
                efeito.VibrarCamera();
            }

            if (GerenciadorJogo.Instancia != null)
            {
                GerenciadorJogo.Instancia.FinalizarJogo();
            }
        }
    }
}
