document.addEventListener("DOMContentLoaded", function () {
    let justClicked = false;
    let showLoader = false; // 🔥 این متغیر فقط وقتی کلیک شد true میشه

    updateComparePopup();

    // افزودن محصول به مقایسه
    window.addToCompare = function (productId) {
        let compareList = JSON.parse(localStorage.getItem("compare_list")) || [];

        if (!compareList.includes(productId)) {
            compareList.push(productId);
            localStorage.setItem("compare_list", JSON.stringify(compareList));

            // تغییر ظاهر دکمه
            const btn = document.querySelector(`.moboland-compare-btn[onclick="addToCompare(${productId})"]`);
            if (btn) {
                btn.classList.add('added');
                btn.querySelector('.add')?.style?.setProperty('display', 'none');
                btn.querySelector('.added')?.style?.setProperty('display', 'inline');
            }

            showCompareToast();
        }

        justClicked = true;
        showLoader = true; // 🔥 فعال‌سازی لودر فقط در اینجا
        updateComparePopup();
    };

    // حذف یک محصول از مقایسه
    window.removeCompareItem = function (productId) {
        let compareList = JSON.parse(localStorage.getItem("compare_list")) || [];
        compareList = compareList.filter(id => id !== productId);
        localStorage.setItem("compare_list", JSON.stringify(compareList));

        const loader = document.getElementById("compare-loader");
        if (loader) loader.style.display = "flex"; // ⬅️ نمایش لودر

        updateComparePopup();

        setTimeout(() => {
            if (loader) loader.style.display = "none"; // ⬅️ مخفی‌سازی لودر
            showRemoveToast(); // ⬅️ نمایش پیام حذف
        }, 800); // ⬅️ تاخیر برای هماهنگی با انیمیشن بارگذاری (اختیاری)
    };


    // پاک کردن کل لیست مقایسه
    window.clearCompareList = function () {
        const loader = document.getElementById("compare-loader");

        if (loader) loader.style.display = "flex"; // ⬅️ نمایش لودر

        localStorage.removeItem("compare_list");

        updateComparePopup();

        // کمی تأخیر برای طبیعی بودن بارگذاری
        setTimeout(() => {
            if (loader) loader.style.display = "none"; // ⬅️ مخفی‌سازی لودر
            showRemoveToast("لیست مقایسه با موفقیت پاک شد."); // ✅ نمایش پیام موفقیت
        }, 800);
    };


    // نمایش یا مخفی‌سازی پاپ‌آپ مقایسه
    function updateComparePopup() {
        const compareList = JSON.parse(localStorage.getItem("compare_list")) || [];
        const popup = document.getElementById("compare-popup");
        const popupContent = document.getElementById("compare-popup-content");
        const loader = document.getElementById("compare-loader");

        if (!popup || !popupContent) return;

        if (compareList.length === 0) {
            popup.style.display = "none";
            return;
        }

        if (loader && showLoader) loader.style.display = "flex";

        const url = mobolandCompareData.rest_url + "?ids=" + compareList.join(",");

        fetch(url)
            .then(res => res.text())
            .then(html => {
                popupContent.innerHTML = html;

                if (justClicked) {
                    popup.style.display = "flex";
                    justClicked = false;
                }

                if (loader) loader.style.display = "none";
                showLoader = false; // 🔥 ریست
            })
            .catch(err => {
                popupContent.innerHTML = "<p>خطا در بارگذاری مقایسه.</p>";

                if (justClicked) {
                    popup.style.display = "flex";
                    justClicked = false;
                }

                if (loader) loader.style.display = "none";
                showLoader = false;
                console.error(err);
            });
    }

    // نمایش toast بالای صفحه
    window.showCompareToast = function (message = "محصول به لیست مقایسه اضافه شد.") {
        const toast = document.getElementById("compare-toast");
        if (!toast) return;

        toast.textContent = message;
        toast.style.display = "block";

        toast.classList.remove("compare-toast");
        void toast.offsetWidth;
        toast.classList.add("compare-toast");

        setTimeout(() => {
            toast.style.display = "none";
        }, 3000);
    };
});



window.showRemoveToast = function (message = "محصول از لیست مقایسه حذف شد.") {
    const toast = document.getElementById("compare-toast-remove");
    if (!toast) return;

    toast.textContent = message;
    toast.style.display = "block";

    toast.classList.remove("compare-toast-remove");
    void toast.offsetWidth; // ریست انیمیشن
    toast.classList.add("compare-toast-remove");

    setTimeout(() => {
        toast.style.display = "none";
    }, 3000);
};

