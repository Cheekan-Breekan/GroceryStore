function addProductToCartInfoPageAjax() {
    $(document).on("submit", '#quantity-script', function (event) {
        console.log("form submitted");
        event.preventDefault();
        let productId = $(this).data("productid");
        let productQuantity = $(this).find(".quantity-input").val();
        let token = document.getElementsByName("__RequestVerificationToken")[0].value;
        let $formElement = $(this);

        $.ajax({
            type: "POST",
            url: '/Home/AddProductToCartFromInfo',
            data: {
                productId: productId,
                productQuantity: productQuantity,
            },
            headers: {
                RequestVerificationToken: token
            },

            success: function (response) {
                let $quantityElement = $formElement.find("#refresh-cartquantity");
                $quantityElement.empty();
                let cartItemId = $(response).find("[id='cart-item-id']").val();
                let $response = $(`<div class='quantity-field-${cartItemId}'></div>`).html(response);
                $quantityElement.html($response);
                toastr.success("Продукт был добавлен в корзину");
                applyQuantityAjaxEventListeners($(document));
            },
            error: function (xhr) {
                var notification = xhr.responseJSON;
                if (notification) {
                    if (notification.isError) {
                        toastr.error(notification.message);
                    }
                    else if (notification.status === 401) {
                        window.location.href = "/Account/Login";
                    }
                    else {
                        toastr.error("Произошла ошибка при добавлении в корзину");
                    }
                }
                console.error(xhr.responseText);
            }
        });
    });
}
$(() => {
    addProductToCartInfoPageAjax();
});