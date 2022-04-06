//== Class definition

var DatatableStatistic = function () {
    //== Private functions

    // basic demo
    var statistic = function () {

        var datatable = $('.m_datatable').mDatatable({
            // datasource definition
            data: {
                type: 'remote',
                source: {
                    read: {
                        // sample GET method
                        method: 'GET',
                        url: '/Loan/SearchLoansAdmin',
                        map: function (raw) {
                            // sample data mapping
                            var dataSet = raw;
                            if (typeof raw.data !== 'undefined') {
                                dataSet = raw.data;
                                $("#total-purchase-amount").text(formatNumber(raw.totalPurchase, ",", "."));
                                $("#total-payment-amount").text(formatNumber(raw.totalPayment, ",", "."));
                                $("#total-profit-amount").text(formatNumber(raw.totalProfit, ",", "."));
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
            search: {
                input: $('#generalSearch'),
            },

            // columns definition
            columns: [
                {
                    field: 'LoanId',
                    title: 'Mã đơn',
                    width: 140,
                    textAlign: 'center',
                }, {
                    field: 'FullName',
                    title: 'Tên người mua',
                }, {
                    field: 'PhoneNumber',
                    title: 'Số ĐT',
                    width: 150,
                }, {
                    field: 'Address',
                    title: 'Địa chỉ',
                },
                {
                    field: 'CreatedDate',
                    title: 'Ngày tạo',
                    type: 'date',
                    format: 'DD/MM/YYYY',
                }],
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

        $(document).on('click', ".daterangepicker .applyBtn", function () {
            // shortcode to datatable.getDataSourceParam('query');
            var query = datatable.getDataSourceQuery();
            query.Range = $("#m_daterangepicker_2 .form-control").val().toLowerCase();
            // shortcode to datatable.setDataSourceParam('query', query);
            datatable.setDataSourceQuery(query);
            datatable.load();
        }).val(typeof query.Range !== 'undefined' ? query.Range : '');
    };

    return {
        // public functions
        init: function () {
            statistic();
        },
    };
}();

jQuery(document).ready(function () {
    DatatableStatistic.init();
    $('#m_daterangepicker_2').daterangepicker({
        "locale": {
            "format": "DD/MM/YYYY",
            "separator": " - ",
            "applyLabel": "Áp dụng",
            "cancelLabel": "Hủy",
            "fromLabel": "Từ",
            "toLabel": "Đến",
            "customRangeLabel": "Custom",
            "daysOfWeek": [
                "CN",
                "T2",
                "T3",
                "T4",
                "T5",
                "T6",
                "T7"
            ],
            "monthNames": [
                "Th1",
                "Th2",
                "Th3",
                "Th4",
                "Th5",
                "Th6",
                "Th7",
                "Th8",
                "Th9",
                "Th10",
                "Th11",
                "Th12"
            ],
            "firstDay": 1
        }
    }, function (start, end, label) {
        $('#m_daterangepicker_2 .form-control').val(start.format('DD-MM-YYYY') + ' / ' + end.format('DD-MM-YYYY'));
    });
});