const api = '/api/avaliacoes';
const formulario = document.querySelector('#avaliacao');
const tipo = document.querySelector('#tipo');
const lista = document.querySelector('#lista');
const mensagem = document.querySelector('#mensagem');
const dialogo = document.querySelector('#dialogo');
const formularioRecuperacao = document.querySelector('#recuperacao');
const notaRecuperacao = document.querySelector('#nota-recuperacao');
const erroRecuperacao = document.querySelector('#erro-recuperacao');
const numero = new Intl.NumberFormat('pt-BR', { maximumFractionDigits: 2 });
let edicao = null;
let provaEmRecuperacao = null;

async function requisitar(url, metodo = 'GET', dados) {
  const opcoes = { method: metodo };
  if (dados !== undefined) {
    opcoes.headers = { 'Content-Type': 'application/json' };
    opcoes.body = JSON.stringify(dados);
  }
  const resposta = await fetch(url, opcoes);
  if (!resposta.ok) {
    const problema = await resposta.json().catch(() => null);
    const erros = Object.values(problema?.errors ?? {}).flat();
    throw new Error(erros[0] ?? problema?.title ?? 'Não foi possível concluir a operação.');
  }
  return resposta.status === 204 ? null : resposta.json();
}

function informar(texto, erro = false) {
  mensagem.textContent = texto;
  mensagem.classList.toggle('erro', erro);
}

function atualizarCampos() {
  for (const grupo of formulario.querySelectorAll('fieldset')) {
    grupo.hidden = grupo.disabled = grupo.dataset.tipo !== tipo.value;
  }
}

function prepararFormulario(avaliacao = null) {
  edicao = avaliacao?.id ?? null;
  formulario.reset();
  if (avaliacao) {
    for (const campo of formulario.querySelectorAll('[name]')) {
      campo.value = avaliacao[campo.name] ?? '';
    }
  }
  atualizarCampos();
  document.querySelector('#titulo-formulario').textContent = avaliacao ? 'Editar avaliação' : 'Nova avaliação';
  document.querySelector('#salvar').textContent = avaliacao ? 'Salvar alterações' : 'Cadastrar';
  document.querySelector('#cancelar').hidden = !avaliacao;
}

function elemento(tag, texto, classe) {
  const no = document.createElement(tag);
  if (texto) no.textContent = texto;
  if (classe) no.className = classe;
  return no;
}

function botao(texto, acao, classe = 'secundario') {
  const controle = elemento('button', texto, classe);
  controle.type = 'button';
  controle.addEventListener('click', acao);
  return controle;
}

function detalhes(avaliacao) {
  switch (avaliacao.tipo) {
    case 'Prova': {
      const base = `Prova · ${avaliacao.acertos}/${avaliacao.total} acertos`;
      return avaliacao.notaRecuperacao == null ? base :
        `${base} · Recuperação ${numero.format(avaliacao.notaRecuperacao)}`;
    }
    case 'Trabalho':
      return `Trabalho · Conteúdo ${numero.format(avaliacao.conteudo)} · Apresentação ${numero.format(avaliacao.apresentacao)}`;
    case 'Participacao':
      return `Participação · ${avaliacao.presencas}/${avaliacao.aulas} presenças`;
  }
}

function abrirRecuperacao(avaliacao) {
  provaEmRecuperacao = avaliacao.id;
  document.querySelector('#prova-recuperacao').textContent = avaliacao.titulo;
  notaRecuperacao.value = avaliacao.notaRecuperacao ?? '';
  erroRecuperacao.textContent = '';
  dialogo.showModal();
}

function cartao(avaliacao) {
  const item = elemento('li');
  const resumo = elemento('div', '', 'resumo');
  resumo.append(elemento('h3', avaliacao.titulo), elemento('strong', `Nota ${numero.format(avaliacao.nota)}`, 'nota'));
  const acoes = elemento('div', '', 'acoes');
  acoes.append(botao('Editar', () => {
    prepararFormulario(avaliacao);
    formulario.elements.titulo.focus();
  }));
  if (avaliacao.tipo === 'Prova') acoes.append(botao('Recuperação', () => abrirRecuperacao(avaliacao)));
  acoes.append(botao('Excluir', async (evento) => {
    if (!confirm(`Excluir “${avaliacao.titulo}”?`)) return;
    const controle = evento.currentTarget;
    controle.disabled = true;
    try {
      await requisitar(`${api}/${avaliacao.id}`, 'DELETE');
      if (edicao === avaliacao.id) prepararFormulario();
      informar('Avaliação excluída.');
      await carregar();
    } catch (erro) {
      informar(erro.message, true);
    } finally {
      controle.disabled = false;
    }
  }, 'secundario excluir'));
  item.append(resumo, elemento('p', detalhes(avaliacao), 'detalhes'), acoes);
  return item;
}

async function carregar() {
  const contagem = document.querySelector('#contagem');
  try {
    const avaliacoes = await requisitar(api);
    lista.replaceChildren(...avaliacoes.map(cartao));
    contagem.textContent = avaliacoes.length === 0 ? 'Nenhuma avaliação cadastrada.' :
      `${avaliacoes.length} ${avaliacoes.length === 1 ? 'avaliação' : 'avaliações'}`;
  } catch (erro) {
    contagem.textContent = 'Lista indisponível.';
    informar(erro.message, true);
  }
}

formulario.addEventListener('submit', async (evento) => {
  evento.preventDefault();
  const id = edicao;
  const dados = Object.fromEntries(new FormData(formulario));
  dados.titulo = dados.titulo.trim();
  for (const campo of Object.keys(dados)) {
    if (campo !== 'titulo' && campo !== 'tipo') dados[campo] = Number(dados[campo]);
  }
  const salvar = document.querySelector('#salvar');
  salvar.disabled = true;
  try {
    await requisitar(id === null ? api : `${api}/${id}`, id === null ? 'POST' : 'PUT', dados);
    prepararFormulario();
    informar(id === null ? 'Avaliação cadastrada.' : 'Avaliação atualizada.');
    await carregar();
  } catch (erro) {
    informar(erro.message, true);
  } finally {
    salvar.disabled = false;
  }
});

formularioRecuperacao.addEventListener('submit', async (evento) => {
  evento.preventDefault();
  const salvar = formularioRecuperacao.querySelector('[type="submit"]');
  salvar.disabled = true;
  try {
    await requisitar(`${api}/${provaEmRecuperacao}/recuperacao`, 'PUT', { nota: notaRecuperacao.valueAsNumber });
    dialogo.close();
    informar('Recuperação registrada.');
    await carregar();
  } catch (erro) {
    erroRecuperacao.textContent = erro.message;
  } finally {
    salvar.disabled = false;
  }
});

tipo.addEventListener('change', atualizarCampos);
document.querySelector('#cancelar').addEventListener('click', () => prepararFormulario());
document.querySelector('#cancelar-recuperacao').addEventListener('click', () => dialogo.close());
carregar();
