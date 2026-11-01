// FR-30 Nhập Excel - Create (upload) và Delete (xóa phiên nhập) bằng jQuery AJAX
$(function () {
    'use strict';

    const token = $('input[name="__RequestVerificationToken"]').val();
    const $body = $('#phienNhapBody');

    // Tạo ô bảng bằng .text() để tránh XSS
    function cell(text, cls) {
        return $('<td>').text(text).addClass(cls || '');
    }

    function errorMessage(xhr, fallback) {
        if (xhr.responseJSON && xhr.responseJSON.message) return xhr.responseJSON.message;
        if (xhr.status === 401 || xhr.status === 403) return 'Bạn không có quyền thực hiện thao tác này.';
        if (xhr.status === 413) return 'File vượt quá dung lượng cho phép.';
        return fallback;
    }

    function renderRow(p) {
        return $('<tr>').attr('data-id', p.id).append(
            cell(p.id),
            cell(p.thoiDiemFormatted),
            cell(p.username),
            cell(p.tenFile),
            cell(p.soThemMoi, 'text-end'),
            cell(p.soCapNhat, 'text-end'),
            cell(p.soLoi, 'text-end' + (p.soLoi > 0 ? ' text-danger fw-semibold' : '')),
            $('<td class="text-center">').append(
                $('<button type="button" class="btn btn-sm btn-outline-danger btn-xoa-phien">')
                    .attr('data-id', p.id)
                    .attr('title', 'Xóa phiên nhập')
                    .append('<i class="bi bi-trash"></i>')
            )
        );
    }

    function renderEmpty() {
        $body.empty().append(
            $('<tr>').append(cell('Chưa có lần nhập nào.', 'text-center text-muted py-4').attr('colspan', 8))
        );
    }

    let currentPage = 1;
    const pageSize = 10;

    // Gọi AJAX lấy một trang (server dùng Skip/Take)
    function loadPhienNhap(page) {
        currentPage = page || currentPage;
        $.ajax({
            url: '/XuatNhap/DanhSachPhienNhap',
            type: 'GET',
            data: { page: currentPage, pageSize: pageSize },
            dataType: 'json'
        }).done(function (res) {
            currentPage = res.pageIndex;
            $('#phienNhapTotal').text(res.totalItems + ' phiên');
            renderPhanTrang(document.getElementById('phienNhapPaging'), res.pageIndex, res.totalPages, loadPhienNhap);
            if (!res.items.length) return renderEmpty();
            $body.empty().append(res.items.map(renderRow));
        }).fail(function (xhr) {
            $body.empty().append(
                $('<tr>').append(cell(errorMessage(xhr, 'Không tải được lịch sử nhập.'), 'text-center text-danger').attr('colspan', 8))
            );
        });
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

    // CREATE: upload file bằng FormData
    $('#formImport').on('submit', function (e) {
        e.preventDefault();

        const file = $('#fileExcel')[0].files[0];
        if (!file) return showError('Vui lòng chọn file Excel.');

        const formData = new FormData();
        formData.append('file', file);

        $.ajax({
            url: '/XuatNhap/Import',
            type: 'POST',
            data: formData,
            processData: false, // giữ nguyên FormData, không chuyển thành query string
            contentType: false, // để trình duyệt tự đặt multipart boundary
            headers: { 'RequestVerificationToken': token },
            beforeSend: function () {
                $('#btnImport').prop('disabled', true);
                $('#importSpinner').removeClass('d-none');
            }
        }).done(function (res) {
            showResult(res);
            $('#formImport')[0].reset();
            loadPhienNhap(1);
        }).fail(function (xhr) {
            showError(errorMessage(xhr, 'Nhập file thất bại.'));
        }).always(function () {
            $('#btnImport').prop('disabled', false);
            $('#importSpinner').addClass('d-none');
        });
    });

    // DELETE: xóa phiên nhập, gỡ dòng khỏi DOM
    $body.on('click', '.btn-xoa-phien', function () {
        const id = $(this).data('id');
        if (!confirm('Xóa phiên nhập #' + id + '? Hồ sơ sinh viên đã nhập không bị ảnh hưởng.')) return;

        const $row = $(this).closest('tr');
        $.ajax({
            url: '/XuatNhap/XoaPhienNhap/' + encodeURIComponent(id),
            type: 'DELETE',
            headers: { 'RequestVerificationToken': token }
        }).done(function () {
            // Tải lại trang hiện tại để dồn bản ghi từ trang sau lên
            $row.fadeOut(300, function () { loadPhienNhap(); });
        }).fail(function (xhr) {
            alert(errorMessage(xhr, 'Xóa phiên nhập thất bại.'));
        });
    });

    loadPhienNhap(1);
});
