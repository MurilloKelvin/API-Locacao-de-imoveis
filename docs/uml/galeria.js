'use strict';

const views = [
  {id:'03-classes-camadas', group:'Arquitetura', title:'Colaboracao entre camadas', description:'As referencias que conectam a entrada HTTP ao dominio e a persistencia.', guide:'Da API ao banco', intro:'Leia de cima para baixo. Cada bloco reune os tipos de uma camada.', steps:['Os controllers chamam os services de aplicacao.','Os services usam interfaces de repositorio e entidades do dominio.','Os repositorios implementam contratos e usam AppDbContext.'], scope:'Vista resumida das quatro camadas, incluindo reservas e health.', doc:'classes-e-arquitetura.md'},
  {id:'02-classes-dominio', group:'Arquitetura', title:'Domínio de clientes', description:'Estado, comportamento e objetos de valor do cadastro de clientes.', guide:'O núcleo do cadastro', intro:'Cliente reúne a identidade e os comportamentos. CPF e e-mail são objetos de valor.', steps:['Comece pelos atributos e operações de Cliente.','Siga as associações até Cpf e Email.','Observe ClienteStatus e a dependência de ValidaCPF.'], scope:'O domínio de imoveis e reservas aparece nas vistas especificas.', doc:'classes-e-arquitetura.md'},
  {id:'04-classes-dtos', group:'Arquitetura', title:'DTOs de clientes', description:'Dados de cadastro, atualização, consulta paginada e resposta.', guide:'Entradas e resposta', intro:'Cada caixa mostra um DTO e os tipos de suas propriedades.', steps:['CriarClienteRequestDTO inclui nome, telefone, e-mail e CPF.','AtualizarClienteRequest recebe somente nome e telefone.','ListarClientesRequest aceita nome e status opcionais, página 1 e tamanho 10 por padrão.','ClienteResponse inclui ID, dados do cliente e status.'], scope:'Uma página sem resultados retorna []; a listagem não informa o total de registros.', doc:'classes-e-arquitetura.md'},
  {id:'01-casos-uso-atual', group:'Funcionalidades', title:'Operações da API', description:'Clientes, imoveis, reservas e health disponíveis ao consumidor.', guide:'O que o consumidor faz', intro:'O retângulo delimita a API e os pacotes agrupam os recursos.', steps:['Localize o ator Consumidor da API.','Percorra os fluxos de clientes, imoveis e reservas.','Consulte o health para verificar o estado da API.'], scope:'Nao ha autenticacao ou perfis de acesso implementados.', doc:'casos-de-uso.md'},
  {id:'05-sequencia-criar-cliente', group:'Fluxos implementados', title:'Cadastrar cliente · sucesso', description:'Da requisição HTTP à gravação, em três etapas de leitura.', guide:'Receber, verificar, persistir', intro:'Siga as mensagens numeradas de cima para baixo. Os retornos usam linhas tracejadas.', steps:['Construir o cliente e validar os dados no domínio.','Consultar a existência do CPF no repositório.','Persistir e retornar o ID com HTTP 201.'], scope:'Este recorte mostra o sucesso. As falhas estão em “Cadastrar cliente · erros”.', doc:'sequencias.md'},
  {id:'12-sequencia-erros-cadastro', group:'Fluxos implementados', title:'Cadastrar cliente · erros', description:'Dados inválidos e duplicidade de e-mail ou CPF.', guide:'Quatro caminhos de rejeição', intro:'O fragmento alt separa as alternativas do cadastro.', steps:['O domínio rejeita nome, e-mail ou CPF inválido: HTTP 400.','A consulta encontra e-mail existente: EmailJaCadastradoException e HTTP 409.','A consulta encontra CPF existente: CpfJaCadastradoException e HTTP 409.','O banco rejeita uma restricao de unicidade: erro traduzido para HTTP 409.'], scope:'As notas resumem operações internas; o acesso ao contexto e ao banco está na vista de sucesso.', doc:'sequencias.md'},
  {id:'06-sequencia-atualizar-cliente', group:'Fluxos implementados', title:'Atualizar cliente', description:'Localização, validação do domínio e persistência das alterações.', guide:'Do ID à atualização', intro:'A sequência explicita os caminhos de cliente ausente, dados inválidos e sucesso.', steps:['FindAsync obtém a entidade rastreada ou consulta o banco.','Cliente valida nome antes de atribuir nome e telefone.','SaveChangesAsync persiste as alterações; a API retorna 204.'], scope:'CPF, e-mail, ID e status não são alterados por este endpoint.', doc:'sequencias.md'},
  {id:'10-classes-operacoes', group:'Referência de classes', title:'Servicos e contratos', description:'Assinaturas dos services e interfaces de repositorio da aplicacao.', guide:'Contrato e coordenacao', intro:'A vista reune os services de clientes, imoveis e reservas.', steps:['ClienteService recebe ListarClientesRequest para filtros e paginação.','ImovelService coordena filtros e disponibilidade.','ReservaService recebe ListarReservasRequest para filtros e paginação.'], scope:'As interfaces isolam a aplicacao dos detalhes de persistencia.', doc:'classes-e-arquitetura.md'},
  {id:'13-classes-api', group:'Referência de classes', title:'Controllers da API', description:'Os controllers de clientes, imoveis, reservas e health.', guide:'A porta de entrada HTTP', intro:'As assinaturas preservam as operacoes publicas dos controllers.', steps:['Observe as dependencias dos services.','Identifique as rotas de cada recurso.','Confira o endpoint de health sem check de banco.'], scope:'Rotas, codigos HTTP e alternativas estao detalhados nos casos de uso.', doc:'classes-e-arquitetura.md'},
  {id:'14-classes-infrastructure', group:'Referência de classes', title:'Persistência e contexto', description:'Os repositorios concretos e o contexto do EF Core.', guide:'Acesso aos dados', intro:'Os repositorios usam o contexto para consultar e salvar os tres recursos.', steps:['Consulte ClienteRepository, ImovelRepository e ReservaRepository.','Observe a referencia _context.','Confira Clientes, Imoveis, Reservas e OnModelCreating em AppDbContext.'], scope:'O contexto configura value objects, indices unicos e chaves estrangeiras.', doc:'classes-e-arquitetura.md'},
  {id:'11-classes-heranca', group:'Referência de classes', title:'Herança dos tipos', description:'As classes do projeto e seus tipos-base externos.', guide:'Triângulos apontam para a base', intro:'As relações de herança estão isoladas para evitar cruzamentos no diagrama de camadas.', steps:['ClienteController e ImoveisController derivam de ControllerBase.','CpfJaCadastradoException deriva de Exception.','AppDbContext deriva de DbContext.'], scope:'A realização de IClienteRepository aparece na vista de colaboração entre camadas.', doc:'classes-e-arquitetura.md'},
  {"id": "19-casos-uso-imoveis", "group": "Imóveis", "title": "Operações de imóveis", "description": "Cadastro, consulta, filtros, disponibilidade, atualização e status.", "guide": "O que já pode ser solicitado", "intro": "O ator externo acessa as operações de imóveis.", "steps": ["Cadastrar e consultar usando o ID.", "Listar em páginas com filtros.", "Consultar disponibilidade por periodo e hospedes."], "scope": "A disponibilidade considera reservas pendentes ou confirmadas e o status do imovel.", "doc": "casos-de-uso.md"},
  {"id": "15-modelo-imovel", "group": "Imóveis", "title": "Modelo e DTOs de imóveis", "description": "A entidade, seus estados e os dados recebidos/devolvidos.", "guide": "Dados e validações", "intro": "O construtor inicia o imóvel como Disponível.", "steps": ["Endereço e título são obrigatórios; capacidade e diária devem ser positivas.", "O pedido de criação usa CapacidadeDeHospedes.", "A resposta usa CapacidadeHospedes, além do ID e status."], "scope": "Endereco é string. AlterarStatusImovel rejeita enum inválido e estado repetido.", "doc": "classes-e-arquitetura.md"},
  {"id": "16-camadas-imovel", "group": "Imóveis", "title": "Camadas de imóveis", "description": "Da entrada HTTP ao contexto de persistência.", "guide": "Responsabilidades separadas", "intro": "O serviço usa uma interface; a infraestrutura fornece a implementação.", "steps": ["ImoveisController recebe o pedido e o sinal de cancelamento.", "ImovelService valida filtros, coordena alterações e monta respostas.", "ImovelRepository realiza IImovelRepository e usa AppDbContext."], "scope": "CancellationToken é propagado nas operações. Parâmetros foram omitidos da figura para manter as assinaturas legíveis.", "doc": "classes-e-arquitetura.md"},
  {"id": "17-sequencia-criar-imovel", "group": "Imóveis", "title": "Cadastrar imóvel", "description": "Validação da entidade, gravação e resposta de criação.", "guide": "Criar um cadastro válido", "intro": "Siga as mensagens de cima para baixo.", "steps": ["O serviço constrói Imovel e o domínio valida os valores.", "O repositório executa AddAsync e SaveChangesAsync com o sinal de cancelamento.", "A API retorna 201 com ID ou 400 para dados rejeitados."], "scope": "O cabeçalho Location ainda não é retornado.", "doc": "sequencias.md"},
  {"id": "18-sequencia-listar-imoveis", "group": "Imóveis", "title": "Listar imóveis", "description": "Validação da página, consulta parcial e projeção da resposta.", "guide": "Como a paginação funciona", "intro": "Sem parâmetros, a API usa página 1 e tamanho 10.", "steps": ["Rejeitar página menor que 1 ou tamanho fora de 1 a 100.", "Aplicar filtros, ordenar por título e paginar com Skip/Take.", "Retornar uma lista de ImovelResponse, ou [] se vazia."], "scope": "Filtros e OrderBy(Titulo) implementados. Sem total ou desempate de títulos iguais.", "doc": "sequencias.md"},
  {id:'07-casos-uso-requisitos', group:'Reservas', title:'Operacoes de reservas', description:'Criacao, consulta paginada, confirmacao, conclusao, cancelamento e reembolso.', guide:'Ciclo da reserva', intro:'A vista mostra os objetivos disponiveis no controller de reservas.', steps:['Criar a reserva depois de validar cliente, imovel e periodo.','Consultar por ID ou listar por cliente, imóvel e status em páginas.','Cancelar e registrar o reembolso calculado.'], scope:'A regra detalhada esta em casos-de-uso.md.', doc:'casos-de-uso.md'},
  {id:'08-sequencia-requisito-criar-reserva', group:'Reservas', title:'Criar reserva', description:'Fluxo interno de validacao, desconto e persistencia.', guide:'Criar uma reserva', intro:'O fluxo e serializado pelo SemaphoreSlim durante a criacao.', steps:['Conferir cliente, limite, imovel, capacidade e conflito.','Calcular noites, desconto e valor total.','Criar como Pendente e salvar com CancellationToken.'], scope:'A API retorna 201 com o ID da reserva.', doc:'sequencias.md'},
  {id:'09-sequencia-requisito-cancelar-reserva', group:'Reservas', title:'Cancelar reserva', description:'Busca, calculo de reembolso, transicao e persistencia.', guide:'Cancelamento e reembolso', intro:'O estado da reserva determina se o cancelamento pode prosseguir.', steps:['Buscar a reserva e calcular a antecedencia usando UTC-3.','Calcular 100%, 50% ou 0% conforme a politica.','Salvar CanceladaEm e ValorReembolso.',], scope:'Reservas concluidas ou canceladas nao podem ser canceladas.', doc:'sequencias.md'},
  {id:'20-sequencia-listar-clientes', group:'Fluxos implementados', title:'Listar clientes', description:'Filtros de nome e status com paginação no SQLite.', guide:'Consulta por página', intro:'Sem parâmetros, a API retorna a página 1 com até 10 clientes.', steps:['Validar página, tamanho e status; valores inválidos retornam 400.','Filtrar nome e status, ordenar por nome e ID e aplicar Skip/Take no banco.','Projetar ClienteResponse e retornar 200 com lista ou [].'], scope:'Tamanho entre 1 e 100; sem total de registros na resposta.', doc:'sequencias.md'},
  {id:'21-sequencia-listar-reservas', group:'Reservas', title:'Listar reservas', description:'Filtros de cliente, imóvel e status com paginação no SQLite.', guide:'Consulta por página', intro:'Sem parâmetros, a API retorna a página 1 com até 10 reservas.', steps:['Validar página, tamanho e status; valores inválidos retornam 400.','Filtrar cliente, imóvel e status, ordenar por data de entrada e ID e aplicar Skip/Take no banco.','Projetar ReservaResponse e retornar 200 com lista ou [].'], scope:'Tamanho entre 1 e 100; sem filtro por período nem total de registros.', doc:'sequencias.md'},
  {id:'22-dtos-reservas', group:'Reservas', title:'DTOs de reservas', description:'Dados de criação, filtros da listagem e resposta.', guide:'Pedidos e resposta', intro:'ListarReservasRequest define filtros opcionais e paginação.', steps:['CriarReservaRequest recebe cliente, imóvel, datas e hóspedes.','ListarReservasRequest usa página 1 e tamanho 10 por padrão; filtra por cliente, imóvel e status.','ReservaResponse traz status, valor e dados de cancelamento.'], scope:'O período ainda não é filtro da listagem de reservas.', doc:'classes-e-arquitetura.md'}
];

const normalize = value => value.normalize('NFD').replace(/[\u0300-\u036f]/g,'').toLowerCase();
const matches = (view, query) => normalize([view.id,view.group,view.title,view.description].join(' ')).includes(normalize(query.trim()));
const fitScale = (width,height,availableWidth,availableHeight) => Math.max(.05,Math.min(1,(availableWidth-64)/width,(availableHeight-64)/height));
const clampScale = value => Math.max(.05,Math.min(4,value));

function initializeGallery() {
  const $ = id => document.getElementById(id);
  const image = $('diagram'), viewport = $('viewport');
  let current = 0, scale = 1, mode = 'fit', width = 1, height = 1, ready = false, drag = null;

  function buildNavigation() {
    const query = $('search').value;
    $('navigation').replaceChildren();
    let group = null, section = null, count = 0;
    views.forEach((view,index) => {
      if (!matches(view,query)) return;
      count++;
      if (group !== view.group) {
        group = view.group; section = document.createElement('section'); section.className = 'nav-group';
        const heading = document.createElement('h2'); heading.textContent = group;
        section.append(heading); $('navigation').append(section);
      }
      const link = document.createElement('a'); link.href = '#' + view.id; link.className = 'nav-link';
      const number = document.createElement('span'); number.className = 'nav-index'; number.textContent = view.id.slice(0,2);
      const label = document.createElement('span'); label.textContent = view.title;
      link.append(number,label);
      if (view.pending) { const dot = document.createElement('span'); dot.className = 'pending-dot'; dot.setAttribute('aria-hidden','true'); link.append(dot); }
      if (index === current) link.setAttribute('aria-current','page');
      section.append(link);
    });
    $('empty').hidden = count !== 0;
  }

  function drawScale(next,center = true) {
    if (!ready) return;
    const oldRect = image.getBoundingClientRect(), viewRect = viewport.getBoundingClientRect();
    const fx = (viewRect.left + viewport.clientWidth/2 - oldRect.left)/(width*scale);
    const fy = (viewRect.top + viewport.clientHeight/2 - oldRect.top)/(height*scale);
    scale = clampScale(next);
    image.style.width = Math.round(width*scale) + 'px'; image.style.height = Math.round(height*scale) + 'px';
    $('zoom-value').textContent = Math.round(scale*100) + '%';
    $('zoom-out').disabled = scale <= .05; $('zoom-in').disabled = scale >= 4;
    if (center) {
      const newRect = image.getBoundingClientRect();
      viewport.scrollLeft += newRect.left + fx*width*scale - viewRect.left - viewport.clientWidth/2;
      viewport.scrollTop += newRect.top + fy*height*scale - viewRect.top - viewport.clientHeight/2;
    } else { viewport.scrollTop = 0; viewport.scrollLeft = 0; }
  }
  function fit() { mode = 'fit'; drawScale(fitScale(width,height,viewport.clientWidth,viewport.clientHeight),false); }
  function zoom(factor) { mode = 'manual'; drawScale(scale*factor); }
  function expand(force) {
    const enabled = force === undefined ? !$('reader').classList.contains('expanded') : force;
    $('reader').classList.toggle('expanded',enabled); document.body.classList.toggle('is-expanded',enabled);
    $('expand').setAttribute('aria-pressed',String(enabled)); $('expand').textContent = enabled ? 'Recolher ↙' : 'Expandir ↗';
    requestAnimationFrame(() => { if(mode === 'fit') fit(); });
  }
  function selectFromHash() {
    const id = location.hash.slice(1), found = views.findIndex(view => view.id === id);
    current = found < 0 ? 0 : found;
    const view = views[current]; ready = false; mode = 'fit'; image.style.visibility = 'hidden';
    $('category').textContent = view.group; $('eyebrow').textContent = `VISTA ${view.id.slice(0,2)} · ${view.group.toUpperCase()}`;
    $('title').textContent = view.title; $('description').textContent = view.description;
    $('status').textContent = view.pending ? 'Requisito · não concluído' : 'Implementado'; $('status').classList.toggle('pending',!!view.pending);
    $('filename').textContent = view.id + '.svg'; $('notes-title').textContent = view.guide; $('notes-intro').textContent = view.intro;
    $('reading-steps').replaceChildren(...view.steps.map(text => { const li=document.createElement('li'); li.textContent=text; return li; }));
    $('scope-note').textContent = view.scope; $('scope-note').classList.toggle('pending',!!view.pending);
    $('detail-link').href = '../' + view.doc;
    $('svg-link').href = `imagens/${view.id}.svg`; $('png-link').href = `imagens/${view.id}.png`; $('source-link').href = `fontes/${view.id}.puml`;
    $('position').textContent = `${String(current+1).padStart(2,'0')} / ${views.length}`;
    $('previous').disabled = current === 0; $('next').disabled = current === views.length-1;
    $('notes').scrollTop = 0; $('image-error').hidden = true;
    image.alt = `Diagrama UML: ${view.title}${view.pending ? ' — requisito não concluído' : ''}`;
    image.src = `imagens/${view.id}.svg`;
    document.title = `${view.title} · UML`; buildNavigation();
    if (image.complete && image.naturalWidth) onImageLoad();
  }
  function onImageLoad() {
    width = image.naturalWidth; height = image.naturalHeight;
    if (!width || !height) return;
    ready = true; image.style.visibility = 'visible'; fit();
  }
  function move(direction) { const next = current + direction; if (next>=0 && next<views.length) location.hash=views[next].id; }
  image.addEventListener('load',onImageLoad);
  image.addEventListener('error',() => { ready=false; $('image-error').hidden=false; });
  $('search').addEventListener('input',buildNavigation);
  $('zoom-in').addEventListener('click',() => zoom(1.25)); $('zoom-out').addEventListener('click',() => zoom(.8));
  $('fit').addEventListener('click',fit); $('actual').addEventListener('click',() => {mode='manual'; drawScale(1);});
  $('expand').addEventListener('click',() => expand());
  $('notes-toggle').addEventListener('click',() => { $('notes').hidden=!$('notes').hidden; $('notes-toggle').setAttribute('aria-expanded',String(!$('notes').hidden)); requestAnimationFrame(() => {if(mode==='fit') fit();}); });
  $('previous').addEventListener('click',() => move(-1)); $('next').addEventListener('click',() => move(1));
  window.addEventListener('hashchange',selectFromHash);
  window.addEventListener('resize',() => { if(mode==='fit') fit(); });
  if (typeof ResizeObserver !== 'undefined') new ResizeObserver(() => {if(mode==='fit') fit();}).observe(viewport);
  viewport.addEventListener('pointerdown',event => {
    if (event.button!==0 || event.pointerType==='touch') return;
    drag={x:event.clientX,y:event.clientY,left:viewport.scrollLeft,top:viewport.scrollTop};
    viewport.setPointerCapture(event.pointerId); viewport.classList.add('dragging'); event.preventDefault();
  });
  viewport.addEventListener('pointermove',event => {if(drag){viewport.scrollLeft=drag.left+drag.x-event.clientX;viewport.scrollTop=drag.top+drag.y-event.clientY;}});
  function stopDrag(){drag=null;viewport.classList.remove('dragging');}
  viewport.addEventListener('pointerup',stopDrag); viewport.addEventListener('pointercancel',stopDrag); viewport.addEventListener('lostpointercapture',stopDrag);
  document.addEventListener('keydown',event => {
    if (event.target.matches('input,textarea,select,[contenteditable="true"]') || event.ctrlKey || event.metaKey || event.altKey) return;
    if(event.key==='/'){event.preventDefault();$('search').focus();}
    else if(event.key==='+' || event.key==='='){event.preventDefault();zoom(1.25);}
    else if(event.key==='-'){event.preventDefault();zoom(.8);}
    else if(event.key.toLowerCase()==='f'){event.preventDefault();fit();}
    else if(event.key==='1'){event.preventDefault();mode='manual';drawScale(1);}
    else if(event.key==='Escape'){expand(false);}
  });
  $('total').textContent = views.length;
  selectFromHash();
}

if (typeof module !== 'undefined' && module.exports) module.exports={views,matches,fitScale,clampScale};
if (typeof document !== 'undefined') initializeGallery();
