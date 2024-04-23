function addProductToCartHomePageAjax() {
    $(document).on("submit", '#quantity-script', function (event) {
        console.log("form submitted");
        event.preventDefault();
        let productId = $(this).data("productid");
        let productQuantity = $(this).find(".quantity-input").val();
        let token = document.getElementsByName("__RequestVerificationToken")[0].value;
        let $formElement = $(this);

        $.ajax({
            type: "POST",
            url: '/Home/AddProductToCart',
            data: {
                productId: productId,
                productQuantity: productQuantity,
            },
            headers: {
                RequestVerificationToken: token
            },

            success: function (response) {
                $formElement.closest("#incartorquantity").html(response);
                $formElement.remove();
                toastr.success("Продукт был добавлен в корзину");
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
    addProductToCartHomePageAjax();
});