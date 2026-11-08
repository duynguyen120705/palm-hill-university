// FR-01 – FR-05 Quản lý tài khoản Admin/Giáo vụ
$(function () {
    'use strict';

    const TK_HOAT_DONG = 1; // TrangThaiTaiKhoan.HoatDong
    const VAI_TRO_CAN_BO = 'CanBo';

    const form = document.getElementById('formLoc');
    const tbody = document.getElementById('tkBody');
    const status = document.getElementById('tkStatus');
    // Bootstrap 5.1: Modal nhận phần tử DOM (chưa hỗ trợ chuỗi selector)
    const modalTaiKhoan = new bootstrap.Modal(document.getElementById('modalTaiKhoan'));
    const modalMatKhau = new bootstrap.Modal(document.getElementById('modalMatKhau'));
    const modalTrangThai = new bootstrap.Modal(document.getElementById('modalTrangThai'));

    const TEN_VAI_TRO = { Admin: 'Quản trị viên', CanBo: 'Giáo vụ' };
    let danhSach = [];   // các tài khoản của trang đang hiển thị
    let currentPage = 1;
    let requestSeq = 0;  // chỉ hiển thị phản hồi của request mới nhất

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
        const hoatDong = tk.trangThai === TK_HOAT_DONG;

        const badge = document.createElement('span');
        badge.className = 'badge ' + (hoatDong ? 'bg-success' : 'bg-secondary');
        badge.textContent = tk.trangThaiText;
        const tdTrangThai = document.createElement('td');
        tdTrangThai.appendChild(badge);

        const tdThaoTac = document.createElement('td');
        tdThaoTac.className = 'text-end text-nowrap';
        tdThaoTac.append(
            nut('Sửa', 'bi-pencil', 'btn-outline-primary me-1', 'sua', tk.id),
            nut('Cấp lại mật khẩu', 'bi-key', 'btn-outline-warning me-1', 'matkhau', tk.id),
            hoatDong
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

    function buildUrl(page) {
        const params = new URLSearchParams(new FormData(form)); // gồm cả keyword
        for (const [k, v] of Array.from(params.entries())) {
            if (!v.trim()) params.delete(k);
        }
        params.set('page', page);
        params.set('pageSize', document.getElementById('locPageSize').value);
        return '/Users/List?' + params.toString();
    }

    // page bỏ trống: tải lại trang hiện tại (sau khi tạo/khóa tài khoản)
    function loadDanhSach(page) {
        currentPage = page || currentPage;
        const seq = ++requestSeq;

        status.textContent = 'Đang tải...';
        App.get(buildUrl(currentPage)).then(function (res) {
            if (seq !== requestSeq) return;
            const d = res.data;
            // Server kẹp page về trang cuối nếu trang yêu cầu không còn (VD: sau khi lọc)
            currentPage = d.page;
            danhSach = d.items;
            renderTable(d.items);
            App.renderPaging(document.getElementById('tkPaging'), d.page, d.totalPages, loadDanhSach);
            status.textContent = 'Tổng: ' + d.totalItems + ' tài khoản' +
                (d.totalPages > 1 ? ' — trang ' + d.page + '/' + d.totalPages : '');
        }).catch(function (err) {
            if (seq === requestSeq) status.textContent = err.message;
        });
    }

    // Debounce 300 ms: chỉ gọi API khi người dùng ngừng gõ
    let debounceTimer = null;
    document.getElementById('locKeyword').addEventListener('input', function () {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(function () { loadDanhSach(1); }, 300);
    });

    function loadKhoa() {
        App.get('/Catalog/Lookup?type=khoa').then(function (res) {
            [document.getElementById('locKhoa'), document.getElementById('tkKhoa')].forEach(function (select) {
                res.data.forEach(function (k) {
                    const opt = document.createElement('option');
                    opt.value = k.ma;
                    opt.textContent = k.ten + ' (' + k.ma + ')';
                    select.appendChild(opt);
                });
            });
        }).catch(function (err) { status.textContent = err.message; });
    }

    // Thay một dòng sau khi cập nhật, không tải lại cả bảng
    function capNhatDong(tk) {
        const i = danhSach.findIndex(function (x) { return x.id === tk.id; });
        if (i >= 0) danhSach[i] = tk;
        const tr = tbody.querySelector('tr[data-id="' + CSS.escape(tk.id) + '"]');
        if (tr) tr.replaceWith(taoDong(tk));
    }

    function loiModal($modal, err) {
        $modal.find('.loi-modal').removeClass('d-none').text(App.errorText(err));
    }

    function moModal($modal) {
        $modal.find('.loi-modal').addClass('d-none').text('');
    }

    function capNhatOKhoa() {
        const laCanBo = $('#tkVaiTro').val() === VAI_TRO_CAN_BO;
        $('#tkKhoa').prop('disabled', !laCanBo);
        if (!laCanBo) $('#tkKhoa').val('');
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

    // Tạo (FR-02) / cập nhật (FR-03)
    $('#formTaiKhoan').on('submit', function (e) {
        e.preventDefault();
        const $m = $('#modalTaiKhoan');
        const id = $('#tkId').val();
        const $btn = $(this).find('button[type="submit"]').prop('disabled', true);

        const request = id
            ? App.post('/Users/Edit', {
                id: id,
                hoTen: $('#tkHoTen').val().trim(),
                vaiTro: $('#tkVaiTro').val(),
                maKhoaPhuTrach: $('#tkKhoa').val() || null
            })
            : App.post('/Users/Create', {
                userName: $('#tkUserName').val().trim(),
                hoTen: $('#tkHoTen').val().trim(),
                vaiTro: $('#tkVaiTro').val(),
                maKhoaPhuTrach: $('#tkKhoa').val() || null,
                matKhau: $('#tkMatKhau').val(),
                xacNhanMatKhau: $('#tkXacNhan').val()
            });

        request.then(function (res) {
            modalTaiKhoan.hide();
            App.toast(res.message);
            if (id) {
                capNhatDong(res.data);
            } else {
                // Tài khoản mới có thể nằm ở trang khác (sắp theo tên đăng nhập) - tải lại trang hiện tại
                loadDanhSach();
            }
        }).catch(function (err) {
            // Giữ nguyên dữ liệu trong biểu mẫu để người dùng sửa
            loiModal($m, err);
        }).finally(function () {
            $btn.prop('disabled', false);
        });
    });

    // Cấp lại mật khẩu (FR-04)
    $('#formMatKhau').on('submit', function (e) {
        e.preventDefault();
        const $btn = $(this).find('button[type="submit"]').prop('disabled', true);

        App.post('/Users/ResetPassword', {
            id: $('#mkId').val(),
            matKhauMoi: $('#mkMoi').val(),
            xacNhanMatKhau: $('#mkXacNhan').val()
        }).then(function (res) {
            modalMatKhau.hide();
            App.toast(res.message);
        }).catch(function (err) {
            loiModal($('#modalMatKhau'), err);
        }).finally(function () {
            $btn.prop('disabled', false);
        });
    });

    // Khóa/mở khóa (FR-05)
    $('#formTrangThai').on('submit', function (e) {
        e.preventDefault();
        const $btn = $('#ttSubmit').prop('disabled', true);

        App.post('/Users/SetLock', {
            id: $('#ttId').val(),
            khoa: $('#ttKhoa').val() === '1',
            lyDo: $('#ttLyDo').val().trim()
        }).then(function (res) {
            modalTrangThai.hide();
            App.toast(res.message);
            // Đang lọc theo trạng thái thì dòng không còn khớp bộ lọc - tải lại; ngược lại chỉ thay dòng
            if ($('#locTrangThai').val()) loadDanhSach(); else capNhatDong(res.data);
        }).catch(function (err) {
            loiModal($('#modalTrangThai'), err);
        }).finally(function () {
            $btn.prop('disabled', false);
        });
    });

    // Đổi tiêu chí lọc hoặc số dòng/trang thì quay về trang 1
    $('#locVaiTro, #locKhoa, #locTrangThai, #locPageSize').on('change', function () { loadDanhSach(1); });
    $(form).on('submit', function (e) { e.preventDefault(); loadDanhSach(); });

    loadKhoa();
    loadDanhSach(1);
});
