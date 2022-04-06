//== Class definition

var DatatableStatistic = function () {
    //== Private functions

    // basic demo

    var statistic = function () {
        var moth = $('#DataOfMonth').val();
        var gnSearch = $('#generalSearch').val()
        var datatable = $('.m_datatable').mDatatable({
            // datasource definition
            data: {
                type: 'remote',
                source: {
                    read: {
                        // sample GET method
                        method: 'GET',
                        url: '/Account/MatchingUserRegister/?monthOfYear=' + moth + '&generalSearch=' + gnSearch,
                        map: function (raw) {
                            // sample data mapping
                            var dataSet = raw;
                            if (typeof raw.data !== 'undefined') {
                                dataSet = raw.data;
                            }
                            return dataSet;
                        },
                    },
                },
                pageSize: 10,
                serverPaging: true,
                serverFiltering: true,
                serverSorting: true,
            },

            // layout definition
            layout: {
                theme: 'default', // datatable theme
                //class: '', // custom wrapper class
                scroll: false, // enable/disable datatable scroll both horizontal and vertical when needed.
                footer: false // display/hide footer
            },

            // column sorting
            sortable: true,

            pagination: true,

            toolbar: {
                // toolbar items
                items: {
                    // pagination
                    pagination: {
                        // page size select
                        pageSizeSelect: [10, 20, 30, 50, 100],
                    },
                },
            },

            search: {
                input: $('#generalSearch'),
            },
            translate: {
                records: {
                    processing: 'Đang tải dữ liệu...',
                    noRecords: 'Không có dữ liệu'
                },
                toolbar: {
                    pagination: {
                        items: {
                            default: {
                                first: 'Về trang đầu',
                                prev: 'Trang trước',
                                next: 'Trang sau',
                                last: 'Đến trang cuối',
                            },
                            info: 'Tống số {{total}} bản ghi'
                        }
                    }
                }
            },
            // columns definition
            columns: [
                {
                    field: 'UserId',
                    title: 'ID',
                    width: 40,
                    textAlign: 'center',
                    
                
                }, {
                    field: 'Email',
                    title: 'Email',
                    width: 180,
                    template: function (data) {
                        return '<a href="/Account/ExportMatchingUser?userId=' + data.UserId+'" class="export_matchingUser" target="_blank" data="' + data.UserId +'">' + data.Email +'</a>';
                    }
                }, {
                    field: 'FullName',
                    title: 'Tên thành viên',
                    width: 160,
                }, {
                    field: 'Phone',
                    title: 'Số ĐT',
                    width: 90,
                }, {
                    field: 'AffCode',
                    title: 'Mã liên kết',
                    width: 80,
                },
                {
                    field: 'AvailableBalance',
                    title: 'Số dư khả dụng đầu kỳ',
                    width: 100,
                    template: function (data) {
                        return formatNumber(data.AvailableBalance, '.', ',');
                    },

                },
                {
                    field: 'AlreadyAmountTranfer',
                    title: 'Số tiền cần chốt',
                    width: 100,
                    template: function (data) {
                        return formatNumber(data.AlreadyAmountTranfer, '.', ',');
                    },

                },
                {
                    field: 'TotalAvailableAmountTranfer',
                    title: 'Số dư khả dụng sau chốt',
                    width: 100,
                    template: function (data) {
                        return formatNumber(data.TotalAvailableAmountTranfer, '.', ',');
                    },

                }, {
                    field: 'StatusAmount',
                    title: 'Trạng thái',
                    width: 100,
                    template: function (row) {
                        var status = {
                            0: { 'title': 'Chưa đối soát', 'class': 'm-badge--danger' },
                            1: { 'title': 'Đã đối soát', 'class': 'm-badge--primary' },
                        };
                        return '<span class="m-badge ' + status[row.StatusAmount].class + ' m-badge--wide">' + status[row.StatusAmount].title + '</span>';
                    }
                },
            ],
        });


        $("#btnProcessData").click(function () {
            $('#m_modal_3').modal('hide');
            mApp.block('.m-datatable__head', {
                overlayColor: '#000000',
                type: 'loader',
                state: 'primary',
                message: 'Processing...'
            });
            $.ajax({
                url: "/Account/ProcessMatchingUser",
                type: "Post",
                data: { monthOfYear: $('#DataOfMonth').val(), generalSearch: $('#generalSearch').val() },
                success: function (response) {
                    mApp.unblock('.m-datatable__head');
                    if (response.IsError === false) {
                        $.notify({
                            icon: 'glyphicon glyphicon-star',
                            message: "Đối soát dữ liệu thành công!"
                        }, {
                            type: 'success',
                            animate: {
                                enter: 'animated fadeInUp',
                                exit: 'animated fadeOutRight'
                            },
                            placement: {
                                from: "top",
                                align: "right"
                            },
                            offset: 20,
                            spacing: 10,
                            z_index: 1031,
                        });
                        //var query = datatable.getDataSourceQuery();
                        //query.Type = $(this).val().toLowerCase();
                        //// shortcode to datatable.setDataSourceParam('query', query);
                        //datatable.setDataSourceQuery(query);
                        datatable.load();
                        //window.location.href = '/Account/MatchingUser'

                    }
                    else {
                        //alert(response.Message);
                        $.notify({
                            icon: 'glyphicon glyphicon-star',
                            message: "Có lỗi xảy ra trong quá trình đối xoát. Xin vui lòng thử lại!"
                        }, {
                            type: 'danger',
                            animate: {
                                enter: 'animated fadeInUp',
                                exit: 'animated fadeOutRight'
                            },
                            placement: {
                                from: "top",
                                align: "right"
                            },
                            offset: 20,
                            spacing: 10,
                            z_index: 1031,
                        });
                    }
                }
            });
        });

        function formatNumber(nStr, decSeperate, groupSeperate) {
            nStr += '';
            x = nStr.split(decSeperate);
            x1 = x[0];
            x2 = x.length > 1 ? '.' + x[1] : '';
            var rgx = /(\d+)(\d{3})/;
            while (rgx.test(x1)) {
                x1 = x1.replace(rgx, '$1' + groupSeperate + '$2');
            }
            return x1 + x2;
        }
        var query = datatable.getDataSourceQuery();

        $('#m_form_status').on('change', function () {
            // shortcode to datatable.getDataSourceParam('query');
            var query = datatable.getDataSourceQuery();
            query.Status = $(this).val().toLowerCase();
            // shortcode to datatable.setDataSourceParam('query', query);
            datatable.setDataSourceQuery(query);
            datatable.load();
        }).val(typeof query.Status !== 'undefined' ? query.Status : '');

        $('#m_form_type').on('change', function () {
            // shortcode to datatable.getDataSourceParam('query');
            var query = datatable.getDataSourceQuery();
            query.Type = $(this).val().toLowerCase();
            // shortcode to datatable.setDataSourceParam('query', query);
            datatable.setDataSourceQuery(query);
            datatable.load();
        }).val(typeof query.Type !== 'undefined' ? query.Type : '');

        $('#m_form_status, #m_form_type').selectpicker();


    };

    return {
        // public functions
        init: function () {
            statistic();
            //processMatching();
            //processPayment();
        },
    };
}();

jQuery(document).ready(function () {
    $("#DataOfMonth").datepicker({
        format: "mm-yyyy",
        viewMode: "months",
        minViewMode: "months",
        language: "vi"
    });
    $('#m_datepicker_123').datepicker({
        format: "mm-yyyy",
        viewMode: "months",
        minViewMode: "months",
        language: "vi"
    });
    DatatableStatistic.init();


 
});