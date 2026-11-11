// Tiện ích dùng chung: gọi API, escape HTML, thông báo, giọng đọc, âm thanh

export const state = { toi: null, nangLuc: [] };

export class ApiError extends Error {
  constructor(status, message) { super(message); this.status = status; }
}

export async function api(url, { method = 'GET', body } = {}) {
  const res = await fetch(url, {
    method,
    headers: body !== undefined ? { 'Content-Type': 'application/json' } : {},
    body: body !== undefined ? JSON.stringify(body) : undefined,
    credentials: 'same-origin',
  });
  if (res.status === 401 && !url.startsWith('/api/auth/')) {
    state.toi = null;
    location.hash = '#/dangnhap';
  }
  if (!res.ok) {
    let msg = await res.text();
    try { const j = JSON.parse(msg); msg = typeof j === 'string' ? j : (j.title || msg); } catch { /* text thường */ }
    throw new ApiError(res.status, msg || 'Có lỗi xảy ra, vui lòng thử lại.');
  }
  return res.status === 204 ? null : res.json();
}

export const esc = s => String(s ?? '').replace(/[&<>"']/g, c =>
  ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));

let toastTimer;
export function toast(msg) {
  const el = document.getElementById('toast');
  el.textContent = msg;
  el.classList.add('show');
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => el.classList.remove('show'), 2600);
}

export const stars = (n, max = 3) =>
  `<span class="stars">${Array.from({ length: max }, (_, i) => `<span class="${i < n ? '' : 'off'}">⭐</span>`).join('')}</span>`;

export const nangLucTheoMa = ma => state.nangLuc.find(n => n.ma === ma);

// ---------- Giọng đọc tiếng Việt (Web Speech API, tuỳ trình duyệt có giọng vi-VN hay không) ----------
let giong = null;
function chonGiong() {
  const ds = speechSynthesis.getVoices();
  giong = ds.find(v => v.lang?.toLowerCase().startsWith('vi')) || null;
}
if ('speechSynthesis' in window) {
  chonGiong();
  speechSynthesis.addEventListener?.('voiceschanged', chonGiong);
}
export function noi(text) {
  if (!('speechSynthesis' in window) || !text) return;
  speechSynthesis.cancel();
  const u = new SpeechSynthesisUtterance(text);
  u.lang = 'vi-VN';
  if (giong) u.voice = giong;
  u.rate = 0.95;
  u.pitch = 1.15;
  speechSynthesis.speak(u);
}
export const imLang = () => 'speechSynthesis' in window && speechSynthesis.cancel();

// ---------- Âm thanh ngắn bằng WebAudio ----------
let ac;
function tone(freq, start, dur, type = 'sine', vol = 0.18) {
  ac ??= new (window.AudioContext || window.webkitAudioContext)();
  const o = ac.createOscillator(), g = ac.createGain();
  o.type = type; o.frequency.value = freq;
  g.gain.setValueAtTime(vol, ac.currentTime + start);
  g.gain.exponentialRampToValueAtTime(0.001, ac.currentTime + start + dur);
  o.connect(g).connect(ac.destination);
  o.start(ac.currentTime + start); o.stop(ac.currentTime + start + dur);
}
export const am = {
  dung() { try { tone(660, 0, .12); tone(880, .1, .18); } catch { } },
  sai() { try { tone(220, 0, .25, 'square', .08); } catch { } },
  lat() { try { tone(520, 0, .06, 'triangle', .1); } catch { } },
  xong() { try { [523, 659, 784, 1047].forEach((f, i) => tone(f, i * .12, .25)); } catch { } },
};

export function phaoGiay(soLuong = 80) {
  if (matchMedia('(prefers-reduced-motion: reduce)').matches) return;
  const box = document.createElement('div');
  box.className = 'confetti';
  const mau = ['#ff8a00', '#3a86ff', '#2ec27e', '#ef476f', '#ffd166', '#8338ec'];
  box.innerHTML = Array.from({ length: soLuong }, () =>
    `<i style="left:${Math.random() * 100}%;background:${mau[Math.floor(Math.random() * mau.length)]};` +
    `animation-duration:${1.8 + Math.random() * 1.8}s;animation-delay:${Math.random() * .6}s"></i>`).join('');
  document.body.appendChild(box);
  setTimeout(() => box.remove(), 4500);
}

export const tron = arr => {
  const a = [...arr];
  for (let i = a.length - 1; i > 0; i--) { const j = Math.floor(Math.random() * (i + 1)); [a[i], a[j]] = [a[j], a[i]]; }
  return a;
};
