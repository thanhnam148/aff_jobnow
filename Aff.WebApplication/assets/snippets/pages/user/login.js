//== Class Definition
var SnippetLogin = function () {

    var login = $('#m_login');

    var showErrorMsg = function (form, type, msg) {
        var alert = $('<div class="m-alert m-alert--outline alert alert-' + type + ' alert-dismissible" role="alert">\
			<button type="button" class="close" data-dismiss="alert" aria-label="Close"></button>\
			<span></span>\
		</div>');

        form.find('.alert').remove();
        alert.prependTo(form);
        alert.animateClass('fadeIn animated');
        alert.find('span').html(msg);
    }

    //== Private Functions

    var displaySignUpForm = function () {
        login.removeClass('m-login--forget-password');
        login.removeClass('m-login--signin');

        login.addClass('m-login--signup');
        login.find('.m-login__signup').animateClass('flipInX animated');
    }

    var displaySignInForm = function () {
        login.removeClass('m-login--forget-password');
        login.removeClass('m-login--signup');

        login.addClass('m-login--signin');
        login.find('.m-login__signin').animateClass('flipInX animated');
    }

    var displayForgetPasswordForm = function () {
        login.removeClass('m-login--signin');
        login.removeClass('m-login--signup');

        login.addClass('m-login--forget-password');
        login.find('.m-login__forget-password').animateClass('flipInX animated');
    }

    var handleFormSwitch = function () {
        $('#m_login_forget_password').click(function (e) {
            e.preventDefault();
            displayForgetPasswordForm();
        });

        $('#m_login_forget_password_cancel').click(function (e) {
            e.preventDefault();
            displaySignInForm();
        });

        $('#m_login_signup').click(function (e) {
            e.preventDefault();
            displaySignUpForm();
        });

        $('#m_login_signup_cancel').click(function (e) {
            e.preventDefault();
            displaySignInForm();
        });
    }

    var handleSignInFormSubmit = function () {
        $('#m_login_signin_submit').click(function (e) {
            e.preventDefault();
            var btn = $(this);
            var form = $(this).closest('form');

            form.validate({
                rules: {
                    Email: {
                        required: true,
                        email: true
                    },
                    Password: {
                        required: true
                    }
                },
                messages: {
                    Email: {
                        required: "Trường này cần phải nhập",
                        email: "Vui lòng nhập địa chỉ email hợp lệ"
                },
                    Password: {
                    required: "Trường này cần phải nhập",
                    }
            },
            });

            if (!form.valid()) {
                return;
            }

            btn.addClass('m-loader m-loader--right m-loader--light').attr('disabled', true);
            form.ajaxSubmit({
                type: "POST",
                url: '/Account/Login',
                success: function (response, status, xhr, $form) {
                    if (response != null && response.IsError === true) {
                        setTimeout(function () {
                            btn.removeClass('m-loader m-loader--right m-loader--light').attr('disabled', false);
                            showErrorMsg(form, 'danger', 'Tên truy cập hoặc mật khẩu không đúng. Xin vui lòng thử lại.');
                        }, 100);
                    } else {
                        window.location.href = "/admin";
                    }
                }
            });
        });
    }

    var handleSignUpFormSubmit = function () {
        $('#m_login_signup_submit').click(function (e) {
            e.preventDefault();

            var btn = $(this);
            var form = $(this).closest('form');

            form.validate({
                rules: {
                    fullname: {
                        required: true
                    },
                    email: {
                        required: true,
                        email: true
                    },
                    password: {
                        required: true
                    },
                    rpassword: {
                        required: true,
                        minlength: 4,
                    },
                    rrpassword: {
                        required: true,
                        minlength: 4,
                        passwordMatchRegis: true
                    },
                    Phone: {
                        required: true,
                        minlength: 10,
                        number: true
                    },

                    //AffCode: {
                    //    required: true
                    //},
                    agree: {
                        required: true
                    }
                },
                messages: {
                    fullname: {
                        required: "Trường này cần phải nhập"
                    },
                    email: {
                        required: "Trường này cần phải nhập"
                    },
                    Phone: {
                        required: "Trường này cần phải nhập",
                        minlength: "Xin vui lòng nhập đúng số điện thoại",
                        number: "Xin vui lòng nhập đúng số điện thoại",
                    },

                    //AffCode: {
                    //    required: "Trường này cần phải nhập"
                    //},
                    agree: {
                        required: "Trường này cần phải chọn"
                    },
                    password: {
                        required: "Trường này cần phải nhập",
                        minlength: "Mật khẩu của bạn phải nhiều hơn 4 ký tự"
                    },
                    rpassword: {
                        required: "Trường này cần phải nhập",
                        minlength: "Mật khẩu của bạn phải nhiều hơn 4 ký tự"
                    },
                    rrpassword: {
                        required: "Trường này cần phải nhập",
                        minlength: "Mật khẩu của bạn phải nhiều hơn 4 ký tự",
                        passwordMatchRegis: "Mật khẩu nhập lại phải trùng với mật khẩu mới"
                    }
                },
            });

            if (!form.valid()) {
                return;
            }

            btn.addClass('m-loader m-loader--right m-loader--light').attr('disabled', true);

            form.ajaxSubmit({
                url: '/Account/Register',
                success: function (response, status, xhr, $form) {
                    if (response != null && response.IsError === true) {
                        setTimeout(function () {
                            btn.removeClass('m-loader m-loader--right m-loader--light').attr('disabled', false);
                            //form.clearForm();
                            //form.validate().resetForm();
                            showErrorMsg(form, 'danger', response.Message);
                        }, 100);
                    } else {
                        window.location.href = "/admin";
                    }

                    // similate 2s delay
                    //setTimeout(function() {
                    //    btn.removeClass('m-loader m-loader--right m-loader--light').attr('disabled', false);
                    //    form.clearForm();
                    //    form.validate().resetForm();

                    //    // display signup form
                    //    displaySignInForm();
                    //    var signInForm = login.find('.m-login__signin form');
                    //    signInForm.clearForm();
                    //    signInForm.validate().resetForm();

                    //    showErrorMsg(signInForm, 'success', 'Thank you. To complete your registration please check your email.');
                    //}, 2000);
                }
            });
        });
    }

    var handleForgetPasswordFormSubmit = function () {
        $('#m_login_forget_password_submit').click(function (e) {
            e.preventDefault();

            var btn = $(this);
            var form = $(this).closest('form');

            form.validate({
                rules: {
                    email: {
                        required: true,
                        email: true
                    }
                }
            });

            if (!form.valid()) {
                return;
            }

            btn.addClass('m-loader m-loader--right m-loader--light').attr('disabled', true);
            var emailReset = $('.m-login__forget-password #m_email').val();
            form.ajaxSubmit({
                url: '/Account/ResetPassword',
                success: function (response, status, xhr, $form) {
                    if (response && response.IsError == true)
                    {
                        btn.removeClass('m-loader m-loader--right m-loader--light').attr('disabled', false); // remove 
                        form.clearForm(); // clear form
                        form.validate().resetForm(); // reset validation states

                        // display signup form
                        displaySignInForm();
                        var signInForm = login.find('.m-login__signin form');
                        signInForm.clearForm();
                        signInForm.validate().resetForm();

                        showErrorMsg(signInForm, 'success', 'Cool! Hướng dẫn khôi phục mật khẩu đã được gửi đến email của bạn.');
                    }
                    else {
                        btn.removeClass('m-loader m-loader--right m-loader--light').attr('disabled', false); // remove 
                        form.validate().resetForm(); // reset validation states
                        showErrorMsg(form, 'danger', response.Message);
                    }
                    
                }
            });
        });
    }

    //== Public Functions
    return {
        // public functions
        init: function () {
            handleFormSwitch();
            handleSignInFormSubmit();
            handleSignUpFormSubmit();
            handleForgetPasswordFormSubmit();
            if (document.URL.indexOf("/Register") > 0 || document.URL.indexOf("/register") > 0) {
                displaySignUpForm();
            }
        }
    };
}();

//== Class Initialization
jQuery(document).ready(function () {
    
    jQuery.validator.addMethod('passwordMatchRegis', function () {
        var password = $("#rPassword").val();
        var confirmPassword = $("#rrPassword").val();
        if (password != confirmPassword) {
            return false;
        } else {
            return true;
        }
    }, "Your Passwords Must Match");
    SnippetLogin.init();
    var aff = getCookie('giaodich.aff');
    if (aff != '') {
        $("#AffCode").val(getCookie('giaodich.aff'));
    }
});