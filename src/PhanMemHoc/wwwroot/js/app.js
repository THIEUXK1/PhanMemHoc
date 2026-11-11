import { api, esc, toast, stars, state, nangLucTheoMa, noi } from './core.js';
import { manChoi } from './game.js';
import { manAdmin, manSuaBai } from './admin.js';

const root = document.getElementById('app');
const AVATAR = ['🐻', '🐰', '🐱', '🐶', '🦊', '🐼', '🐯', '🐸', '🐵', '🦄', '🐧', '🐥'];

// ---------- Router ----------
const routes = [
  [/^#\/dangnhap$/, () => manDangNhap()],
  [/^#\/?$/, () => manPhuHuynh()],
  [/^#\/be\/(\d+)$/, (id) => manBanDo(+id)],
  [/^#\/be\/(\d+)\/hoc\/(\d+)$/, (id, bai) => manChoi(root, +id, +bai)],
  [/^#\/be\/(\d+)\/baocao$/, (id) => manBaoCao(+id)],
  [/^#\/admin$/, () => canAdmin(() => manAdmin(root, topbar))],
  [/^#\/admin\/bai\/(\d+|moi)$/, (id) => canAdmin(() => manSuaBai(root, topbar, id === 'moi' ? null : +id))],
];

async function render() {
  const hash = location.hash || '#/';
  if (!state.toi && hash !== '#/dangnhap') {
    try { state.toi = await api('/api/auth/toi'); }
    catch { location.hash = '#/dangnhap'; return; }
  }
  if (!state.nangLuc.length) state.nangLuc = await api('/api/nangluc');
  window.scrollTo(0, 0);
  for (const [re, fn] of routes) {
    const m = hash.match(re);
    if (m) {
      try { await fn(...m.slice(1)); }
      catch (e) { if (e.status !== 401) { console.error(e); toast(e.message); } }
      return;
    }
  }
  location.hash = '#/';
}
window.addEventListener('hashchange', render);
render();

function canAdmin(fn) {
  if (!state.toi?.laAdmin) { toast('Chỉ quản trị viên mới vào được trang này.'); location.hash = '#/'; return; }
  return fn();
}

export function topbar({ back, title, right = '' } = {}) {
  return `
    <div class="topbar">
      ${back ? `<a class="icon-btn" href="${back}" style="display:grid;place-items:center;text-decoration:none" aria-label="Quay lại">⬅️</a>` : ''}
      <div class="brand">${title ?? '🦉 Bé Vui Học'}</div>
      <div class="spacer"></div>
      ${right}
    </div>`;
}

// ---------- Đăng nhập / đăng ký ----------
function manDangNhap() {
  let dangKy = false;
  const ve = () => {
    root.innerHTML = `
      <div class="wrap">
        <div class="auth">
          <div class="mascot">🦉</div>
          <h1>Bé Vui Học</h1>
          <p class="muted">Học mà chơi – chơi mà học cho bé 3–6 tuổi</p>
          <div class="card">
            <div class="tabs">
              <button class="${dangKy ? '' : 'on'}" data-tab="0">Đăng nhập</button>
              <button class="${dangKy ? 'on' : ''}" data-tab="1">Đăng ký</button>
            </div>
            <form id="f">
              ${dangKy ? `<label class="field"><span>Họ tên phụ huynh</span><input type="text" name="hoTen" required autocomplete="name"></label>` : ''}
              <label class="field"><span>Email</span><input type="email" name="email" required autocomplete="email"></label>
              <label class="field"><span>Mật khẩu</span><input type="password" name="matKhau" required minlength="${dangKy ? 6 : 1}" autocomplete="${dangKy ? 'new-password' : 'current-password'}"></label>
              <p class="err" id="err"></p>
              <button class="btn big" style="width:100%">${dangKy ? 'Tạo tài khoản' : 'Vào học'}</button>
            </form>
          </div>
        </div>
      </div>`;
    root.querySelectorAll('[data-tab]').forEach(b => b.onclick = () => { dangKy = b.dataset.tab === '1'; ve(); });
    root.querySelector('#f').onsubmit = async e => {
      e.preventDefault();
      const fd = Object.fromEntries(new FormData(e.target));
      try {
        state.toi = await api(dangKy ? '/api/auth/dangky' : '/api/auth/dangnhap', { method: 'POST', body: fd });
        location.hash = '#/';
      } catch (err) { root.querySelector('#err').textContent = err.message; }
    };
  };
  ve();
}

async function dangXuat() {
  await api('/api/auth/dangxuat', { method: 'POST' });
  state.toi = null;
  location.hash = '#/dangnhap';
}

// ---------- Khu phụ huynh: danh sách bé ----------
async function manPhuHuynh() {
  const dsBe = await api('/api/be');
  root.innerHTML = `
    <div class="wrap">
      ${topbar({
        right: `
          <span class="chip">👤 ${esc(state.toi.hoTen)}</span>
          ${state.toi.laAdmin ? `<a class="btn blue small" href="#/admin">⚙️ Quản trị</a>` : ''}
          <button class="btn light small" id="out">Đăng xuất</button>`,
      })}
      <div class="section-title">👶 Chọn bé để bắt đầu học</div>
      <div class="grid" id="kids">
        ${dsBe.map(b => `
          <div class="card kid">
            <div class="avatar">${esc(b.anhDaiDien)}</div>
            <h3>${esc(b.ten)}</h3>
            <div class="muted">${b.tuoi} tuổi · ⭐ ${b.tongSao}</div>
            <div class="actions">
              <a class="btn green" href="#/be/${b.id}">▶️ Vào học</a>
            </div>
            <div class="actions">
              <a class="btn light small" href="#/be/${b.id}/baocao">📊 Báo cáo</a>
              <button class="btn light small" data-sua="${b.id}">✏️ Sửa</button>
            </div>
          </div>`).join('')}
        <button class="add-kid" id="them"><b>➕</b>Thêm hồ sơ bé</button>
      </div>
      <div id="form-be"></div>
    </div>`;
  root.querySelector('#out').onclick = dangXuat;
  root.querySelector('#them').onclick = () => formBe(null);
  root.querySelectorAll('[data-sua]').forEach(b => b.onclick = () => formBe(dsBe.find(x => x.id === +b.dataset.sua)));
  if (!dsBe.length) formBe(null);
}

function formBe(be) {
  const namNay = new Date().getFullYear();
  let avatar = be?.anhDaiDien ?? AVATAR[0];
  const box = root.querySelector('#form-be');
  box.innerHTML = `
    <div class="card" style="margin-top:18px">
      <h2 style="margin-bottom:14px">${be ? 'Sửa hồ sơ bé' : 'Tạo hồ sơ cho bé'}</h2>
      <form id="fb">
        <div class="cols-3">
          <label class="field"><span>Tên bé</span><input type="text" name="ten" required maxlength="40" value="${esc(be?.ten ?? '')}"></label>
          <label class="field"><span>Năm sinh</span>
            <select name="namSinh">
              ${Array.from({ length: 8 }, (_, k) => namNay - 2 - k)
                .map(n => `<option value="${n}" ${n === (be?.namSinh ?? namNay - 4) ? 'selected' : ''}>${n} (${namNay - n} tuổi)</option>`).join('')}
            </select>
          </label>
        </div>
        <div class="field"><span class="muted" style="font-size:15px">Hình đại diện</span>
          <div class="avatars">${AVATAR.map(a => `<button type="button" class="${a === avatar ? 'on' : ''}" data-av="${a}">${a}</button>`).join('')}</div>
        </div>
        <p class="err" id="errb"></p>
        <div class="row">
          <button class="btn green">💾 Lưu</button>
          <button type="button" class="btn light" id="huy">Huỷ</button>
          ${be ? `<span class="spacer" style="flex:1"></span><button type="button" class="btn danger small" id="xoa">🗑️ Xoá hồ sơ</button>` : ''}
        </div>
      </form>
    </div>`;
  box.scrollIntoView({ behavior: 'smooth', block: 'center' });
  box.querySelectorAll('[data-av]').forEach(b => b.onclick = () => {
    avatar = b.dataset.av;
    box.querySelectorAll('[data-av]').forEach(x => x.classList.toggle('on', x === b));
  });
  box.querySelector('#huy').onclick = () => { box.innerHTML = ''; };
  box.querySelector('#fb').onsubmit = async e => {
    e.preventDefault();
    const fd = new FormData(e.target);
    const body = { ten: fd.get('ten'), namSinh: +fd.get('namSinh'), anhDaiDien: avatar };
    try {
      await api(be ? `/api/be/${be.id}` : '/api/be', { method: be ? 'PUT' : 'POST', body });
      toast('Đã lưu hồ sơ bé');
      manPhuHuynh();
    } catch (err) { box.querySelector('#errb').textContent = err.message; }
  };
  box.querySelector('#xoa')?.addEventListener('click', async () => {
    if (!confirm(`Xoá hồ sơ của bé ${be.ten} và toàn bộ kết quả học?`)) return;
    await api(`/api/be/${be.id}`, { method: 'DELETE' });
    toast('Đã xoá hồ sơ');
    manPhuHuynh();
  });
}

// ---------- Bản đồ học của bé ----------
async function manBanDo(beId) {
  const lt = await api(`/api/be/${beId}/lotrinh`);
  const tatCa = lt.nhom.flatMap(n => n.bai.map(b => ({ ...b, nhom: n })));
  const goiY = tatCa.find(b => b.id === lt.goiYBaiId);

  root.innerHTML = `
    <div class="wrap">
      ${topbar({
        back: '#/',
        title: `<span style="font-size:40px">${esc(lt.be.anhDaiDien)}</span> ${esc(lt.be.ten)}`,
        right: `<span class="chip">⭐ ${lt.tongSao}</span>`,
      })}
      ${goiY ? `
        <div class="hero">
          <div class="big-icon">${esc(goiY.icon)}</div>
          <div class="grow">
            <div style="opacity:.9;font-weight:700">${goiY.saoCaoNhat ? 'Ôn lại nhé' : 'Bài học hôm nay'} · ${esc(goiY.nhom.icon)} ${esc(goiY.nhom.ten)}</div>
            <h2>${esc(goiY.tieuDe)}</h2>
          </div>
          <a class="btn big light" href="#/be/${beId}/hoc/${goiY.id}">▶️ Học ngay</a>
        </div>` : `<div class="card">🎉 Bé đã hoàn thành tất cả bài học!</div>`}
      ${lt.nhom.map(n => `
        <div class="section-title">${esc(n.icon)} ${esc(n.ten)}</div>
        <div class="path">
          ${n.bai.map(b => `
            <button class="node" data-bai="${b.id}" ${b.daMo ? '' : 'disabled'} title="${esc(b.tieuDe)}">
              <div class="bubble ${b.saoCaoNhat ? 'done' : ''} ${b.id === lt.goiYBaiId ? 'next' : ''}">
                ${esc(b.icon)}${b.daMo ? '' : '<span class="lock">🔒</span>'}
              </div>
              <div class="name">${esc(b.tieuDe)}</div>
              ${stars(b.saoCaoNhat)}
            </button>`).join('')}
        </div>`).join('')}
      ${!lt.nhom.length ? `<div class="card muted">Chưa có bài học phù hợp với độ tuổi của bé.</div>` : ''}
    </div>`;

  root.querySelectorAll('[data-bai]').forEach(b => b.onclick = () => { location.hash = `#/be/${beId}/hoc/${b.dataset.bai}`; });
  if (goiY) setTimeout(() => location.hash === `#/be/${beId}` && noi(`Chào ${lt.be.ten}! Hôm nay mình học ${goiY.tieuDe} nhé!`), 400);
}

// ---------- Báo cáo cho phụ huynh ----------
async function manBaoCao(beId) {
  const { be, baoCao: bc } = await api(`/api/be/${beId}/baocao`);
  const maxPhut = Math.max(5, ...bc.bayNgay.map(n => n.phut));
  const thu = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'];
  const mauTiLe = p => p >= 80 ? 'var(--green)' : p >= 50 ? 'var(--yellow)' : 'var(--red)';
  const yeu = bc.nenLuyenThem ? nangLucTheoMa(bc.nenLuyenThem) : null;

  root.innerHTML = `
    <div class="wrap">
      ${topbar({
        back: '#/',
        title: `📊 Báo cáo của ${esc(be.anhDaiDien)} ${esc(be.ten)}`,
        right: `<a class="btn green small" href="#/be/${beId}">▶️ Vào học</a>`,
      })}
      <div class="stats">
        <div class="stat"><b>${bc.tongLuot}</b><span>lượt học</span></div>
        <div class="stat"><b>⭐ ${bc.tongSao}</b><span>sao đạt được</span></div>
        <div class="stat"><b>${bc.tongPhut}</b><span>phút học</span></div>
        <div class="stat"><b>${bc.soNgayHoc7}/7</b><span>ngày học trong tuần</span></div>
      </div>

      ${yeu ? `
        <div class="card tip">
          <b>💡 Gợi ý cho bố mẹ:</b> Bé cần luyện thêm <b>${esc(yeu.icon)} ${esc(yeu.ten)}</b>.<br>
          ${esc(yeu.hoatDongThucTe)}
        </div>` : ''}

      <div class="card">
        <h3 style="margin-bottom:6px">⏱️ Thời gian học 7 ngày qua (phút)</h3>
        <div class="bars">
          ${bc.bayNgay.map((n, k) => {
            const d = new Date(n.ngay + 'T00:00:00');
            return `<div class="col ${k === 6 ? 'today' : ''}" title="${n.phut} phút">
              <b>${n.phut || ''}</b><i style="height:${Math.round(100 * n.phut / maxPhut)}%"></i>
              <small>${k === 6 ? 'Hôm nay' : thu[d.getDay()]}</small></div>`;
          }).join('')}
        </div>
      </div>

      <div class="card">
        <h3 style="margin-bottom:6px">🧩 Năng lực tư duy</h3>
        ${bc.nangLuc.map(n => `
          <div class="skill">
            <div class="ic">${esc(n.icon)}</div>
            <div><b>${esc(n.ten)}</b> <span class="muted" style="font-size:14px">· ${esc(n.moTa)}</span></div>
            <div style="font-weight:700">${n.soLuot ? n.tiLeDung + '%' : '—'}</div>
            <div class="meter"><i style="width:${n.tiLeDung}%;background:${mauTiLe(n.tiLeDung)}"></i></div>
            <div class="muted" style="font-size:14px">${n.soBaiXong}/${n.tongBai} bài</div>
          </div>`).join('')}
        <p class="muted" style="font-size:14px;margin-bottom:0">Tỉ lệ đúng tính theo lần chọn đầu tiên của bé ở mỗi câu.</p>
      </div>

      <div class="card">
        <h3 style="margin-bottom:6px">🏡 Hoạt động thực tế cùng bé</h3>
        ${bc.nangLuc.map(n => `<p style="margin:8px 0"><b>${esc(n.icon)} ${esc(n.ten)}:</b> ${esc(n.hoatDongThucTe)}</p>`).join('')}
      </div>

      <div class="card">
        <h3 style="margin-bottom:6px">🕘 Lượt học gần đây</h3>
        ${bc.ganDay.length ? `
          <div class="scroll-x"><table class="list">
            <thead><tr><th>Bài học</th><th>Kết quả</th><th>Sao</th><th>Thời gian</th></tr></thead>
            <tbody>${bc.ganDay.map(g => `
              <tr><td>${esc(g.icon)} ${esc(g.tieuDe)}</td><td>${g.soCauDung}/${g.tongCau}</td><td>${stars(g.sao)}</td>
              <td class="muted">${new Date(g.ngayHoc).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' })}</td></tr>`).join('')}
            </tbody></table></div>` : `<p class="muted">Bé chưa học bài nào.</p>`}
      </div>
    </div>`;
}
