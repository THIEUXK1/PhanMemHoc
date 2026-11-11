// Quản trị nội dung: danh sách bài học và trình soạn bài/câu hỏi
import { api, esc, toast, state, nangLucTheoMa } from './core.js';

export async function manAdmin(root, topbar) {
  const [ds, tk] = await Promise.all([api('/api/admin/baihoc'), api('/api/admin/thongke')]);
  let loc = '';

  const ve = () => {
    const hien = ds.filter(b => !loc || b.nangLuc === loc);
    root.innerHTML = `
      <div class="wrap">
        ${topbar({ back: '#/', title: '⚙️ Quản trị nội dung', right: `<a class="btn green small" href="#/admin/bai/moi">➕ Thêm bài học</a>` })}
        <div class="stats">
          <div class="stat"><b>${tk.phuHuynh}</b><span>phụ huynh</span></div>
          <div class="stat"><b>${tk.be}</b><span>hồ sơ bé</span></div>
          <div class="stat"><b>${tk.baiHoc}</b><span>bài học</span></div>
          <div class="stat"><b>${tk.luotHoc}</b><span>lượt học</span></div>
        </div>
        <div class="card">
          <div class="row" style="margin-bottom:10px">
            <label style="font-weight:600">Nhóm năng lực:</label>
            <select id="loc" style="max-width:260px">
              <option value="">Tất cả</option>
              ${state.nangLuc.map(n => `<option value="${n.ma}" ${n.ma === loc ? 'selected' : ''}>${n.icon} ${esc(n.ten)}</option>`).join('')}
            </select>
          </div>
          <div class="scroll-x"><table class="list">
            <thead><tr><th>Bài học</th><th>Nhóm</th><th>Thứ tự</th><th>Tuổi</th><th>Số câu</th><th>Lượt học</th><th></th></tr></thead>
            <tbody>
              ${hien.map(b => {
                const nl = nangLucTheoMa(b.nangLuc);
                return `<tr>
                  <td><b>${esc(b.icon)} ${esc(b.tieuDe)}</b> ${b.hienThi ? '' : '<span class="badge off">Ẩn</span>'}</td>
                  <td>${esc(nl?.icon)} ${esc(nl?.ten)}</td><td>${b.thuTu}</td><td>${b.tuoiTu}–${b.tuoiDen}</td>
                  <td>${b.soCau}</td><td>${b.soLuot}</td>
                  <td style="white-space:nowrap">
                    <a class="btn light small" href="#/admin/bai/${b.id}">✏️ Sửa</a>
                    <button class="btn danger small" data-xoa="${b.id}" aria-label="Xoá">🗑️</button>
                  </td></tr>`;
              }).join('') || `<tr><td colspan="7" class="muted">Chưa có bài học.</td></tr>`}
            </tbody>
          </table></div>
        </div>
      </div>`;
    root.querySelector('#loc').onchange = e => { loc = e.target.value; ve(); };
    root.querySelectorAll('[data-xoa]').forEach(btn => btn.onclick = async () => {
      const b = ds.find(x => x.id === +btn.dataset.xoa);
      if (!confirm(`Xoá bài "${b.tieuDe}"? ${b.soLuot} lượt học của bài này cũng sẽ bị xoá.`)) return;
      await api(`/api/admin/baihoc/${b.id}`, { method: 'DELETE' });
      ds.splice(ds.indexOf(b), 1);
      tk.baiHoc--; tk.luotHoc -= b.soLuot;
      toast('Đã xoá bài học');
      ve();
    });
  };
  ve();
}

const LOAI = { chon: 'Chọn đáp án', ghep: 'Lật thẻ ghép cặp', noi: 'Nối đôi', sapxep: 'Xếp thứ tự' };
const cauMoi = loai => ({
  ghep: { loai, deBai: 'Bé tìm các cặp giống nhau nhé!', hien: '', luaChon: [], dapAn: 0, cap: ['🐶', '🐱', '🐰'] },
  noi: { loai, deBai: 'Bé nối các hình đi cùng nhau nhé!', hien: '', luaChon: [], dapAn: 0, cap: ['🐶=🦴', '🐰=🥕', '🐵=🍌'] },
  sapxep: { loai, deBai: 'Bé xếp từ bé đến lớn nhé!', hien: '', luaChon: ['1', '2', '3'], dapAn: 0, cap: [] },
}[loai] ?? { loai: 'chon', deBai: '', hien: '', luaChon: ['', '', ''], dapAn: 0, cap: [] });
const dong = v => v.split(/\r?\n/).map(x => x.trim()).filter(Boolean);

export async function manSuaBai(root, topbar, id) {
  const bai = id ? await api(`/api/admin/baihoc/${id}`) : {
    tieuDe: '', icon: '⭐', nangLuc: state.nangLuc[0].ma, thuTu: 1, tuoiTu: 3, tuoiDen: 6, hienThi: true, cauHoi: [cauMoi('chon')],
  };

  const ve = () => {
    root.innerHTML = `
      <div class="wrap">
        ${topbar({ back: '#/admin', title: id ? '✏️ Sửa bài học' : '➕ Bài học mới' })}
        <form id="f" novalidate>
          <div class="card">
            <div class="cols-3">
              <label class="field" style="grid-column:span 2"><span>Tiêu đề</span><input type="text" data-k="tieuDe" value="${esc(bai.tieuDe)}" maxlength="60"></label>
              <label class="field"><span>Biểu tượng (emoji)</span><input type="text" data-k="icon" value="${esc(bai.icon)}" maxlength="8"></label>
              <label class="field"><span>Nhóm năng lực</span>
                <select data-k="nangLuc">${state.nangLuc.map(n => `<option value="${n.ma}" ${n.ma === bai.nangLuc ? 'selected' : ''}>${n.icon} ${esc(n.ten)}</option>`).join('')}</select>
              </label>
              <label class="field"><span>Thứ tự trong nhóm</span><input type="number" data-k="thuTu" data-num value="${bai.thuTu}" min="0"></label>
              <label class="field"><span>Tuổi từ</span><input type="number" data-k="tuoiTu" data-num value="${bai.tuoiTu}" min="2" max="8"></label>
              <label class="field"><span>Tuổi đến</span><input type="number" data-k="tuoiDen" data-num value="${bai.tuoiDen}" min="2" max="8"></label>
            </div>
            <label class="row" style="font-weight:600"><input type="checkbox" data-k="hienThi" ${bai.hienThi ? 'checked' : ''} style="width:22px;height:22px"> Hiển thị cho bé</label>
          </div>

          <div class="section-title">❓ Câu hỏi (${bai.cauHoi.length})</div>
          ${bai.cauHoi.map((c, i) => `
            <div class="q-card" data-i="${i}">
              <div class="q-head">
                <b>Câu ${i + 1}</b>
                <select data-q="loai" style="max-width:220px">
                  ${Object.entries(LOAI).map(([ma, ten]) => `<option value="${ma}" ${c.loai === ma ? 'selected' : ''}>${ten}</option>`).join('')}
                </select>
                <span style="flex:1"></span>
                <button type="button" class="btn light small" data-act="len" ${i === 0 ? 'disabled' : ''} aria-label="Lên">⬆️</button>
                <button type="button" class="btn light small" data-act="xuong" ${i === bai.cauHoi.length - 1 ? 'disabled' : ''} aria-label="Xuống">⬇️</button>
                <button type="button" class="btn danger small" data-act="xoa" aria-label="Xoá câu">🗑️</button>
              </div>
              <label class="field"><span>Đề bài (sẽ được đọc to cho bé)</span><input type="text" data-q="deBai" value="${esc(c.deBai)}"></label>
              ${c.loai === 'noi' ? `
                <label class="field"><span>Các cặp nối (2–5 cặp, mỗi dòng một cặp dạng trái=phải, ví dụ 🐶=🦴)</span>
                  <textarea rows="4" data-q="capdong">${esc(c.cap.join('\n'))}</textarea></label>`
              : c.loai === 'sapxep' ? `
                <label class="field"><span>Các ô theo ĐÚNG thứ tự (2–6 ô, mỗi dòng một ô — bé sẽ thấy xáo trộn)</span>
                  <textarea rows="4" data-q="thutu">${esc(c.luaChon.join('\n'))}</textarea></label>`
              : c.loai === 'ghep' ? `
                <label class="field"><span>Các hình để ghép cặp (2–8 hình, cách nhau bằng dấu cách)</span>
                  <input type="text" data-q="cap" value="${esc(c.cap.join(' '))}"></label>` : `
                <label class="field"><span>Hình minh hoạ (tuỳ chọn, ví dụ 🍎🍎🍎)</span><input type="text" data-q="hien" value="${esc(c.hien)}"></label>
                <span class="muted" style="font-size:15px">Lựa chọn — đánh dấu ● vào đáp án đúng</span>
                ${c.luaChon.map((lc, k) => `
                  <div class="opt">
                    <input type="radio" name="dapan-${i}" data-dapan="${k}" ${k === c.dapAn ? 'checked' : ''} aria-label="Đáp án đúng">
                    <input type="text" data-lc="${k}" value="${esc(lc)}" placeholder="Lựa chọn ${k + 1}">
                    <button type="button" class="btn light small" data-act="xoalc" data-k="${k}" ${c.luaChon.length <= 2 ? 'disabled' : ''} aria-label="Xoá lựa chọn">✖️</button>
                  </div>`).join('')}
                ${c.luaChon.length < 6 ? `<button type="button" class="btn light small" data-act="themlc">➕ Thêm lựa chọn</button>` : ''}`}
            </div>`).join('')}
          <div class="row" style="margin-bottom:18px">
            <button type="button" class="btn light" data-them="chon">➕ Câu chọn đáp án</button>
            <button type="button" class="btn light" data-them="ghep">➕ Câu lật thẻ</button>
            <button type="button" class="btn light" data-them="noi">➕ Câu nối đôi</button>
            <button type="button" class="btn light" data-them="sapxep">➕ Câu xếp thứ tự</button>
          </div>
          <div class="card row" style="position:sticky;bottom:12px;z-index:5">
            <p class="err" id="err" style="flex:1;margin:0"></p>
            <a class="btn light" href="#/admin">Huỷ</a>
            <button class="btn green">💾 Lưu bài học</button>
          </div>
        </form>
      </div>`;
    ganSuKien();
  };

  // Đọc toàn bộ ô nhập vào object bai trước mỗi thao tác thay đổi cấu trúc
  const docForm = () => {
    root.querySelectorAll('[data-k]').forEach(el => {
      if (el.tagName === 'BUTTON') return;
      bai[el.dataset.k] = el.type === 'checkbox' ? el.checked : el.dataset.num !== undefined ? +el.value : el.value;
    });
    root.querySelectorAll('.q-card').forEach(card => {
      const c = bai.cauHoi[+card.dataset.i];
      card.querySelectorAll('[data-q]').forEach(el => {
        if (el.dataset.q === 'cap') c.cap = el.value.split(/[\s,]+/).filter(Boolean);
        else if (el.dataset.q === 'capdong') c.cap = dong(el.value);
        else if (el.dataset.q === 'thutu') c.luaChon = dong(el.value);
        else c[el.dataset.q] = el.value;
      });
      card.querySelectorAll('[data-lc]').forEach(el => { c.luaChon[+el.dataset.lc] = el.value; });
      const r = card.querySelector('[data-dapan]:checked');
      if (r) c.dapAn = +r.dataset.dapan;
    });
  };

  const ganSuKien = () => {
    root.querySelectorAll('[data-q="loai"]').forEach(sel => sel.onchange = () => {
      docForm();
      const i = +sel.closest('.q-card').dataset.i;
      bai.cauHoi[i] = { ...cauMoi(sel.value), deBai: bai.cauHoi[i].deBai };
      ve();
    });
    root.querySelectorAll('[data-act]').forEach(btn => btn.onclick = () => {
      docForm();
      const i = +btn.closest('.q-card').dataset.i, ds = bai.cauHoi, c = ds[i];
      switch (btn.dataset.act) {
        case 'len': [ds[i - 1], ds[i]] = [ds[i], ds[i - 1]]; break;
        case 'xuong': [ds[i + 1], ds[i]] = [ds[i], ds[i + 1]]; break;
        case 'xoa': if (!confirm(`Xoá câu ${i + 1}?`)) return; ds.splice(i, 1); break;
        case 'themlc': c.luaChon.push(''); break;
        case 'xoalc': {
          const k = +btn.dataset.k;
          c.luaChon.splice(k, 1);
          if (c.dapAn === k) c.dapAn = 0; else if (c.dapAn > k) c.dapAn--;
          break;
        }
      }
      ve();
    });
    root.querySelectorAll('[data-them]').forEach(btn => btn.onclick = () => {
      docForm();
      bai.cauHoi.push(cauMoi(btn.dataset.them));
      ve();
      [...root.querySelectorAll('.q-card')].at(-1)?.scrollIntoView({ behavior: 'smooth', block: 'center' });
    });
    root.querySelector('#f').onsubmit = async e => {
      e.preventDefault();
      docForm();
      try {
        const kq = await api(id ? `/api/admin/baihoc/${id}` : '/api/admin/baihoc', { method: id ? 'PUT' : 'POST', body: bai });
        toast('Đã lưu bài học');
        if (!id) { location.hash = `#/admin/bai/${kq.id}`; return; }
        Object.assign(bai, kq);
        ve();
      } catch (err) { root.querySelector('#err').textContent = err.message; }
    };
  };

  ve();
}
