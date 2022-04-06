//== Class definition

var DatatableStatistic = function() {
  //== Private functions

  // basic demo
  var statistic = function() {

    var datatable = $('.m_datatable').mDatatable({
      // datasource definition
      data: {
        type: 'remote',
        source: {
          read: {
            // sample GET method
            method: 'GET',
            url: '/Transaction/SearchTransaction',
            map: function(raw) {
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

      // columns definition
      columns: [
        {
            field: 'TransactionId',
          title: 'Mã giao dịch',
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
        }, {
            field: 'TotalAmount',
          title: 'Số tiền mua đơn',
          width: 180,
        }, {
            field: 'CreatedDate',
          title: 'Ngày tạo',
          type: 'date',
          format: 'DD/MM/YYYY',
        }],
    });

    var query = datatable.getDataSourceQuery();

    $('#m_form_status').on('change', function() {
      // shortcode to datatable.getDataSourceParam('query');
      var query = datatable.getDataSourceQuery();
      query.Status = $(this).val().toLowerCase();
      // shortcode to datatable.setDataSourceParam('query', query);
      datatable.setDataSourceQuery(query);
      datatable.load();
    }).val(typeof query.Status !== 'undefined' ? query.Status : '');

    $('#m_form_type').on('change', function() {
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
    init: function() {
        statistic();
    },
  };
}();

jQuery(document).ready(function() {
    DatatableStatistic.init();
});