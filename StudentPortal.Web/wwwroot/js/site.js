// Thư viện JavaScript dùng chung cho mọi trang (mục 3.2.1.2): phân trang, tải file xuất.
// Mã riêng của từng trang nằm ở wwwroot/js/pages/<trang>.js

// Thanh phân trang dùng chung: renderPhanTrang(container, pageIndex, totalPages, onChange)
window.renderPhanTrang = function (container, pageIndex, totalPages, onChange) {
    'use strict';

    container.replaceChildren();
    if (totalPages <= 1) return;

    function item(label, page, disabled, active) {
        const li = document.createElement('li');
        li.className = 'page-item' + (disabled ? ' disabled' : '') + (active ? ' active' : '');

        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'page-link';
        btn.textContent = label;
        if (active) btn.setAttribute('aria-current', 'page');
        if (!disabled && !active && page) {
            btn.addEventListener('click', function () { onChange(page); });
        }

        li.appendChild(btn);
        container.appendChild(li);
    }

    item('‹ Trước', pageIndex - 1, pageIndex <= 1, false);

    // Hiển thị trang đầu, cuối và 2 trang quanh trang hiện tại
    const from = Math.max(1, pageIndex - 2);
    const to = Math.min(totalPages, pageIndex + 2);
    if (from > 1) item('1', 1, false, false);
    if (from > 2) item('…', null, true, false);
    for (let p = from; p <= to; p++) item(String(p), p, false, p === pageIndex);
    if (to < totalPages - 1) item('…', null, true, false);
    if (to < totalPages) item(String(totalPages), totalPages, false, false);

    item('Sau ›', pageIndex + 1, pageIndex >= totalPages, false);
};

// Tải file xuất (FR-28, FR-29) bằng fetch(): lỗi trả về JSON thì hiện thông báo thay vì tải file hỏng
window.taiFileXuat = async function (url, onMessage) {
    'use strict';

    onMessage('Đang tạo file...');
    try {
        const res = await fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });

        // fetch() không reject với HTTP 4xx/5xx - phải tự kiểm tra res.ok
        if (!res.ok) {
            let message = res.status === 403 ? 'Bạn không có quyền xuất dữ liệu.' : 'Xuất file thất bại.';
            try { message = (await res.json()).message || message; } catch (e) { /* không phải JSON */ }
            onMessage(message, true);
            return;
        }

        const disposition = res.headers.get('Content-Disposition') || '';
        const match = /filename\*?=(?:UTF-8'')?"?([^";]+)"?/i.exec(disposition);
        const fileName = match ? decodeURIComponent(match[1]) : 'export';

        // File chỉ nằm trong bộ nhớ trình duyệt, giải phóng URL sau khi tải
        const blobUrl = URL.createObjectURL(await res.blob());
        const a = document.createElement('a');
        a.href = blobUrl;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        a.remove();
        URL.revokeObjectURL(blobUrl);
        onMessage('Đã tải file ' + fileName);
    } catch (e) {
        onMessage('Không kết nối được máy chủ.', true);
    }
};

// Tải lựa chọn Khoa, Khóa, Lớp cho bộ lọc (Catalog/Lookup, đã giới hạn theo phạm vi dữ liệu).
// Trả về Promise<{ khoas, khoaHocs, lops }>
window.taiDanhMucBoLoc = function () {
    'use strict';

    function lookup(type) {
        return fetch('/Catalog/Lookup?type=' + type, {
            headers: { 'Accept': 'application/json', 'X-Requested-With': 'XMLHttpRequest' }
        }).then(function (res) {
            return res.ok ? res.json() : Promise.reject(new Error('HTTP ' + res.status));
        }).then(function (res) {
            return res.success ? res.data : Promise.reject(new Error(res.message));
        });
    }

    return Promise.all([lookup('khoa'), lookup('khoahoc'), lookup('lop')]).then(function (kq) {
        return { khoas: kq[0], khoaHocs: kq[1], lops: kq[2] };
    });
};
