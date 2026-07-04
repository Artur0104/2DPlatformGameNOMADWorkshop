using System.Collections;
using UnityEngine;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.Componentes;
using ZeroToHero.Core.SO;

namespace ZeroToHero.Cenas.Runner
{
    /// <summary>
    /// Dispara projéteis conforme o padrão da arma configurada.
    /// Suporta: Frente, Arco, Rajada, Teleguiado, Rapido.
    /// </summary>
    public class DisparadorArmasRunner : MonoBehaviour
    {
        [Header("Colisão")]
        [SerializeField, Tooltip("LayerMask dos alvos que o projétil atinge.")]
        private LayerMask camadaAlvo = -1;

        private void Start()
        {
            if (camadaAlvo == -1)
                camadaAlvo = LayerMask.GetMask("Inimigos");
        }
        /// <summary>
        /// Dispara projéteis conforme a configuração da arma.
        /// </summary>
        /// <param name="config">Configuração da arma.</param>
        /// <param name="origem">Posição de origem do disparo.</param>
        /// <param name="prefab">Prefab do projétil.</param>
        /// <param name="danoMultiplicador">Multiplicador de dano global.</param>
        public void Disparar(ConfiguracaoArmaRunner config, Vector2 origem, GameObject prefab, float danoMultiplicador)
        {
            if (config == null || prefab == null) return;

            switch (config.Padrao)
            {
                case PadraoArmaRunner.Frente:
                    DispararFrente(config, origem, prefab, danoMultiplicador);
                    break;
                case PadraoArmaRunner.Arco:
                    StartCoroutine(DispararArco(config, origem, prefab, danoMultiplicador));
                    break;
                case PadraoArmaRunner.Rajada:
                    StartCoroutine(DispararRajada(config, origem, prefab, danoMultiplicador));
                    break;
                case PadraoArmaRunner.Teleguiado:
                    DispararTeleguiado(config, origem, prefab, danoMultiplicador);
                    break;
                case PadraoArmaRunner.Rapido:
                    DispararFrente(config, origem, prefab, danoMultiplicador);
                    break;
            }
        }

        private void DispararFrente(ConfiguracaoArmaRunner config, Vector2 origem, GameObject prefab, float multiplicador)
        {
            CriarProjetil(prefab, origem, Vector2.up, config.VelocidadeProjetil,
                config.Dano * multiplicador, config.Temporaria ? config.Duracao : 5f, false);
        }

        private IEnumerator DispararArco(ConfiguracaoArmaRunner config, Vector2 origem, GameObject prefab, float multiplicador)
        {
            int count = config.ProjeteisPorDisparo;
            float anguloTotal = config.AnguloAbertura * (count - 1);
            float anguloInicial = -anguloTotal * 0.5f + 90f;

            for (int i = 0; i < count; i++)
            {
                float anguloAtual = anguloInicial + i * config.AnguloAbertura;
                float rad = anguloAtual * Mathf.Deg2Rad;
                Vector2 direcao = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                CriarProjetil(prefab, origem, direcao, config.VelocidadeProjetil,
                    config.Dano * multiplicador, 5f, false);

                if (count > 1 && i < count - 1)
                    yield return new WaitForSeconds(0.02f);
            }
        }

        private IEnumerator DispararRajada(ConfiguracaoArmaRunner config, Vector2 origem, GameObject prefab, float multiplicador)
        {
            for (int i = 0; i < config.ProjeteisPorDisparo; i++)
            {
                CriarProjetil(prefab, origem, Vector2.up, config.VelocidadeProjetil,
                    config.Dano * multiplicador, 5f, false);
                yield return new WaitForSeconds(config.IntervaloRajada);
            }
        }

        private void DispararTeleguiado(ConfiguracaoArmaRunner config, Vector2 origem, GameObject prefab, float multiplicador)
        {
            GameObject projetilGo = CriarProjetil(prefab, origem, Vector2.up, config.VelocidadeProjetil,
                config.Dano * multiplicador, 5f, true);

            if (projetilGo != null)
            {
                ComponenteProjetil compProjetil = projetilGo.GetComponent<ComponenteProjetil>();
                if (compProjetil != null)
                {
                    compProjetil.DefinirModo(ComponenteProjetil.ModoTrajetoria.Teleguiado);

                    GameObject inimigoMaisProximo = BuscarInimigoMaisProximo(origem);
                    if (inimigoMaisProximo != null)
                        compProjetil.DefinirAlvo(inimigoMaisProximo.transform);
                }
            }
        }

        private GameObject CriarProjetil(GameObject prefab, Vector2 origem, Vector2 direcao,
            float velocidade, float dano, float tempoVida, bool atravessaInimigos)
        {
            GameObject projetilGo = Object.Instantiate(prefab, origem, Quaternion.identity);
            if (projetilGo == null) return null;

            projetilGo.layer = LayerMask.NameToLayer("ProjetilJogador");

            ComponenteProjetil compProjetil = projetilGo.GetComponent<ComponenteProjetil>();
            if (compProjetil != null)
            {
                compProjetil.Disparar(direcao);
                compProjetil.InicializarValores(dano, velocidade, tempoVida, atravessaInimigos, camadaAlvo);
            }

            return projetilGo;
        }

        private GameObject BuscarInimigoMaisProximo(Vector2 posicao)
        {
            GameObject[] inimigos = GameObject.FindGameObjectsWithTag("Inimigo");
            GameObject maisProximo = null;
            float menorDistancia = Mathf.Infinity;

            for (int i = 0; i < inimigos.Length; i++)
            {
                float distancia = Vector2.Distance(posicao, inimigos[i].transform.position);
                if (distancia < menorDistancia)
                {
                    menorDistancia = distancia;
                    maisProximo = inimigos[i];
                }
            }

            return maisProximo;
        }
    }
}
