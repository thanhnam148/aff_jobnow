var inActive = 0;
var active = 1;

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
                        url: '/Account/GetAllUserFisrt',
                        map: function (raw) {
                            // sample data mapping
                            console.log(raw);
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
                        return '<a href="/Account/UserDetail?userId=' + data.UserId + '" class="export_matchingUser" target="_blank" data="' + data.UserId + '">' + data.Email + '</a>';
                    }
                }, {
                    field: 'FullName',
                    title: 'Tên thành viên',
                    width: 160,
                }, {
                    field: 'Phone',
                    title: 'Số ĐT',
                    width: 90,
                },

                {
                    field: 'Address',
                    title: 'Địa chỉ',
                },
                {
                    field: 'LevelString',
                    title: 'Cấp độ',
                },
                {
                    field: 'CreatedDate',
                    title: 'Ngày tạo',
                    type: 'date',
                    format: 'DD/MM/YYYY',
                },
                {
                    field: 'TotalBalanceDirect',
                    title: 'Thu nhập từ liên kết trực tiếp',
                    template: function (data) {
                        return formatNumber(data.TotalBalanceDirect, '.', ',');
                    },

                },
                {
                    field: 'TotalBalanceBonus',
                    title: 'Thu nhập từ mã giới thiệu',
                    template: function (data) {
                        return formatNumber(data.TotalBalanceBonus, '.', ',');
                    },

                },
                {
                    field: 'AvailableBalance',
                    title: 'Số dư hiện có',
                    template: function (data) {
                        return formatNumber(data.AvailableBalance, '.', ',');
                    },

                },
                {
                    field: 'BalanceAfterFee',
                    title: 'Số dư sau ví',
                    template: function (data) {
                        return formatNumber(data.BalanceAfterFee, '.', ',');
                    },
                },
            ],
        });

        function formattedDateHourMinutes(date) {
            var d = new Date(date);
            let month = String(d.getMonth() + 1);
            let day = String(d.getDate());
            const year = String(d.getFullYear());
            let hour = "" + d.getHours(); if (hour.length == 1) { hour = "0" + hour; }
            let minute = "" + d.getMinutes(); if (minute.length == 1) { minute = "0" + minute; }
            //let second = "" + d.getSeconds(); if (second.length == 1) { second = "0" + second; }
            if (month.length < 2) month = '0' + month;
            if (day.length < 2) day = '0' + day;
            return `${day}/${month}/${year} ${hour}:${minute} `;
        }
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
            console.log(query);
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