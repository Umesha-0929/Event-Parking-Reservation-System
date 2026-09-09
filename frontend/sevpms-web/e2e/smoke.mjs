import { spawn } from 'node:child_process';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const frontendRoot = resolve(here, '..');
const angularCli = resolve(frontendRoot, 'node_modules', '@angular', 'cli', 'bin', 'ng.js');

// Run Angular CLI through the current Node executable instead of spawning npm.cmd.
// This is cross-platform and avoids Windows spawn EINVAL errors.
const child = spawn(
  process.execPath,
  [angularCli, 'serve', '--proxy-config', 'proxy.conf.json', '--host', '127.0.0.1', '--port', '4200'],
  {
    cwd: frontendRoot,
    stdio: ['ignore', 'pipe', 'pipe'],
    env: { ...process.env, BROWSER: 'none' },
  },
);

let spawnError = null;
child.on('error', (error) => {
  spawnError = error;
});
child.stdout.on('data', (data) => process.stdout.write(data));
child.stderr.on('data', (data) => process.stderr.write(data));

const sleep = (ms) => new Promise((resolveSleep) => setTimeout(resolveSleep, ms));

async function waitFor(url) {
  let lastError;
  for (let attempt = 0; attempt < 90; attempt += 1) {
    if (spawnError) throw spawnError;
    if (child.exitCode !== null) {
      throw new Error(`Angular dev server exited before E2E checks completed (exit code ${child.exitCode}).`);
    }

    try {
      const response = await fetch(url);
      if (response.ok) return response;
      lastError = new Error(`${url} returned ${response.status}`);
    } catch (error) {
      lastError = error;
    }
    await sleep(1000);
  }
  throw lastError ?? new Error(`Timed out waiting for ${url}`);
}

async function stopChild() {
  if (child.exitCode !== null) return;

  child.kill('SIGTERM');
  await Promise.race([
    new Promise((resolveExit) => child.once('exit', resolveExit)),
    sleep(3000),
  ]);

  if (child.exitCode === null) {
    child.kill('SIGKILL');
  }
}

try {
  const home = await waitFor('http://127.0.0.1:4200/');
  const homeText = await home.text();
  if (!homeText.includes('Nvent')) throw new Error('Home page did not render the Nvent shell.');

  const login = await fetch('http://127.0.0.1:4200/login');
  if (!login.ok) throw new Error(`/login returned ${login.status}`);

  const events = await fetch('http://127.0.0.1:4200/events');
  if (!events.ok) throw new Error(`/events returned ${events.status}`);

  console.log('E2E smoke passed: /, /login and /events rendered successfully.');
} finally {
  await stopChild();
}
