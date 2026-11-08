// Thư viện JavaScript dùng chung (mục 3.2.1.2): gọi Ajax kèm anti-forgery, toast, xác nhận, phân trang, tải file.
// Mã riêng của từng trang nằm ở wwwroot/js/pages/<trang>.js và chỉ gọi Ajax qua window.App (Phụ lục B.3).
(function () {
    'use strict';

    function antiForgeryToken() {
        return $('input[name="__RequestVerificationToken"]').first().val();
    }

    // Lỗi trả về cho trang gọi: { status, message, errors }
    function taoLoi(xhr, macDinh) {
        const res = xhr.responseJSON || {};
        let message = res.message || macDinh || 'Thao tác thất bại.';
        if (!res.message) {
            if (xhr.status === 403) message = 'Bạn không có quyền thực hiện thao tác này.';
            else if (xhr.status === 413) message = 'File vượt quá dung lượng cho phép.';
            else if (xhr.status === 0) message = 'Không kết nối được máy chủ.';
        }
        return { status: xhr.status, message: message, errors: res.errors || null };
    }

    // Hết phiên: chuyển về trang đăng nhập (mục 3.2.1.5)
    function veTrangDangNhap() {
        const returnUrl = encodeURIComponent(location.pathname + location.search);
        location.href = '/Account/Login?returnUrl=' + returnUrl;
    }

    function goi(options) {
        return new Promise(function (resolve, reject) {
            $.ajax(Object.assign({
                dataType: 'json',
                headers: {
                    'RequestVerificationToken': antiForgeryToken(),
                    'X-Requested-With': 'XMLHttpRequest'
                }
            }, options)).done(function (res) {
                resolve(res);
            }).fail(function (xhr) {
                if (xhr.status === 401) veTrangDangNhap();
                reject(taoLoi(xhr));
            });
        });
    }

    function hopToast() {
        let hop = document.getElementById('toastContainer');
        if (!hop) {
            hop = document.createElement('div');
            hop.id = 'toastContainer';
            hop.className = 'toast-container position-fixed top-0 end-0 p-3';
            hop.style.zIndex = '1090';
            document.body.appendChild(hop);
        }
        return hop;
    }

    function renderPaging(container, page, totalPages, onChange) {
        container.replaceChildren();
        if (totalPages <= 1) return;

        function item(label, target, disabled, active) {
            const li = document.createElement('li');
            li.className = 'page-item' + (disabled ? ' disabled' : '') + (active ? ' active' : '');

            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'page-link';
            btn.textContent = label;
            if (active) btn.setAttribute('aria-current', 'page');
            if (!disabled && !active && target) {
                btn.addEventListener('click', function () { onChange(target); });
            }

            li.appendChild(btn);
            container.appendChild(li);
        }

        item('‹ Trước', page - 1, page <= 1, false);

        // Hiển thị trang đầu, cuối và 2 trang quanh trang hiện tại
        const from = Math.max(1, page - 2);
        const to = Math.min(totalPages, page + 2);
        if (from > 1) item('1', 1, false, false);
        if (from > 2) item('…', null, true, false);
        for (let p = from; p <= to; p++) item(String(p), p, false, p === page);
        if (to < totalPages - 1) item('…', null, true, false);
        if (to < totalPages) item(String(totalPages), totalPages, false, false);

        item('Sau ›', page + 1, page >= totalPages, false);
    }

    window.App = {
        get: function (url) {
            return goi({ url: url, type: 'GET' });
        },

        post: function (url, payload) {
            return goi({
                url: url,
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(payload)
            });
        },

        upload: function (url, formData) {
            return goi({ url: url, type: 'POST', data: formData, processData: false, contentType: false });
        },

        // Gộp lỗi theo từng trường (errors) hoặc thông báo chung thành một chuỗi
        errorText: function (err) {
            if (err.errors) return Object.values(err.errors).flat().join(' ');
            return err.message;
        },

        toast: function (message, loai) {
            const el = document.createElement('div');
            el.className = 'toast align-items-center border-0 text-bg-' + (loai || 'success');
            el.setAttribute('role', 'status');

            const dong = document.createElement('div');
            dong.className = 'd-flex';
            const body = document.createElement('div');
            body.className = 'toast-body';
            body.textContent = message;
            const nutDong = document.createElement('button');
            nutDong.type = 'button';
            nutDong.className = 'btn-close btn-close-white me-2 m-auto';
            nutDong.setAttribute('data-bs-dismiss', 'toast');
            nutDong.setAttribute('aria-label', 'Đóng');
            dong.append(body, nutDong);
            el.appendChild(dong);

            hopToast().appendChild(el);
            el.addEventListener('hidden.bs.toast', function () { el.remove(); });
            new bootstrap.Toast(el, { delay: 4000 }).show();
        },

        // Hộp thoại xác nhận trước thao tác quan trọng; trả về Promise<boolean>
        confirm: function (message, nhanDongY) {
            return new Promise(function (resolve) {
                const el = document.getElementById('modalXacNhan'); // Views/Shared/_Modal.cshtml
                const dongY = el.querySelector('[data-xac-nhan]');
                el.querySelector('.modal-body').textContent = message;
                dongY.textContent = nhanDongY || 'Xác nhận';

                const modal = bootstrap.Modal.getOrCreateInstance(el);
                let ketQua = false;
                function chon() { ketQua = true; modal.hide(); }
                dongY.addEventListener('click', chon);
                el.addEventListener('hidden.bs.modal', function () {
                    dongY.removeEventListener('click', chon);
                    resolve(ketQua);
                }, { once: true });
                modal.show();
            });
        },

        renderPaging: renderPaging,

        // Hiển thị lỗi server tại đúng trường của form (span data-valmsg-for); lỗi không có trường thì trả về false
        showFieldErrors: function ($form, errors) {
            let daHienThi = false;
            Object.keys(errors || {}).forEach(function (truong) {
                const $span = $form.find('[data-valmsg-for]').filter(function () {
                    return this.getAttribute('data-valmsg-for').toLowerCase() === truong.toLowerCase();
                });
                if ($span.length) {
                    $span.removeClass('field-validation-valid').addClass('field-validation-error').text(errors[truong].join(' '));
                    daHienThi = true;
                }
            });
            return daHienThi;
        },

        clearFormErrors: function ($form) {
            $form.find('[data-valmsg-for]').removeClass('field-validation-error').addClass('field-validation-valid').text('');
            $form.find('.loi-modal').addClass('d-none').text('');
            const validator = $form.data('validator');
            if (validator) validator.resetForm();
        },

        // Thông báo hiển thị ở trang kế tiếp sau khi chuyển trang
        flash: function (message) {
            try { sessionStorage.setItem('appFlash', message); } catch (e) { /* trình duyệt chặn lưu trữ */ }
        },

        // Tải file xuất (FR-28, FR-29): lỗi trả về JSON thì hiện thông báo thay vì tải file hỏng
        download: async function (url, onMessage) {
            onMessage('Đang tạo file...');
            try {
                const res = await fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });

                if (res.status === 401) {
                    veTrangDangNhap();
                    return;
                }
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
        },

        // Lựa chọn Khoa, Khóa, Lớp cho bộ lọc, đã giới hạn theo phạm vi dữ liệu
        loadFilters: function () {
            function lookup(type) {
                return App.get('/Catalog/Lookup?type=' + type).then(function (res) { return res.data; });
            }
            return Promise.all([lookup('khoa'), lookup('khoahoc'), lookup('lop')]).then(function (kq) {
                return { khoas: kq[0], khoaHocs: kq[1], lops: kq[2] };
            });
        }
    };

    $(function () {
        try {
            const flash = sessionStorage.getItem('appFlash');
            if (flash) {
                sessionStorage.removeItem('appFlash');
                App.toast(flash);
            }
        } catch (e) { /* trình duyệt chặn lưu trữ */ }
    });

    // Adapter jQuery Validation cho [NgaySinh]; chỉ đăng ký ở trang có nạp _ValidationScriptsPartial
    $(function () {
        if (!$.validator || !$.validator.unobtrusive) return;

        $.validator.addMethod('ngaysinh', function (value, element, minAge) {
            if (this.optional(element)) return true;
            const ngay = new Date(value);
            const moc = new Date();
            moc.setFullYear(moc.getFullYear() - parseInt(minAge, 10));
            return !isNaN(ngay) && ngay <= moc;
        });
        $.validator.unobtrusive.adapters.addSingleVal('ngaysinh', 'minage');
    });
})();
