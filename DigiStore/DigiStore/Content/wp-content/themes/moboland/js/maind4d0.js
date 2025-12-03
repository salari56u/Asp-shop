jQuery(document).ready(function ($) {
    function showLoader(button) {
        var loader = $('<div class="loader-circle"></div>');
        button.prop("disabled", true).append(loader);
    }

    function hideLoader(button) {
        button.prop("disabled", false).find(".loader-circle").remove();
    }

    function handleSendOtp() {
        var mobile = $("#mobile").val();
        var button = $("#send-otp-btn");

        if (!mobile.match(/^09[0-9]{9}$/)) {
            alert("❌ لطفاً شماره موبایل معتبر وارد کنید.");
            return;
        }

        showLoader(button);
        setTimeout(function () {
            sendOtp(mobile, button);
        }, 2000);
    }

    function handleVerifyOtp() {
        var mobile = $("#mobile").val();
        var otp_code = $("#otp_code").val();
        var button = $("#verify-otp-btn");

        showLoader(button);

        $.ajax({
            type: "POST",
            url: ajax_object.ajax_url,
            data: {
                action: "verify_otp",
                mobile: mobile,
                otp_code: otp_code
            },
            success: function (response) {
                hideLoader(button);
                if (response.success) {
                    setTimeout(function () {
                        window.location.reload();
                    }, 500);
                } else {
                    $("#otp-error-message").remove();
                    $("#verify-otp-form").append('<p id="otp-error-message" style="padding: 8px; margin-top: 5px; background: #e74c3c; color: #fff; border-radius: 8px;">کد وارد شده صحیح نیست لطفا مجدد بررسی کنید</p>');
                }
            },
            error: function (xhr, status, error) {
                hideLoader(button);
                console.error("❌ خطای `AJAX`:", error, xhr.responseText);
                alert("❌ خطای `AJAX`: " + xhr.responseText);
            }
        });
    }

    $(document).on("click", "#send-otp-btn", handleSendOtp);
    $(document).on("click", "#verify-otp-btn", handleVerifyOtp);
    $(document).on("click", "#resend-otp-btn", function () {
        var mobile = $("#mobile").val();
        var button = $(this);
        showLoader(button);
        setTimeout(function () {
            sendOtp(mobile, button);
        }, 2000);
    });

    $(document).on("keydown", function (e) {
        if (e.key === "Enter" || e.keyCode === 13 || e.keyCode === 108) { // Detect both Enter keys
            e.preventDefault(); // Prevent form submission if necessary
            if ($("#mobile").is(":focus")) {
                handleSendOtp();
            } else if ($("#otp_code").is(":focus")) {
                handleVerifyOtp();
            }
        }
    });

    function sendOtp(mobile, button) {
        $.ajax({
            type: "POST",
            url: ajax_object.ajax_url,
            data: {
                action: "send_otp",
                mobile: mobile
            },
            success: function (response) {
                hideLoader(button);
                if (response.success) {
                    $("#send-otp-form").hide();
                    $("#verify-otp-form").show();
                    $("#otp-sent-message").show();
                    startCountdown();
                } else {
                    alert(response.data);
                }
            },
            error: function (xhr, status, error) {
                hideLoader(button);
                console.error("❌ خطای `AJAX`:", error);
                alert("❌ خطای `AJAX`: " + xhr.responseText);
            }
        });
    }

    function startCountdown() {
        let countdownElement = $("#countdown");
        let resendButton = $("#resend-otp-btn");
        let timerMessage = $("#timer-message");
        let verifyButton = $("#verify-otp-btn");

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



$('.slider-articles-related').owlCarousel({
    loop:false,
    margin:15,
    nav:true,
    rtl: true,
    navText: "",
    dots: false,
    responsive:{
        0: {
            items: 1
        },
        576: {
            items: 2
        },
        768: {
            items: 3
        },
        992: {
            items: 3
        },
        1200: {
            items: 3
        },
    }
})


/***********timer Amazing***********/
$(document).ready(function (){
    function countdownTimer() {
        var countdownElements = document.querySelectorAll('.box-timer');
        countdownElements.forEach(function (countdownElement) {
            var daysElements = countdownElement.querySelectorAll('.days-value');
            var hoursElements = countdownElement.querySelectorAll('.hours-value');
            var minutesElements = countdownElement.querySelectorAll('.minutes-value');
            var secondsElements = countdownElement.querySelectorAll('.seconds-value');
            var messageElement = countdownElement.querySelector('.timer-message');
            var messagesElement = countdownElement.querySelector('.massages-heddin');
            var targetYear = parseInt(countdownElement.dataset.targetYear);
            var targetMonth = parseInt(countdownElement.dataset.targetMonth) - 1;
            var targetDay = parseInt(countdownElement.dataset.targetDay);
            var targetHour = parseInt(countdownElement.dataset.targetHour);
            var targetMinute = parseInt(countdownElement.dataset.targetMinute);
            var targetSecond = parseInt(countdownElement.dataset.targetSecond);
            var targetDate = new Date(targetYear, targetMonth, targetDay, targetHour, targetMinute, targetSecond);

            function updateTimer() {
                var now = new Date().getTime();
                var timeRemaining = targetDate - now;
                var days = Math.floor(timeRemaining / (1000 * 60 * 60 * 24));
                var hours = Math.floor((timeRemaining % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
                var minutes = Math.floor((timeRemaining % (1000 * 60 * 60)) / (1000 * 60));
                var seconds = Math.floor((timeRemaining % (1000 * 60)) / 1000);
                for (var i = 0; i < daysElements.length; i++) {
                    daysElements[i].innerHTML = days;
                }
                for (var i = 0; i < hoursElements.length; i++) {
                    hoursElements[i].innerHTML = hours;
                }
                for (var i = 0; i < minutesElements.length; i++) {
                    minutesElements[i].innerHTML = minutes;
                }
                for (var i = 0; i < secondsElements.length; i++) {
                    secondsElements[i].innerHTML = seconds;
                }
                if (timeRemaining > 0) {
                    setTimeout(updateTimer, 1000);
                } else {
                    messagesElement.style.display = 'none';
                    /*messageElement.style.display = 'block';
                    var messageTextElement = messageElement.querySelector('.timer-message-text');
                    messageTextElement.innerHTML = 'تایمر به پایان رسیده است';*/
                }
            }

            updateTimer();
        });
    }
    countdownTimer();
});

/************contact button************/

$(document).ready(function (){
    $('.floating-button i').click(function (){
        $('.contact-list').slideToggle();
        if ($(this).hasClass('fa-comment-dots')) {
            $(this).removeClass('fa-comment-dots').addClass('fa-multiply');
            $(this).css({
                'transform' : 'rotate(180deg)',
                'transition' : 'all .2s ease-in-out',
            })
        }
        else {
            $(this).removeClass('fa-multiply').addClass('fa-comment-dots');
            $(this).css({
                'transform' : 'rotate(0)',
                'transition' : 'all .2s ease-in-out',
            })
        }
    })
})

/************go up page button************/

$("#go-up").click(function (){
    $('html').animate({scrollTop:0}, 1000)
})

// چرخش دکمه افزودن به سبد خرید

$(".addtocart-button a.product_type_simple").click(function (){
    $(this).css({
        'transform' : 'rotate(1080deg)',
    })
    $('#modal_add_to_cart').delay(1000).fadeIn();
    $('#modal_add_to_cart').delay(2000).fadeOut();
})

$(".add-to-cart a.product_type_simple").click(function (){
    $('#modal_add_to_cart').delay(1000).fadeIn();
    $('#modal_add_to_cart').delay(2000).fadeOut();
})




// سوئیچر فرم لاگین
$('.message a').click(function(){
    $('.form-login-moboland form').animate({height: "toggle", opacity: "toggle"}, "slow");
});


// start modal login

$(document).ready(function () {
    var modal_login = document.getElementById("modal_login");
    var btn_modal_login = document.getElementById("btn_modal_login");
    var close_login = document.getElementsByClassName("close_login")[0];
    btn_modal_login.onclick = function() {
        modal_login.style.display = "block";
    }
    close_login.onclick = function() {
        modal_login.style.display = "none";
    }
    window.onclick = function(event) {
        if (event.target == modal_login) {
            modal_login.style.display = "none";
        }
    }
})
// end modal login

// start modal video

$(document).ready(function () {
    var modal_video = document.getElementById("modal_video");
    var btn_modal_video = document.getElementById("btn_modal_video");
    var close_video = document.getElementsByClassName("close_video")[0];
    btn_modal_video.onclick = function() {
        modal_video.style.display = "block";
    }
    close_video.onclick = function() {
        modal_video.style.display = "none";
    }
})
// end modal video


// start modal share

$(document).ready(function () {
    var modal_share = document.getElementById("modal_share");
    var btn_modal_share = document.getElementById("btn_modal_share");
    var close_share = document.getElementsByClassName("close_share")[0];
    btn_modal_share.onclick = function() {
        modal_share.style.display = "block";
    }
    close_share.onclick = function() {
        modal_share.style.display = "none";
    }
    window.onclick = function(event) {
        if (event.target == modal_video || event.target == modal_login || event.target == modal_share) {
            modal_video.style.display = "none";
            modal_login.style.display = "none";
            modal_share.style.display = "none";
        }
    }
})
// end modal share





/************quantity number plus and minus************/
$(document).on( 'click', 'button.plus, button.minus', function() {
    var qty = $( this ).parent( '.quantity' ).find( '.qty' );
    var val = parseFloat(qty.val());
    var max = parseFloat(qty.attr( 'max' ));
    var min = parseFloat(qty.attr( 'min' ));
    var step = parseFloat(qty.attr( 'step' ));
    if ( $( this ).is( '.plus' ) ) {
        if ( max && ( max <= val ) ) {
            qty.val( max ).change();
        } else {
            qty.val( val + step ).change();
        }
    } else {
        if ( min && ( min >= val ) ) {
            qty.val( min ).change();
        } else if ( val > 1 ) {
            qty.val( val - step ).change();
        }
    }
});

/************show and hidden search form************/
$(document).ready(function () {
    $('.search-main').click(function () {
        $('.overlay').addClass('show');
        // searchbox-two
        $('.searchbox-two form').slideDown();
        $('.searchbox-co form').slideDown();
        $('.search-input').focus();
    })
})
$(document).click(function (e) {
    $('.content-ajax-search').removeClass('show');
    $('.loader-ajax-search').removeClass('show');

    if (!$(e.target).closest('.search-main').length) {
        $('.overlay').removeClass('show');
        // searchbox-two
        $('.searchbox-two form').slideUp();
        $('.searchbox-co form').slideUp();
    }
});

/************show and hidden filter in shop page************/
$(document).ready(function () {
    $('.filter-shop-page').click(function () {
        $('.hero-single .side-single-shop-page').css({
            'transform' : 'translateX(0px)'
        });

        $('.close-menu-responsive-page-moboland').css({
            'opacity' : '1',
            'pointer-events' : 'unset'
        });
    })

    $('.close-menu-responsive-page-moboland').click(function () {
        $('.hero-single .side-single-shop-page').css({
            'transform' : 'translateX(1000px)'
        });

        $('.close-menu-responsive-page-moboland').css({
            'opacity' : '0',
            'pointer-events' : 'none'
        });
    })
})

/************search box in mobile************/
$(document).ready(function () {
    $('.search-menu-bottom-mobile').click(function () {
        $('.search-mobile').fadeIn();
        $('.search-input').focus();

    });
    $('#close_search_mobile').click(function () {
        $('.search-mobile').fadeOut();
    });
});


/************with sub menu************/
$(document).ready(function (){
    var cnt = $(".container").width();
    $(".moboland-megamenu > ul > li > ul").innerWidth(cnt - 230),
        $(window).resize(function () {
            var rcnt = $(".container").width();
            $(".moboland-megamenu > ul > li > ul").innerWidth(rcnt - 230);
        });
});


/************arrow for menu************/
$(document).ready(function () {
    var menu_mobile = document.getElementById("menu_mobile");
    var hamburger = document.getElementById("hamburger");
    var close_menu = document.getElementsByClassName("close_menu")[0];
    hamburger.onclick = function() {
        menu_mobile.style.display = "block";
    }
    close_menu.onclick = function() {
        menu_mobile.style.display = "none";
    }
    window.onclick = function(event) {
        if (event.target == menu_mobile) {
            menu_mobile.style.display = "none";
        }
    }

    $('.navigation-content ul.sub-menu').before("<i class='i-icon fa-solid fa-caret-left'></i>");
    $('.header nav.main-menu-header .menu-header > ul > li:has(ul.sub-menu) > ul.sub-menu').before("<i class='fa-solid fa-angle-down'></i>");

    $(".navigation-content .i-icon").click(function () {
        if ($(this).hasClass("fa-caret-left")) {
            $(this).next("ul.sub-menu").slideToggle();
            $(this).removeClass("fa-caret-left").addClass("fa-caret-down");
        }
        else {
            $(this).next("ul.sub-menu").hide(500);
            $(this).removeClass("fa-caret-down").addClass("fa-caret-left");
        }
    });

// main menu for moboland co
    $('.header-co nav.main-menu > ul > li:has(ul.sub-menu) > ul.sub-menu').before("<i class='fa-solid fa-angle-down'></i>");
    $('.header-co nav.main-menu ul.sub-menu li:has(ul.sub-menu) > ul.sub-menu').before("<i class='fa-solid fa-angle-left'></i>");
})
// end menu



// تب منوی داخل منوی موبایل
function openCity(evt, cityName) {
    var i, tabcontent, tablinks;
    tabcontent = document.getElementsByClassName("tabcontent");
    for (i = 0; i < tabcontent.length; i++) {
        tabcontent[i].style.display = "none";
    }
    tablinks = document.getElementsByClassName("tablinks");
    for (i = 0; i < tablinks.length; i++) {
        tablinks[i].className = tablinks[i].className.replace(" active", "");
    }
    document.getElementById(cityName).style.display = "block";
    evt.currentTarget.className += " active";
}

// Get the element with id="defaultOpen" and click on it
document.getElementById("defaultOpen").click();


// remove mobile & desktop
$(document).ready(function (){
    if ($(window).width() < 993) {
        $('.desktop').remove();
    }
    if ($(window).width() > 992) {
        $('.mobile').remove();
    }
});


// start modal login

$(document).ready(function () {
    var modal_login = document.getElementById("modal_login");
    var btn_modal_login2 = document.getElementById("btn_modal_login2");
    var close_login = document.getElementsByClassName("close_login")[0];
    btn_modal_login2.onclick = function() {
        modal_login.style.display = "block";
    }
    close_login.onclick = function() {
        modal_login.style.display = "none";
    }
    window.onclick = function(event) {
        if (event.target == modal_login) {
            modal_login.style.display = "none";
        }
    }
})
// end modal login






