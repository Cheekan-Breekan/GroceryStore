$(function () {
    if (localStorage.getItem("scrollPosition") != null) {
        $(window).scrollTop(localStorage.getItem("scrollPosition"));
    }

    $(window).on("scroll", function () {
        localStorage.setItem("scrollPosition", $(window).scrollTop());
    });
});