// FR-10 Đổi mật khẩu, FR-12 Cập nhật thông tin liên hệ - jQuery AJAX gửi JSON
$(function () {
    'use strict';

    const token = $('input[name="__RequestVerificationToken"]').val();

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

    function thongBao($el, message, loai) {
        $el.removeClass('d-none alert-success alert-danger').addClass('alert-' + loai).text(message);
    }

    function loiTuServer(xhr) {
        const res = xhr.responseJSON || {};
        if (res.errors && res.errors.length) return res.errors.join(' ');
        if (xhr.status === 401) return 'Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại.';
        if (xhr.status === 403) return 'Bạn không có quyền thực hiện thao tác này.';
        return res.message || 'Thao tác thất bại.';
    }

    // FR-12: chỉ gửi đúng 3 trường được phép sửa
    $('#formLienHe').on('submit', function (e) {
        e.preventDefault();
        const $btn = $(this).find('button[type="submit"]').prop('disabled', true);

        postJson('/HoSo/CapNhatLienHe', {
            sdt: $('#lhSdt').val().trim() || null,
            email: $('#lhEmail').val().trim() || null,
            diaChi: $('#lhDiaChi').val().trim() || null
        }).done(function (res) {
            // Hiển thị lại giá trị đã chuẩn hóa từ server
            $('#lhSdt').val(res.data.sdt || '');
            $('#lhEmail').val(res.data.email || '');
            $('#lhDiaChi').val(res.data.diaChi || '');
            thongBao($('#lhMessage'), res.message, 'success');
        }).fail(function (xhr) {
            thongBao($('#lhMessage'), loiTuServer(xhr), 'danger');
        }).always(function () {
            $btn.prop('disabled', false);
        });
    });

    // FR-10
    $('#formDoiMatKhau').on('submit', function (e) {
        e.preventDefault();
        const $form = $(this);
        const $btn = $form.find('button[type="submit"]').prop('disabled', true);

        postJson('/HoSo/DoiMatKhau', {
            matKhauHienTai: $('#mkHienTai').val(),
            matKhauMoi: $('#mkMoi').val(),
            xacNhanMatKhauMoi: $('#mkXacNhan').val()
        }).done(function (res) {
            $form[0].reset();
            thongBao($('#mkMessage'), res.message, 'success');
        }).fail(function (xhr) {
            thongBao($('#mkMessage'), loiTuServer(xhr), 'danger');
        }).always(function () {
            $btn.prop('disabled', false);
        });
    });
});
