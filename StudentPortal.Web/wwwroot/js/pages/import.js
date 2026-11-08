// FR-30 Nhập danh sách sinh viên từ Excel
$(function () {
    'use strict';

    const KICH_THUOC_TOI_DA = 5 * 1024 * 1024;
    let fileLoiBase64 = null;

    // Tạo ô bảng bằng .text() để tránh XSS
    function cell(text, className) {
        return $('<td>').text(text).addClass(className || '');
    }

    function anKetQua() {
        $('#importResult, #importLoi').addClass('d-none');
    }

    // Báo cáo kết quả BM05
    function showResult(d) {
        $('#importMeta').text('File: ' + d.tenFile + ' – Người nhập: ' + d.nguoiNhap + ' – ' + d.thoiDiem);
        $('#soTong').text(d.tongSoDong);
        $('#soThem').text(d.soThemMoi);
        $('#soSua').text(d.soCapNhat);
        $('#soLoi').text(d.soLoi);

        const $errors = $('#importErrors').empty();
        d.loi.forEach(function (e) {
            $errors.append($('<tr>').append(cell(e.dong, 'text-center'), cell(e.maSV || ''), cell(e.cot), cell(e.noiDung)));
        });
        $('#importErrorsWrap').toggleClass('d-none', d.loi.length === 0);

        fileLoiBase64 = d.fileLoiBase64;
        $('#importResult').removeClass('d-none');
    }

    function showError(message) {
        $('#importLoi').removeClass('d-none').text(message);
    }

    $('#btnTaiLoi').on('click', function () {
        if (!fileLoiBase64) return;
        const bytes = Uint8Array.from(atob(fileLoiBase64), function (c) { return c.charCodeAt(0); });
        const blobUrl = URL.createObjectURL(new Blob([bytes], {
            type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
        }));
        const a = document.createElement('a');
        a.href = blobUrl;
        a.download = 'DanhSachLoiNhap.xlsx';
        document.body.appendChild(a);
        a.click();
        a.remove();
        URL.revokeObjectURL(blobUrl);
    });

    $('#formImport').on('submit', function (e) {
        e.preventDefault();
        anKetQua();

        const file = $('#fileExcel')[0].files[0];
        if (!file) return showError('Vui lòng chọn file Excel.');
        if (file.size > KICH_THUOC_TOI_DA) return showError('File vượt quá 5 MB.');

        const formData = new FormData();
        formData.append('file', file);

        $('#btnImport').prop('disabled', true);
        $('#importProgress').removeClass('d-none');

        App.upload('/Import/Upload', formData).then(function (res) {
            showResult(res.data);
            $('#formImport')[0].reset();
        }).catch(function (err) {
            showError(err.message);
        }).finally(function () {
            $('#btnImport').prop('disabled', false);
            $('#importProgress').addClass('d-none');
        });
    });
});
