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
