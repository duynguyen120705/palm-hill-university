// FR-23 – FR-26 Danh mục Khoa, Khóa, Lớp
$(function () {
    'use strict';

    const root = document.getElementById('catalogRoot');
    const tab = root.dataset.tab;
    const duocThaoTac = root.dataset.thaoTac === '1';
    const tbody = document.getElementById('dmBody');
    const status = document.getElementById('dmStatus');
    const modalEl = document.getElementById('modalDanhMuc');
    const modal = modalEl ? new bootstrap.Modal(modalEl) : null;

    const CAU_HINH = {
        khoa: {
            url: '/Faculties',
            nhan: 'khoa',
            ma: function (x) { return x.maKhoa; },
            cot: function (x) { return [[x.maKhoa, 'fw-semibold'], [x.tenKhoa], [x.soLop, 'text-end']]; },
            truongMa: 'maKhoa'
        },
        khoahoc: {
            url: '/Courses',
            nhan: 'khóa',
            ma: function (x) { return x.maKhoaHoc; },
            cot: function (x) { return [[x.maKhoaHoc, 'fw-semibold'], [x.namNhapHoc], [x.soLop, 'text-end']]; },
            truongMa: 'maKhoaHoc'
        },
        lop: {
            url: '/Classes',
            nhan: 'lớp',
            ma: function (x) { return x.maLop; },
            cot: function (x) {
                return [[x.maLop, 'fw-semibold'], [x.tenLop], [x.tenKhoa], [x.maKhoaHoc], [x.soSinhVien, 'text-end']];
            },
            truongMa: 'maLop'
        }
    };
    const cfg = CAU_HINH[tab];

    let danhSach = [];
    let currentPage = 1;
    let requestSeq = 0;
    let dangSua = false;

    function cell(text, className) {
        const td = document.createElement('td');
        td.textContent = text == null ? '' : text;
        if (className) td.className = className;
        return td;
    }

    function nut(label, icon, cls, action, ma) {
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'btn btn-sm ' + cls;
        btn.dataset.action = action;
        btn.dataset.ma = ma;
        btn.title = label;
        btn.setAttribute('aria-label', label);
        const i = document.createElement('i');
        i.className = 'bi ' + icon;
        btn.appendChild(i);
        return btn;
    }

    function renderTable(items) {
        tbody.replaceChildren();
        if (!items.length) {
            const tr = document.createElement('tr');
            const td = cell('Không có dữ liệu', 'text-center text-muted py-4');
            td.colSpan = root.querySelectorAll('thead th').length;
            tr.appendChild(td);
            tbody.appendChild(tr);
            return;
        }
        items.forEach(function (x) {
            const tr = document.createElement('tr');
            cfg.cot(x).forEach(function (c) { tr.appendChild(cell(c[0], c[1])); });
            if (duocThaoTac) {
                const td = document.createElement('td');
                td.className = 'text-end text-nowrap';
                td.append(
                    nut('Sửa', 'bi-pencil', 'btn-outline-primary me-1', 'sua', cfg.ma(x)),
                    nut('Xóa', 'bi-trash', 'btn-outline-danger', 'xoa', cfg.ma(x))
                );
                tr.appendChild(td);
            }
            tbody.appendChild(tr);
        });
    }

    function buildUrl(page) {
        const params = new URLSearchParams();
        const locKhoa = document.getElementById('locKhoa');
        const locKhoaHoc = document.getElementById('locKhoaHoc');
        if (locKhoa && locKhoa.value) params.set('maKhoa', locKhoa.value);
        if (locKhoaHoc && locKhoaHoc.value) params.set('maKhoaHoc', locKhoaHoc.value);
        params.set('page', page);
        return cfg.url + '/List?' + params.toString();
    }

    function loadDanhSach(page) {
        currentPage = page || currentPage;
        const seq = ++requestSeq;

        status.textContent = 'Đang tải...';
        App.get(buildUrl(currentPage)).then(function (res) {
            if (seq !== requestSeq) return;
            const d = res.data;
            currentPage = d.page;
            danhSach = d.items;
            renderTable(d.items);
            App.renderPaging(document.getElementById('dmPaging'), d.page, d.totalPages, loadDanhSach);
            status.textContent = 'Tổng: ' + d.totalItems;
        }).catch(function (err) {
            if (seq === requestSeq) status.textContent = err.message;
        });
    }

    function themLuaChon(select, items, nhan) {
        items.forEach(function (it) {
            const opt = document.createElement('option');
            opt.value = it.ma;
            opt.textContent = nhan ? nhan(it) : it.ten;
            select.appendChild(opt);
        });
    }

    function napDropdownLop() {
        if (tab !== 'lop') return Promise.resolve();
        return App.loadFilters().then(function (data) {
            const tenKhoa = function (k) { return k.ten + ' (' + k.ma + ')'; };
            themLuaChon(document.getElementById('locKhoa'), data.khoas, tenKhoa);
            themLuaChon(document.getElementById('locKhoaHoc'), data.khoaHocs);
            if (document.getElementById('dmKhoa')) {
                themLuaChon(document.getElementById('dmKhoa'), data.khoas, tenKhoa);
                themLuaChon(document.getElementById('dmKhoaHoc'), data.khoaHocs);
            }
        }).catch(function () { status.textContent = 'Không tải được bộ lọc.'; });
    }

    function moModal(item) {
        dangSua = !!item;
        const $m = $(modalEl);
        $m.find('.loi-modal').addClass('d-none').text('');
        $('#formDanhMuc')[0].reset();
        $('#modalDanhMucTitle').text((dangSua ? 'Cập nhật ' : 'Thêm ') + cfg.nhan);
        $('#dmMa').val(dangSua ? cfg.ma(item) : '').prop('disabled', dangSua);

        if (tab === 'khoa') {
            $('#dmTen').val(dangSua ? item.tenKhoa : '');
        } else if (tab === 'khoahoc') {
            const khoaNam = dangSua && item.coSinhVien;
            $('#dmNam').val(dangSua ? item.namNhapHoc : '').prop('disabled', khoaNam);
            $('#dmNamGhiChu').toggleClass('d-none', !khoaNam);
        } else {
            const conSinhVien = dangSua && item.soSinhVien > 0;
            $('#dmTen').val(dangSua ? item.tenLop : '');
            $('#dmKhoa').val(dangSua ? item.maKhoa : '').prop('disabled', conSinhVien);
            $('#dmKhoaHoc').val(dangSua ? item.maKhoaHoc : '').prop('disabled', conSinhVien);
            $('#dmLopGhiChu').toggleClass('d-none', !conSinhVien);
        }
        modal.show();
    }

    function layDuLieuForm() {
        const data = {};
        data[cfg.truongMa] = $('#dmMa').val().trim();
        if (tab === 'khoa') {
            data.tenKhoa = $('#dmTen').val().trim();
        } else if (tab === 'khoahoc') {
            const nam = $('#dmNam').val();
            data.namNhapHoc = nam === '' ? null : parseInt(nam, 10);
        } else {
            data.tenLop = $('#dmTen').val().trim();
            data.maKhoa = $('#dmKhoa').val();
            data.maKhoaHoc = $('#dmKhoaHoc').val();
        }
        return data;
    }

    $('#btnThem').on('click', function () { moModal(null); });

    $('#formDanhMuc').on('submit', function (e) {
        e.preventDefault();
        const $btn = $(this).find('button[type="submit"]').prop('disabled', true);

        App.post(cfg.url + (dangSua ? '/Edit' : '/Create'), layDuLieuForm()).then(function (res) {
            modal.hide();
            App.toast(res.message);
            loadDanhSach();
        }).catch(function (err) {
            // Giữ nguyên biểu mẫu để người dùng sửa
            $(modalEl).find('.loi-modal').removeClass('d-none').text(App.errorText(err));
        }).finally(function () {
            $btn.prop('disabled', false);
        });
    });

    $(tbody).on('click', 'button[data-action]', function () {
        const ma = this.dataset.ma;
        const item = danhSach.find(function (x) { return cfg.ma(x) === ma; });
        if (!item) return;

        if (this.dataset.action === 'sua') {
            moModal(item);
            return;
        }

        App.confirm('Xóa ' + cfg.nhan + ' "' + ma + '"? Mã đã xóa không được dùng lại.', 'Xóa').then(function (dongY) {
            if (!dongY) return;
            App.post(cfg.url + '/Delete', { ma: ma }).then(function (res) {
                App.toast(res.message);
                loadDanhSach();
            }).catch(function (err) {
                App.toast(App.errorText(err), 'danger');
            });
        });
    });

    $('#locKhoa, #locKhoaHoc').on('change', function () { loadDanhSach(1); });
    $('#formLoc').on('submit', function (e) { e.preventDefault(); loadDanhSach(1); });

    napDropdownLop();
    loadDanhSach(1);
});
