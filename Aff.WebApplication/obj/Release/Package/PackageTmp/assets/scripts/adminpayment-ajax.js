//== Class definition

var DatatableStatistic = function() {
  //== Private functions

    // basic demo

  var statistic = function() {
      var status = $('#userStatusSelected').val();
    var datatable = $('.m_datatable').mDatatable({
      // datasource definition
      data: {
        type: 'remote',
        source: {
          read: {
            // sample GET method
            method: 'GET',
            url: '/Account/SearchUserAvailableBalance?userstatus=' + status,
            map: function(raw) {
                // sample data mapping
                if ($('#userStatusSelected').val() == null)
                {
                    $('#userStatusSelected').val(1);
                }
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
            field: 'UserId',
            title: 'ID',
            width: 40,
            textAlign: 'center',
        }, {
            field: 'FullName',
            title: 'Tên thành viên',
            width: 130,
        }, {
            field: 'Email',
            title: 'Email',
            width: 150,
        },  {
            field: 'Phone',
            title: 'Số ĐT',
            width: 90,
        },  {
            field: 'Address',
            title: 'Địa chỉ',
        },
        {
            field: 'Company',
            title: 'Công ty',
            width: 80,
        },
        {
            field: 'AvailableBalance',
            title: 'Số dư khả dụng',
            width: 110,
            template: function (data) {
                return formatNumber(data.AvailableBalance, '.', ',');
            },

        }, {
            field: 'BankName',
            title: 'Thông tin tài khoản',
            width: 300,
            template: function (row) {
                if (row.BankAccount && row.BankAccount != "")
                {
                    return "Ngân hàng: " + row.BankName + " - Số TK: " + row.BankAccount + " - Tên TK: " + row.BankOwnerName + " - Chi nhánh: " + row.BankAddress + ".";
                }
                return "";
                
            }
        }, 
      {
          field: 'Actions',
          width: 110,
          title: 'Actions',
          sortable: false,
          overflow: 'visible',
          template: function (row, index, datatable) {
              if (row.AvailableBalance >= 50000 && row.BankAccount && row.BankAccount != "") {
                  return "<button type=\"button\" class=\"btn btn-sm m-btn--pill btn-info btn-show-model\" row-id='" + row.UserId + "' row-value='" + row.AvailableBalance + "' data-toggle=\"modal\" data-target=\"#m_modal_3\">Chuyển tiền</button>";
              }
              else return "";
          },
      }],
    });

    $("#process-payment").click(function () {
        var idU = $("#confirmId").val();
        //var rowValue = $("#m_touchspin_2_2").val();
        var txtSearch = $("#generalSearch").val();
        $('#m_modal_3').modal('hide');
        mApp.block('.m-datatable__head', {
            overlayColor: '#000000',
            type: 'loader',
            state: 'primary',
            message: 'Processing...'
        });
        $.ajax({
            type: "Post",
            url: "/Account/CreateNewPayment",
            data: { currentUserId: idU, generalSearch: txtSearch },
            success: function (response) {
                mApp.unblock('.m-datatable__head');
                if (response.IsError === false) {
                    $.notify({
                        icon: 'glyphicon glyphicon-star',
                        message: "Xử lý thành công!"
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
                    datatable.load();
                }
                else {
                    $.notify({
                        icon: 'glyphicon glyphicon-star',
                        message: "Có lỗi xảy ra trong quá trình xử lý. Xin vui lòng thử lại!"
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
                    //window.location.href = '/Payment/AdminPayment';
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

    $('#m_form_status').on('change', function() {
      // shortcode to datatable.getDataSourceParam('query');
      var query = datatable.getDataSourceQuery();
      query.Status = $(this).val().toLowerCase();
      // shortcode to datatable.setDataSourceParam('query', query);
      datatable.setDataSourceQuery(query);
      datatable.load();
    }).val(typeof query.Status !== 'undefined' ? query.Status : '');

      $('#userStatusSelected').on('change', function () {
      // shortcode to datatable.getDataSourceParam('query');
      var query = datatable.getDataSourceQuery();
      query.Type = $(this).val().toLowerCase();
      // shortcode to datatable.setDataSourceParam('query', query);
      datatable.setDataSourceQuery(query);
      datatable.load();
    }).val(typeof query.Type !== 'undefined' ? query.Type : '');

      $('#btn-user-export').on('click', function () {
          window.open('/Account/ExportUserToExcel', '_blank');
      });

      $('#btn-bk-ds-export').on('click', function () {
          window.open('/Account/ExportBangKeDoiSoatToExcel', '_blank');
      });

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
    $(document).on("click", ".btn-show-model", function () {
        var rowId = $(this).attr("row-id");
        var rowData = $(this).attr("row-value");
        $("#confirmId").val(rowId);
        $("#custom-print").val(rowData);
    });
    $(document).on("click", ".btn-print", function () {
        var htmlData = $(".m-datatable--loaded").html();
        $("#mCSB_2_container").html(htmlData);
        $("#mCSB_2_container .m-datatable__pager").remove();
        $('#m_modal_print .m-datatable__table tr').find('th:last, td:last').remove();

        setTimeout(function () { $('#m_modal_print').modal('hide') }, 500);
        setTimeout(function () {
            if ($('#m_modal_print').css('display') == 'block') {
                //var modalId = $(event.target).closest('.modal').attr('id');
                $('body').css('visibility', 'hidden');
                $("#m_modal_print").css('visibility', 'visible');
                $("#ajax_data").css('display', 'none');
                $('#m_modal_print').removeClass('modal');
                window.print();
                $('body').css('visibility', 'visible');
                $("#ajax_data").css('display', 'block');
                $('#m_modal_print').addClass('modal');
                //$('#m_modal_print').modal('hide');
            } else {
                //window.print();
            }
        }, 200);
        

        
    });
    $('#m_touchspin_2, #m_touchspin_2_2').TouchSpin({
        buttondown_class: 'btn btn-secondary',
        buttonup_class: 'btn btn-secondary',

        min: 50000,
        max: 50000000,
        stepinterval: 50,
        maxboostedstep: 10000000,
        step: 100,
        postfix: '$'
    });
});