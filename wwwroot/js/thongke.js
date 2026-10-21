// FR-27 Thống kê sinh viên - render bảng bằng XMLHttpRequest
(function () {
    'use strict';

    const form = document.getElementById('formThongKe');
    const selKhoa = document.getElementById('selKhoa');
    const selKhoaHoc = document.getElementById('selKhoaHoc');
    const selLop = document.getElementById('selLop');
    const tbody = document.getElementById('tkBody');
    const tfoot = document.getElementById('tkFoot');
    const status = document.getElementById('tkStatus');

    let allLops = [];

    // Gửi GET bằng XMLHttpRequest, trả JSON qua callback
    function xhrGetJson(url, onSuccess, onError) {
        const xhr = new XMLHttpRequest();
        xhr.open('GET', url, true);
        xhr.setRequestHeader('Accept', 'application/json');
        xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');

        xhr.onreadystatechange = function () {
            if (xhr.readyState !== XMLHttpRequest.DONE) return;

            if (xhr.status === 200) {
                try {
                    const res = JSON.parse(xhr.responseText);
                    if (res.success) onSuccess(res.data);
                    else onError(res.message || 'Yêu cầu không thành công.');
                } catch (e) {
                    onError('Phản hồi không đúng định dạng JSON.');
                }
            } else if (xhr.status === 401 || xhr.status === 403) {
                onError('Bạn không có quyền xem thống kê.');
            } else {
                onError('Lỗi máy chủ (HTTP ' + xhr.status + ').');
            }
        };
        xhr.onerror = function () {
            onError('Không kết nối được máy chủ.');
        };
        xhr.send();
    }

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

    function renderTable(data) {
        tbody.replaceChildren();
        tfoot.replaceChildren();

        if (data.rows.length === 0) {
            const tr = document.createElement('tr');
            const td = cell('Không có sinh viên phù hợp với tiêu chí đã chọn.', 'text-center text-muted py-4');
            td.colSpan = 8;
            tr.appendChild(td);
            tbody.appendChild(tr);
        }

        data.rows.forEach(function (r) {
            const tr = document.createElement('tr');
            tr.appendChild(cell(r.tenKhoa));
            tr.appendChild(cell(r.maKhoaHoc));
            tr.appendChild(cell(r.tenLop + ' (' + r.maLop + ')'));
            tr.appendChild(cell(r.dangHoc, 'text-end'));
            tr.appendChild(cell(r.baoLuu, 'text-end'));
            tr.appendChild(cell(r.daTotNghiep, 'text-end'));
            tr.appendChild(cell(r.thoiHoc, 'text-end'));
            tr.appendChild(cell(r.tong, 'text-end fw-semibold'));
            tbody.appendChild(tr);
        });

        const t = data.tongCong;
        const tr = document.createElement('tr');
        const label = cell('Tổng cộng (' + data.rows.length + ' lớp)');
        label.colSpan = 3;
        tr.appendChild(label);
        tr.appendChild(cell(t.dangHoc, 'text-end'));
        tr.appendChild(cell(t.baoLuu, 'text-end'));
        tr.appendChild(cell(t.daTotNghiep, 'text-end'));
        tr.appendChild(cell(t.thoiHoc, 'text-end'));
        tr.appendChild(cell(t.tong, 'text-end'));
        tfoot.appendChild(tr);
    }

    function filterParams() {
        const params = new URLSearchParams(new FormData(form));
        for (const [k, v] of Array.from(params.entries())) {
            if (!v) params.delete(k);
        }
        return params;
    }

    // FR-29: xuất đúng tiêu chí đang chọn
    function xuat(format) {
        const params = filterParams();
        params.set('format', format);
        taiFileXuat('/ThongKe/Xuat?' + params.toString(), function (msg) {
            status.textContent = msg;
        });
    }

    function loadThongKe() {
        const params = filterParams();

        status.textContent = 'Đang tải thống kê...';
        const started = performance.now();

        xhrGetJson('/ThongKe/Data?' + params.toString(), function (data) {
            renderTable(data);
            status.textContent = 'Cập nhật lúc ' + new Date().toLocaleTimeString('vi-VN') +
                ' (' + Math.round(performance.now() - started) + ' ms)';
        }, function (msg) {
            status.textContent = msg;
        });
    }

    function loadBoLoc() {
        xhrGetJson('/ThongKe/BoLoc', function (data) {
            fillSelect(selKhoa, data.khoas);
            fillSelect(selKhoaHoc, data.khoaHocs);
            allLops = data.lops;
            refreshLopOptions();
        }, function (msg) {
            status.textContent = msg;
        });
    }

    selKhoa.addEventListener('change', function () { refreshLopOptions(); loadThongKe(); });
    selKhoaHoc.addEventListener('change', function () { refreshLopOptions(); loadThongKe(); });
    selLop.addEventListener('change', loadThongKe);
    document.getElementById('selTrangThai').addEventListener('change', loadThongKe);
    document.getElementById('btnXuatExcel').addEventListener('click', function () { xuat('excel'); });
    document.getElementById('btnXuatPdf').addEventListener('click', function () { xuat('pdf'); });
    form.addEventListener('submit', function (e) {
        e.preventDefault();
        loadThongKe();
    });

    loadBoLoc();
    loadThongKe();
})();
