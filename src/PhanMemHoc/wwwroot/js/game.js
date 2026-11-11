// Màn chơi một bài học: câu "chon" (chọn đáp án) và "ghep" (lật thẻ tìm cặp)
import { api, esc, noi, imLang, am, phaoGiay, tron, toast, stars } from './core.js';

const KHEN = ['Giỏi quá!', 'Đúng rồi!', 'Tuyệt vời!', 'Bé giỏi lắm!', 'Chính xác!'];
const DONG_VIEN = ['Thử lại nhé!', 'Gần đúng rồi, thử lại nào!', 'Bé chọn lại nhé!'];
const ngauNhien = a => a[Math.floor(Math.random() * a.length)];
const laEmoji = s => !/[0-9A-Za-zÀ-ỹ]/.test(s);

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

  const thoat = () => { huy = true; imLang(); location.hash = `#/be/${beId}`; };
  window.addEventListener('hashchange', () => { huy = true; imLang(); }, { once: true });

  function khung(noiDung) {
    const pct = Math.round(100 * i / bai.cauHoi.length);
    root.innerHTML = `
      <div class="play">
        <div class="play-top">
          <button class="icon-btn" data-thoat title="Thoát" aria-label="Thoát">✖️</button>
          <div class="progress" aria-label="Tiến độ"><i style="width:${pct}%"></i></div>
          <button class="icon-btn" data-doc title="Nghe lại" aria-label="Nghe lại câu hỏi">🔊</button>
        </div>
        ${noiDung}
      </div>`;
    root.querySelector('[data-thoat]').onclick = thoat;
    const c = bai.cauHoi[i];
    root.querySelector('[data-doc]').onclick = () => noi(c?.deBai);
  }

  function tiepTheo() {
    if (huy) return;
    i++;
    if (i < bai.cauHoi.length) hienCau(); else ketThuc();
  }

  function hienCau() {
    const c = bai.cauHoi[i];
    if (c.loai === 'ghep') cauGhep(c); else cauChon(c);
    setTimeout(() => !huy && noi(c.deBai), 250);
  }

  function cauChon(c) {
    const thuTu = tron(c.luaChon.map((_, k) => k));
    const chu = c.luaChon.some(x => !laEmoji(x));
    khung(`
      <div class="question">
        <h2>${esc(c.deBai)}</h2>
        ${c.hien ? `<div class="show">${esc(c.hien)}</div>` : ''}
        <div class="choices">
          ${thuTu.map(k => `<button class="choice ${chu ? 'text' : ''}" data-k="${k}">${esc(c.luaChon[k])}</button>`).join('')}
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
        am.dung(); noi(ngauNhien(KHEN));
        setTimeout(tiepTheo, 1100);
      } else {
        btn.classList.add('wrong'); btn.disabled = true;
        am.sai(); noi(ngauNhien(DONG_VIEN));
      }
    });
  }

  function cauGhep(c) {
    const the = tron([...c.cap, ...c.cap]);
    const cot = the.length <= 6 ? 3 : the.length <= 8 ? 4 : the.length <= 12 ? 4 : 5;
    khung(`
      <div class="question">
        <h2>${esc(c.deBai)}</h2>
        <div class="memory" style="--cols:${cot}">
          ${the.map((x, k) => `<button class="mcard" data-k="${k}" aria-label="Thẻ ${k + 1}">${esc(x)}</button>`).join('')}
        </div>
      </div>`);

    const nut = [...root.querySelectorAll('.mcard')];
    let mo = [], soSai = 0, soCap = 0, khoa = false;
    nut.forEach(btn => btn.onclick = () => {
      if (khoa || btn.classList.contains('open') || btn.classList.contains('done')) return;
      am.lat();
      btn.classList.add('open');
      mo.push(btn);
      if (mo.length < 2) return;

      const [a, b] = mo; mo = [];
      if (the[a.dataset.k] === the[b.dataset.k]) {
        a.classList.replace('open', 'done'); b.classList.replace('open', 'done');
        am.dung();
        if (++soCap === c.cap.length) {
          traLoi[i] = soSai;
          noi(ngauNhien(KHEN));
          setTimeout(tiepTheo, 1200);
        }
      } else {
        soSai++; khoa = true;
        setTimeout(() => { a.classList.remove('open'); b.classList.remove('open'); khoa = false; }, 800);
      }
    });
  }

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
    khung(`
      <div class="finish">
        <div class="trophy">${kq.sao >= 2 ? '🏆' : kq.sao === 1 ? '🎉' : '💪'}</div>
        <div class="big-stars">${stars(kq.sao)}</div>
        <h2>${esc(loiKhen)}</h2>
        <p class="muted">Đúng ${kq.soCauDung}/${kq.tongCau} câu</p>
        ${kq.moKhoaMoi.map(m => `<div class="unlock">🔓 Mở khoá bài mới: ${esc(m.icon)} ${esc(m.tieuDe)}</div>`).join('<br>')}
        <div class="row" style="justify-content:center;margin-top:18px">
          <button class="btn light big" data-lai>🔁 Chơi lại</button>
          ${tiep ? `<button class="btn green big" data-tiep>▶️ Học tiếp</button>` : ''}
          <button class="btn blue big" data-ve>🗺️ Bản đồ</button>
        </div>
      </div>`);

    if (kq.sao >= 1) { am.xong(); phaoGiay(); }
    noi(loiKhen);
    root.querySelector('[data-lai]').onclick = () => manChoi(root, beId, baiId);
    root.querySelector('[data-ve]').onclick = thoat;
    if (tiep) root.querySelector('[data-tiep]').onclick = () => { location.hash = `#/be/${beId}/hoc/${tiep}`; };
  }

  hienCau();
}
