// Set new default font family and font color to mimic Bootstrap's default styling
(Chart.defaults.global.defaultFontFamily = "body-font"),
'body-font,sans-serif';
Chart.defaults.global.defaultFontColor = "#858796";

// Pie Chart Example
var ctx = document.getElementById("myPieChart");
var myPieChart = new Chart(ctx, {
    type: "doughnut",
    data: {
        labels: ["صفحه اصلی", "شبکه های اجتماعی","تلویزیون هوشمند", "معرفی کاربران"],
        datasets: [{
            data: [20, 15, 10, 40],
            backgroundColor: [
                "rgba(0, 97, 242, 1)",
                "rgba(0, 172, 105, 1)",
                "rgba(255,165,0,1)",
                "rgb(137,179,0,1)"
            ],
            hoverBackgroundColor: [
                "rgba(0, 97, 242, 0.9)",
                "rgba(0, 172, 105, 0.9)",
                "rgba(255,165,0,0.67)",
                "rgb(137,179,0,0.67)"
            ],
            hoverBorderColor: "rgba(234, 236, 244, 1)"
        }]
    },
    options: {
        maintainAspectRatio: false,
        tooltips: {
            backgroundColor: "rgb(255,255,255)",
            bodyFontColor: "#858796",
            borderColor: "#dddfeb",
            borderWidth: 1,
            xPadding: 15,
            yPadding: 15,
            displayColors: false,
            caretPadding: 10
        },
        legend: {
            display: false
        },
        cutoutPercentage: 80
    }
});
