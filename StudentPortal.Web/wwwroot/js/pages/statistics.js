// FR-27 Thống kê sinh viên, FR-29 Xuất báo cáo thống kê
(function () {
    'use strict';

    const form = document.getElementById('formThongKe');
    const selKhoa = document.getElementById('selKhoa');
    const selKhoaHoc = document.getElementById('selKhoaHoc');
    const selLop = document.getElementById('selLop');
    const tbody = document.getElementById('tkBody');
    const tfoot = document.getElementById('tkFoot');
    const status = document.getElementById('tkStatus');
    const btnExcel = document.getElementById('btnXuatExcel');
    const btnPdf = document.getElementById('btnXuatPdf');

    let allLops = [];
    let requestSeq = 0;      // chỉ hiển thị phản hồi của request mới nhất
    let tieuChiDaXem = null; // tiêu chí của kết quả đang hiển thị, dùng khi xuất file

    // Tạo ô bảng bằng textContent để tránh XSS
    function cell(text, className) {
        const td = document.createElement('td');
        td.textContent = text;
        if (className) td.className = className;
        return td;
    }

    function fillSelect(select, items) {
        select.length = 1; // giữ lại option "Tất cả"
        items.forEach(function (it) {
            const opt = document.createElement('option');
            opt.value = it.ma;
            opt.textContent = it.ten;
            select.appendChild(opt);
        });
    }

    // Lớp phụ thuộc Khoa/Khóa đang chọn
    function refreshLopOptions() {
        const lops = allLops.filter(function (l) {
            return (!selKhoa.value || l.maKhoa === selKhoa.value) &&
                   (!selKhoaHoc.value || l.maKhoaHoc === selKhoaHoc.value);
        });
        const current = selLop.value;
        fillSelect(selLop, lops);
        if (lops.some(function (l) { return l.ma === current; })) selLop.value = current;
    }

    function dongSoLieu(ten, r, tenClass) {
        const tr = document.createElement('tr');
        tr.append(
            cell(ten, tenClass),
            cell(r.dangHoc, 'text-end'),
            cell(r.baoLuu, 'text-end'),
            cell(r.daTotNghiep, 'text-end'),
            cell(r.thoiHoc, 'text-end'),
            cell(r.tong, 'text-end fw-semibold')
        );
        return tr;
    }

    function renderTable(data) {
        document.getElementById('thNhom').textContent = data.tenNhom;
        tbody.replaceChildren();
        tfoot.replaceChildren();

        if (data.rows.length === 0) {
            const tr = document.createElement('tr');
            const td = cell('Không có sinh viên phù hợp với tiêu chí đã chọn.', 'text-center text-muted py-4');
            td.colSpan = 6;
            tr.appendChild(td);
            tbody.appendChild(tr);
            return;
        }

        data.rows.forEach(function (r) { tbody.appendChild(dongSoLieu(r.ten, r)); });
        tfoot.appendChild(dongSoLieu('Tổng cộng', data.tongCong));
    }

    function filterParams() {
        const params = new URLSearchParams(new FormData(form));
        for (const [k, v] of Array.from(params.entries())) {
            if (!v) params.delete(k);
        }
        return params;
    }

    function capNhatNutXuat(coDuLieu) {
        btnExcel.disabled = !coDuLieu;
        btnPdf.disabled = !coDuLieu;
    }

    // FR-29: xuất đúng tiêu chí của kết quả đang hiển thị
    function xuat(format) {
        if (!tieuChiDaXem) return;
        const params = new URLSearchParams(tieuChiDaXem);
        params.set('format', format);
        App.download('/Statistics/Export?' + params.toString(), function (msg, laLoi) {
            status.textContent = msg;
            if (laLoi) App.toast(msg, 'danger');
        });
    }

    function loadThongKe() {
        const seq = ++requestSeq;
        const tieuChi = filterParams().toString();
        status.textContent = 'Đang tải thống kê...';

        App.get('/Statistics/Data?' + tieuChi).then(function (res) {
            if (seq !== requestSeq) return;
            renderTable(res.data);
            const coDuLieu = res.data.rows.length > 0;
            tieuChiDaXem = coDuLieu ? tieuChi : null;
            capNhatNutXuat(coDuLieu);
            status.textContent = 'Cập nhật lúc ' + new Date().toLocaleTimeString('vi-VN');
        }).catch(function (err) {
            if (seq === requestSeq) status.textContent = err.message;
        });
    }

    selKhoa.addEventListener('change', refreshLopOptions);
    selKhoaHoc.addEventListener('change', refreshLopOptions);
    btnExcel.addEventListener('click', function () { xuat('excel'); });
    btnPdf.addEventListener('click', function () { xuat('pdf'); });
    form.addEventListener('submit', function (e) {
        e.preventDefault();
        loadThongKe();
    });

    App.loadFilters().then(function (data) {
        fillSelect(selKhoa, data.khoas);
        fillSelect(selKhoaHoc, data.khoaHocs);
        allLops = data.lops;
        refreshLopOptions();
    }).catch(function () {
        status.textContent = 'Không tải được bộ lọc.';
    });
})();
