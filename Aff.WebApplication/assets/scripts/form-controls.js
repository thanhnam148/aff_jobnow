//== Class definition

var FormControls = function () {
    //== Private functions
    var validWithdraw = function () {
        $("#customwithdraw").click(function (){
            //alert("Hi");
            $.ajax({
                url: "/Payment/ValidePayment",
                type: "Post",
                success: function (response) {
                    if (response.IsError === false) {
                        window.location.href = '/payment/CreatePayment'
                    }
                    else
                    {
                        //alert(response.Message);
                        var alert = $('#m_form_1_msg');
                        $('#m_form_1_msg .m-alert__text').html(response.Message);
                        alert.removeClass('m--hide').show();
                        mApp.scrollTo(alert, -200);
                    }
                }
            });
        });       
    }

    var createpayment = function () {
        $("#create-payment-valid").validate({
            // define validation rules
            rules: {
                PublisherEarning: {
                    required: true,
                    digits: true
                }
            },

            //display error alert on form submit  
            invalidHandler: function (event, validator) {
                var alert = $('#m_form_1_msg');
                alert.removeClass('m--hide').show();
                mApp.scrollTo(alert, -200);
            },

            submitHandler: function (form) {
                form[0].submit(); // submit the form
            }
        });
    }
    var UpdateUserInfo = function () {
        $("#update-user-info").validate({
            // define validation rules
            rules: {
                FullName: {
                    required: true
                },
                Phone: {
                    required: true
                }
                ,
                BankAccount: {
                    required: true
                },
                BankName: {
                    required: true
                }
                ,
                BankAddress: {
                    required: true
                },
                BankOwnerName: {
                    required: true
                }
            },

            //display error alert on form submit  
            invalidHandler: function (event, validator) {
                var alert = $('#m_form_1_msg');
                alert.removeClass('m--hide').show();
                mApp.scrollTo(alert, -200);
            },

            submitHandler: function (form) {
                form[0].submit(); // submit the form
            }
        });
    }

    var ChangePasswordUser = function () {
        $("#change-password-user").validate({
            // define validation rules
            rules: {
                CurrentPassword: {
                    required: true
                },
                NewPassword: {
                    required: true,
                    minlength: 4
                }
                ,
                ReNewPassword: {
                    required: true,
                    minlength: 4,
                    passwordMatch: true
                }
            },
            messages: {
                CurrentPassword: {
                    required: "Trường này cần phải nhập"
                },
                NewPassword: {
                    required: "Trường này cần phải nhập",
                    minlength: "Mật khẩu của bạn phải nhiều hơn 4 ký tự"
                },
                ReNewPassword: {
                    required: "Trường này cần phải nhập",
                    minlength: "Mật khẩu của bạn phải nhiều hơn 4 ký tự",
                    passwordMatch: "Mật khẩu nhập lại phải trùng với mật khẩu mới" // custom message for mismatched passwords
                }
            },
            //display error alert on form submit  
            invalidHandler: function (event, validator) {
                var alert = $('#m_form_1_msg');
                alert.removeClass('m--hide').show();
                mApp.scrollTo(alert, -200);
            },

            submitHandler: function (form) {
                form[0].submit(); // submit the form
            }
        });
    }

    var demo1 = function () {
        $( "#m_form_1" ).validate({
            // define validation rules
            rules: {
                email: {
                    required: true,
                    email: true,
                    minlength: 10 
                },
                url: {
                    required: true 
                },
                digits: {
                    required: true,
                    digits: true
                },
                creditcard: {
                    required: true,
                    creditcard: true 
                },
                phone: {
                    required: true,
                    phoneUS: true 
                },
                option: {
                    required: true
                },
                options: {
                    required: true,
                    minlength: 2,
                    maxlength: 4
                },
                memo: {
                    required: true,
                    minlength: 10,
                    maxlength: 100
                },

                checkbox: {
                    required: true
                },
                checkboxes: {
                    required: true,
                    minlength: 1,
                    maxlength: 2
                },
                radio: {
                    required: true
                }
            },
            
            //display error alert on form submit  
            invalidHandler: function(event, validator) {     
                var alert = $('#m_form_1_msg');
                alert.removeClass('m--hide').show();
                mApp.scrollTo(alert, -200);
            },

            submitHandler: function (form) {
                //form[0].submit(); // submit the form
            }
        });       
    }

    var demo2 = function () {
        $( "#m_form_2" ).validate({
            // define validation rules
            rules: {
                email: {
                    required: true,
                    email: true 
                },
                url: {
                    required: true 
                },
                digits: {
                    required: true,
                    digits: true
                },
                creditcard: {
                    required: true,
                    creditcard: true 
                },
                phone: {
                    required: true,
                    phoneUS: true 
                },
                option: {
                    required: true
                },
                options: {
                    required: true,
                    minlength: 2,
                    maxlength: 4
                },
                memo: {
                    required: true,
                    minlength: 10,
                    maxlength: 100
                },

                checkbox: {
                    required: true
                },
                checkboxes: {
                    required: true,
                    minlength: 1,
                    maxlength: 2
                },
                radio: {
                    required: true
                }
            },
            
            //display error alert on form submit  
            invalidHandler: function(event, validator) {     
                var alert = $('#m_form_2_msg');
                alert.removeClass('m--hide').show();
                mApp.scrollTo(alert, -200);
            },

            submitHandler: function (form) {
                //form[0].submit(); // submit the form
            }
        });       
    }

    return {
        // public functions
        init: function () {
            validWithdraw();
            createpayment();
            UpdateUserInfo();
            ChangePasswordUser();
            //demo1(); 
            //demo2(); 
        }
    };
}();

jQuery(document).ready(function () {
    jQuery.validator.addMethod('passwordMatch', function (value, element) {
        var password = $("#NewPassword").val();
        var confirmPassword = $("#ReNewPassword").val();
        if (password != confirmPassword) {
            return false;
        } else {
            return true;
        }
    }, "Your Passwords Must Match");
    FormControls.init();
});