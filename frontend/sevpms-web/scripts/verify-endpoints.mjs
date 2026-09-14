import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const here=path.dirname(fileURLToPath(import.meta.url));
const root=path.resolve(here,'..');
const serviceDir=path.join(root,'src','app','core','services');
const controllerDir=path.resolve(root,'../../backend/src/SEVPMS.Api/Controllers');

function walk(dir,out=[]){for(const e of fs.readdirSync(dir,{withFileTypes:true})){const p=path.join(dir,e.name);e.isDirectory()?walk(p,out):out.push(p);}return out;}
function normRoute(route){return route
  .replace(/^\/?api\//,'')
  .replace(/\?.*$/,'')
  .replace(/\$\{[^}]+\}/g,'{param}')
  .replace(/\{[^}:]+(?::[^}]+)?\}/g,'{param}')
  .replace(/^\/+|\/+$/g,'')
  .replace(/\/+/g,'/');}

const backend=[];
for(const file of walk(controllerDir).filter(f=>f.endsWith('.cs'))){
  const text=fs.readFileSync(file,'utf8');
  const classMatch=text.match(/\[Route\("([^"]+)"\)\][\s\S]{0,450}?class\s+\w+Controller/);
  const prefix=classMatch?.[1]??'';
  for(const m of text.matchAll(/\[Http(Get|Post|Put|Delete|Patch)(?:\("([^"]*)"\))?\]/g)){
    const verb=m[1].toUpperCase(); const sub=m[2]??'';
    let full='';
    if(sub.startsWith('api/') || sub.startsWith('/api/') || sub.startsWith('~/api/')) full=sub.replace(/^~?\//,'');
    else full=[prefix,sub].filter(Boolean).map(v=>v.replace(/^\/+|\/+$/g,'')).join('/');
    backend.push({verb,route:normRoute(full),file:path.basename(file)});
  }
}

const frontend=[];
for(const file of walk(serviceDir).filter(f=>f.endsWith('.ts'))){
  const text=fs.readFileSync(file,'utf8');
  // ApiService wrapper calls.
  for(const m of text.matchAll(/this\.api\.(get|post|put|patch|delete|blob)<[^>]*>?\s*\(\s*([`'"])([\s\S]*?)\2/g)){
    frontend.push({verb:m[1]==='blob'?'GET':m[1].toUpperCase(),route:normRoute(m[3]),file:path.basename(file),raw:m[3]});
  }
  for(const m of text.matchAll(/this\.api\.(get|post|put|patch|delete|blob)\s*\(\s*([`'"])([\s\S]*?)\2/g)){
    frontend.push({verb:m[1]==='blob'?'GET':m[1].toUpperCase(),route:normRoute(m[3]),file:path.basename(file),raw:m[3]});
  }
  // Direct HttpClient calls based on environment.apiBaseUrl.
  for(const m of text.matchAll(/this\.http\.(get|post|put|patch|delete)(?:<[^>]+>)?\s*\(\s*`\$\{environment\.apiBaseUrl\}\/([^`]+)`/g)){
    frontend.push({verb:m[1].toUpperCase(),route:normRoute(m[2]),file:path.basename(file),raw:m[2]});
  }
  // Network health uses a constructed url; record explicitly if present.
  if(text.includes("}/health`")||text.includes("}/health'")) frontend.push({verb:'GET',route:'health',file:path.basename(file),raw:'health'});
}

function samePattern(a,b){
  const aa=a.split('/'),bb=b.split('/');
  if(aa.length!==bb.length)return false;
  return aa.every((x,i)=>x===bb[i]||x==='{param}'||bb[i]==='{param}');
}
const uniqueFront=[...new Map(frontend.map(x=>[`${x.verb} ${x.route}`,x])).values()];
const missing=[];
for(const f of uniqueFront){
  if(!backend.some(b=>b.verb===f.verb&&samePattern(b.route,f.route)))missing.push(f);
}
console.log('Nvent frontend/backend endpoint verification');
console.log(`  Backend controller actions indexed: ${backend.length}`);
console.log(`  Frontend service operations checked: ${uniqueFront.length}`);
if(missing.length){
  console.error(`\nUNMATCHED (${missing.length})`);
  for(const m of missing)console.error(`  - ${m.verb} ${m.raw} (${m.file})`);
  process.exitCode=1;
}else console.log('\nPASS: every detected frontend service operation matches a backend controller route.');
