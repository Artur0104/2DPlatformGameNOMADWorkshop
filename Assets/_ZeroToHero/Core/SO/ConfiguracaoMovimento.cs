using UnityEngine;

namespace ZeroToHero.Core.SO
{
    /// <summary>
    /// ScriptableObject de configuração de movimento.
    /// Altere os valores no Inspector para ajustar o comportamento sem código.
    /// </summary>
    [CreateAssetMenu(fileName = "ConfiguracaoMovimento", menuName = "ZeroToHero/Configuração de Movimento", order = 2)]
    public class ConfiguracaoMovimento : ScriptableObject
    {
        [Header("Movimento Horizontal")]
        [Tooltip("Velocidade de movimento horizontal (unidades por segundo).")]
        public float velocidadeAndar = 5f;

        [Header("Pulo")]
        [Tooltip("Força do pulo (impulso vertical).")]
        public float forcaPulo = 10f;

        [Tooltip("Número máximo de pulos consecutivos. 1 = pulo simples, 2 = double jump.")]
        public int pulosMaximos = 1;

        [Tooltip("Tempo extra (s) que o jogador pode pular após sair de uma plataforma (Coyote Time).")]
        public float tempoCoyote = 0.1f;

        [Tooltip("Tempo (s) em que um pulo pressionado antes de tocar o chão é armazenado (Jump Buffer).")]
        public float bufferPulo = 0.1f;

        [Header("Gravidade")]
        [Tooltip("Gravidade customizada. 0 = usa a gravidade padrão do Rigidbody2D.")]
        public float gravidade = 0f;

        [Header("Dash")]
        [Tooltip("Velocidade do dash.")]
        public float velocidadeDash = 15f;

        [Tooltip("Duração do dash em segundos.")]
        public float tempoDash = 0.2f;

        [Tooltip("Tempo de espera entre dashes consecutivos (segundos).")]
        public float cooldownDash = 0.5f;
    }
}
