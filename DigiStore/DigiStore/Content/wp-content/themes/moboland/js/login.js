jQuery(document).ready(function ($) {
    function showLoader(button) {
        var loader = $('<div class="loader-circle"></div>');
        button.prop("disabled", true).append(loader);
    }

    function hideLoader(button) {
        button.prop("disabled", false).find(".loader-circle").remove();
    }



    $('#send-otp-btn_mellipayamak').click(function () {
        var mobile = $('#mobile_mellipayamak').val();
        var button = $(this);

        if (mobile === '') {
            alert('لطفاً شماره موبایل را وارد کنید!');
            return;
        }

        showLoader(button);
        button.text("در حال ارسال...");

        $.ajax({
            type: 'POST',
            url: my_ajax_object.ajax_url,
            data: {
                action: 'send_otp',
                mobile: mobile
            },
            success: function (response) {
                if (response.success) {
                    $('#otp-sent-message_mellipayamak').show();
                    $('#send-otp-form_mellipayamak').hide();
                    $('#verify-otp-form_mellipayamak').show();

                    // شروع تایمر شمارش معکوس (60 ثانیه)
                    startCountdown(60, $("#countdown"));
                } else {
                    alert(response.data.message);
                }
                hideLoader(button);
                button.text("دریافت کد تأیید");
            },
            error: function () {
                alert('خطا در برقراری ارتباط! لطفاً دوباره امتحان کنید.');
                hideLoader(button);
                button.text("دریافت کد تأیید");
            }
        });
    });

    $('#verify-otp-btn_mellipayamak').click(function () {
        var otp_code = $('#otp_code_mellipayamak').val();
        var mobile = $('#mobile_mellipayamak').val();
        var button = $(this);

        if (otp_code === '') {
            alert('لطفاً کد تأیید را وارد کنید!');
            return;
        }

        showLoader(button);
        button.text("در حال بررسی...");

        $.ajax({
            type: 'POST',
            url: my_ajax_object.ajax_url,
            data: {
                action: 'verify_otp',
                otp_code: otp_code,
                mobile: mobile
            },
                success: function (response) {
                    if (response.success) {
                        window.location.href = "/"; // 🔴 هدایت به صفحه اصلی
                        setTimeout(function () {
                            location.reload(); // 🔄 ریفرش صفحه پس از 1 ثانیه
                        }, 1000);
                    } else {
                        alert("کد وارد شده نادرست است!");
                    }
                    hideLoader(button);
                    button.text("تأیید و ورود");
                },
            error: function () {
                alert('خطا در برقراری ارتباط! لطفاً دوباره امتحان کنید.');
                hideLoader(button);
                button.text("تأیید و ورود");
            }
        });
    });

    $('#resend-otp-btn_mellipayamak').click(function () {
        $('#send-otp-btn_mellipayamak').trigger("click"); // ارسال مجدد OTP
        $(this).hide();
    });



    function startCountdown() {
        let countdownElement = $("#countdown");
        let resendButton = $("#resend-otp-btn_mellipayamak");
        let timerMessage = $("#timer-message");
        let verifyButton = $("#verify-otp-btn_mellipayamak");

        let timeLeft = 60;
        countdownElement.text(timeLeft);
        resendButton.hide();
        timerMessage.show();
        verifyButton.show();

        let timer = setInterval(function () {
            timeLeft--;
            countdownElement.text(timeLeft);

            if (timeLeft <= 0) {
                clearInterval(timer);
                timerMessage.hide();
                resendButton.show();
                verifyButton.hide();
            }
        }, 1000);
    }
});
