// FR-31 Tra cứu nhật ký thao tác
(function () {
    'use strict';

    const form = document.getElementById('formNhatKy');
    const tbody = document.getElementById('nkBody');
    const status = document.getElementById('nkStatus');
    const total = document.getElementById('nkTotal');

    let tieuChi = '';     // tiêu chí của lần bấm Tìm gần nhất, dùng khi chuyển trang
    let requestSeq = 0;   // chỉ hiển thị phản hồi của request mới nhất

    function cell(text) {
        const td = document.createElement('td');
        td.textContent = text == null ? '' : text;
        return td;
    }

    function renderRows(items) {
        tbody.replaceChildren();
        if (!items.length) {
            const tr = document.createElement('tr');
            const td = cell('Không có dữ liệu');
            td.colSpan = 6;
            td.className = 'text-center text-muted py-4';
            tr.appendChild(td);
            tbody.appendChild(tr);
            return;
        }
        items.forEach(function (n) {
            const tr = document.createElement('tr');
            tr.append(cell(n.thoiDiem), cell(n.username), cell(n.hanhDong), cell(n.doiTuongLoai), cell(n.doiTuongMa), cell(n.chiTiet));
            tbody.appendChild(tr);
        });
    }

    function loadPage(page) {
        const seq = ++requestSeq;
        const params = new URLSearchParams(tieuChi);
        params.set('page', page || 1);

        status.textContent = 'Đang tải...';
        App.get('/AuditLogs/List?' + params.toString()).then(function (res) {
            if (seq !== requestSeq) return;
            const d = res.data;
            renderRows(d.items);
            App.renderPaging(document.getElementById('nkPaging'), d.page, d.totalPages, loadPage);
            total.textContent = 'Tổng: ' + d.totalItems + ' bản ghi';
            status.textContent = d.totalPages ? 'Trang ' + d.page + '/' + d.totalPages : '';
        }).catch(function (err) {
            if (seq === requestSeq) status.textContent = err.message;
        });
    }

    form.addEventListener('submit', function (e) {
        e.preventDefault();
        const params = new URLSearchParams(new FormData(form));
        for (const [k, v] of Array.from(params.entries())) {
            if (!v.trim()) params.delete(k);
        }
        tieuChi = params.toString();
        loadPage(1);
    });
})();
