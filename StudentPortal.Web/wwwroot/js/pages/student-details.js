// FR-14 – FR-20 Chi tiết hồ sơ và các thao tác trên hồ sơ, tài khoản sinh viên
$(function () {
    'use strict';

    let sv = JSON.parse(document.getElementById('svData').textContent);

    function modal(id) {
        return bootstrap.Modal.getOrCreateInstance(document.getElementById(id));
    }

    function hienThi(d) {
        sv = d;
        document.querySelectorAll('[data-field]').forEach(function (el) {
            const v = d[el.dataset.field];
            el.textContent = v == null || v === '' ? '—' : v;
        });
        $('#btnDoiTrangThai').toggleClass('d-none', !d.coTheDoiTrangThai);
        $('#btnXoa').toggleClass('d-none', !d.coTheXoa);
        $('#btnCapLaiMK').toggleClass('d-none', !d.coTheCapLaiMatKhau);
        $('#btnKhoa').toggleClass('d-none', !d.coTheKhoa);
        $('#btnMoKhoa').toggleClass('d-none', !d.coTheMoKhoa);
    }

    function hienLoi($form, err) {
        if (err.errors && App.showFieldErrors($form, err.errors)) return;
        $form.find('.loi-modal').removeClass('d-none').text(App.errorText(err));
    }

    // Gửi form trong modal: thành công thì đóng modal, cập nhật trang bằng Ajax
    function guiForm($form, idModal, url, payload) {
        const $btn = $form.find('button[type="submit"]').prop('disabled', true);
        $form.find('.loi-modal').addClass('d-none').text('');

        return App.post(url, payload).then(function (res) {
            modal(idModal).hide();
            App.toast(res.message);
            if (res.data) hienThi(res.data);
        }).catch(function (err) {
            hienLoi($form, err);
        }).finally(function () {
            $btn.prop('disabled', false);
        });
    }

    // ===================== FR-15 Sửa =====================

    $('#btnSua').on('click', function () {
        const $f = $('#formSua');
        App.clearFormErrors($f);
        $('#HoTen').val(sv.hoTen);
        $('#NgaySinh').val(sv.ngaySinh);
        $('#GioiTinh').val(String(sv.gioiTinh));
        $('#SDT').val(sv.sdt || '');
        $('#Email').val(sv.email || '');
        $('#DiaChi').val(sv.diaChi || '');
        modal('modalSua').show();
    });

    $('#formSua').on('submit', function (e) {
        e.preventDefault();
        const $f = $(this);
        if (!$f.valid()) return;

        guiForm($f, 'modalSua', '/Students/Edit', {
            maSV: sv.maSV,
            hoTen: $('#HoTen').val().trim(),
            ngaySinh: $('#NgaySinh').val() || null,
            gioiTinh: parseInt($('#GioiTinh').val(), 10) || null,
            sdt: $('#SDT').val().trim() || null,
            email: $('#Email').val().trim() || null,
            diaChi: $('#DiaChi').val().trim() || null
        });
    });

    // ===================== FR-16 Đổi trạng thái =====================

    let trangThaiDuocPhep = [];

    function capNhatBatBuocLyDo() {
        const chon = trangThaiDuocPhep.find(function (t) { return String(t.value) === $('#ttMoi').val(); });
        $('#ttBatBuoc').toggleClass('d-none', !(chon && chon.batBuocLyDo));
    }

    $('#btnDoiTrangThai').on('click', function () {
        App.clearFormErrors($('#formTrangThai'));
        $('#ttLyDo').val('');
        App.get('/Students/AllowedStatuses/' + encodeURIComponent(sv.maSV)).then(function (res) {
            trangThaiDuocPhep = res.data;
            const $sel = $('#ttMoi').empty();
            res.data.forEach(function (t) { $sel.append($('<option>').val(t.value).text(t.text)); });
            capNhatBatBuocLyDo();
            modal('modalTrangThai').show();
        }).catch(function (err) { App.toast(err.message, 'danger'); });
    });

    $('#ttMoi').on('change', capNhatBatBuocLyDo);

    $('#formTrangThai').on('submit', function (e) {
        e.preventDefault();
        guiForm($(this), 'modalTrangThai', '/Students/ChangeStatus', {
            maSV: sv.maSV,
            trangThaiMoi: parseInt($('#ttMoi').val(), 10) || null,
            lyDo: $('#ttLyDo').val().trim() || null
        });
    });

    // ===================== FR-17 Chuyển lớp =====================

    $('#btnChuyenLop').on('click', function () {
        App.clearFormErrors($('#formChuyenLop'));
        $('#clLyDo').val('');
        App.get('/Catalog/Lookup?type=lop').then(function (res) {
            const $sel = $('#clLop').empty().append($('<option>').val('').text('-- Chọn lớp đích --'));
            res.data.filter(function (l) { return l.ma !== sv.maLop; }).forEach(function (l) {
                $sel.append($('<option>').val(l.ma).text(l.ten + ' (' + l.ma + ')'));
            });
            modal('modalChuyenLop').show();
        }).catch(function (err) { App.toast(err.message, 'danger'); });
    });

    $('#formChuyenLop').on('submit', function (e) {
        e.preventDefault();
        if (!$(this).valid()) return;
        guiForm($(this), 'modalChuyenLop', '/Students/Transfer', {
            maSV: sv.maSV,
            maLopDich: $('#clLop').val(),
            lyDo: $('#clLyDo').val().trim() || null
        });
    });

    // ===================== FR-19 Cấp lại mật khẩu =====================

    $('#btnCapLaiMK').on('click', function () {
        App.clearFormErrors($('#formMatKhau'));
        $('#formMatKhau')[0].reset();
        modal('modalMatKhau').show();
    });

    $('#formMatKhau').on('submit', function (e) {
        e.preventDefault();
        if (!$(this).valid()) return;
        guiForm($(this), 'modalMatKhau', '/Students/ResetPassword', {
            maSV: sv.maSV,
            matKhauMoi: $('#mkMoi').val(),
            xacNhanMatKhau: $('#mkXacNhan').val()
        });
    });

    // ===================== FR-18 Xóa mềm, FR-20 Khóa/mở khóa: xác nhận kèm lý do =====================

    let hanhDong = null; // 'xoa' | 'khoa' | 'mokhoa'

    function moModalLyDo(loai, tieuDe, nhanNut, doDai) {
        hanhDong = loai;
        App.clearFormErrors($('#formLyDo'));
        $('#formLyDo')[0].reset();
        $('#ldNoiDung').attr('maxlength', doDai);
        $('#modalLyDoTitle').text(tieuDe);
        $('#ldXacNhan').text(nhanNut);
        modal('modalLyDo').show();
    }

    $('#btnXoa').on('click', function () { moModalLyDo('xoa', 'Xóa hồ sơ sinh viên ' + sv.maSV, 'Xóa', 500); });
    $('#btnKhoa').on('click', function () { moModalLyDo('khoa', 'Khóa tài khoản sinh viên', 'Khóa', 255); });
    $('#btnMoKhoa').on('click', function () { moModalLyDo('mokhoa', 'Mở khóa tài khoản sinh viên', 'Mở khóa', 255); });

    $('#formLyDo').on('submit', function (e) {
        e.preventDefault();
        const $f = $(this);
        if (!$f.valid()) return;
        const lyDo = $('#ldNoiDung').val().trim();

        if (hanhDong === 'xoa') {
            const $btn = $('#ldXacNhan').prop('disabled', true);
            $f.find('.loi-modal').addClass('d-none').text('');
            // UC14: thành công thì về danh sách sinh viên và thông báo
            App.post('/Students/Delete', { maSV: sv.maSV, lyDo: lyDo }).then(function (res) {
                App.flash(res.message);
                location.href = '/Students';
            }).catch(function (err) {
                hienLoi($f, err);
                $btn.prop('disabled', false);
            });
            return;
        }

        guiForm($f, 'modalLyDo', '/Students/SetLock', { maSV: sv.maSV, khoa: hanhDong === 'khoa', lyDo: lyDo });
    });

    hienThi(sv);
});
