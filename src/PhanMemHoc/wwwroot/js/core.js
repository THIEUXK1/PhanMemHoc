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
// Không có giọng vi-VN (vd. Chrome/Windows) → phát mp3 từ /api/tts, tránh giọng tiếng Anh đọc chữ Việt.
const coTts = 'speechSynthesis' in window;
let giong = null;
let audio = null;
function chonGiong() {
  const ds = speechSynthesis.getVoices().filter(v => v.lang?.toLowerCase().replace('_', '-').startsWith('vi'));
  // Ưu tiên giọng tự nhiên (Edge: HoaiMy/NamMinh Online Natural), nữ trước.
  const diem = v => (/natural|online/i.test(v.name) ? 2 : 0) + (/hoaimy|nữ|female/i.test(v.name) ? 1 : 0);
  giong = ds.sort((a, b) => diem(b) - diem(a))[0] || null;
}
if (coTts) {
  chonGiong();
  speechSynthesis.addEventListener?.('voiceschanged', chonGiong);
}
export function imLang() {
  if (coTts) speechSynthesis.cancel();
  if (audio) { audio.pause(); audio = null; }
}
export function noi(text) {
  if (!text) return;
  imLang();
  if (coTts && giong) {
    const u = new SpeechSynthesisUtterance(text);
    u.lang = giong.lang;
    u.voice = giong;
    u.rate = 0.95;
    u.pitch = 1.1;
    speechSynthesis.speak(u);
    return;
  }
  audio = new Audio(`/api/tts?q=${encodeURIComponent(text.slice(0, 200))}`);
  audio.play().catch(() => {});
}

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
  pop() { try { tone(760, 0, .07, 'sine', .12); } catch { } },
  dat() { try { tone(392, 0, .1, 'triangle', .14); tone(587, .08, .16, 'triangle', .14); } catch { } },
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

// ---------- Linh vật "Cú Bông" (SVG tự vẽ) ----------
// camXuc: '' (bình thường) · 'vui' (nhảy, mắt cười) · 'buon' (nghiêng đầu) · 'chao' (vẫy cánh)
export function cuBong(camXuc = '', cls = '') {
  return `
  <svg class="cu ${camXuc} ${cls}" viewBox="0 0 120 132" aria-hidden="true">
    <ellipse class="cu-bong" cx="60" cy="127" rx="30" ry="5"/>
    <g class="cu-than">
      <path d="M30 34 L20 6 L50 24Z M90 34 L100 6 L70 24Z" fill="#8a5434"/>
      <g class="cu-canh-t"><ellipse cx="17" cy="80" rx="11" ry="25" fill="#8a5434"/></g>
      <g class="cu-canh-p"><ellipse cx="103" cy="80" rx="11" ry="25" fill="#8a5434"/></g>
      <ellipse cx="60" cy="74" rx="45" ry="50" fill="#b9774b"/>
      <ellipse cx="60" cy="90" rx="29" ry="31" fill="#f6dcb5"/>
      <path d="M47 84q4 4 8 0M65 84q4 4 8 0M56 98q4 4 8 0" stroke="#dcb487" stroke-width="2.5" fill="none" stroke-linecap="round"/>
      <circle cx="41" cy="50" r="19" fill="#fff" stroke="#8a5434" stroke-width="3"/>
      <circle cx="79" cy="50" r="19" fill="#fff" stroke="#8a5434" stroke-width="3"/>
      <g class="cu-mat">
        <circle cx="43" cy="52" r="8.5" fill="#2b2d42"/><circle cx="77" cy="52" r="8.5" fill="#2b2d42"/>
        <circle cx="46.5" cy="48" r="3" fill="#fff"/><circle cx="80.5" cy="48" r="3" fill="#fff"/>
      </g>
      <g class="cu-cuoi" stroke="#2b2d42" stroke-width="4.5" fill="none" stroke-linecap="round">
        <path d="M32 55q9-11 18 0M70 55q9-11 18 0"/>
      </g>
      <g class="cu-may" stroke="#5c3520" stroke-width="4" stroke-linecap="round">
        <path d="M28 30l20 7M92 30l-20 7"/>
      </g>
      <g class="cu-mi" fill="#b9774b"><ellipse cx="41" cy="50" rx="20" ry="20"/><ellipse cx="79" cy="50" rx="20" ry="20"/></g>
      <circle cx="27" cy="68" r="6" fill="#ff8fa3" opacity=".55"/><circle cx="93" cy="68" r="6" fill="#ff8fa3" opacity=".55"/>
      <path d="M53 62h14l-7 11z" fill="#ff9f1c" stroke="#e07a00" stroke-width="2" stroke-linejoin="round"/>
      <path d="M44 121q-6 5-12 3M50 122q0 5-4 7M76 121q6 5 12 3M70 122q0 5 4 7" stroke="#ff9f1c" stroke-width="4" fill="none" stroke-linecap="round"/>
    </g>
  </svg>`;
}

/** Khung linh vật + bong bóng lời thoại; đặt trong view, điều khiển bằng lvNoi(). */
export const linhVat = (cls = '', loi = '') =>
  `<div class="lv ${cls}"><div class="lv-boi" ${loi ? '' : 'hidden'}>${esc(loi)}</div>${cuBong('chao')}</div>`;

let lvTimer;
/** Cú Bông nói (bong bóng + giọng đọc) và đổi cảm xúc một lúc. */
export function lvNoi(text, camXuc = '', { doc = true } = {}) {
  const lv = document.querySelector('.lv');
  if (lv) {
    const boi = lv.querySelector('.lv-boi');
    boi.textContent = text;
    boi.hidden = !text;
    boi.classList.remove('moi'); void boi.offsetWidth; boi.classList.add('moi');
    const svg = lv.querySelector('.cu');
    svg.classList.remove('vui', 'buon', 'chao');
    if (camXuc) {
      void svg.getBoundingClientRect();
      svg.classList.add(camXuc);
      clearTimeout(lvTimer);
      lvTimer = setTimeout(() => svg.classList.remove(camXuc), 1800);
    }
  }
  if (doc) noi(text);
}

/** Bung ngôi sao lấp lánh tại một phần tử (khi bé làm đúng). */
export function lapLanh(el, kyTu = ['⭐', '✨', '🌟']) {
  if (!el || matchMedia('(prefers-reduced-motion: reduce)').matches) return;
  const r = el.getBoundingClientRect();
  const box = document.createElement('div');
  box.className = 'lap-lanh';
  box.style.left = `${r.left + r.width / 2}px`;
  box.style.top = `${r.top + r.height / 2}px`;
  box.innerHTML = Array.from({ length: 10 }, (_, k) => {
    const goc = (k / 10) * Math.PI * 2, xa = 60 + Math.random() * 50;
    return `<i style="--dx:${Math.cos(goc) * xa}px;--dy:${Math.sin(goc) * xa}px">${kyTu[k % kyTu.length]}</i>`;
  }).join('');
  document.body.appendChild(box);
  setTimeout(() => box.remove(), 900);
}
