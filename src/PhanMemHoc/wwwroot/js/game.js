// Màn chơi một bài học: "chon" (chọn đáp án) · "ghep" (lật thẻ) · "noi" (nối đôi) · "sapxep" (xếp thứ tự, kéo thả)
import { api, esc, noi, imLang, am, phaoGiay, tron, toast, stars, linhVat, lvNoi, lapLanh, cuBong } from './core.js';

const KHEN = ['Giỏi quá!', 'Đúng rồi!', 'Tuyệt vời!', 'Bé giỏi lắm!', 'Chính xác!', 'Hoan hô!'];
const DONG_VIEN = ['Thử lại nhé!', 'Gần đúng rồi, thử lại nào!', 'Bé chọn lại nhé!', 'Không sao, mình thử tiếp nào!'];
const ngauNhien = a => a[Math.floor(Math.random() * a.length)];
const laEmoji = s => !/[0-9A-Za-zÀ-ỹ]/.test(s);
const MAU_DAY = ['#ff8a00', '#3a86ff', '#2ec27e', '#ef476f', '#8338ec'];

export async function manChoi(root, beId, baiId) {
  let bai;
  try {
    bai = await api(`/api/be/${beId}/baihoc/${baiId}`);
  } catch (e) {
    toast(e.message);
    location.hash = `#/be/${beId}`;
    return;
  }

  const traLoi = [];
  const batDau = Date.now();
  let i = 0;
  let huy = false; // người dùng rời màn → dừng các hẹn giờ
  const conChoi = () => !huy && root.isConnected;

  const thoat = () => { huy = true; imLang(); location.hash = `#/be/${beId}`; };
  window.addEventListener('hashchange', () => { huy = true; imLang(); }, { once: true });

  const khen = el => { am.dung(); lapLanh(el); lvNoi(ngauNhien(KHEN), 'vui'); };
  const che = () => { am.sai(); lvNoi(ngauNhien(DONG_VIEN), 'buon'); };

  function khung(noiDung) {
    const pct = Math.round(100 * i / bai.cauHoi.length);
    root.innerHTML = `
      <div class="play">
        <div class="play-top">
          <button class="icon-btn" data-thoat title="Thoát" aria-label="Thoát">✖️</button>
          <div class="progress" aria-label="Tiến độ"><i style="width:${pct}%"></i><b style="left:${pct}%">${esc(bai.icon)}</b></div>
          <button class="icon-btn" data-doc title="Nghe lại" aria-label="Nghe lại câu hỏi">🔊</button>
        </div>
        ${noiDung}
        ${linhVat('lv-choi')}
      </div>`;
    root.querySelector('[data-thoat]').onclick = thoat;
    const c = bai.cauHoi[i];
    root.querySelector('[data-doc]').onclick = () => c && lvNoi(c.deBai);
  }

  function tiepTheo() {
    if (!conChoi()) return;
    i++;
    if (i < bai.cauHoi.length) hienCau(); else ketThuc();
  }

  function hienCau() {
    const c = bai.cauHoi[i];
    ({ ghep: cauGhep, noi: cauNoi, sapxep: cauSapXep }[c.loai] ?? cauChon)(c);
    setTimeout(() => conChoi() && lvNoi(c.deBai), 300);
  }

  // ---------- Chọn đáp án ----------
  function cauChon(c) {
    const thuTu = tron(c.luaChon.map((_, k) => k));
    const chu = c.luaChon.some(x => !laEmoji(x) && [...x].length > 3); // chữ/số ngắn vẫn hiện cỡ to
    khung(`
      <div class="question">
        <h2>${esc(c.deBai)}</h2>
        ${c.hien ? `<div class="show">${esc(c.hien)}</div>` : ''}
        <div class="choices">
          ${thuTu.map((k, n) => `<button class="choice ${chu ? 'text' : ''}" style="--i:${n}" data-k="${k}">${esc(c.luaChon[k])}</button>`).join('')}
        </div>
      </div>`);

    let daTraLoi = false, xong = false;
    root.querySelectorAll('.choice').forEach(btn => btn.onclick = () => {
      if (xong) return;
      const k = +btn.dataset.k;
      if (!daTraLoi) { traLoi[i] = k; daTraLoi = true; }
      if (k === c.dapAn) {
        xong = true;
        btn.classList.add('right');
        khen(btn);
        setTimeout(tiepTheo, 1300);
      } else {
        btn.classList.add('wrong'); btn.disabled = true;
        che();
      }
    });
  }

  // ---------- Lật thẻ tìm cặp ----------
  function cauGhep(c) {
    const the = tron([...c.cap, ...c.cap]);
    const cot = the.length <= 6 ? 3 : the.length <= 12 ? 4 : 5;
    khung(`
      <div class="question">
        <h2>${esc(c.deBai)}</h2>
        <div class="memory" style="--cols:${cot}">
          ${the.map((x, k) => `<button class="mcard" style="--i:${k}" data-k="${k}" aria-label="Thẻ ${k + 1}"><span>${esc(x)}</span></button>`).join('')}
        </div>
      </div>`);

    let mo = [], soSai = 0, soCap = 0, khoa = false;
    root.querySelectorAll('.mcard').forEach(btn => btn.onclick = () => {
      if (khoa || btn.classList.contains('open') || btn.classList.contains('done')) return;
      am.lat();
      btn.classList.add('open');
      mo.push(btn);
      if (mo.length < 2) return;

      const [a, b] = mo; mo = [];
      if (the[a.dataset.k] === the[b.dataset.k]) {
        a.classList.replace('open', 'done'); b.classList.replace('open', 'done');
        am.dung(); lapLanh(b);
        if (++soCap === c.cap.length) {
          traLoi[i] = soSai;
          lvNoi(ngauNhien(KHEN), 'vui');
          setTimeout(tiepTheo, 1300);
        }
      } else {
        soSai++; khoa = true;
        setTimeout(() => { a.classList.remove('open'); b.classList.remove('open'); khoa = false; }, 850);
      }
    });
  }

  // ---------- Nối đôi: chạm trái rồi phải (hoặc kéo dây từ ô này sang ô kia) ----------
  function cauNoi(c) {
    const cap = c.cap.map(x => x.split('=').map(s => s.trim()));
    const trai = tron(cap.map((_, k) => k)), phai = tron(cap.map((_, k) => k));
    const o = (ben, k, n) => {
      const txt = cap[k][ben === 't' ? 0 : 1];
      return `<button class="noi-o ${laEmoji(txt) ? '' : 'chu'}" style="--i:${n}" data-ben="${ben}" data-k="${k}">${esc(txt)}</button>`;
    };
    khung(`
      <div class="question">
        <h2>${esc(c.deBai)}</h2>
        <div class="noi">
          <svg class="noi-day" aria-hidden="true"></svg>
          <div class="noi-cot">${trai.map((k, n) => o('t', k, n)).join('')}</div>
          <div class="noi-cot">${phai.map((k, n) => o('p', k, n)).join('')}</div>
        </div>
      </div>`);

    const khungNoi = root.querySelector('.noi'), svg = khungNoi.querySelector('.noi-day');
    const daNoi = []; // [k, màu]
    let chon = null, soSai = 0, xong = false, dayTam = null;

    const tam = el => {
      const r = el.getBoundingClientRect(), g = khungNoi.getBoundingClientRect();
      const t = el.dataset.ben === 't';
      return [t ? r.right - g.left - 4 : r.left - g.left + 4, r.top - g.top + r.height / 2];
    };
    const duong = ([x1, y1], [x2, y2], mau, cls = '') =>
      `<path class="${cls}" d="M${x1} ${y1}C${(x1 + x2) / 2} ${y1} ${(x1 + x2) / 2} ${y2} ${x2} ${y2}" stroke="${mau}"/>`;
    const ve = () => {
      if (!khungNoi.isConnected) { removeEventListener('resize', ve); return; }
      svg.innerHTML = daNoi.map(([k, mau]) =>
        duong(tam(khungNoi.querySelector(`[data-ben="t"][data-k="${k}"]`)), tam(khungNoi.querySelector(`[data-ben="p"][data-k="${k}"]`)), mau)).join('')
        + (dayTam ? duong(dayTam[0], dayTam[1], '#7b7f95', 'tam') : '');
    };
    addEventListener('resize', ve);

    const boChon = () => { chon?.classList.remove('chon'); chon = null; dayTam = null; ve(); };
    const thu = (a, b) => {
      if (a.dataset.ben === b.dataset.ben) { boChon(); chon = b; b.classList.add('chon'); am.pop(); return; }
      if (a.dataset.k === b.dataset.k) {
        const mau = MAU_DAY[daNoi.length % MAU_DAY.length];
        daNoi.push([a.dataset.k, mau]);
        [a, b].forEach(x => { x.classList.remove('chon'); x.classList.add('xong'); x.style.setProperty('--mau', mau); x.disabled = true; });
        chon = null; dayTam = null; ve();
        if (daNoi.length === cap.length) {
          xong = true; traLoi[i] = soSai;
          khen(b);
          setTimeout(tiepTheo, 1400);
        } else { am.dung(); lapLanh(b); }
      } else {
        soSai++;
        b.classList.add('wrong'); setTimeout(() => b.classList.remove('wrong'), 450);
        boChon(); che();
      }
    };

    khungNoi.addEventListener('pointerdown', e => {
      const el = e.target.closest('.noi-o');
      if (!el || el.disabled || xong) return;
      e.preventDefault();
      if (chon && chon !== el) { thu(chon, el); return; }
      if (!chon) { chon = el; el.classList.add('chon'); am.pop(); }
      const goc = tam(el), g = khungNoi.getBoundingClientRect();
      const keo = ev => { dayTam = [goc, [ev.clientX - g.left, ev.clientY - g.top]]; ve(); };
      const tha = ev => {
        removeEventListener('pointermove', keo); removeEventListener('pointerup', tha);
        const dich = document.elementFromPoint(ev.clientX, ev.clientY)?.closest('.noi-o');
        dayTam = null; ve();
        if (dich && dich !== el && !dich.disabled && chon) thu(chon, dich);
      };
      addEventListener('pointermove', keo); addEventListener('pointerup', tha);
    });
  }

  // ---------- Xếp thứ tự: kéo thẻ vào ô kế tiếp, hoặc chạm thẻ theo đúng thứ tự ----------
  function cauSapXep(c) {
    const n = c.luaChon.length;
    const chu = c.luaChon.some(x => !laEmoji(x));
    khung(`
      <div class="question">
        <h2>${esc(c.deBai)}</h2>
        <div class="xep-o" style="--n:${n}">
          ${c.luaChon.map((_, k) => `<div class="o-trong ${k ? '' : 'ke'}" data-o="${k}"><small>${k + 1}</small></div>`).join('<span class="mui">➜</span>')}
        </div>
        <div class="xep-the">
          ${tron(c.luaChon.map((_, k) => k)).map((k, m) => `<button class="the-xep ${chu ? 'chu' : ''}" style="--i:${m}" data-k="${k}">${esc(c.luaChon[k])}</button>`).join('')}
        </div>
      </div>`);

    let ke = 0, soSai = 0;
    const oKe = () => root.querySelector(`[data-o="${ke}"]`);
    const dat = btn => {
      const o = oKe();
      o.classList.remove('ke');
      o.classList.add('day');
      o.innerHTML = `<span class="vao-o">${btn.innerHTML}</span>`;
      btn.classList.add('da-dat'); btn.disabled = true;
      ke++;
      if (ke === n) {
        traLoi[i] = soSai;
        khen(o);
        setTimeout(tiepTheo, 1400);
      } else {
        am.dat(); lapLanh(o, ['✨']);
        oKe().classList.add('ke');
      }
    };
    const thu = btn => {
      if (+btn.dataset.k === ke) { dat(btn); return; }
      soSai++;
      btn.classList.add('wrong'); setTimeout(() => btn.classList.remove('wrong'), 450);
      che();
    };

    root.querySelector('.xep-the').addEventListener('pointerdown', e => {
      const btn = e.target.closest('.the-xep');
      if (!btn || btn.disabled || ke >= n) return;
      e.preventDefault();
      const x0 = e.clientX, y0 = e.clientY;
      let daKeo = false;
      const keo = ev => {
        const dx = ev.clientX - x0, dy = ev.clientY - y0;
        if (!daKeo && Math.hypot(dx, dy) < 8) return;
        daKeo = true;
        btn.classList.add('dang-keo');
        btn.style.transform = `translate(${dx}px, ${dy}px) scale(1.1)`;
      };
      const tha = ev => {
        removeEventListener('pointermove', keo); removeEventListener('pointerup', tha);
        btn.classList.remove('dang-keo');
        btn.style.transform = '';
        if (!daKeo) { thu(btn); return; }
        btn.style.visibility = 'hidden';
        const duoi = document.elementFromPoint(ev.clientX, ev.clientY);
        btn.style.visibility = '';
        if (duoi?.closest('.xep-o')) thu(btn);
      };
      addEventListener('pointermove', keo); addEventListener('pointerup', tha);
    });
  }

  // ---------- Kết thúc ----------
  async function ketThuc() {
    khung(`<div class="finish"><div class="trophy">⏳</div></div>`);
    let kq;
    try {
      kq = await api(`/api/be/${beId}/ketqua`, {
        method: 'POST',
        body: { baiHocId: bai.id, traLoi, thoiGianGiay: Math.round((Date.now() - batDau) / 1000) },
      });
    } catch (e) {
      toast('Không lưu được kết quả: ' + e.message);
      location.hash = `#/be/${beId}`;
      return;
    }
    if (huy) return;

    const loiKhen = kq.sao === 3 ? 'Xuất sắc! Bé làm đúng hết rồi!'
      : kq.sao === 2 ? 'Giỏi lắm! Bé làm rất tốt!'
      : kq.sao === 1 ? 'Bé làm tốt lắm! Cố thêm chút nữa nhé!'
      : 'Bé đã cố gắng rồi! Mình chơi lại nhé!';
    const tiep = kq.goiYBaiId && kq.goiYBaiId !== bai.id ? kq.goiYBaiId : null;

    i = bai.cauHoi.length;
    root.innerHTML = `
      <div class="play">
        <div class="finish">
          <div class="finish-cu">${cuBong(kq.sao >= 1 ? 'vui lap' : 'buon')}</div>
          <div class="big-stars">${stars(kq.sao)}</div>
          <h2>${esc(loiKhen)}</h2>
          <p class="muted">Đúng ${kq.soCauDung}/${kq.tongCau} câu</p>
          ${kq.huyHieuMoi.length ? `
            <div class="hh-moi">
              <b>🎁 Bé nhận huy hiệu mới!</b>
              <div class="hh-ds">${kq.huyHieuMoi.map((h, k) => `
                <div class="hh co" style="--i:${k}"><span class="hh-ic">${esc(h.icon)}</span><b>${esc(h.ten)}</b></div>`).join('')}
              </div>
            </div>` : ''}
          ${kq.moKhoaMoi.map(m => `<div class="unlock">🔓 Mở khoá bài mới: ${esc(m.icon)} ${esc(m.tieuDe)}</div>`).join('<br>')}
          <div class="row" style="justify-content:center;margin-top:18px">
            <button class="btn light big" data-lai>🔁 Chơi lại</button>
            ${tiep ? `<button class="btn green big" data-tiep>▶️ Học tiếp</button>` : ''}
            <button class="btn blue big" data-ve>🗺️ Bản đồ</button>
          </div>
        </div>
      </div>`;

    if (kq.sao >= 1) { am.xong(); phaoGiay(); }
    noi(loiKhen + (kq.huyHieuMoi.length ? ' Bé nhận được huy hiệu mới nữa đấy!' : ''));
    root.querySelector('[data-lai]').onclick = () => manChoi(root, beId, baiId);
    root.querySelector('[data-ve]').onclick = thoat;
    if (tiep) root.querySelector('[data-tiep]').onclick = () => { location.hash = `#/be/${beId}/hoc/${tiep}`; };
  }

  hienCau();
}
