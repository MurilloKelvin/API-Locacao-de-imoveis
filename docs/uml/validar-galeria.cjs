// Validação local de dados, links e cálculos; não substitui teste visual no navegador.
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const {views,matches,fitScale,clampScale} = require('./galeria.js');
const root = __dirname;
const sources=fs.readdirSync(path.join(root,"fontes")).filter(name=>name.endsWith(".puml"));
assert.equal(views.length,sources.length);
assert.equal(new Set(views.map(view=>view.id)).size,views.length);
assert.deepEqual(views.map(view=>view.id+".puml").sort(),sources.sort());
for (const view of views) {
  for (const relative of [`imagens/${view.id}.svg`,`imagens/${view.id}.png`,`fontes/${view.id}.puml`,`../${view.doc}`]) {
    assert.ok(fs.existsSync(path.resolve(root,relative)),relative);
  }
  assert.ok(view.steps.length >= 3 && view.steps.length <= 4);
  assert.ok(view.scope && view.description && view.guide);
}
assert.ok(matches(views.find(view=>view.id==='02-classes-dominio'),'DOMINIO'));
assert.ok(matches(views.find(view=>view.id==='14-classes-infrastructure'),'persistencia'));
assert.equal(views.filter(view=>matches(view,'inexistenteXYZ')).length,0);
assert.equal(views.filter(view=>matches(view,'')).length,views.length);
// Tanto a largura quanto a altura precisam caber, sem ampliar além do tamanho original.
for (const [w,h,vw,vh] of [[1600,900,900,600],[500,1500,700,600],[300,200,900,700]]) {
  const scale=fitScale(w,h,vw,vh);
  assert.ok(w*scale <= vw-64+.001);
  assert.ok(h*scale <= vh-64+.001);
  assert.ok(scale<=1);
}
assert.equal(clampScale(10),4);
assert.equal(clampScale(-1),.05);
const html=fs.readFileSync(path.join(root,'galeria.html'),'utf8');
const js=fs.readFileSync(path.join(root,'galeria.js'),'utf8');
const ids=[...html.matchAll(/\bid="([^"]+)"/g)].map(match=>match[1]);
assert.equal(ids.length,new Set(ids).size,'IDs HTML duplicados');
for(const match of js.matchAll(/\$\('([^']+)'\)/g)) assert.ok(ids.includes(match[1]),`ID ausente: ${match[1]}`);
for(const match of html.matchAll(/\b(?:href|src)="([^"]+)"/g)) {
  const target=match[1];
  if(target.startsWith('#')) assert.ok(ids.includes(target.slice(1)) || views.some(view=>view.id===target.slice(1)),target);
  else assert.ok(fs.existsSync(path.resolve(root,target)),target);
}
console.log(`OK: ${views.length} vistas, fontes e imagens correspondentes, busca, zoom e referências HTML/JS.`);
