function applyRemoveCartItemButtonListeners() {
    $( ".remove-cartitem-btn" ).on('click', function () {
        let $parentElement = $(this).closest("#quantity-script");
        let cartItemId = $parentElement.data('productid');

        RemoveCartItemAjax(cartItemId, $parentElement);
    });
}
function RemoveCartItemAjax(cartItemId, parentElement) {
    var token = document.getElementsByName("__RequestVerificationToken")[0].value;
    $.ajax({
        type: 'POST',
        url: '/Carts/RemoveCartItem',
        data: {
            cartItemId: cartItemId
        },
        headers: { RequestVerificationToken: token },

        success: function (response) {
            toastr.success("Продукт был удален из корзины");
            //обновляем цену корзины
            let $priceCardElem = $(`#total-price-${cartItemId}`);
            let productPrice = parseFloat($priceCardElem.data("price").replace(',', '.'));
            let quantityChange = parseInt($(`#quantity-input-${cartItemId}`).val());
            updateCartPrice(productPrice, -quantityChange);
            //обновляем количество продуктов в корзине
            let $cartItemCountElem = $("#cart-items-count");
            let refreshedCount = parseInt($cartItemCountElem.text()) - 1;
            $cartItemCountElem.text(refreshedCount);

            parentElement.remove();
        },
        error: function (xhr) {
            console.error('CartItem not removed. Error message: ' + error);
            toastr.error("Произошла ошибка при удалении из корзины");
        }
    });
}
$(() => {
    applyRemoveCartItemButtonListeners();
});