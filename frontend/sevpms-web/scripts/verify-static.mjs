import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { createRequire } from 'node:module';

const here = path.dirname(fileURLToPath(import.meta.url));
const root = path.resolve(here, '..');
const appRoot = path.join(root, 'src', 'app');
const require = createRequire(import.meta.url);
const ts = require('typescript');
const failures = [];
const notes = [];
const routeFile = path.join(appRoot, 'app.routes.ts');
const sidebarFile = path.join(appRoot, 'shared', 'components', 'app-sidebar.component.ts');

// 0) Angular application entry/configuration files must point to real files.
const angularConfigPath = path.join(root, 'angular.json');
const angularConfig = JSON.parse(fs.readFileSync(angularConfigPath, 'utf8'));
const project = angularConfig.projects?.['nvent-web'];
const buildOptions = project?.architect?.build?.options ?? {};
const requiredConfiguredFiles = [
  ['browser entry', buildOptions.browser],
  ['TypeScript config', buildOptions.tsConfig],
  ...((buildOptions.styles ?? []).map((value) => ['global style', value])),
];
for (const [label, configuredPath] of requiredConfiguredFiles) {
  if (!configuredPath || !fs.existsSync(path.join(root, configuredPath))) {
    failures.push(`Angular ${label} is missing: ${configuredPath ?? '(not configured)'}`);
  } else {
    notes.push(`Angular ${label} exists: ${configuredPath}.`);
  }
}
const indexFile = path.join(root, 'src', 'index.html');
if (!fs.existsSync(indexFile)) failures.push('Angular index file is missing: src/index.html');
else notes.push('Angular index file exists: src/index.html.');
const publicDir = path.join(root, 'public');
if (!fs.existsSync(publicDir) || !fs.statSync(publicDir).isDirectory()) failures.push('Angular public asset directory is missing: public/');
else notes.push('Angular public asset directory exists: public/.');

function walk(dir, out = []) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) walk(full, out);
    else out.push(full);
  }
  return out;
}

const sourceFiles = walk(appRoot).filter(f => /\.(ts|html)$/.test(f));

// 0b) Parse every TypeScript source with the TypeScript compiler so syntax errors are caught even without Angular dependencies.
const tsFilesForParse = walk(appRoot).filter(f => f.endsWith('.ts'));
let parsedTsFiles = 0;
let tsSyntaxErrors = 0;
for (const file of tsFilesForParse) {
  const source = fs.readFileSync(file, 'utf8');
  const result = ts.transpileModule(source, {
    compilerOptions: { target: ts.ScriptTarget.ES2022, module: ts.ModuleKind.ESNext },
    fileName: file,
    reportDiagnostics: true
  });
  const diagnostics = (result.diagnostics ?? []).filter(d => d.category === ts.DiagnosticCategory.Error);
  if (diagnostics.length) {
    tsSyntaxErrors += diagnostics.length;
    for (const d of diagnostics) {
      const message = ts.flattenDiagnosticMessageText(d.messageText, ' ');
      const pos = d.file && typeof d.start === 'number' ? d.file.getLineAndCharacterOfPosition(d.start) : null;
      const where = pos ? `:${pos.line + 1}:${pos.character + 1}` : '';
      failures.push(`TypeScript parse error in ${path.relative(root, file)}${where}: ${message}`);
    }
  } else {
    parsedTsFiles++;
  }
}
notes.push(`${parsedTsFiles}/${tsFilesForParse.length} TypeScript files parsed; ${tsSyntaxErrors} syntax diagnostics.`);


// 0c) Resolve every relative TypeScript import/export target, not only route-level lazy imports.
let relativeImportCount = 0;
let unresolvedRelativeImports = 0;
const resolveLocalModule = (fromFile, specifier) => {
  const base = path.resolve(path.dirname(fromFile), specifier);
  const candidates = [
    base,
    `${base}.ts`,
    `${base}.tsx`,
    `${base}.html`,
    `${base}.css`,
    path.join(base, 'index.ts'),
  ];
  return candidates.some(candidate => fs.existsSync(candidate));
};
for (const file of tsFilesForParse) {
  const sourceText = fs.readFileSync(file, 'utf8');
  const sourceFile = ts.createSourceFile(file, sourceText, ts.ScriptTarget.Latest, true, ts.ScriptKind.TS);
  const inspectModule = (node) => {
    let specifier = null;
    if ((ts.isImportDeclaration(node) || ts.isExportDeclaration(node)) && node.moduleSpecifier && ts.isStringLiteralLike(node.moduleSpecifier)) {
      specifier = node.moduleSpecifier.text;
    } else if (ts.isCallExpression(node) && node.expression.kind === ts.SyntaxKind.ImportKeyword && node.arguments.length === 1 && ts.isStringLiteralLike(node.arguments[0])) {
      specifier = node.arguments[0].text;
    }
    if (specifier?.startsWith('.')) {
      relativeImportCount++;
      if (!resolveLocalModule(file, specifier)) {
        unresolvedRelativeImports++;
        failures.push(`Unresolved relative import in ${path.relative(root, file)}: ${specifier}`);
      }
    }
    ts.forEachChild(node, inspectModule);
  };
  inspectModule(sourceFile);
}
notes.push(`${relativeImportCount} relative TypeScript imports/exports checked; ${unresolvedRelativeImports} unresolved.`);

// 0d) Prevent explicit TypeScript `any` from creeping into application code.
let explicitAnyCount = 0;
for (const file of tsFilesForParse) {
  const sourceText = fs.readFileSync(file, 'utf8');
  const sourceFile = ts.createSourceFile(file, sourceText, ts.ScriptTarget.Latest, true, ts.ScriptKind.TS);
  const inspectAny = (node) => {
    if (node.kind === ts.SyntaxKind.AnyKeyword) {
      explicitAnyCount++;
      const pos = sourceFile.getLineAndCharacterOfPosition(node.getStart(sourceFile));
      failures.push(`Explicit TypeScript any in ${path.relative(root, file)}:${pos.line + 1}:${pos.character + 1}`);
    }
    ts.forEachChild(node, inspectAny);
  };
  inspectAny(sourceFile);
}
notes.push(`${explicitAnyCount} explicit TypeScript any usages.`);

// 0e) Feature/domain services should expose typed request bodies. The generic ApiService transport may accept unknown safely.
let untypedServiceBodies = 0;
for (const file of tsFilesForParse.filter(file => file.includes(`${path.sep}core${path.sep}services${path.sep}`) && !file.endsWith(`${path.sep}api.service.ts`))) {
  const sourceText = fs.readFileSync(file, 'utf8');
  if (/\bbody\s*:\s*unknown\b/.test(sourceText)) {
    untypedServiceBodies++;
    failures.push(`Untyped service request body in ${path.relative(root, file)}`);
  }
}
notes.push(`${untypedServiceBodies} feature/domain service request bodies typed as unknown.`);
const routeSource = fs.readFileSync(routeFile, 'utf8');
const sidebarSource = fs.readFileSync(sidebarFile, 'utf8');

// 1) Every lazy route import must resolve to a TypeScript source file.
const imports = [...routeSource.matchAll(/import\(['"](.+?)['"]\)/g)].map(m => m[1]);
let resolvedImports = 0;
for (const rel of imports) {
  const resolved = path.resolve(path.dirname(routeFile), `${rel}.ts`);
  if (!fs.existsSync(resolved)) failures.push(`Missing lazy-route component source: ${rel}.ts`);
  else resolvedImports++;
}
notes.push(`${resolvedImports}/${imports.length} lazy-route imports resolved.`);

// 2) Sidebar/menu paths must correspond to route segments declared in app.routes.ts.
const menuPaths = [...sidebarSource.matchAll(/path:'(\/[^']+)'/g)].map(m => m[1]);
const uniqueMenuPaths = [...new Set(menuPaths)];
for (const menuPath of uniqueMenuPaths) {
  const parts = menuPath.split('/').filter(Boolean);
  if (!parts.every(segment => routeSource.includes(`path: '${segment}'`) || /^\w+-?\w*$/.test(segment) && routeSource.includes(`path: '${segment}`))) {
    failures.push(`Sidebar route could not be matched to app routes: ${menuPath}`);
  }
}
notes.push(`${uniqueMenuPaths.length} unique sidebar/bottom-nav paths checked.`);

// 3) Static form-control accessibility audit for inline/external templates.
let controlCount = 0;
let unnamedControls = 0;
for (const file of sourceFiles) {
  const text = fs.readFileSync(file, 'utf8');
  const templateText = file.endsWith('.html') ? text : [...text.matchAll(/template\s*:\s*`([\s\S]*?)`/g)].map(m => m[1]).join('\n');
  if (!templateText) continue;
  for (const match of templateText.matchAll(/<(input|select|textarea)\b([^>]*)>/gi)) {
    controlCount++;
    const attrs = match[2];
    if (/type\s*=\s*["']hidden["']/i.test(attrs)) continue;
    const named = /aria-label(?:ledby)?\s*=|\[attr\.aria-label\]|\[attr\.aria-labelledby\]|\bid\s*=/.test(attrs);
    if (!named) {
      unnamedControls++;
      failures.push(`Potential unnamed ${match[1]} in ${path.relative(root, file)}`);
    }
  }
}
notes.push(`${controlCount} form controls scanned; ${unnamedControls} potential unnamed controls.`);

// 4) Flag obvious developer-facing copy in rendered templates only.
const bannedVisible = [/\bbackend\b/i, /\bAPI endpoint\b/i, /\bfrontend specification\b/i];
let visibleCopyFlags = 0;
for (const file of sourceFiles) {
  const text = fs.readFileSync(file, 'utf8');
  const templateText = file.endsWith('.html') ? text : [...text.matchAll(/template\s*:\s*`([\s\S]*?)`/g)].map(m => m[1]).join('\n');
  if (!templateText) continue;
  for (const pattern of bannedVisible) {
    if (pattern.test(templateText)) {
      visibleCopyFlags++;
      failures.push(`Developer-facing copy (${pattern}) found in ${path.relative(root, file)}`);
    }
  }
}
notes.push(`${visibleCopyFlags} developer-copy flags in rendered templates.`);


// 4b) Approved Nvent UI intentionally avoids decorative marketing slogans/filler copy.
const bannedSlogans = [
  /good experiences[^<\n]*brighter days/i,
  /making moments happen/i,
  /good events[^<\n]*brighter days/i,
  /more than events/i,
  /good food[^<\n]*brighter/i,
  /good food brings people together/i,
  /explore\s*[•·|.-]\s*experience\s*[•·|.-]\s*belong/i,
];
let sloganFlags = 0;
for (const file of sourceFiles) {
  const text = fs.readFileSync(file, 'utf8');
  const templateText = file.endsWith('.html') ? text : [...text.matchAll(/template\s*:\s*`([\s\S]*?)`/g)].map(m => m[1]).join('\n');
  if (!templateText) continue;
  for (const pattern of bannedSlogans) {
    if (pattern.test(templateText)) {
      sloganFlags++;
      failures.push(`Disallowed decorative slogan found in ${path.relative(root, file)}`);
    }
  }
}
notes.push(`${sloganFlags} disallowed decorative-slogan flags.`);

// 5) Production UI should use the shared confirmation/status surfaces, not browser confirm/alert popups.
let browserDialogFlags=0;
for (const file of sourceFiles.filter(f=>f.endsWith('.ts') && !f.endsWith('confirm-dialog.component.ts'))) {
  const text=fs.readFileSync(file,'utf8');
  if (/\b(?:window\.)?confirm\s*\(/.test(text) || /\b(?:window\.)?alert\s*\(/.test(text)) {
    browserDialogFlags++;
    failures.push(`Native browser confirm/alert found in ${path.relative(root,file)}`);
  }
}
notes.push(`${browserDialogFlags} native browser confirm/alert usages.`);


// 6) Catch a small set of Angular-template expression mistakes that TypeScript parsing cannot see.
// This does not replace ng build; it prevents known silent inline-template typos from escaping source review.
let templateSyntaxFlags = 0;
const suspiciousTemplatePatterns = [
  { re: /\?\.(?=\s*[0-9])/g, label: 'optional chaining followed by a numeric literal (likely malformed ternary)' },
];
for (const file of sourceFiles) {
  const text = fs.readFileSync(file, 'utf8');
  const templateText = file.endsWith('.html') ? text : [...text.matchAll(/template\s*:\s*`([\s\S]*?)`/g)].map(m => m[1]).join('\n');
  if (!templateText) continue;
  for (const { re, label } of suspiciousTemplatePatterns) {
    re.lastIndex = 0;
    if (re.test(templateText)) {
      templateSyntaxFlags++;
      failures.push(`Suspicious Angular template expression (${label}) in ${path.relative(root, file)}`);
    }
  }
}
notes.push(`${templateSyntaxFlags} suspicious Angular-template expression flags.`);


// 7) Every button gets an explicit type; duplicate type attributes are invalid/ambiguous markup.
let buttonTypeFlags = 0;
let buttonCount = 0;
for (const file of sourceFiles) {
  const text = fs.readFileSync(file, 'utf8');
  const templateText = file.endsWith('.html') ? text : [...text.matchAll(/template\s*:\s*`([\s\S]*?)`/g)].map(m => m[1]).join('\n');
  if (!templateText) continue;
  for (const match of templateText.matchAll(/<button\b([^>]*)>/gi)) {
    buttonCount++;
    const attrs = match[1];
    const types = [...attrs.matchAll(/\btype\s*=\s*["'][^"']+["']/gi)];
    if (types.length !== 1) {
      buttonTypeFlags++;
      failures.push(`Button must have exactly one explicit type in ${path.relative(root, file)}`);
    }
  }
}
notes.push(`${buttonCount} buttons scanned; ${buttonTypeFlags} button-type flags.`);


// 8) Angular templates do not expose ordinary JavaScript globals unless a component deliberately exposes them.
let templateGlobalFlags = 0;
const templateGlobals = ['String','Number','Math','Date','Object','Array','JSON'];
for (const file of sourceFiles) {
  const text = fs.readFileSync(file, 'utf8');
  const templateText = file.endsWith('.html') ? text : [...text.matchAll(/template\s*:\s*`([\s\S]*?)`/g)].map(m => m[1]).join('\n');
  if (!templateText) continue;
  for (const name of templateGlobals) {
    const used = new RegExp(`\\b${name}(?:\\.|\\()`).test(templateText);
    if (!used) continue;
    const explicitlyExposed = new RegExp(`\\b${name}\\s*=\\s*${name}\\b`).test(text);
    if (!explicitlyExposed) {
      templateGlobalFlags++;
      failures.push(`Template uses JavaScript global ${name} without exposing it on the component in ${path.relative(root, file)}`);
    }
  }
}
notes.push(`${templateGlobalFlags} unexposed JavaScript-global template usages.`);

console.log('Nvent static verification');
for (const note of notes) console.log(`  ✓ ${note}`);
if (failures.length) {
  console.error(`\nFAILED (${failures.length})`);
  for (const item of failures) console.error(`  - ${item}`);
  process.exitCode = 1;
} else {
  console.log('\nPASS: all static checks passed.');
}
