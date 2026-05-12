// Stratum.js — Canvas drawing module exposed to .NET via [JSImport].
// DO NOT MODIFY in app projects.
export default function (canvas, ctx) {
  let audioCtx = null;
  function getAudio() {
    if (!audioCtx) audioCtx = new (window.AudioContext || window.webkitAudioContext)();
    return audioCtx;
  }

  const soundPresets = {
    click:   { type: 'sine',     freq: 800, duration: 0.04, decay: 0.03 },
    toggle:  { type: 'sine',     freq: 600, duration: 0.05, decay: 0.04 },
    chime:   { type: 'sine',     freq: 520, duration: 0.18, decay: 0.15, freq2: 660 },
    success: { type: 'sine',     freq: 440, duration: 0.20, decay: 0.16, freq2: 660 },
    warning: { type: 'triangle', freq: 380, duration: 0.22, decay: 0.18, freq2: 320 },
    error:   { type: 'sawtooth', freq: 300, duration: 0.25, decay: 0.20, freq2: 200 },
  };

  function playUiSound(soundId, volume) {
    const preset = soundPresets[soundId];
    if (!preset) return;
    try {
      const ac  = getAudio();
      const osc = ac.createOscillator();
      const env = ac.createGain();
      osc.connect(env);
      env.connect(ac.destination);
      osc.type = preset.type;
      osc.frequency.setValueAtTime(preset.freq, ac.currentTime);
      if (preset.freq2) osc.frequency.linearRampToValueAtTime(preset.freq2, ac.currentTime + preset.duration * 0.5);
      env.gain.setValueAtTime(volume, ac.currentTime);
      env.gain.exponentialRampToValueAtTime(0.0001, ac.currentTime + preset.duration);
      osc.start(ac.currentTime);
      osc.stop(ac.currentTime + preset.duration + 0.01);
    } catch (_) {}
  }

  return {
    clearRect:  (x,y,w,h) => ctx.clearRect(x,y,w,h),
    fillRect:   (x,y,w,h) => ctx.fillRect(x,y,w,h),
    strokeRect: (x,y,w,h) => ctx.strokeRect(x,y,w,h),
    fillText:   (t,x,y)   => ctx.fillText(t,x,y),
    strokeText: (t,x,y)   => ctx.strokeText(t,x,y),
    measureText:(t)       => ctx.measureText(t).width,
    beginPath:  ()        => ctx.beginPath(),
    closePath:  ()        => ctx.closePath(),
    moveTo:     (x,y)     => ctx.moveTo(x,y),
    lineTo:     (x,y)     => ctx.lineTo(x,y),
    arc:        (x,y,r,a,b,ccw) => ctx.arc(x,y,r,a,b,ccw),
    roundRect:  (x,y,w,h,r) => { if (ctx.roundRect) ctx.roundRect(x,y,w,h,r);
                                  else { ctx.rect(x,y,w,h); } },
    fill:       ()        => ctx.fill(),
    stroke:     ()        => ctx.stroke(),
    save:       ()        => ctx.save(),
    restore:    ()        => ctx.restore(),
    setClip:    (x,y,w,h) => { ctx.beginPath(); ctx.rect(x,y,w,h); ctx.clip(); },
    setFillStyle:   c => { ctx.fillStyle = c; },
    setStrokeStyle: c => { ctx.strokeStyle = c; },
    setLineWidth:   w => { ctx.lineWidth = w; },
    setFont:        f => { ctx.font = f; },
    setTextBaseline:b => { ctx.textBaseline = b; },
    setTextAlign:   a => { ctx.textAlign = a; },
    setGlobalAlpha: a => { ctx.globalAlpha = a; },
    getCanvasWidth:  () => canvas.clientWidth,
    getCanvasHeight: () => canvas.clientHeight,
    requestFrame:   (cb) => requestAnimationFrame(() => cb()),
    playUiSound:    (id, vol) => playUiSound(id, vol),
  };
}
