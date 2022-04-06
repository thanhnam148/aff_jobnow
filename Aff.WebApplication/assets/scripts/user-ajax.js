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
            url: '/Account/SearchUserRegister',
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
            width: 90,
        }, {
            field: 'Address',
          title: 'Địa chỉ',
        },
        //{
        //    field: 'Company',
        //  title: 'Công ty',
        //},
        {
            field: 'AvailableBalance',
            title: 'Số dư khả dụng',
            width: 100,
            template: function (data) {
                return formatNumber(data.AvailableBalance, '.', ',');
            },

        }, {
            field: 'IsActive',
            title: 'Trạng thái',
            width: 100,
            template: function (row) {
                var status = {
                    true: { 'title': 'Hoạt động', 'class': 'm-badge--primary' },
                    false: { 'title': 'Ngừng hoạt động', 'class': ' m-badge--danger' },
                };
                return '<span class="m-badge ' + status[row.IsActive].class + ' m-badge--wide">' + status[row.IsActive].title + '</span>';
            }
        }, {
            field: 'CreatedDate',
          title: 'Ngày tạo',
          
        },
      {
          field: 'Actions',
          width: 110,
          title: 'Actions',
          sortable: false,
          overflow: 'visible',
          template: function (row, index, datatable) {
              var dropup = (datatable.getPageSize() - index) <= 4 ? 'dropup' : '';
              return '\
                        <div class="dropdown ' + dropup + '">\
							<a href="#" class="btn m-btn m-btn--hover-accent m-btn--icon m-btn--icon-only m-btn--pill" data-toggle="dropdown">\
                                <i class="la la-ellipsis-h"></i>\
                            </a>\
						  	<div class="dropdown-menu dropdown-menu-right">\
						    	<a class="dropdown-item remotelogin" data-value="' + row.UserId + '" href="#"><i class="la la-leaf"></i>Remote Login</a>\
						  	</div>\
						</div>\
						<a href="#" class="m-portlet__nav-link btn m-btn m-btn--hover-accent m-btn--icon m-btn--icon-only m-btn--pill" title="Edit details">\
							<i class="la la-edit"></i>\
						</a>\
						<a href="#" class="m-portlet__nav-link btn m-btn m-btn--hover-danger m-btn--icon m-btn--icon-only m-btn--pill" title="Delete">\
							<i class="la la-trash"></i>\
						</a>\
					';
          },
      }],
    });

    $(".remotelogin").click(function () {
        var rowId = $(this).attr("data-value");
        $.ajax({
            url: "/Account/RemoteLoginAdmin",
            type: "Post",
            data: { userid: rowId },
            success: function (response) {
                if (response.IsError === false) {
                    window.open('/Admin', '_blank');
                }
            }
        });
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

    $('#m_form_status').on('change', function() {
      // shortcode to datatable.getDataSourceParam('query');
      var query = datatable.getDataSourceQuery();
      query.Status = $(this).val().toLowerCase();
      // shortcode to datatable.setDataSourceParam('query', query);
      datatable.setDataSourceQuery(query);
      datatable.load();
    }).val(typeof query.Status !== 'undefined' ? query.Status : '');

      $(document).on('click', ".daterangepicker .applyBtn", function () {
      // shortcode to datatable.getDataSourceParam('query');
      var query = datatable.getDataSourceQuery();
      query.Range = $("#m_daterangepicker_2 .form-control").val().toLowerCase();
      // shortcode to datatable.setDataSourceParam('query', query);
      datatable.setDataSourceQuery(query);
      datatable.load();
      }).val(typeof query.Range !== 'undefined' ? query.Range : '');

    $('#m_form_status, #m_form_type').selectpicker();

    $(document).on("click", ".remotelogin", function () {
        var rowId = $(this).attr("data-value");
        $.ajax({
            url: "/Account/RemoteLoginAdmin",
            type: "Post",
            data: { userid: rowId },
            success: function (response) {
                if (response.IsError === false) {
                    window.location = "/Admin";
                }
            }
        });
    });

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

    $(document).on("click", ".btn-show-model", function () {
        console.log('111');
        var rowId = $(this).attr("row-id");
        var rowData = $(this).attr("row-value");
        $("#confirmId").val(rowId);
        $("#m_touchspin_2_2").val(rowData);
    });
});