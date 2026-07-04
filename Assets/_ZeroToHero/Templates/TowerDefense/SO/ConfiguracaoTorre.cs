using UnityEngine;

namespace ZeroToHero.Core.SO
{
    /// <summary>Tipo de torre para Tower Defense.</summary>
    public enum TipoTorre
    {
        Fisico,
        Fogo,
        Gelo,
        Veneno,
        Eletrico,
        Explosivo
    }

    /// <summary>
    /// ScriptableObject de configuração de torre.
    /// Define atributos, progressão por nível, tipo e efeitos de status.
    /// </summary>
    [CreateAssetMenu(fileName = "ConfiguracaoTorre", menuName = "ZeroToHero/Configuração de Torre", order = 6)]
    public class ConfiguracaoTorre : ScriptableObject
    {
        [Header("Identificação")]
        [SerializeField, Tooltip("Nome da torre.")]
        public string nomeTorre = "Torre";

        [SerializeField, Tooltip("Ícone exibido na UI de seleção.")]
        public Sprite icone;

        [SerializeField, Tooltip("Sprite usado ao construir a torre.")]
        public Sprite spriteTorre;

        [SerializeField, Tooltip("Descrição da torre.")]
        [TextArea] public string descricao;

        [Header("Atributos")]
        [SerializeField, Tooltip("Custo em moedas para construir.")]
        public int custo = 50;

        [SerializeField, Tooltip("Custo em moedas para melhorar.")]
        public int custoMelhoria = 75;

        [SerializeField, Tooltip("Dano por ataque.")]
        public float dano = 15f;

        [SerializeField, Tooltip("Alcance de ataque em unidades.")]
        public float alcance = 3f;

        [SerializeField, Tooltip("Ataques por segundo.")]
        public float velocidadeAtaque = 1f;

        [SerializeField, Tooltip("Nível máximo permitido.")]
        public int nivelMaximo = 3;

        [Header("Progressão por Nível")]
        [SerializeField, Tooltip("Multiplicador de dano a cada nível.")]
        public float multiplicadorDanoPorNivel = 1.5f;

        [SerializeField, Tooltip("Multiplicador de alcance a cada nível.")]
        public float multiplicadorAlcancePorNivel = 1.1f;

        [SerializeField, Tooltip("Multiplicador de velocidade a cada nível.")]
        public float multiplicadorVelocidadePorNivel = 1.2f;

        [Header("Tipo de Torre")]
        [SerializeField, Tooltip("Tipo da torre (afeta efeitos aplicados).")]
        public TipoTorre tipo = TipoTorre.Fisico;

        [SerializeField, Tooltip("Duração do efeito de status em segundos.")]
        public float duracaoStatus = 3f;

        [SerializeField, Tooltip("Intensidade do status (0 a 1).")]
        public float intensidadeStatus = 0.5f;

        [SerializeField, Tooltip("Raio de explosão (> 0 ativa dano em área).")]
        public float raioExplosao = 0f;

        [Header("Projétil")]
        [SerializeField, Tooltip("Prefab do projétil disparado.")]
        public GameObject projetilPrefab;

        [SerializeField, Tooltip("Cor do projétil (usada no LineRenderer).")]
        public Color corProjetil = Color.white;
    }
}
