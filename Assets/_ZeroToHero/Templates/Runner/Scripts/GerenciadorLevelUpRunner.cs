using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.SO;

namespace ZeroToHero.Cenas.Runner
{
    /// <summary>
    /// Gerenciador de Level Up do Infinite Runner (Singleton).
    /// Gerencia XP, nível e popup de 3 opções ao subir de nível.
    /// Pausa o jogo (Time.timeScale = 0) ao abrir o popup.
    /// </summary>
    public class GerenciadorLevelUpRunner : MonoBehaviour
    {
        [Header("XP e Nível")]
        [SerializeField, Tooltip("XP atual do jogador.")]
        private int xpAtual = 0;

        [SerializeField, Tooltip("XP necessária para o próximo nível.")]
        private int xpParaProximoNivel = 20;

        [SerializeField, Tooltip("Multiplicador de XP necessária a cada nível.")]
        private float multiplicadorXpPorNivel = 1.5f;

        [SerializeField, Tooltip("Nível atual do jogador.")]
        private int nivel = 1;

        [Header("Pool de Power-Ups")]
        [SerializeField, Tooltip("Power-ups disponíveis para sorteio no level up.")]
        private ConfiguracaoPowerUp[] poolPowerUps;

        [Header("UI — Popup Level Up")]
        [SerializeField, Tooltip("GameObject raiz do painel de level up.")]
        private GameObject painelLevelUp;

        [SerializeField, Tooltip("Botões de escolha (3).")]
        private Button[] botoesEscolha;

        [SerializeField, Tooltip("Textos das escolhas (3).")]
        private TMP_Text[] textosEscolha;

        [SerializeField, Tooltip("Ícones das escolhas (3).")]
        private Image[] iconesEscolha;

        [Header("UI — HUD")]
        [SerializeField, Tooltip("Slider da barra de XP.")]
        private Slider barraXp;

        [SerializeField, Tooltip("Texto do nível atual.")]
        private TMP_Text textoNivel;

        [SerializeField, Tooltip("Texto de XP (ex: 15/20).")]
        private TMP_Text textoXp;

        private ConfiguracaoPowerUp[] _escolhasAtuais;

        public static GerenciadorLevelUpRunner Instancia { get; private set; }

        public int Nivel
        {
            get { return nivel; }
        }

        public int XpAtual
        {
            get { return xpAtual; }
        }

        public int XpParaProximoNivel
        {
            get { return xpParaProximoNivel; }
            set { xpParaProximoNivel = value; }
        }

        public float MultiplicadorXpPorNivel
        {
            get { return multiplicadorXpPorNivel; }
            set { multiplicadorXpPorNivel = value; }
        }

        private void Awake()
        {
            if (Instancia != null)
            {
                Destroy(gameObject);
                return;
            }

            Instancia = this;
        }

        private void Start()
        {
            if (painelLevelUp != null)
                painelLevelUp.SetActive(false);

            AtualizarHUD();
        }

        /// <summary>
        /// Adiciona XP ao jogador e verifica se sobe de nível.
        /// </summary>
        /// <param name="quantidade">Quantidade de XP a adicionar.</param>
        public void AdicionarXP(int quantidade)
        {
            if (quantidade <= 0) return;

            xpAtual += quantidade;

            AtualizarHUD();
            VerificarLevelUp();
        }

        private void VerificarLevelUp()
        {
            if (xpAtual < xpParaProximoNivel) return;

            AbrirPopup();
        }

        private void AbrirPopup()
        {
            if (painelLevelUp == null) return;

            Time.timeScale = 0f;
            painelLevelUp.SetActive(true);

            _escolhasAtuais = SortearPowerUps();

            for (int i = 0; i < botoesEscolha.Length && i < 3; i++)
            {
                if (botoesEscolha[i] == null) continue;

                int indice = i;
                botoesEscolha[i].onClick.RemoveAllListeners();

                if (i < _escolhasAtuais.Length && _escolhasAtuais[i] != null)
                {
                    ConfiguracaoPowerUp powerUp = _escolhasAtuais[i];

                    if (textosEscolha != null && i < textosEscolha.Length && textosEscolha[i] != null)
                    {
                        string texto = powerUp.NomePowerUp;
                        if (!string.IsNullOrEmpty(powerUp.Descricao))
                            texto += "\n" + powerUp.Descricao;
                        textosEscolha[i].text = texto;
                    }

                    if (iconesEscolha != null && i < iconesEscolha.Length && iconesEscolha[i] != null)
                    {
                        iconesEscolha[i].sprite = powerUp.Icone;
                    }

                    botoesEscolha[i].gameObject.SetActive(true);
                    botoesEscolha[i].onClick.AddListener(delegate { EscolherPowerUp(indice); });
                }
                else
                {
                    botoesEscolha[i].gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Aplica o power-up escolhido, fecha o popup e retoma o jogo.
        /// </summary>
        /// <param name="indice">Índice da opção escolhida (0 a 2).</param>
        public void EscolherPowerUp(int indice)
        {
            if (_escolhasAtuais == null || indice >= _escolhasAtuais.Length) return;

            ConfiguracaoPowerUp powerUp = _escolhasAtuais[indice];
            if (powerUp == null) return;

            AplicarPowerUp(powerUp);

            xpAtual -= xpParaProximoNivel;
            xpParaProximoNivel = Mathf.RoundToInt(xpParaProximoNivel * multiplicadorXpPorNivel);
            nivel++;

            if (painelLevelUp != null)
                painelLevelUp.SetActive(false);

            Time.timeScale = 1f;

            AtualizarHUD();

            if (xpAtual >= xpParaProximoNivel)
                VerificarLevelUp();
        }

        private void AplicarPowerUp(ConfiguracaoPowerUp powerUp)
        {
            if (powerUp == null) return;

            NaveJogadorRunner jogador = FindFirstObjectByType<NaveJogadorRunner>();
            switch (powerUp.Tipo)
            {
                case TipoPowerUp.ArmaTemporaria:
                {
                    ControladorArmasRunner controlador =
                        FindFirstObjectByType<ControladorArmasRunner>();
                    if (controlador != null && powerUp.Arma != null)
                        controlador.EquiparArmaTemporaria(powerUp.Arma);
                    break;
                }

                case TipoPowerUp.Invulnerabilidade:
                {
                    if (jogador != null)
                    {
                        jogador.InvencivelTemporario = true;
                        StartCoroutine(DesativarInvencibilidade(jogador, powerUp.Duracao));
                    }
                    break;
                }

                case TipoPowerUp.BoostStatus:
                {
                    if (jogador == null) break;

                    float duracao = powerUp.Duracao;
                    bool temporario = duracao > 0f;

                    switch (powerUp.TipoBoost)
                    {
                        case TipoBoostStatus.Vida:
                            jogador.Curar(Mathf.RoundToInt(powerUp.ValorBoost));
                            break;

                        case TipoBoostStatus.Velocidade:
                            if (temporario)
                            {
                                float original = jogador.VelocidadeAtual;
                                jogador.VelocidadeAtual *= powerUp.ValorBoost;
                                StartCoroutine(ReverterBoost(() => jogador.VelocidadeAtual = original, duracao));
                            }
                            else
                                jogador.VelocidadeAtual *= powerUp.ValorBoost;
                            break;

                        case TipoBoostStatus.Dano:
                            if (temporario)
                            {
                                float original = jogador.DanoAtual;
                                jogador.DanoAtual += powerUp.ValorBoost;
                                StartCoroutine(ReverterBoost(() => jogador.DanoAtual = original, duracao));
                            }
                            else
                                jogador.DanoAtual += powerUp.ValorBoost;
                            break;

                        case TipoBoostStatus.FrequenciaTiro:
                            if (temporario)
                            {
                                float anterior = jogador.FrequenciaTiroBoost;
                                jogador.FrequenciaTiroBoost = powerUp.ValorBoost;
                                StartCoroutine(ReverterBoost(() => jogador.FrequenciaTiroBoost = anterior, duracao));
                            }
                            else
                                jogador.FrequenciaTiroBoost = powerUp.ValorBoost;
                            break;
                    }
                    break;
                }
            }

            AtualizarOverlayAjuste();
        }

        private void AtualizarOverlayAjuste()
        {
            NaveJogadorRunner jogador = FindFirstObjectByType<NaveJogadorRunner>();
            if (jogador == null) return;

            PainelAjusteRunner painel = FindFirstObjectByType<PainelAjusteRunner>();
            if (painel == null) return;

            painel.AtualizarValorSlider("S_Velocidade", jogador.VelocidadeAtual);
            painel.AtualizarValorSlider("S_DanoBase", jogador.DanoAtual);
        }

        private System.Collections.IEnumerator DesativarInvencibilidade(NaveJogadorRunner jogador, float duracao)
        {
            yield return new WaitForSeconds(duracao);
            if (jogador != null)
                jogador.InvencivelTemporario = false;
        }

        private System.Collections.IEnumerator ReverterBoost(System.Action reverter, float duracao)
        {
            yield return new WaitForSeconds(duracao);
            if (reverter != null)
                reverter();
        }

        private ConfiguracaoPowerUp[] SortearPowerUps()
        {
            if (poolPowerUps == null || poolPowerUps.Length == 0)
                return new ConfiguracaoPowerUp[0];

            System.Collections.Generic.List<ConfiguracaoPowerUp> disponiveis =
                new System.Collections.Generic.List<ConfiguracaoPowerUp>(poolPowerUps);

            int quantidade = Mathf.Min(3, disponiveis.Count);
            ConfiguracaoPowerUp[] sorteados = new ConfiguracaoPowerUp[quantidade];

            for (int i = 0; i < quantidade; i++)
            {
                int indiceAleatorio = Random.Range(0, disponiveis.Count);
                sorteados[i] = disponiveis[indiceAleatorio];
                disponiveis.RemoveAt(indiceAleatorio);
            }

            return sorteados;
        }

        private void AtualizarHUD()
        {
            if (barraXp != null && xpParaProximoNivel > 0)
            {
                barraXp.minValue = 0;
                barraXp.maxValue = xpParaProximoNivel;
                barraXp.value = xpAtual;
            }

            if (textoNivel != null)
                textoNivel.text = "Nível " + nivel.ToString();

            if (textoXp != null)
                textoXp.text = xpAtual.ToString() + " / " + xpParaProximoNivel.ToString();
        }
    }
}
