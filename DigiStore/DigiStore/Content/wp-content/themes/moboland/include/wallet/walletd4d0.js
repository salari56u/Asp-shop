jQuery(function($){
    // دکمه "استفاده از اعتبار کیف پول"
    $('#use-wallet-btn').on('click', function() {
        var walletBalance = parseFloat($(this).data('balance'));
        var totalText = $('.order-total .woocommerce-Price-amount').first().text();
        var total = parseFloat(totalText.replace(/[, تومان]/g, ''));
        var usedWallet = Math.min(walletBalance, total);

        // مخفی کردن دکمه "استفاده از اعتبار کیف پول" و نمایش دکمه "بازگشت اعتبار"
        $(this).hide();
        $('#refund-wallet-btn').show();


        $('.wallet-checkout-info > strong').fadeOut();
        $('#wallet-balance').fadeOut();


        // نمایش لودر و ریست پیام‌ها
        $('#wallet-loader').show();
        $('#wallet-deduction-result').hide().html('');
        $('#wallet-success-message').hide();

        // ارسال Ajax برای اعمال اعتبار
        $.ajax({
            url: wc_checkout_params.ajax_url,
            type: 'POST',
            data: {
                action: 'set_wallet_usage',
                amount: usedWallet
            },
            success: function(response) {
                if (response.success) {
                    // // نمایش مابه‌التفاوت
                    // $('#wallet-deduction-result').html(
                    //     'مبلغ قابل پرداخت پس از استفاده از اعتبار کیف پول: <span style="color: green;">' +
                    //     (total).toLocaleString() + ' تومان</span>'
                    // ).fadeIn();

                    // نمایش پیام موفقیت
                    $('#wallet-success-message').fadeIn().text('✅ اعتبار کیف پول با موفقیت اعمال شد');

                    // بروزرسانی فاکتور
                    $('body').trigger('update_checkout');
                } else {
                    $('#wallet-deduction-result').html(
                        '<span style="color: red;">خطا: ' + response.data.message + '</span>'
                    ).fadeIn();
                }
            },
            error: function() {
                $('#wallet-deduction-result').html(
                    '<span style="color: red;">خطای شبکه! لطفاً دوباره تلاش کنید.</span>'
                ).fadeIn();
            },
            complete: function() {
                $('#wallet-loader').fadeOut();
            }
        });
    });

    // دکمه "بازگشت اعتبار کیف پول"
    $('#refund-wallet-btn').on('click', function() {
        // مخفی کردن دکمه "بازگشت اعتبار" و نمایش دوباره دکمه "استفاده از اعتبار"
        $('#refund-wallet-btn').hide();
        $('#use-wallet-btn').show();

        // نمایش لودر و تغییر متن آن به "در حال برگشت اعتبار کیف پول"
        $('#wallet-loader').show();
        $('#wallet-loader .loading-text').text('در حال برگشت اعتبار کیف پول');

        $('#wallet-success-message').fadeOut();
        $('#wallet-deduction-result').fadeOut();

        $('.wallet-checkout-info > strong').fadeIn();
        $('#wallet-balance').fadeIn();

        // ارسال درخواست برای لغو اعتبار
        $.ajax({
            url: wc_checkout_params.ajax_url,
            type: 'POST',
            data: {
                action: 'refund_wallet_usage'  // نام اکشن باید با اکشنی که برای بازگشت اعتبار در سرور تنظیم می‌کنید مطابقت داشته باشد
            },
            success: function(response) {
                if (response.success) {
                    // بروزرسانی صفحه برای بازگشت مبلغ اصلی فاکتور
                    $('body').trigger('update_checkout');
                } else {
                    alert('خطا در بازگشت اعتبار!');
                }
            },
            error: function() {
                alert('خطای شبکه! لطفاً دوباره تلاش کنید.');
            },
            complete: function() {
                $('#wallet-loader').fadeOut();
            }
        });
    });
});











function setupWalletPopup() {
    const openBtn = document.querySelector(".wallet-charge");
    const formContainer = document.querySelector(".wallet-form-container");
    const overlay = document.querySelector(".overlay");
    const closeBtn = document.querySelector(".close-form");

    if (!openBtn || !formContainer || !overlay || !closeBtn) return;

    openBtn.addEventListener("click", () => {
        formContainer.classList.add("active");
        overlay.style.visibility = "visible";
        overlay.style.opacity = "1";
    });

    closeBtn.addEventListener("click", closeForm);
    overlay.addEventListener("click", closeForm);

    function closeForm() {
        formContainer.classList.remove("active");
        overlay.style.visibility = "hidden";
        overlay.style.opacity = "0";
    }
}

// اجرا بعد از لود کامل DOM
document.addEventListener("DOMContentLoaded", setupWalletPopup);



