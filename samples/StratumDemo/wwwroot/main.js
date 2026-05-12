// main.js — Stratum bootstrap (DO NOT MODIFY in app projects)
import { dotnet } from './_framework/dotnet.js';
import StratumDraw from './Stratum.js';

const canvas = document.getElementById('appCanvas');
const ctx = canvas.getContext('2d');
ctx.textBaseline = 'middle';

// Make the canvas pixel-buffer match its display size, accounting for DPR.
function resizeCanvas() {
  const dpr = window.devicePixelRatio || 1;
  const cssW = canvas.clientWidth, cssH = canvas.clientHeight;
  canvas.width  = Math.floor(cssW * dpr);
  canvas.height = Math.floor(cssH * dpr);
  ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
}
resizeCanvas();

// Bind drawing functions to this canvas's context, then expose to .NET.
const drawingModule = StratumDraw(canvas, ctx);

const { setModuleImports, getAssemblyExports, getConfig, runMain } =
  await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

setModuleImports('Stratum.js', drawingModule);

const config = getConfig();
// [JSExport] methods live in Stratum.Runtime.dll, not in the entry assembly.
const runtimeExports = await getAssemblyExports('Stratum.Runtime');

// Input wiring — calls C# [JSExport] methods on Stratum.Runtime.InputBridge
const InputBridge = runtimeExports.Stratum.Runtime.InputBridge;

function relPos(e) {
  const r = canvas.getBoundingClientRect();
  return [Math.floor(e.clientX - r.left), Math.floor(e.clientY - r.top)];
}

canvas.addEventListener('mousemove', e => { const [x,y]=relPos(e); InputBridge.OnMouseMove(x,y); });
canvas.addEventListener('mousedown', e => { canvas.focus(); const [x,y]=relPos(e); InputBridge.OnMouseDown(x,y,e.button); });
canvas.addEventListener('mouseup',   e => { const [x,y]=relPos(e); InputBridge.OnMouseUp(x,y,e.button); });

window.addEventListener('keydown', e => {
  InputBridge.OnKeyDown(e.key, e.code, e.ctrlKey, e.shiftKey, e.altKey);
  if (['ArrowLeft','ArrowRight','ArrowUp','ArrowDown','Tab',' ','Backspace'].includes(e.key)) e.preventDefault();
});
window.addEventListener('keypress', e => {
  if (e.key && e.key.length === 1) InputBridge.OnKeyPress(e.key);
});

window.addEventListener('resize', () => {
  resizeCanvas();
  InputBridge.OnResize(canvas.clientWidth, canvas.clientHeight);
});
InputBridge.OnResize(canvas.clientWidth, canvas.clientHeight);

await runMain(config.mainAssemblyName, []);
