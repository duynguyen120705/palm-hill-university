// FR-28 Danh sách sinh viên cần xuất - tìm kiếm và phân trang bằng fetch(), render DOM từ JSON
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
    let controller = null;   // AbortController của request đang chạy
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

    async function loadPage(page) {
        // Hủy request cũ để kết quả gõ trước không ghi đè kết quả gõ sau
        if (controller) controller.abort();
        controller = new AbortController();

        const params = filterParams();
        params.set('page', page || 1);
        params.set('pageSize', document.getElementById('selPageSize').value);

        status.textContent = 'Đang tải...';
        try {
            const res = await fetch('/XuatNhap/DanhSachData?' + params.toString(), {
                headers: { 'Accept': 'application/json' },
                signal: controller.signal
            });
            if (!res.ok) {
                status.textContent = res.status === 403 ? 'Bạn không có quyền xem danh sách.' : 'Không tải được danh sách.';
                return;
            }

            const data = await res.json();
            renderRows(data.items);
            renderPhanTrang(document.getElementById('dsPaging'), data.pageIndex, data.totalPages, loadPage);
            total.textContent = 'Tổng: ' + data.totalItems + ' sinh viên';
            status.textContent = data.totalPages ? 'Trang ' + data.pageIndex + '/' + data.totalPages : '';
        } catch (e) {
            if (e.name !== 'AbortError') status.textContent = 'Không kết nối được máy chủ.';
        }
    }

    function xuat(format) {
        const params = filterParams();
        params.set('format', format);
        taiFileXuat('/XuatNhap/XuatDanhSach?' + params.toString(), function (msg) {
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

    fetch('/ThongKe/BoLoc')
        .then(function (res) { return res.ok ? res.json() : Promise.reject(res.status); })
        .then(function (res) {
            fillSelect(selKhoa, res.data.khoas);
            fillSelect(selKhoaHoc, res.data.khoaHocs);
            allLops = res.data.lops;
            refreshLopOptions();
        })
        .catch(function () { status.textContent = 'Không tải được bộ lọc.'; });

    loadPage(1);
})();
