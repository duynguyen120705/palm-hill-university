// FR-10 Đổi mật khẩu, FR-12 Cập nhật thông tin liên hệ
$(function () {
    'use strict';

    function thongBaoLoi($el, err) {
        $el.removeClass('d-none alert-success').addClass('alert-danger').text(App.errorText(err));
    }

    // FR-12: chỉ gửi đúng 3 trường được phép sửa
    $('#formLienHe').on('submit', function (e) {
        e.preventDefault();
        const $btn = $(this).find('button[type="submit"]').prop('disabled', true);
        $('#lhMessage').addClass('d-none');

        App.post('/Account/UpdateContact', {
            sdt: $('#lhSdt').val().trim() || null,
            email: $('#lhEmail').val().trim() || null,
            diaChi: $('#lhDiaChi').val().trim() || null
        }).then(function (res) {
            // Hiển thị lại giá trị đã chuẩn hóa từ server
            $('#lhSdt').val(res.data.sdt || '');
            $('#lhEmail').val(res.data.email || '');
            $('#lhDiaChi').val(res.data.diaChi || '');
            App.toast(res.message);
        }).catch(function (err) {
            thongBaoLoi($('#lhMessage'), err);
        }).finally(function () {
            $btn.prop('disabled', false);
        });
    });

    // FR-10
    $('#formDoiMatKhau').on('submit', function (e) {
        e.preventDefault();
        const $form = $(this);
        const $btn = $form.find('button[type="submit"]').prop('disabled', true);
        $('#mkMessage').addClass('d-none');

        App.post('/Account/ChangePassword', {
            matKhauHienTai: $('#mkHienTai').val(),
            matKhauMoi: $('#mkMoi').val(),
            xacNhanMatKhauMoi: $('#mkXacNhan').val()
        }).then(function (res) {
            $form[0].reset();
            App.toast(res.message);
        }).catch(function (err) {
            thongBaoLoi($('#mkMessage'), err);
        }).finally(function () {
            $btn.prop('disabled', false);
        });
    });
});
