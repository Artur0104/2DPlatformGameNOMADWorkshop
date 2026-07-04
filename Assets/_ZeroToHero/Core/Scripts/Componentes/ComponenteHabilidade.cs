using UnityEngine;
using UnityEngine.Events;
using ZeroToHero.Core.SO;

namespace ZeroToHero.Core.Componentes
{
    /// <summary>
    /// Componente de habilidade especial.
    /// Gerencia cooldown, custo e desbloqueio.
    /// Pode ser usada para qualquer tipo de habilidade (dash, magia, ultimate, etc.).
    /// </summary>
    public class ComponenteHabilidade : MonoBehaviour
    {
        [Header("Configuração (via SO ou manual)")]
        [SerializeField, Tooltip("ScriptableObject com a configuração da habilidade. Se definido, sobrescreve os campos abaixo.")]
        private ConfiguracaoHabilidade config;

        [SerializeField, Tooltip("Nome da habilidade (exibido na HUD).")]
        private string nomeHabilidade = "Habilidade";

        [SerializeField, Tooltip("Ícone da habilidade (exibido na HUD).")]
        private Sprite icone;

        [SerializeField, Tooltip("Tempo de recarga em segundos.")]
        private float cooldown = 5f;

        [SerializeField, Tooltip("Custo em mana/vida/energia. 0 = sem custo.")]
        private float custo = 0f;

        [SerializeField, Tooltip("Se a habilidade começa desbloqueada.")]
        private bool desbloqueadaInicial = true;

        [Header("Eventos")]
        [SerializeField, Tooltip("Disparado quando a habilidade é usada.")]
        private UnityEvent onUsar;

        [SerializeField, Tooltip("Disparado quando a habilidade termina o cooldown e fica pronta.")]
        private UnityEvent onPronta;

        private float _timerCooldown = 0f;
        private bool _desbloqueada;

        public string Nome => nomeHabilidade;
        public Sprite Icone => icone;
        public bool EstaDesbloqueada => _desbloqueada;
        public bool EstaPronta => _desbloqueada && _timerCooldown <= 0f;
        public float CooldownRestante => _timerCooldown;
        public float CooldownTotal => cooldown;

        private void Awake()
        {
            if (config != null)
            {
                AplicarConfiguracao(config);
            }
            else
            {
                _desbloqueada = desbloqueadaInicial;
            }
        }

        private void Update()
        {
            if (_timerCooldown > 0f)
            {
                _timerCooldown -= Time.deltaTime;

                if (_timerCooldown <= 0f)
                {
                    _timerCooldown = 0f;
                    onPronta?.Invoke();
                }
            }
        }

        /// <summary>
        /// Usa a habilidade. Só funciona se estiver desbloqueada e fora de cooldown.
        /// O método pode ser sobrescrito em classes filhas para adicionar
        /// comportamentos específicos da habilidade.
        /// </summary>
        public virtual void Usar()
        {
            if (!EstaPronta) return;

            _timerCooldown = cooldown;
            onUsar?.Invoke();
        }

        /// <summary>
        /// Reinicia o cooldown da habilidade (deixa pronta para uso).
        /// </summary>
        public void Recarregar()
        {
            _timerCooldown = 0f;
            onPronta?.Invoke();
        }

        /// <summary>
        /// Desbloqueia a habilidade permanentemente.
        /// </summary>
        public void Desbloquear()
        {
            _desbloqueada = true;
        }

        /// <summary>
        /// Bloqueia a habilidade (impede uso).
        /// </summary>
        public void Bloquear()
        {
            _desbloqueada = false;
        }

        /// <summary>
        /// Define o cooldown em tempo de execução.
        /// </summary>
        public void DefinirCooldown(float novoCooldown)
        {
            cooldown = novoCooldown;
        }

        /// <summary>
        /// Aplica as configurações de um ScriptableObject ConfiguracaoHabilidade.
        /// </summary>
        public void AplicarConfiguracao(ConfiguracaoHabilidade novaConfig)
        {
            if (novaConfig == null) return;

            config = novaConfig;
            nomeHabilidade = novaConfig.nome;
            icone = novaConfig.icone;
            cooldown = novaConfig.cooldown;
            custo = novaConfig.custo;
            _desbloqueada = novaConfig.desbloqueadaInicial;
        }
    }
}
