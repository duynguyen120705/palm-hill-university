// FR-02, FR-03 Form tạo, cập nhật tài khoản: khoa phụ trách chỉ dành cho Giáo vụ
$(function () {
    'use strict';

    const $vaiTro = $('[data-vai-tro]');
    const $khoa = $('[data-khoa]');

    function capNhatOKhoa() {
        const laCanBo = $vaiTro.val() === 'CanBo';
        $khoa.prop('disabled', !laCanBo);
        if (!laCanBo) $khoa.val('');
    }

    $vaiTro.on('change', capNhatOKhoa);
    capNhatOKhoa();
});
