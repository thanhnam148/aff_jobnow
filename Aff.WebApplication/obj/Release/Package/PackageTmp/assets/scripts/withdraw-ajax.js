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
                        url: '/Payment/SearchPayments',
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
                  field: 'PaymentId',
                  title: 'ID',
                  width: 140,
                  textAlign: 'center',
              },
              {
                  field: 'CreatedDate',
                  title: 'Ngày tạo',
                  type: 'date',
                  format: 'DD/MM/YYYY, h:MM:ss TT',
                  width: 150,
              },
              {
                  field: 'PaymentStatus',
                  title: 'Trạng thái',
                  width: 100,
                  template: function (row) {
                      var status = {
                          1: { 'title': 'Chờ duyệt', 'class': 'm-badge--brand' },
                          2: { 'title': 'Chờ thanh toán', 'class': ' m-badge--metal' },
                          3: { 'title': 'Đã thanh toán', 'class': ' m-badge--primary' },
                          0: { 'title': 'Hủy', 'class': ' m-badge--danger' },
                      };
                      return '<span class="m-badge ' + status[row.PaymentStatus].class + ' m-badge--wide">' + status[row.PaymentStatus].title + '</span>';
                  }
              },
              {
                  field: 'AmountEarning',
                  title: 'Số tiền chờ rút',
                  width: 150,
                  template: function (data) {
                      return formatNumber(data.AmountEarning, '.', ',');
                  },
              },
              {
                  field: 'PaymentMethod',
                  title: 'Phương thức rút tiền',
                  template: function (row) {
                      var status = {
                          1: { 'title': 'Chuyển khoản', 'class': 'm-badge--info' },
                      };
                      return '<span class="m-badge ' + status[row.PaymentMethod].class + ' m-badge--wide">' + status[row.PaymentMethod].title + '</span>';
                  }
              },

            {
                field: 'AccountInfo',
                title: 'Tài khoản rút tiền',
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
});