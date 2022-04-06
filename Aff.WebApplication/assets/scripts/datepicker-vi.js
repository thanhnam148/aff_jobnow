;(function($){
	$.fn.datepicker.dates['vi'] = {
		days: ["Chủ nhật", "Thứ hai", "Thứ ba", "Thứ tư", "Thứ năm", "Thứ sáu", "Thứ bảy"],
		daysShort: ["CN", "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7"],
		daysMin: ["CN", "T2", "T3", "T4", "T5", "T6", "T7"],
		months: ["Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6", "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12"],
		monthsShort: ["Th1", "Th2", "Th3", "Th4", "Th5", "Th6", "Th7", "Th8", "Th9", "Th10", "Th11", "Th12"],
		today: "Hôm nay",
		clear: "Xóa",
		format: "dd/mm/yyyy"
	};

}(jQuery));


$('#m_datepicker_3, #m_datepicker_3_validate, .m_datepicker').datepicker({
    
    todayBtn: "linked",
    clearBtn: true,
    todayHighlight: true,
    templates: {
        leftArrow: '<i class="la la-angle-left"></i>',
        rightArrow: '<i class="la la-angle-right"></i>'
    },
    language: "vi"
});

$("#m_dateRangePicker").daterangepicker({
	buttonClasses: "m-btn btn",
	applyClass: "btn-primary",
    cancelClass: "btn-secondary",
	locale: {
        format: 'DD/MM/YYYY',
        applyLabel: "Chọn",
        cancelLabel: "Hủy",
		firstDay: 1,
                daysOfWeek: [
                             "CN",
                             "T2",
                             "T3",
                             "T4",
                             "T5",
                             "T6",
                             "T7",
                ],
                "monthNames": [
                           "Tháng 1",
                           "Tháng 2",
                           "Tháng 3",
                           "Tháng 4",
                           "Tháng 5",
                           "Tháng 6",
                           "Tháng 7",
                           "Tháng 8",
                           "Tháng 9",
                           "Tháng 10",
                           "Tháng 11",
                           "Tháng 12"
                ],
	},
	language:"cn"
}, function(a, t, n) {
    $(".m_datepicker .form-control").val(a.format("DD/MM/YYYY") + " - " + t.format("DD/MM/YYYY"))
})