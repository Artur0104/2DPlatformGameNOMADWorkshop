using UnityEngine;
using ZeroToHero.Core.Base;
using ZeroToHero.Core.Componentes;
using ZeroToHero.Core.Gerenciadores;
using ZeroToHero.Core.SO;

namespace ZeroToHero.Cenas.TowerDefense
{
    /// <summary>
    /// Grade de posicionamento para Tower Defense.
    /// Renderiza células, gerencia preview fantasma, restrições de construção,
    /// limite de torres e integração com SeletorTorre/ConfiguracaoTorre.
    /// </summary>
    public class GradePosicionamento : MonoBehaviour
    {
        [Header("Grade")]
        [SerializeField, Tooltip("Número de colunas da grade.")]
        private int colunas = 10;

        [SerializeField, Tooltip("Número de linhas da grade.")]
        private int linhas = 6;

        [SerializeField, Tooltip("Tamanho de cada célula.")]
        private float tamanhoCelula = 1f;

        [SerializeField, Tooltip("Prefab visual da célula (quadrado com sprite).")]
        private GameObject celulaPrefab;

        [Header("Torre")]
        [SerializeField, Tooltip("Prefab da torre a ser instanciada (template base).")]
        private GameObject torrePrefab;

        [SerializeField, Tooltip("LayerMask do que é considerado 'chão válido'.")]
        private LayerMask camadaChao;

        [SerializeField, Tooltip("LayerMask de áreas onde NÃO se pode construir (ex: caminho).")]
        private LayerMask camadaNaoConstruir = -1;

        [Header("Limites")]
        [SerializeField, Tooltip("Limite máximo de torres na grade (0 = sem limite).")]
        private int limiteMaximoTorres = 0;

        [Header("Cores")]
        [SerializeField, Tooltip("Cor da célula livre.")]
        private Color corLivre = new Color(0f, 1f, 0f, 0.3f);

        [SerializeField, Tooltip("Cor da célula ocupada.")]
        private Color corOcupada = new Color(1f, 0f, 0f, 0.3f);

        [SerializeField, Tooltip("Cor do fantasma em célula válida.")]
        private Color corPreviewValida = new Color(0f, 1f, 0f, 0.5f);

        [SerializeField, Tooltip("Cor do fantasma em célula inválida.")]
        private Color corPreviewInvalida = new Color(1f, 0f, 0f, 0.5f);

        private bool[,] _celulasOcupadas;
        private GameObject[,] _celulasVisuais;
        private Vector2 _origemGrade;
        private int _torresConstruidas;

        private GameObject _fantasmaTorre;
        private SpriteRenderer _srFantasma;
        private GameObject _alcanceFantasma;
        private SpriteRenderer _srAlcance;
        private Camera _camera;

        /// <summary>
        /// Total de torres construídas na grade.
        /// </summary>
        public int TorresConstruidas
        {
            get { return _torresConstruidas; }
        }

        private void Start()
        {
            _camera = Camera.main;

            _celulasOcupadas = new bool[colunas, linhas];
            _celulasVisuais = new GameObject[colunas, linhas];

            float larguraTotal = colunas * tamanhoCelula;
            float alturaTotal = linhas * tamanhoCelula;
            _origemGrade = (Vector2)transform.position - new Vector2(larguraTotal / 2f, alturaTotal / 2f) + new Vector2(tamanhoCelula / 2f, tamanhoCelula / 2f);

            CriarGradeVisual();
            InicializarFantasma();

            Torre.OnTorreDestruida += delegate { if (_torresConstruidas > 0) _torresConstruidas--; };
        }

        private void Update()
        {
            if (GerenciadorJogo.Instancia != null && GerenciadorJogo.Instancia.EstadoAtual != EstadoJogo.Jogando) return;

            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;

            ConfiguracaoTorre config = ObterConfigTorre();

            if (config != null)
            {
                AtualizarFantasma(config);

                if (Input.GetMouseButtonDown(0))
                {
                    if (ClicouEmTorreExistente())
                    {
                        CancelarSelecao();
                    }
                    else
                    {
                        TentarConstruirTorre();
                    }
                }

                if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
                {
                    CancelarSelecao();
                }
            }
            else
            {
                EsconderFantasma();

                if (Input.GetMouseButtonDown(0))
                {
                    VerificarCliqueTorre();
                }
            }
        }

        private void InicializarFantasma()
        {
            _fantasmaTorre = GameObject.Find("Fantasma_Torre");

            if (_fantasmaTorre == null)
            {
                _fantasmaTorre = new GameObject("Fantasma_Torre");
                _fantasmaTorre.SetActive(false);
                _srFantasma = _fantasmaTorre.AddComponent<SpriteRenderer>();
                _srFantasma.sortingOrder = 50;

                _alcanceFantasma = new GameObject("Alcance");
                _alcanceFantasma.transform.SetParent(_fantasmaTorre.transform, false);
                _alcanceFantasma.transform.localPosition = Vector3.zero;
                _srAlcance = _alcanceFantasma.AddComponent<SpriteRenderer>();
                _srAlcance.sortingOrder = 49;
                _srAlcance.sprite = CriarSpriteCirculo();
            }
            else
            {
                _srFantasma = _fantasmaTorre.GetComponent<SpriteRenderer>();
                if (_srFantasma == null)
                    _srFantasma = _fantasmaTorre.AddComponent<SpriteRenderer>();

                Transform alcanceTr = _fantasmaTorre.transform.Find("Alcance");
                if (alcanceTr != null)
                {
                    _alcanceFantasma = alcanceTr.gameObject;
                    _srAlcance = _alcanceFantasma.GetComponent<SpriteRenderer>();
                    if (_srAlcance == null)
                    {
                        _srAlcance = _alcanceFantasma.AddComponent<SpriteRenderer>();
                        _srAlcance.sprite = CriarSpriteCirculo();
                    }
                    else if (_srAlcance.sprite == null)
                        _srAlcance.sprite = CriarSpriteCirculo();
                }
            }

            if (_srFantasma != null) _srFantasma.sortingOrder = 50;
            if (_srAlcance != null)
            {
                _srAlcance.sortingOrder = 49;
                _srAlcance.color = new Color(1f, 1f, 1f, 0.2f);
            }

            _fantasmaTorre.SetActive(false);
        }

        private Sprite CriarSpriteCirculo()
        {
            int s = 64;
            var tex = new Texture2D(s, s);
            var pix = new Color[s * s];
            float centro = s / 2f;
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(centro, centro));
                    pix[y * s + x] = d <= centro ? new Color(1f, 1f, 1f, 0.3f) : Color.clear;
                }
            tex.SetPixels(pix); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 64f);
        }

        private void AtualizarFantasma(ConfiguracaoTorre config)
        {
            if (_fantasmaTorre == null) return;

            Vector2 posicaoMouse = _camera.ScreenToWorldPoint(Input.mousePosition);
            int cx = Mathf.FloorToInt((posicaoMouse.x - _origemGrade.x + tamanhoCelula / 2f) / tamanhoCelula);
            int cy = Mathf.FloorToInt((posicaoMouse.y - _origemGrade.y + tamanhoCelula / 2f) / tamanhoCelula);

            bool dentroGrade = cx >= 0 && cx < colunas && cy >= 0 && cy < linhas;

            if (dentroGrade)
            {
                Vector2 pos = _origemGrade + new Vector2(cx * tamanhoCelula, cy * tamanhoCelula);
                _fantasmaTorre.transform.position = pos;
                _fantasmaTorre.SetActive(true);

                if (_srFantasma != null)
                {
                    if (config.spriteTorre != null)
                        _srFantasma.sprite = config.spriteTorre;
                    else if (torrePrefab != null)
                    {
                        var srPrefab = torrePrefab.GetComponent<SpriteRenderer>();
                        if (srPrefab != null) _srFantasma.sprite = srPrefab.sprite;
                    }

                    bool podeConstruir = !_celulasOcupadas[cx, cy]
                        && PodeConstruir(cx, cy)
                        && PodePagar(config)
                        && !LimiteAtingido();
                    _srFantasma.color = podeConstruir ? corPreviewValida : corPreviewInvalida;
                }

                if (_alcanceFantasma != null)
                {
                    _alcanceFantasma.SetActive(true);
                    float diametro = config.alcance * 2f;
                    _alcanceFantasma.transform.localScale = new Vector3(diametro, diametro, 1f);
                }
            }
            else
            {
                _fantasmaTorre.SetActive(false);
                if (_alcanceFantasma != null) _alcanceFantasma.SetActive(false);
            }
        }

        private void EsconderFantasma()
        {
            if (_fantasmaTorre != null) _fantasmaTorre.SetActive(false);
            if (_alcanceFantasma != null) _alcanceFantasma.SetActive(false);
        }

        private void CancelarSelecao()
        {
            if (SeletorTorre.Instancia != null)
                SeletorTorre.Instancia.SelecionarTorre(null);
        }

        private void CriarGradeVisual()
        {
            if (celulaPrefab == null) return;

            for (int x = 0; x < colunas; x++)
            {
                for (int y = 0; y < linhas; y++)
                {
                    Vector2 pos = _origemGrade + new Vector2(x * tamanhoCelula, y * tamanhoCelula);
                    GameObject celula = Instantiate(celulaPrefab, pos, Quaternion.identity, transform);
                    celula.transform.localScale = new Vector3(tamanhoCelula, tamanhoCelula, 1f);
                    AtualizarCorCelula(celula, false);
                    _celulasVisuais[x, y] = celula;
                }
            }
        }

        private void TentarConstruirTorre()
        {
            Vector2 posicaoMouse = _camera.ScreenToWorldPoint(Input.mousePosition);

            int cx = Mathf.FloorToInt((posicaoMouse.x - _origemGrade.x + tamanhoCelula / 2f) / tamanhoCelula);
            int cy = Mathf.FloorToInt((posicaoMouse.y - _origemGrade.y + tamanhoCelula / 2f) / tamanhoCelula);

            if (cx < 0 || cx >= colunas || cy < 0 || cy >= linhas) return;
            if (_celulasOcupadas[cx, cy]) return;

            ConfiguracaoTorre config = ObterConfigTorre();
            if (config == null) return;

            if (!PodeConstruir(cx, cy)) return;
            if (LimiteAtingido())
            {
                if (GerenciadorHUD.Instancia != null)
                    GerenciadorHUD.Instancia.MostrarMensagem("Limite de torres atingido!");
                return;
            }

            int custo = config.custo;
            if (GerenciadorHUD.Instancia != null && !GerenciadorHUD.Instancia.GastarMoedas(custo))
            {
                GerenciadorHUD.Instancia.MostrarMensagem("Moedas insuficientes!");
                return;
            }

            ConstruirTorre(cx, cy, config);
        }

        /// <summary>
        /// Valida se a célula permite construção (regras de colisão com caminho, etc.).
        /// </summary>
        protected virtual bool PodeConstruir(int cx, int cy)
        {
            if (torrePrefab == null)
            {
                Debug.LogWarning("[GradePosicionamento] Torre prefab não definido.");
                return false;
            }

            if (camadaNaoConstruir.value != 0)
            {
                Vector2 pos = _origemGrade + new Vector2(cx * tamanhoCelula, cy * tamanhoCelula);
                Vector2 tamanhoOverlap = new Vector2(tamanhoCelula * 0.9f, tamanhoCelula * 0.9f);
                Collider2D hit = Physics2D.OverlapBox(pos, tamanhoOverlap, 0f, camadaNaoConstruir);
                if (hit != null) return false;
            }

            return true;
        }

        private void ConstruirTorre(int cx, int cy, ConfiguracaoTorre config)
        {
            Vector2 pos = _origemGrade + new Vector2(cx * tamanhoCelula, cy * tamanhoCelula);
            GameObject torreGo = Instantiate(torrePrefab, pos, Quaternion.identity);

            Rigidbody2D rb = torreGo.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
            }

            Torre torreComp = torreGo.GetComponent<Torre>();
            if (torreComp != null && config != null)
            {
                torreComp.DefinirConfig(config, cx, cy);
            }

            if (config != null && config.spriteTorre != null)
            {
                SpriteRenderer srTorre = torreGo.GetComponent<SpriteRenderer>();
                if (srTorre != null)
                {
                    srTorre.sprite = config.spriteTorre;
                }
            }

            _celulasOcupadas[cx, cy] = true;
            _torresConstruidas++;
            AtualizarCorCelula(_celulasVisuais[cx, cy], true);

            if (efeito != null)
                efeito.EmitirParticula(pos);
        }

        public void LiberarCelula(int cx, int cy)
        {
            if (cx >= 0 && cx < colunas && cy >= 0 && cy < linhas)
            {
                _celulasOcupadas[cx, cy] = false;
                if (_torresConstruidas > 0) _torresConstruidas--;
                AtualizarCorCelula(_celulasVisuais[cx, cy], false);
            }
        }


        private ComponenteEfeito efeito;

        private void Awake()
        {
            efeito = GetComponent<ComponenteEfeito>();
        }

        private ConfiguracaoTorre ObterConfigTorre()
        {
            if (SeletorTorre.Instancia != null)
                return SeletorTorre.Instancia.TorreSelecionada;
            return null;
        }

        private bool PodePagar(ConfiguracaoTorre config)
        {
            if (GerenciadorHUD.Instancia == null) return true;
            return GerenciadorHUD.Instancia.GetMoedas() >= config.custo;
        }

        private bool LimiteAtingido()
        {
            return limiteMaximoTorres > 0 && _torresConstruidas >= limiteMaximoTorres;
        }

    private void VerificarCliqueTorre()
    {
        Vector2 posicaoMouse = _camera.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(posicaoMouse);

        if (hit != null)
        {
            Torre torre = hit.GetComponent<Torre>();
            if (torre != null && Torre.OnTorreSelecionada != null)
                Torre.OnTorreSelecionada(torre);
        }
        else
        {
            if (Torre.OnTorreSelecionada != null)
                Torre.OnTorreSelecionada(null);
        }
    }

        private bool ClicouEmTorreExistente()
        {
            Vector2 posicaoMouse = _camera.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(posicaoMouse);
            if (hit != null)
            {
                Torre torre = hit.GetComponent<Torre>();
                if (torre != null)
                {
                    if (Torre.OnTorreSelecionada != null)
                        Torre.OnTorreSelecionada(torre);
                    return true;
                }
            }
            return false;
        }


        private void AtualizarCorCelula(GameObject celula, bool ocupada)
        {
            if (celula == null) return;
            SpriteRenderer sr = celula.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = ocupada ? corOcupada : corLivre;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;

            float larguraTotal = colunas * tamanhoCelula;
            float alturaTotal = linhas * tamanhoCelula;
            Vector2 origem = (Vector2)transform.position - new Vector2(larguraTotal / 2f, alturaTotal / 2f);

            for (int x = 0; x <= colunas; x++)
            {
                Vector2 inicio = origem + new Vector2(x * tamanhoCelula, 0f);
                Vector2 fim = inicio + new Vector2(0f, alturaTotal);
                Gizmos.DrawLine(inicio, fim);
            }

            for (int y = 0; y <= linhas; y++)
            {
                Vector2 inicio = origem + new Vector2(0f, y * tamanhoCelula);
                Vector2 fim = inicio + new Vector2(larguraTotal, 0f);
                Gizmos.DrawLine(inicio, fim);
            }
        }
    }
}
