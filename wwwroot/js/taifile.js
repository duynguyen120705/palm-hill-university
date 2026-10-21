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
