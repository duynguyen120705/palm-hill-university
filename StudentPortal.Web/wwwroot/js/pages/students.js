// FR-21, FR-22 Danh sách, tìm kiếm sinh viên; FR-28 Xuất danh sách
(function () {
    'use strict';

    const form = document.getElementById('formDanhSach');
    const selKhoa = document.getElementById('selKhoa');
    const selKhoaHoc = document.getElementById('selKhoaHoc');
    const selLop = document.getElementById('selLop');
    const txtKeyword = document.getElementById('txtKeyword');
    const tbody = document.getElementById('dsBody');
    const status = document.getElementById('dsStatus');
    const total = document.getElementById('dsTotal');

    let allLops = [];
    let requestSeq = 0;      // chỉ hiển thị phản hồi của request mới nhất
    let debounceTimer = null;

    function cell(text, className) {
        const td = document.createElement('td');
        td.textContent = text == null ? '' : text;
        if (className) td.className = className;
        return td;
    }

    function fillSelect(select, items) {
        select.length = 1; // giữ option "Tất cả"
        items.forEach(function (it) {
            const opt = document.createElement('option');
            opt.value = it.ma;
            opt.textContent = it.ten;
            select.appendChild(opt);
        });
    }

    function refreshLopOptions() {
        const lops = allLops.filter(function (l) {
            return (!selKhoa.value || l.maKhoa === selKhoa.value) &&
                   (!selKhoaHoc.value || l.maKhoaHoc === selKhoaHoc.value);
        });
        const current = selLop.value;
        fillSelect(selLop, lops);
        if (lops.some(function (l) { return l.ma === current; })) selLop.value = current;
    }

    // Tiêu chí lọc hiện tại (gồm từ khóa), bỏ trường rỗng
    function filterParams() {
        const params = new URLSearchParams(new FormData(form));
        for (const [k, v] of Array.from(params.entries())) {
            if (!v.trim()) params.delete(k);
        }
        return params;
    }

    function renderRows(items) {
        tbody.replaceChildren();
        if (!items.length) {
            const tr = document.createElement('tr');
            const td = cell('Không tìm thấy sinh viên phù hợp.', 'text-center text-muted py-4');
            td.colSpan = 8;
            tr.appendChild(td);
            tbody.appendChild(tr);
            return;
        }
        items.forEach(function (sv) {
            const tr = document.createElement('tr');
            tr.append(
                cell(sv.maSV, 'fw-semibold'),
                cell(sv.hoTen),
                cell(sv.ngaySinhFormatted),
                cell(sv.gioiTinhText),
                cell(sv.tenLop + ' (' + sv.maLop + ')'),
                cell(sv.sdt),
                cell(sv.email),
                cell(sv.trangThaiText)
            );
            tbody.appendChild(tr);
        });
    }

    function loadPage(page) {
        const seq = ++requestSeq;
        const params = filterParams();
        params.set('page', page || 1);
        params.set('pageSize', document.getElementById('selPageSize').value);

        status.textContent = 'Đang tải...';
        App.get('/Students/Search?' + params.toString()).then(function (res) {
            if (seq !== requestSeq) return;
            const d = res.data;
            renderRows(d.items);
            App.renderPaging(document.getElementById('dsPaging'), d.page, d.totalPages, loadPage);
            total.textContent = 'Tổng: ' + d.totalItems + ' sinh viên';
            status.textContent = d.totalPages ? 'Trang ' + d.page + '/' + d.totalPages : '';
        }).catch(function (err) {
            if (seq === requestSeq) status.textContent = err.message;
        });
    }

    function xuat(format) {
        const params = filterParams();
        params.set('format', format);
        App.download('/Students/Export?' + params.toString(), function (msg) {
            status.textContent = msg;
        });
    }

    // Tìm kiếm khi gõ, debounce 300 ms
    txtKeyword.addEventListener('input', function () {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(function () { loadPage(1); }, 300);
    });
    selKhoa.addEventListener('change', function () { refreshLopOptions(); loadPage(1); });
    selKhoaHoc.addEventListener('change', function () { refreshLopOptions(); loadPage(1); });
    ['selLop', 'selTrangThai', 'selPageSize'].forEach(function (id) {
        document.getElementById(id).addEventListener('change', function () { loadPage(1); });
    });
    form.addEventListener('submit', function (e) { e.preventDefault(); loadPage(1); });
    document.getElementById('btnXuatExcel').addEventListener('click', function () { xuat('excel'); });
    document.getElementById('btnXuatPdf').addEventListener('click', function () { xuat('pdf'); });

    App.loadFilters()
        .then(function (data) {
            fillSelect(selKhoa, data.khoas);
            fillSelect(selKhoaHoc, data.khoaHocs);
            allLops = data.lops;
            refreshLopOptions();
        })
        .catch(function () { status.textContent = 'Không tải được bộ lọc.'; });

    loadPage(1);
})();
