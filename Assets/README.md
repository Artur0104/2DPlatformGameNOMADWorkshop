# ZeroToHero — Template Base NO.MAD

> Template base para criação rápida de protótipos 2D.

---

## Requisitos

- **Unity 6000.3** ou superior
- Pacotes: 2D Animation, 2D Sprite, 2D Tilemap, Input System, TextMeshPro, Universal RP

---

## Como Começar (3 minutos)

1. Abra o projeto no Unity
2. Abra a cena `Assets/Scenes/MenuPrincipal.unity`
3. Dê Play — o menu mostra 3 templates: **Plataforma**, **Tower Defense**, **Infinite Runner**
4. Escolha um e comece a customizar

---

## Estrutura de Pastas

```
Assets/
├── _ZeroToHero/
│   ├── Core/                          # Scripts e assets compartilhados por TODOS os templates
│   │   ├── Scripts/Base/              # EntidadeBase, GerenciadorBase, InputReader
│   │   ├── Scripts/Componentes/       # 10 componentes plug-and-play
│   │   ├── Scripts/Gerenciadores/     # 5 singletons globais
│   │   ├── Scripts/Interfaces/        # IDanificavel, IColetavel, IInteragivel
│   │   ├── Scripts/Ferramentas/       # PainelAjusteBase (overlay de tuning)
│   │   ├── SO/                        # ScriptableObjects GENÉRICOS (4: Entidade, Movimento, Ataque, Habilidade)
│   │   └── Prefabs/Core/              # Prefabs dos gerenciadores + HUD
│   ├── Templates/
│   │   ├── Plataforma/                # Scripts, Prefabs do template Plataforma
│   │   ├── TowerDefense/              # Scripts, Prefabs, SO, Sprites, Tiles do template Tower Defense
│   │   └── Runner/                    # Scripts, Prefabs, SO do template Infinite Runner
│   ├── Menu/Scripts/                  # MenuPrincipal, BotaoCena
│   └── Recursos/
│       ├── Sprites/                   # Sprites compartilhados (placeholders)
│       ├── Sons/                      # Pasta para áudio (+ LEIAME.txt)
│       └── Fisica/                    # PhysicsMaterial2D
├── Scenes/
│   ├── MenuPrincipal.unity            # Cena 0 — Menu inicial
│   ├── Plataforma.unity               # Cena 1 — Plataforma
│   ├── TowerDefense.unity             # Cena 2 — Tower Defense
│   └── InfiniteRunner.unity           # Cena 3 — Infinite Runner
└── InputSystem_Actions.inputactions   # Mapeamento de controles
```

**Regra de ouro:** scripts em `Core/` são compartilhados. Alterações no Core impactam **todos** os templates ao mesmo tempo. Scripts e assets específicos de cada gênero vivem em `Templates/{Nome}/`.

---

## Arquitetura

### Fluxo de Dados

```
Input System → InputReader → GerenciadorJogo (controla estado: Menu/Jogando/Pausado/GameOver/Vitoria)
                                    │
                                    ▼
                              EntidadeBase
                           (cache de componentes)
                                    │
                    ┌───────────────┼───────────────┐
                    │               │               │
            ComponenteVida   ComponenteMovimento  ComponenteAtaque ...
                    │               │               │
                    └───────────────┼───────────────┘
                                    │
                            GerenciadorHUD
                            GerenciadorAudio
                            GerenciadorCena
```

### Componentes Core (10)

Arraste qualquer um destes para um GameObject e configure no Inspector. Cada componente funciona independentemente — combine como quiser.

| Componente | O que faz | Fallback se não configurar |
|-----------|-----------|---------------------------|
| **ComponenteVida** | Vida, dano, cura, morte, invencibilidade. Expõe `onDano`, `onCura`, `onMorte` (UnityEvent) | Destrói o GameObject ao morrer |
| **ComponenteMovimento** | Andar, pular, dash, coyote time, jump buffer, pulo duplo | Usa Rigidbody2D.velocity |
| **ComponenteAnimacao** | Tocar animações, definir parâmetros do Animator | Ignora silenciosamente se sem Animator |
| **ComponenteAtaque** | Ataque corpo a corpo, distância (projétil) ou área | OverlapCircle para melee |
| **ComponenteProjetil** | Projétil com trajetória: reto, parábola, teleguiado, circular. Dano ao colidir | Velocidade linear simples |
| **ComponenteColetavel** | Item coletável: moeda, vida, poder, chave | Efeito aplicado ao coletor |
| **ComponenteEfeito** | Partículas, sons, vibração de câmera | Círculo branco procedural, sem erro se sem áudio |
| **ComponenteHabilidade** | Habilidade com cooldown, custo, desbloqueio. Expõe `onUsar`, `onPronta` | Cooldown simples |
| **ComponenteSpawn** | Spawn de entidades sob demanda ou em ondas | Spawn com intervalo configurável |
| **CameraSuave** | Follow com damping, antecipação de movimento, confinamento | Sem efeito se sem alvo |

### Gerenciadores (5)

| Gerenciador | Função | Como acessar |
|------------|--------|-------------|
| `GerenciadorJogo` | Controla estado global: Menu, Jogando, Pausado, GameOver, Vitoria | `GerenciadorJogo.Instancia` |
| `GerenciadorCena` | Transição entre cenas, recarregar, sair do jogo | `GerenciadorCena.Instancia` |
| `GerenciadorAudio` | Pool de AudioSources, música de fundo, efeitos sonoros | `GerenciadorAudio.Instancia` |
| `GerenciadorParticulas` | Pool de partículas reutilizáveis | `GerenciadorParticulas.Instancia` |
| `GerenciadorHUD` | Vida, moedas, pontuação, ondas, mensagens, telas de vitória/derrota | `GerenciadorHUD.Instancia` |

### Interfaces (3)

| Interface | Métodos | Use em |
|-----------|---------|--------|
| **IDanificavel** | `ReceberDano(float)`, `Curar(float)`, `Morrer()` | Qualquer entidade que recebe dano |
| **IColetavel** | `Coletar(GameObject)`, `GetTipo()` | Itens coletáveis |
| **IInteragivel** | `Interagir(GameObject)`, `GetMensagemInteracao()` | Objetos interagíveis (portas, NPCs) |

### Input Actions

| Ação | Teclado | Gamepad | Usada em |
|------|---------|---------|----------|
| Mover | WASD / Setas | Left Stick | Todos |
| Pular | Espaço | A (South) | Plataforma |
| Atacar | Mouse Esq / Enter | X (West) | Todos |
| Interagir | E | Y (North) | Todos |
| Correr/Dash | Left Shift | LeftStick Press | Plataforma, Infinite Runner |
| Habilidade 1-4 | 1, 2, 3, 4 | DPad Up/Right/Down/Left | Todos |
| Foco | Left Ctrl | RightStick Press | Infinite Runner |
| Pausar | Escape | Start | Todos |

---

## Guia: Template Plataforma

### Conceito

Jogo de plataforma 2D clássico. O jogador corre, pula, faz dash, coleta moedas, desvia de inimigos e espinhos, ativa checkpoints e chega ao portal de fim de fase.

### O que já funciona

| Mecânica | Como funciona |
|----------|--------------|
| **Andar** | WASD / Setas. `ComponenteMovimento` aplica velocidade horizontal |
| **Pular** | Espaço. Suporta coyote time, jump buffer e pulo duplo (configurável) |
| **Dash** | Left Shift. Impulso horizontal rápido com trail visual |
| **Coletar moedas** | Encostar no item. `Coletavel_Moeda.prefab` → `GerenciadorHUD.Instancia.AdicionarMoedas()` |
| **Inimigos patrulha** | `Inimigo_Patrulha.prefab` — segue waypoints, causa dano ao toque, morre com stomp |
| **Stomp** | Pisar no topo do inimigo (caindo) mata ele e dá bounce no jogador |
| **Checkpoint** | Salva posição. Ao morrer, respawna no último checkpoint ativado |
| **Espinhos** | `Zona_Espinhos.prefab` — dano por segundo + empurrão |
| **Portal de vitória** | `Portal_FimFase.prefab` — trigger que chama `GerenciadorJogo.Instancia.VitoriaJogo()` |
| **Game Over** | Morrer sem checkpoint ativo → tela de Game Over com botões Reiniciar / Menu |
| **Pause** | Escape → `GerenciadorJogo.Instancia.Pausar()` → overlay de pause |
| **HUD** | Vida, moedas, timer. Atualizado automaticamente pelo `GerenciadorHUD` |
| **Câmera suave** | `CameraSuave` — segue o jogador com damping e antecipação de movimento |
| **Squash & stretch** | Jogador estica ao pular, comprime ao pousar. Trail no dash |
| **Tilemap** | Chão com CompositeCollider2D. Plataformas com PlatformEffector2D (one-way) |

### Fluxo de Dados Específico

```
InputReader (Mover, Pular, Dash, Interagir)
    │
    ▼
JogadorPlataforma (herda EntidadeBase)
    ├── ComponenteMovimento (aplica física: andar, pular, dash, gravidade)
    ├── ComponenteVida (vida, dano, invencibilidade pós-dano)
    │       └── onMorte → ProcessarMorte() → respawn no checkpoint ou GameOver
    │       └── onDano → piscar sprite + knockback
    ├── ComponenteEfeito (partículas ao pousar)
    └── TrailRenderer (rastro durante dash)

InimigoPatrulha (herda EntidadeBase)
    ├── Waypoints → patrulha entre pontos
    ├── ComponenteVida → onMorte → animação de squash + fade
    ├── OnCollisionEnter2D → stomp (jogador caindo) ou dano (jogador encostando)
    └── StompSensor (trigger filho no topo) → detecta pisada

Checkpoint
    └── OnTriggerEnter2D → salva posição no JogadorPlataforma + cura

PortalFimFase
    └── OnTriggerEnter2D → GerenciadorJogo.VitoriaJogo() + tela de vitória
```

### ScriptableObjects usados

| SO | Tipo | Onde |
|----|------|------|
| `ConfiguracaoMovimento_Plataforma.asset` | `ConfiguracaoMovimento` | `JogadorPlataforma` → `ComponenteMovimento.AplicarConfiguracao()` |

### Prefabs

| Prefab | Função |
|--------|--------|
| `Jogador_Plataforma.prefab` | Jogador com EntidadeBase, ComponenteVida, ComponenteMovimento, TrailRenderer |
| `Inimigo_Patrulha.prefab` | Inimigo com waypoints, StompSensor filho para detecção de pisada |
| `Coletavel_Moeda.prefab` | Moeda coletável com ComponenteColetavel |
| `Checkpoint.prefab` | Ponto de save com evento de ativação |
| `Zona_Espinhos.prefab` | Área de dano contínuo |
| `Portal_FimFase.prefab` | Trigger de vitória ao tocar |
| `Plataforma.prefab` | Base de plataforma móvel |
| `PainelAjuste.prefab` | Overlay de tuning com sliders (hotkey `` ` ``) |

### O que customizar

**Ajustar física do jogador:**
1. Selecione `ConfiguracaoMovimento_Plataforma.asset` no Project
2. Altere `velocidadeAndar`, `forcaPulo`, `pulosMaximos`, `velocidadeDash`, etc.
3. Dê Play — mudanças aplicadas instantaneamente

**Ou use o Painel de Ajuste (mais rápido):**
1. Pressione `` ` `` (backtick) durante o jogo
2. Arraste os sliders em tempo real: velocidade, pulo, dash, gravidade, coyote time
3. Ache os valores ideais → anote → transfira para a SO

**Criar um novo inimigo:**
1. Duplique `Inimigo_Patrulha.prefab`
2. Adicione/remova waypoints filhos
3. Ajuste `velocidade`, `danoContato`, `vidaMaxima` no Inspector
4. Troque o sprite no SpriteRenderer

**Criar um novo nível:**
1. Duplique `Plataforma.unity`
2. No Grid → Tilemap, pinte chão, paredes e plataformas com `Tile_Branco.png`
3. Arraste waypoints para os inimigos
4. Posicione checkpoints, moedas e o portal de fim de fase
5. Adicione à Build Settings

**Adicionar sons:**
1. Coloque .wav/.mp3 em `Recursos/Sons/`
2. Arraste o AudioClip para `ComponenteEfeito` → `somDano`, `somPulo`, etc.

### O que NÃO mexer

| Arquivo | Motivo |
|---------|--------|
| `Core/Scripts/Base/EntidadeBase.cs` | Classe base de TODAS as entidades. Quebrar = quebrar todos os templates |
| `Core/Scripts/Base/GerenciadorBase.cs` | Singleton base de todos os gerenciadores |
| `Core/Scripts/Base/InputReader.cs` | Wrapper do Input System usado por todos os templates |
| `Core/Scripts/Componentes/ComponenteVida.cs` | Usado por plataforma, TD, e Runner |
| `Core/Scripts/Componentes/ComponenteMovimento.cs` | Usado por plataforma e potencialmente outros |
| `Core/Scripts/Gerenciadores/GerenciadorJogo.cs` | Máquina de estado global do jogo |
| `Core/Scripts/Gerenciadores/GerenciadorCena.cs` | Transição de cenas usada por todos |
| `Core/SO/` (4 SOs genéricos) | `ConfiguracaoEntidade`, `ConfiguracaoMovimento`, `ConfiguracaoAtaque`, `ConfiguracaoHabilidade` — alterar impacta múltiplos templates |

---

## Guia: Template Tower Defense

### Conceito

Jogo de defesa de torres. Inimigos seguem um caminho pré-definido até a base do jogador. Você constrói torres em células da grade para detê-los. Cada torre tem tipo, alcance, dano e pode aplicar efeitos de status. Ondas de inimigos aumentam progressivamente.

### O que já funciona

| Mecânica | Como funciona |
|----------|--------------|
| **Grade de construção** | Clique na grade → preview fantasma (verde = válido, vermelho = inválido) |
| **Construir torre** | Clique esquerdo → instancia `Torre_Base.prefab` na célula. Custo em moedas |
| **5 tipos de torre** | Arqueiro (Físico), Canhão (Explosivo), Fogo (DOT), Gelo (Slow), Veneno (DOT + Slow) |
| **Melhorar torre** | Clique na torre → painel de info → botão Melhorar (custa moedas, aumenta dano/alcance) |
| **Vender torre** | Clique na torre → painel de info → botão Vender (reembolso parcial) |
| **Inimigos por waypoints** | Seguem caminho sequencial. Ao chegar ao fim → dano à base |
| **Ondas** | Spawn automático de inimigos em ondas. Recompensa em moedas ao completar |
| **Moedas** | Ganhas ao matar inimigos e completar ondas. Gastas em construir/melhorar torres |
| **Barra de vida** | Inimigos exibem barra de vida acima do sprite |
| **Efeitos de status** | Slow (gelo), DoT (fogo/veneno), Explosão em área (canhão) |
| **Game Over** | Base do jogador destruída → tela de derrota |
| **Vitória** | Todas as ondas completadas → tela de vitória |
| **Seletor de torre** | Painel inferior com botões por tipo de torre. Exibe custo e ícone |

### Fluxo de Dados Específico

```
GerenciadorOndas
    ├── ConfiguracaoOnda[] (cada onda: quais inimigos, quantos, intervalo, recompensa)
    ├── Spawna inimigos via Instantiate(prefabInimigo)
    └── Eventos: onOndaIniciada, onOndaCompleta, onTodasOndasCompletas

InimigoCaminho (herda EntidadeBase)
    ├── Waypoints → MoveTowards sequencial
    ├── ComponenteVida → onMorte → recompensa em moedas + notifica GerenciadorOndas
    ├── BarraVida (filho com SpriteRenderer escalado horizontalmente)
    └── ChegarAoFim() → dano à base + Destroy

SeletorTorre (singleton)
    ├── ConfiguracaoTorre[] torresDisponiveis → cria botões dinamicamente
    └── TorreSelecionada → GradePosicionamento lê para preview fantasma

GradePosicionamento
    ├── Renderiza grade visual (células verdes/vermelhas)
    ├── Preview fantasma (torre + círculo de alcance)
    ├── Clique → valida (célula livre? moedas >= custo? limite de torres?)
    └── Instancia torre via Instantiate(torrePrefab)

Torre (herda EntidadeBase)
    ├── ConfiguracaoTorre → dano, alcance, velocidadeAtaque, tipo, status effects
    ├── EncontrarAlvo() → Physics2D.OverlapCircleAll com LayerMask
    ├── AtacarAlvo() → projétil ou hit scan. Aplica status effects conforme tipo
    ├── Melhorar() → aumenta nível, dano, alcance (custa moedas)
    └── Vender() → Destroy + reembolso parcial

Base (herda EntidadeBase)
    └── ComponenteVida → onMorte → GameOver
```

### ScriptableObjects usados

| SO | Tipo | Onde |
|----|------|------|
| `Torre_Arqueiro.asset` | `ConfiguracaoTorre` | `SeletorTorre.torresDisponiveis[]` |
| `Torre_Canhao.asset` | `ConfiguracaoTorre` | `SeletorTorre.torresDisponiveis[]` |
| `Torre_Fogo.asset` | `ConfiguracaoTorre` | `SeletorTorre.torresDisponiveis[]` |
| `Torre_Gelo.asset` | `ConfiguracaoTorre` | `SeletorTorre.torresDisponiveis[]` |
| `Torre_Veneno.asset` | `ConfiguracaoTorre` | `SeletorTorre.torresDisponiveis[]` |
| `Onda_01.asset` a `Onda_03.asset` | `ConfiguracaoOnda` | `GerenciadorOndas.ondas[]` |

### Prefabs

| Prefab | Função |
|--------|--------|
| `Torre_Base.prefab` | Template de torre com Torre, SpriteRenderer, CircleCollider2D |
| `Inimigo_Caminho.prefab` | Inimigo com waypoints, barra de vida, ComponenteVida |
| `Base_Jogador.prefab` | Base do jogador com Base, ComponenteVida |
| `Projetil_Torre.prefab` | Projétil disparado pelas torres |
| `Posicao_Grade.prefab` | Célula visual da grade (quadrado colorido) |
| `SeletorTorre.prefab` | Painel UI inferior com botões de seleção de torre |
| `PainelTorreInfo.prefab` | Painel popup com info da torre + botões Melhorar/Vender |
| `BotaoTorre_Template.prefab` | Template de botão para cada tipo de torre |
| `TextoFlutuante.prefab` | Dano flutuante ao acertar inimigos |

### O que customizar

**Adicionar um novo tipo de torre:**
1. Duplique `Torre_Canhao.asset` → renomeie (ex: `Torre_Raio.asset`)
2. Altere os campos: `dano`, `alcance`, `velocidadeAtaque`, `tipo`, `custo`, `statusDuracao`
3. Atribua `spriteTorre` e `icone` (atualmente vazios — arraste sprites do Project)
4. Adicione a nova SO ao array `torresDisponiveis` do `SeletorTorre` na cena
5. O tipo `Eletrico` já existe no enum `TipoTorre` — use como base para dano em cadeia

**Balancear ondas:**
1. Selecione `Onda_01.asset` → ajuste `quantidade`, `intervaloSpawn`, `recompensa`
2. Adicione/remova prefabs no array `prefabsInimigos`
3. Crie novas ondas: Assets > Create > ZeroToHero > Configuração de Onda
4. Adicione ao array `ondas` do `GerenciadorOndas` na cena

**Mudar o layout do caminho:**
1. Na cena, crie GameObjects vazios como waypoints
2. Posicione-os formando o caminho desejado
3. Arraste-os em ordem para o array `waypoints` do `InimigoCaminho`

**Configurar a grade:**
1. Selecione o `GradePosicionamento` na cena
2. Ajuste `colunas`, `linhas`, `tamanhoCelula`
3. Defina `limiteMaximoTorres` (0 = sem limite)

### O que NÃO mexer

| Arquivo | Motivo |
|---------|--------|
| `Core/Scripts/Gerenciadores/GerenciadorHUD.cs` | Gerencia moedas e UI compartilhada |
| `Templates/TowerDefense/Scripts/GerenciadorOndas.cs` | Lógica central de spawn e progressão |
| `Templates/TowerDefense/Scripts/GradePosicionamento.cs` | Sistema de grid + validação de construção |
| `Templates/TowerDefense/Scripts/Torre.cs` | Comportamento base de todas as torres (status effects, upgrade, venda) |
| `Templates/TowerDefense/SO/ConfiguracaoTorre.cs` | Estrutura de dados das torres — alterar campos = quebrar todas as torres |

---

## Guia: Template Infinite Runner

### Conceito

Sobrevivência estilo "bullet heaven" / Vampire Survivors. Você controla uma nave espacial, atira automaticamente, coleta drops de inimigos derrotados, sobe de nível e escolhe power-ups para ficar mais forte. Inimigos e meteoros spawnam continuamente em ondas crescentes. Um chefão aparece periodicamente. Era pra ser um infinite runner, mas isso aí foi o que saiu.

### O que já funciona

| Mecânica | Como funciona |
|----------|--------------|
| **Movimento 4 direções** | WASD. Nave se move livremente dentro das bordas |
| **Tiro automático** | Atira pra cima automaticamente. Frequência base configurável |
| **5 armas com padrões** | Base (Frente), Arco (Leque), Rajada (Rajada), Teleguiada (Teleguiado), Rápida (disparo rápido). Cada arma tem padrão de projéteis diferente |
| **Armas temporárias** | Power-ups de arma substituem a arma base por duração limitada |
| **Level Up** | Barra de XP enche ao coletar drops. Ao subir de nível → popup com 3 power-ups aleatórios |
| **8 Power-ups** | BoostDano, BoostVelocidade, FrequenciaTiro, Invencibilidade, VelocidadeTemp, ArmaArco, ArmaRajada, ArmaTeleguiada |
| **Inimigos com IA** | Spawnam no topo, descem com desvio lateral em direção ao jogador. 3 tiers: Pequeno, Médio, Grande |
| **Meteoros** | Obstáculos destrutíveis que descem do topo em linha reta. 2 tamanhos |
| **Boss** | Chefão com múltiplas fases. Cada fase = padrão de ataque diferente. Barra de vida no HUD |
| **Drops magnéticos** | Gotas de XP e Vida são atraídas magneticamente ao jogador quando próximas |
| **HUD completo** | Vida (corações), barra de XP, nível atual, boss HP, distância percorrida |
| **Game Over** | Ao morrer → tela de derrota |
| **Vitória** | Derrotar o boss → `GerenciadorJogo.VitoriaJogo()` |

### Fluxo de Dados Específico

```
GerenciadorSpawnRunner
    ├── ConfiguracaoInimigoRunner[] inimigosDisponiveis (pool ponderado)
    ├── ConfiguracaoMeteoro[] meteorosDisponiveis
    ├── ConfiguracaoBossRunner configBoss
    ├── Update → timer → spawna inimigos/meteoros com peso por tempo de jogo
    └── Boss → spawna após tempo configurado

InimigoRunner (herda EntidadeBase)
    ├── ConfiguracaoInimigoRunner → vida, velocidade, dano, XP drop, sprite
    ├── Movimento: desce do topo com desvio lateral (tracking do jogador)
    ├── ComponenteVida → onMorte → dropa XP + chance de drop de vida
    └── Atira projéteis (inimigos grandes)

MeteoroRunner (herda EntidadeBase)
    ├── ConfiguracaoMeteoro → vida, velocidade
    └── Desce em linha reta. Destrutível

BossRunner (herda EntidadeBase)
    ├── ConfiguracaoBossRunner → vida, fases (cada fase = padrão de ataque)
    ├── Barra de vida no HUD (UI Slider)
    └── onMorte → vitória

NaveJogadorRunner (herda EntidadeBase)
    ├── ConfiguracaoNaveJogador → vidas, velocidade, dano base
    ├── InputReader → movimento 4 direções
    ├── ComponenteVida → onMorte → GameOver
    ├── ControladorArmasRunner → gerencia arma atual + temporárias
    └── OnTriggerEnter2D → coleta drops

ControladorArmasRunner
    ├── ConfiguracaoArmaRunner armaBase (arma permanente)
    ├── EquiparArmaTemporaria(ConfiguracaoArmaRunner) → substitui base por duração
    └── Update → timer cooldown → Disparar()

DisparadorArmasRunner
    ├── Disparar(config, origem, prefab, multiplicador)
    └── Modos: Frente, Leque (Arco), Rajada, Teleguiado

GerenciadorLevelUpRunner
    ├── Barra de XP (UI Slider)
    ├── ConfiguracaoPowerUp[] poolPowerUps → sortear 3 opções
    ├── Popup Level Up → Time.timeScale = 0 → 3 botões
    └── AplicarPowerUp(config) → aplica boost/arma/invencibilidade

DropColetavelRunner
    ├── Tipo: XP ou Vida
    ├── Update → atração magnética (Vector2.MoveTowards) se jogador dentro do raio
    └── OnTriggerEnter2D → coleta (adiciona XP/cura)
```

### ScriptableObjects usados

| SO | Tipo | Onde |
|----|------|------|
| `NaveJogador_Default.asset` | `ConfiguracaoNaveJogador` | `NaveJogadorRunner.config` |
| `Inimigo_Pequeno.asset` / `Medio` / `Grande` | `ConfiguracaoInimigoRunner` | `GerenciadorSpawnRunner.inimigosDisponiveis[]` |
| `Boss_Default.asset` | `ConfiguracaoBossRunner` | `GerenciadorSpawnRunner.configBoss` |
| `Meteoro_Pequeno.asset` / `Grande` | `ConfiguracaoMeteoro` | `GerenciadorSpawnRunner.meteorosDisponiveis[]` |
| `Arma_Base.asset` / `Arco` / `Rajada` / `Rapida` / `Teleguiada` | `ConfiguracaoArmaRunner` | `ControladorArmasRunner` e referenciados por PowerUps |
| `PowerUp_BoostDano.asset` / etc. (8 power-ups) | `ConfiguracaoPowerUp` | `GerenciadorLevelUpRunner.poolPowerUps[]` |

### Prefabs

| Prefab | Função |
|--------|--------|
| `Inimigo_Runner.prefab` | Inimigo que persegue o jogador + atira (se grande) |
| `Meteoro_Runner.prefab` | Obstáculo destrutível que desce do topo |
| `Boss_Runner.prefab` | Chefão com múltiplas fases de ataque |
| `Drop_XP.prefab` | Gota de experiência (magnética) |
| `Drop_Vida.prefab` | Gota de cura (magnética) |
| `Projetil_Jogador_Runner.prefab` | Projétil do jogador (dano em inimigos) |
| `Projetil_Inimigo_Runner.prefab` | Projétil dos inimigos (dano no jogador) |
| `Texto_Flutuante.prefab` | Texto de dano flutuante ao acertar inimigos |

### O que customizar

**Balancear a dificuldade:**
1. Selecione `GerenciadorSpawnRunner` na cena
2. Ajuste `intervaloSpawnMin`/`Max`, `tempoAteBoss`, curva de spawn
3. Modifique os pesos (`pesoSpawn`) nos SOs de inimigo: `Inimigo_Pequeno.asset`, etc.
4. Altere `multiplicadorVidaInimigos` ao longo do tempo

**Criar uma nova arma:**
1. Assets > Create > ZeroToHero > Configuração de Arma (Runner)
2. Configure: `padrao` (Frente/Leque/Circular/Aleatorio/Teleguiado), `danoBase`, `cooldownBase`, `projeteisBase`, `nivelMaximo`
3. Atribua um `icone` (Sprite) para aparecer na UI
4. Crie um `PowerUp_Arma_Nova.asset` que referencia esta arma
5. Adicione ao `poolPowerUps[]` do `GerenciadorLevelUpRunner`

**Criar um novo power-up:**
1. Assets > Create > ZeroToHero > Configuração de Power-Up
2. Escolha `Tipo` (ArmaTemporaria, BoostStatus, Invulnerabilidade)
3. Se BoostStatus: escolha `TipoBoost` (Vida, Velocidade, Dano, FrequenciaTiro) + `ValorBoost`
4. Atribua `icone` e `nomePowerUp`
5. Adicione ao `poolPowerUps[]` do `GerenciadorLevelUpRunner`

**Adicionar um novo tipo de inimigo:**
1. Duplique `Inimigo_Medio.asset` → renomeie
2. Ajuste `vida`, `velocidade`, `danoContato`, `xpDrop`, `pesoSpawn`
3. Atribua um `sprite` (campo na SO)
4. Adicione ao `inimigosDisponiveis[]` do `GerenciadorSpawnRunner`

### O que NÃO mexer

| Arquivo | Motivo |
|---------|--------|
| `Templates/Runner/Scripts/GerenciadorSpawnRunner.cs` | Lógica central de spawn: progressão, pool ponderado, boss |
| `Templates/Runner/Scripts/GerenciadorLevelUpRunner.cs` | Sistema de XP + popup + aplicação de power-ups |
| `Templates/Runner/Scripts/ControladorArmasRunner.cs` | Gerencia arma base vs temporárias |
| `Templates/Runner/Scripts/DisparadorArmasRunner.cs` | Implementa todos os padrões de disparo |
| `Templates/Runner/Scripts/NaveJogadorRunner.cs` | Controlador principal do jogador |
| `Templates/Runner/SO/ConfiguracaoArmaRunner.cs` | Estrutura de dados das armas |
| `Templates/Runner/SO/ConfiguracaoPowerUp.cs` | Estrutura de dados dos power-ups |

---

## Ferramentas de Desenvolvimento

### Painel de Ajuste (Tuning Overlay)

Pressione `` ` `` (backtick) durante o jogo para abrir um painel com sliders que alteram valores em tempo real. Cada template tem seu próprio painel:

| Template | Painel | Sliders |
|----------|--------|---------|
| Plataforma | `PainelAjustePlataforma` | Velocidade, pulo, dash, coyote time, gravidade, time scale |
| Tower Defense | `PainelAjusteTowerDefense` (planejado) | Torres, inimigos, ondas, grade |
| Infinite Runner | `PainelAjusteRunner` | Jogador, armas, spawn, boss, level up |

**Fluxo de tuning:** `PainelAjuste → slider → SerializedObject/ScriptableObject → componente em tempo real`

O painel pausa o jogo ao abrir (`Time.timeScale = 0`). Use os sliders para achar os valores ideais, depois transfira os números para as ScriptableObjects no Inspector.

### CameraSuave

Componente de câmera com follow suave. Adicione à Main Camera e configure:

| Campo | Efeito |
|-------|--------|
| `alvo` | Transform que a câmera segue |
| `suavidade` | Quanto menor, mais grudento (0.1 = muito suave, 1 = instantâneo) |
| `antecipacao` | Olha pra frente baseado na velocidade do alvo (X = horizontal, Y = vertical) |
| `confinar` | Limita a câmera a uma área (defina `limiteMin`/`limiteMax`) |

### Acessando Campos Privados em Runtime

Use `SerializedObject` (UnityEngine) para ler/escrever campos `[SerializeField]` privados sem precisar de `#if UNITY_EDITOR`:

```csharp
var so = new SerializedObject(componente);
so.FindProperty("nomeDoCampo").boolValue = true;  // ou .floatValue, .intValue
so.ApplyModifiedProperties();
```

**NUNCA** use `UnityEditor.SerializedObject` com `#if UNITY_EDITOR` — isso quebra em builds.

---

## Inventário de Sprites

### Plataforma

| Sprite | Tam. | Substituível? | Onde é usado |
|--------|------|--------------|-------------|
| `Jogador_Plataforma.png` | 32×64 | Sim — sprite do jogador | `Jogador_Plataforma.prefab` |
| `Checkpoint.png` | 32×64 | Sim | `Checkpoint.prefab` |
| `Moeda.png` | 32×32 | Sim | `Coletavel_Moeda.prefab` |
| `Espinhos.png` | 32×32 | Sim | `Zona_Espinhos.prefab` |
| `Tile_Branco.png` | 1×1 | Não — pixel branco procedural | Chão, paredes, plataformas, inimigos |

### Tower Defense

| Sprite | Tam. | Substituível? | Onde é usado |
|--------|------|--------------|-------------|
| `Torre_Base.png` | 62×62 | Sim — sprite da torre | `Torre_Base.prefab`, `Inimigo_Runner.prefab` |
| `Base_Jogador.png` | 62×62 | Sim | `Base_Jogador.prefab`, `TowerDefense.unity`, `Drop_XP.prefab` |
| `Inimigo_Caminho.png` | 32×32 | Sim | `Inimigo_Caminho.prefab` |
| `Projetil_Torre.png` | 16×16 | Sim | `Projetil_Torre.prefab` |
| `Grade_Celula.png` | 64×64 | Sim | `Posicao_Grade.prefab` |
| `BarraVida.png` | 64×8 | Sim — barra de vida | `Inimigo_Caminho.prefab` |
| `Tile_Grama.png` | 64×64 | Sim | `Paleta_TD.prefab`, `TowerDefense.unity` (84 células) |
| `Tile_Caminho.png` | 64×64 | Sim | `Paleta_TD.prefab`, `TowerDefense.unity` |

### Infinite Runner

| Sprite | Tam. | Substituível? | Onde é usado |
|--------|------|--------------|-------------|
| `Nave_Jogador.png` | 32×30 | Sim — sprite da nave | `InfiniteRunner.unity` (SpriteRenderer + ícone de power-up) |
| `Chefe_Base.png` | 126×126 | Sim — sprite do boss | `Boss_Runner.prefab` |
| `Boss_Runner.png` | 128×128 | Sim | HUD (UI Image — handles de slider) |
| `Inimigo_Pequeno_Runner.png` | 24×24 | Sim | `Inimigo_Pequeno.asset` (campo sprite da SO) |
| `Inimigo_Medio_Runner.png` | 36×36 | Sim | `Inimigo_Medio.asset` |
| `Inimigo_Grande_Runner.png` | 48×48 | Sim | `Inimigo_Grande.asset` |
| `Coracao.png` | 25×24 | Sim | `Drop_Vida.prefab` |
| `Projetil_Jogador.png` | 8×8 | Sim | `Projetil_Jogador_Runner.prefab` |
| `Projetil_Inimigo.png` | 8×8 | Sim | `Projetil_Inimigo_Runner.prefab` |

**Placeholders não usados (podem ser removidos ou substituídos):**
`Fundo_Menu.png`, `Placeholder64.png`, `Hitbox_Ponto.png`, `ParticulaPadrao.png`, `Inimigo_Patrulha.png`, `Chao_Tile.png`, `Parede_Tile.png`, `Nave_Runner.png`, `Meteoro_Runner.png`, `Drop_Vida_Runner.png`, `Drop_XP_Runner.png`, `Projetil_Jogador_Runner.png`, `Projetil_Inimigo_Runner.png`

---

## Como Criar Conteúdo Novo

### Novo tipo de inimigo (qualquer template)

1. Duplique um SO de inimigo existente (ex: `Inimigo_Medio.asset`)
2. Altere os atributos: vida, velocidade, dano, recompensa
3. Atribua um sprite (campo `sprite` na SO)
4. Adicione ao array de inimigos disponíveis do spawner da cena
5. Se precisar de comportamento novo, duplique o script do inimigo e sobrescreva `AoAtualizar()`

### Nova arma (Infinite Runner)

1. Assets > Create > ZeroToHero > Configuração de Arma (Runner)
2. Escolha o `padrao` (Frente, Leque, Circular, Aleatorio, Teleguiado)
3. Ajuste `danoBase`, `cooldownBase`, `projeteisBase`, `nivelMaximo`
4. Crie um PowerUp que reference esta arma: Assets > Create > ZeroToHero > Configuração de Power-Up → Tipo = ArmaTemporaria
5. Adicione o PowerUp ao `poolPowerUps[]` do `GerenciadorLevelUpRunner`

### Nova torre (Tower Defense)

1. Duplique uma SO de torre existente (ex: `Torre_Arqueiro.asset`)
2. Altere os atributos: `dano`, `alcance`, `velocidadeAtaque`, `tipo`, `custo`
3. Atribua `spriteTorre` e `icone`
4. Adicione ao array `torresDisponiveis` do `SeletorTorre` na cena
5. Para tipos de dano novos: adicione ao enum `TipoTorre` e implemente no `switch` de `Torre.AtacarAlvo()`

### Novo nível / cena

1. Duplique a cena do template (ex: `Plataforma.unity`)
2. Redesenhe o layout (tilemaps, waypoints, grade)
3. Ajuste os SOs de configuração (ondas, inimigos, movimento)
4. Adicione ao Build Settings: File > Build Profiles > Scene List

---

## O Que NUNCA Mexer (Regras de Ouro)

### Core é compartilhado

Alterações nestes arquivos impactam **todos os 3 templates simultaneamente**. Só modifique se tiver certeza do impacto:

| Arquivo | Por quê |
|---------|---------|
| `EntidadeBase.cs` | Classe base de TODAS as entidades do jogo |
| `GerenciadorBase.cs` | Singleton base — quebrar = gerenciadores param de funcionar |
| `InputReader.cs` | Wrapper único do Input System para todos os templates |
| `ComponenteVida.cs` | Sistema de vida usado por jogador, inimigos, base, boss |
| `ComponenteMovimento.cs` | Física base usada pelo jogador de plataforma |
| `GerenciadorJogo.cs` | Máquina de estados (Menu/Jogando/Pausado/GameOver/Vitoria) |
| `GerenciadorCena.cs` | `CarregarCena()`, `RecarregarCena()`, `SairDoJogo()` |
| `GerenciadorHUD.cs` | Moedas, vida, ondas, mensagens, telas de fim de jogo |

### Namespaces

Cada pasta tem seu namespace. Respeite a separação:

| Pasta | Namespace |
|-------|-----------|
| `Core/Scripts/Base/` | `ZeroToHero.Core.Base` |
| `Core/Scripts/Componentes/` | `ZeroToHero.Core.Componentes` |
| `Core/Scripts/Gerenciadores/` | `ZeroToHero.Core.Gerenciadores` |
| `Core/Scripts/Interfaces/` | `ZeroToHero.Core.Interfaces` |
| `Core/Scripts/Ferramentas/` | `ZeroToHero.Core.Ferramentas` |
| `Core/SO/` + `Templates/*/SO/` | `ZeroToHero.Core.SO` |
| `Templates/Plataforma/Scripts/` | `ZeroToHero.Cenas.Plataforma` |
| `Templates/TowerDefense/Scripts/` | `ZeroToHero.Cenas.TowerDefense` |
| `Templates/Runner/Scripts/` | `ZeroToHero.Cenas.Runner` |
| `Menu/Scripts/` | `ZeroToHero.Menu` |

### 1 template por sessão

Nunca modifique arquivos de um template enquanto trabalha em outro. Scripts e SOs de `Templates/Plataforma/` não devem ser alterados durante o refino do Tower Defense, e vice-versa.

---

## FAQ

**P: Não sei programar. Consigo usar mesmo assim?**
R: Sim. A maior parte da configuração é feita no Inspector — arrastar SOs, ajustar números, posicionar waypoints. Os scripts já estão prontos.

**P: Como faço meu personagem pular mais alto?**
R: Abra o Painel de Ajuste (`` ` ``), arraste "Força do Pulo". Ou edite `ConfiguracaoMovimento_Plataforma.asset` → `forcaPulo`.

**P: Como adiciono mais inimigos?**
R: Duplique um inimigo na cena (Ctrl+D), mude a posição. Ou duplique o SO de configuração e adicione ao spawner.

**P: Como faço a transição entre cenas?**
R: `GerenciadorCena.Instancia.CarregarCena("NomeDaCena")` ou conecte um botão ao `BotaoCena.CarregarCena()`.

**P: Dá pra fazer outros gêneros além dos 3 templates?**
R: Sim. Os componentes Core são genéricos. Combine `ComponenteMovimento` + `ComponenteVida` + `ComponenteAtaque` para criar qualquer jogo 2D. Exemplos: Beat 'em Up, Idle Battler, Top-Down Shooter.

**P: O jogo não compila. O que fazer?**
R: Verifique o Console (Window > General > Console). Erros comuns: nome de cena errado no Build Settings, SO não atribuído no Inspector, namespace faltando.

---

## Limitações Conhecidas

| O template NÃO inclui | Alternativa |
|------------------------|-------------|
| Pathfinding (A*, NavMesh) | Inimigos seguem waypoints manuais |
| Inteligência Artificial complexa | Sobrescreva `AoAtualizar()` para lógica customizada |
| Sistema de save/load completo | Checkpoint salva posição; expanda conforme necessário |
| Multiplayer | Single-player apenas |
| Áudio embutido | Adicione seus próprios arquivos em `Recursos/Sons/` |
| Sprites finais (arte) | Use os placeholders ou adicione seus próprios |
| Cutscenes / diálogos | Implemente via eventos UnityEvent e HUD |

---

## Licença

Pode passar. Tudo nosso. tmj. 
