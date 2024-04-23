$(() => {
    changeDisplayImage(this);
})
function changeDisplayImage(smallImage) {
    var $displayImage = $("#display-main-image");
    $displayImage.attr("src", smallImage.src);
}