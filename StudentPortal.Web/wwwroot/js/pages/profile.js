// FR-12 Cập nhật thông tin liên hệ
$(function () {
    'use strict';

    const $form = $('#formLienHe');
    if (!$form.length) return;

    const modal = new bootstrap.Modal(document.getElementById('modalLienHe'));
    let hienTai = { sdt: $('#lhSdt').val(), email: $('#lhEmail').val(), diaChi: $('#lhDiaChi').val() };

    function hienThi(d) {
        document.querySelectorAll('[data-lien-he]').forEach(function (el) {
            el.textContent = d[el.dataset.lienHe] || '—';
        });
    }

    $('#btnCapNhatLienHe').on('click', function () {
        App.clearFormErrors($form);
        $('#lhSdt').val(hienTai.sdt);
        $('#lhEmail').val(hienTai.email);
        $('#lhDiaChi').val(hienTai.diaChi);
        modal.show();
    });

    // Chỉ gửi đúng 3 trường được phép sửa
    $form.on('submit', function (e) {
        e.preventDefault();
        if (!$form.valid()) return;

        const $btn = $form.find('button[type="submit"]').prop('disabled', true);
        $form.find('.loi-modal').addClass('d-none').text('');

        App.post('/Account/UpdateContact', {
            sdt: $('#lhSdt').val().trim() || null,
            email: $('#lhEmail').val().trim() || null,
            diaChi: $('#lhDiaChi').val().trim() || null
        }).then(function (res) {
            // Hiển thị lại giá trị đã chuẩn hóa từ server
            hienTai = { sdt: res.data.sdt || '', email: res.data.email || '', diaChi: res.data.diaChi || '' };
            hienThi(hienTai);
            modal.hide();
            App.toast(res.message);
        }).catch(function (err) {
            if (err.errors && App.showFieldErrors($form, err.errors)) return;
            $form.find('.loi-modal').removeClass('d-none').text(App.errorText(err));
        }).finally(function () {
            $btn.prop('disabled', false);
        });
    });
});
