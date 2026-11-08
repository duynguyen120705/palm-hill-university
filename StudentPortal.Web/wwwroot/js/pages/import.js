// FR-30 Nhập danh sách sinh viên từ Excel
$(function () {
    'use strict';

    // Tạo ô bảng bằng .text() để tránh XSS
    function cell(text) {
        return $('<td>').text(text);
    }

    function showResult(res) {
        const d = res.data;
        $('#importResult').removeClass('d-none');
        $('#importSummary')
            .removeClass('alert-success alert-warning alert-danger')
            .addClass(d.soLoi > 0 ? 'alert-warning' : 'alert-success')
            .text(res.message);

        const $errors = $('#importErrors').empty();
        $('#importErrorsWrap').toggleClass('d-none', d.loi.length === 0);
        d.loi.forEach(function (e) {
            $errors.append($('<tr>').append(cell(e.dong), cell(e.maSV || ''), cell(e.loi)));
        });
    }

    function showError(message) {
        $('#importResult').removeClass('d-none');
        $('#importErrorsWrap').addClass('d-none');
        $('#importSummary')
            .removeClass('alert-success alert-warning')
            .addClass('alert-danger')
            .text(message);
    }

    $('#formImport').on('submit', function (e) {
        e.preventDefault();

        const file = $('#fileExcel')[0].files[0];
        if (!file) return showError('Vui lòng chọn file Excel.');

        const formData = new FormData();
        formData.append('file', file);

        $('#btnImport').prop('disabled', true);
        $('#importSpinner').removeClass('d-none');

        App.upload('/Import/Upload', formData).then(function (res) {
            showResult(res);
            $('#formImport')[0].reset();
        }).catch(function (err) {
            showError(err.message);
        }).finally(function () {
            $('#btnImport').prop('disabled', false);
            $('#importSpinner').addClass('d-none');
        });
    });
});
