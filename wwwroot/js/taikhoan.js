// FR-01 – FR-05 Quản lý tài khoản Admin/Giáo vụ
// - Render bảng, phân trang (Skip/Take ở server): XMLHttpRequest
// - Tạo (Create), khóa thay cho xóa (Delete), cập nhật, cấp lại mật khẩu: jQuery AJAX
// - Tìm kiếm: fetch()
$(function () {
    'use strict';

    const token = $('input[name="__RequestVerificationToken"]').val();
    const form = document.getElementById('formLoc');
    const tbody = document.getElementById('tkBody');
    const status = document.getElementById('tkStatus');
    // Bootstrap 5.1: Modal nhận phần tử DOM (chưa hỗ trợ chuỗi selector)
    const modalTaiKhoan = new bootstrap.Modal(document.getElementById('modalTaiKhoan'));
    const modalMatKhau = new bootstrap.Modal(document.getElementById('modalMatKhau'));
    const modalTrangThai = new bootstrap.Modal(document.getElementById('modalTrangThai'));

    const TEN_VAI_TRO = { Admin: 'Admin', GiaoVu: 'Giáo vụ' };
    let danhSach = [];   // các tài khoản của trang đang hiển thị
    let currentPage = 1;

    // ===================== 1. Render bảng bằng XMLHttpRequest =====================

    function xhrGetJson(url, onSuccess, onError) {
        const xhr = new XMLHttpRequest();
        xhr.open('GET', url, true);
        xhr.setRequestHeader('Accept', 'application/json');
        xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');

        xhr.onreadystatechange = function () {
            if (xhr.readyState !== XMLHttpRequest.DONE) return;

            if (xhr.status === 200) {
                let json;
                try {
                    json = JSON.parse(xhr.responseText);
                } catch (e) {
                    onError('Phản hồi không đúng định dạng JSON.');
                    return;
                }
                onSuccess(json);
            } else if (xhr.status === 401 || xhr.status === 403) {
                onError('Chỉ Admin được quản lý tài khoản.');
            } else {
                onError('Lỗi máy chủ (HTTP ' + xhr.status + ').');
            }
        };
        xhr.onerror = function () { onError('Không kết nối được máy chủ.'); };
        xhr.send();
    }

    function cell(text, className) {
        const td = document.createElement('td');
        td.textContent = text == null ? '' : text;
        if (className) td.className = className;
        return td;
    }

    function nut(label, icon, cls, action, id) {
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'btn btn-sm ' + cls;
        btn.dataset.action = action;
        btn.dataset.id = id;
        btn.title = label;
        btn.setAttribute('aria-label', label);
        const i = document.createElement('i');
        i.className = 'bi ' + icon;
        btn.appendChild(i);
        return btn;
    }

    function taoDong(tk) {
        const tr = document.createElement('tr');
        tr.dataset.id = tk.id;

        const badge = document.createElement('span');
        badge.className = 'badge ' + (tk.trangThai === 1 ? 'bg-success' : 'bg-secondary');
        badge.textContent = tk.trangThaiText;
        const tdTrangThai = document.createElement('td');
        tdTrangThai.appendChild(badge);

        const tdThaoTac = document.createElement('td');
        tdThaoTac.className = 'text-end text-nowrap';
        tdThaoTac.append(
            nut('Sửa', 'bi-pencil', 'btn-outline-primary me-1', 'sua', tk.id),
            nut('Cấp lại mật khẩu', 'bi-key', 'btn-outline-warning me-1', 'matkhau', tk.id),
            tk.trangThai === 1
                ? nut('Khóa tài khoản', 'bi-lock', 'btn-outline-danger', 'khoa', tk.id)
                : nut('Mở khóa tài khoản', 'bi-unlock', 'btn-outline-success', 'mokhoa', tk.id)
        );

        tr.append(
            cell(tk.userName, 'fw-semibold'),
            cell(tk.hoTen),
            cell(TEN_VAI_TRO[tk.vaiTro] || tk.vaiTro),
            cell(tk.tenKhoa ? tk.tenKhoa + ' (' + tk.maKhoaPhuTrach + ')' : '—'),
            tdTrangThai,
            tdThaoTac
        );
        return tr;
    }

    function renderTable(items) {
        tbody.replaceChildren();
        if (!items.length) {
            const tr = document.createElement('tr');
            const td = cell('Không có dữ liệu', 'text-center text-muted py-4');
            td.colSpan = 6;
            tr.appendChild(td);
            tbody.appendChild(tr);
            return;
        }
        items.forEach(function (tk) { tbody.appendChild(taoDong(tk)); });
    }

    // ===================== 3. Phân trang: mỗi trang một request, server dùng Skip/Take =====================

    // Số thứ tự request: chỉ hiển thị phản hồi của request mới nhất (XHR hoặc fetch)
    let requestSeq = 0;
    let fetchController = null;

    function buildUrl(page) {
        const params = new URLSearchParams(new FormData(form)); // gồm cả keyword
        for (const [k, v] of Array.from(params.entries())) {
            if (!v.trim()) params.delete(k);
        }
        params.set('page', page);
        params.set('pageSize', document.getElementById('locPageSize').value);
        return '/TaiKhoan/Data?' + params.toString();
    }

    // Dùng chung cho cả XHR và fetch: JSON -> DOM
    function hienThiTrang(res) {
        // Server kẹp page về trang cuối nếu trang yêu cầu không còn (VD: sau khi lọc)
        currentPage = res.pageIndex;
        danhSach = res.items;
        renderTable(res.items);
        renderPhanTrang(document.getElementById('tkPaging'), res.pageIndex, res.totalPages, loadDanhSach);
        status.textContent = 'Tổng: ' + res.totalItems + ' tài khoản' +
            (res.totalPages > 1 ? ' — trang ' + res.pageIndex + '/' + res.totalPages : '');
    }

    // page bỏ trống: tải lại trang hiện tại (sau khi tạo/khóa tài khoản)
    function loadDanhSach(page) {
        currentPage = page || currentPage;
        if (fetchController) fetchController.abort();
        const seq = ++requestSeq;

        status.textContent = 'Đang tải trang ' + currentPage + '...';
        xhrGetJson(buildUrl(currentPage), function (res) {
            if (seq === requestSeq) hienThiTrang(res);
        }, function (msg) {
            if (seq === requestSeq) status.textContent = msg;
        });
    }

    // ===================== 4. Tìm kiếm: fetch() gọi API JSON =====================

    async function timKiem() {
        // Hủy request tìm kiếm cũ đang chạy để kết quả gõ trước không ghi đè kết quả gõ sau
        if (fetchController) fetchController.abort();
        fetchController = new AbortController();
        const seq = ++requestSeq;

        status.textContent = 'Đang tìm...';
        try {
            const res = await fetch(buildUrl(1), {
                headers: { 'Accept': 'application/json' },
                signal: fetchController.signal
            });
            // fetch() không reject với HTTP 4xx/5xx - phải tự kiểm tra
            if (!res.ok) {
                if (seq === requestSeq) {
                    status.textContent = res.status === 401 || res.status === 403
                        ? 'Chỉ Admin được quản lý tài khoản.'
                        : 'Lỗi máy chủ (HTTP ' + res.status + ').';
                }
                return;
            }
            const data = await res.json();
            if (seq === requestSeq) hienThiTrang(data);
        } catch (e) {
            if (e.name !== 'AbortError' && seq === requestSeq) status.textContent = 'Không kết nối được máy chủ.';
        }
    }

    // Debounce 300 ms: chỉ gọi API khi người dùng ngừng gõ
    let debounceTimer = null;
    document.getElementById('locKeyword').addEventListener('input', function () {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(timKiem, 300);
    });

    function loadKhoa() {
        xhrGetJson('/TaiKhoan/Khoas', function (res) {
            const khoas = res.data;
            [document.getElementById('locKhoa'), document.getElementById('tkKhoa')].forEach(function (select) {
                khoas.forEach(function (k) {
                    const opt = document.createElement('option');
                    opt.value = k.ma;
                    opt.textContent = k.ten + ' (' + k.ma + ')';
                    select.appendChild(opt);
                });
            });
        }, function (msg) { status.textContent = msg; });
    }

    // Thay một dòng sau khi cập nhật, không tải lại cả bảng
    function capNhatDong(tk) {
        const i = danhSach.findIndex(function (x) { return x.id === tk.id; });
        if (i >= 0) danhSach[i] = tk;
        const tr = tbody.querySelector('tr[data-id="' + CSS.escape(tk.id) + '"]');
        if (tr) tr.replaceWith(taoDong(tk));
    }

    // ===================== 2. jQuery AJAX =====================

    function postJson(url, payload) {
        return $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(payload),
            dataType: 'json',
            headers: { 'RequestVerificationToken': token }
        });
    }

    function thongBao(message, loai) {
        $('#tkMessage').removeClass('d-none alert-success alert-danger').addClass('alert-' + loai).text(message);
    }

    function loiModal($modal, xhr) {
        const res = xhr.responseJSON || {};
        let message = res.message || 'Thao tác thất bại.';
        if (xhr.status === 403) message = 'Chỉ Admin được thực hiện thao tác này.';
        if (res.errors && res.errors.length) message = res.errors.join(' ');
        $modal.find('.loi-modal').removeClass('d-none').text(message);
    }

    function moModal($modal) {
        $modal.find('.loi-modal').addClass('d-none').text('');
    }

    function capNhatOKhoa() {
        const laGiaoVu = $('#tkVaiTro').val() === 'GiaoVu';
        $('#tkKhoa').prop('disabled', !laGiaoVu);
        if (!laGiaoVu) $('#tkKhoa').val('');
    }

    // Mở modal tạo mới
    $('#btnTaoMoi').on('click', function () {
        const $m = $('#modalTaiKhoan');
        moModal($m);
        $('#formTaiKhoan')[0].reset();
        $('#tkId').val('');
        $('#tkUserName').prop('disabled', false);
        $m.find('.nhom-mat-khau').removeClass('d-none');
        $('#modalTaiKhoanTitle').text('Tạo tài khoản');
        capNhatOKhoa();
        modalTaiKhoan.show();
    });

    $('#tkVaiTro').on('change', capNhatOKhoa);

    // Thao tác trên từng dòng (event delegation vì các dòng render động)
    $(tbody).on('click', 'button[data-action]', function () {
        const tk = danhSach.find(function (x) { return x.id === this.dataset.id; }, this);
        if (!tk) return;

        switch (this.dataset.action) {
            case 'sua': {
                const $m = $('#modalTaiKhoan');
                moModal($m);
                $('#tkId').val(tk.id);
                $('#tkUserName').val(tk.userName).prop('disabled', true);
                $('#tkHoTen').val(tk.hoTen);
                $('#tkVaiTro').val(tk.vaiTro);
                capNhatOKhoa();
                $('#tkKhoa').val(tk.maKhoaPhuTrach || '');
                $m.find('.nhom-mat-khau').addClass('d-none');
                $('#modalTaiKhoanTitle').text('Cập nhật tài khoản');
                modalTaiKhoan.show();
                break;
            }
            case 'matkhau':
                moModal($('#modalMatKhau'));
                $('#formMatKhau')[0].reset();
                $('#mkId').val(tk.id);
                $('#mkUserName').text(tk.userName);
                modalMatKhau.show();
                break;
            case 'khoa':
            case 'mokhoa': {
                const khoa = this.dataset.action === 'khoa';
                moModal($('#modalTrangThai'));
                $('#formTrangThai')[0].reset();
                $('#ttId').val(tk.id);
                $('#ttKhoa').val(khoa ? '1' : '0');
                $('#ttUserName').text(tk.userName);
                $('#modalTrangThaiTitle').text(khoa ? 'Khóa tài khoản' : 'Mở khóa tài khoản');
                $('#ttSubmit').text(khoa ? 'Khóa' : 'Mở khóa')
                    .toggleClass('btn-danger', khoa).toggleClass('btn-success', !khoa);
                modalTrangThai.show();
                break;
            }
        }
    });

    // CREATE (FR-02) / UPDATE (FR-03)
    $('#formTaiKhoan').on('submit', function (e) {
        e.preventDefault();
        const $m = $('#modalTaiKhoan');
        const id = $('#tkId').val();
        const $btn = $(this).find('button[type="submit"]').prop('disabled', true);

        const request = id
            ? postJson('/TaiKhoan/Update', {
                id: id,
                hoTen: $('#tkHoTen').val().trim(),
                vaiTro: $('#tkVaiTro').val(),
                maKhoaPhuTrach: $('#tkKhoa').val() || null
            })
            : postJson('/TaiKhoan/Create', {
                userName: $('#tkUserName').val().trim(),
                hoTen: $('#tkHoTen').val().trim(),
                vaiTro: $('#tkVaiTro').val(),
                maKhoaPhuTrach: $('#tkKhoa').val() || null,
                matKhau: $('#tkMatKhau').val(),
                xacNhanMatKhau: $('#tkXacNhan').val()
            });

        request.done(function (res) {
            modalTaiKhoan.hide();
            thongBao(res.message, 'success');
            if (id) {
                capNhatDong(res.data);
            } else {
                // Tài khoản mới có thể nằm ở trang khác (sắp theo tên đăng nhập) - tải lại trang hiện tại
                loadDanhSach();
            }
        }).fail(function (xhr) {
            // Giữ nguyên dữ liệu trong biểu mẫu để người dùng sửa
            loiModal($m, xhr);
        }).always(function () {
            $btn.prop('disabled', false);
        });
    });

    // Cấp lại mật khẩu (FR-04)
    $('#formMatKhau').on('submit', function (e) {
        e.preventDefault();
        const $btn = $(this).find('button[type="submit"]').prop('disabled', true);

        postJson('/TaiKhoan/DatLaiMatKhau', {
            id: $('#mkId').val(),
            matKhauMoi: $('#mkMoi').val(),
            xacNhanMatKhau: $('#mkXacNhan').val()
        }).done(function (res) {
            modalMatKhau.hide();
            thongBao(res.message + ' (' + $('#mkUserName').text() + ')', 'success');
        }).fail(function (xhr) {
            loiModal($('#modalMatKhau'), xhr);
        }).always(function () {
            $btn.prop('disabled', false);
        });
    });

    // "DELETE" = khóa tài khoản; mở khóa (FR-05)
    $('#formTrangThai').on('submit', function (e) {
        e.preventDefault();
        const $btn = $('#ttSubmit').prop('disabled', true);

        postJson('/TaiKhoan/DoiTrangThai', {
            id: $('#ttId').val(),
            khoa: $('#ttKhoa').val() === '1',
            lyDo: $('#ttLyDo').val().trim()
        }).done(function (res) {
            modalTrangThai.hide();
            thongBao(res.message, 'success');
            // Đang lọc theo trạng thái thì dòng không còn khớp bộ lọc - tải lại; ngược lại chỉ thay dòng
            if ($('#locTrangThai').val()) loadDanhSach(); else capNhatDong(res.data);
        }).fail(function (xhr) {
            loiModal($('#modalTrangThai'), xhr);
        }).always(function () {
            $btn.prop('disabled', false);
        });
    });

    // ===================== Bộ lọc =====================

    // Đổi tiêu chí lọc hoặc số dòng/trang thì quay về trang 1
    $('#locVaiTro, #locKhoa, #locTrangThai, #locPageSize').on('change', function () { loadDanhSach(1); });
    $(form).on('submit', function (e) { e.preventDefault(); loadDanhSach(); });

    loadKhoa();
    loadDanhSach(1);
});
