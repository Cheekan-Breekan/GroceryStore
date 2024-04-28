function applyQuantityAjaxEventListeners($container) {
    // Убавляем кол-во по клику
    $container.find('.quantity-input-inner .btn-minus').on("click", function () {
        let cartItemId = findCartItemId($(this));
        let $input = findCartItemInput(cartItemId);
        let currentValue = findCurrentQuantityValue($input);

        if ((currentValue - 1) < 1) {
            return;
        }

        updateCartItemQuantity(-1, cartItemId, currentValue);
    });

    // Прибавляем кол-во по клику
    $container.find('.quantity-input-inner .btn-plus').on("click", function () {
        let cartItemId = findCartItemId($(this));
        let $input = findCartItemInput(cartItemId);
        let currentValue = findCurrentQuantityValue($input);

        //Проверка на количество на складе
        let $storageQuantityParent = $(this).closest("#quantity-script");
        let storageQuantity = parseInt($storageQuantityParent.find("input[id*='product-storage-quantity-']").val());

        if ((currentValue + 1) > $input.data('max-count') || (currentValue + 1) > storageQuantity) {
            console.log('Cannot add more');
            return;
        }

        updateCartItemQuantity(1, cartItemId, currentValue);
    });

    function findCartItemId(element) {
        return element.attr('id').split('-').pop();
    }
    function findCartItemInput(cartItemId) {
        return $(`#quantity-input-${cartItemId}`);
    }
    function findCurrentQuantityValue($input) {
        return parseInt($input.val());
    }
}

function updateCartItemQuantity(quantityChange, cartItemId, currentValue) {
    var token = document.getElementsByName("__RequestVerificationToken")[0].value;

    $.ajax({
        type: 'POST',
        url: '/Carts/UpdateCartItemQuantity',
        data: {
            cartItemId: cartItemId,
            quantityChange: quantityChange,
            currentValue: currentValue
        },
        headers: {
            RequestVerificationToken: token
        },

        success: function (response) {
            console.log('Quantity updated successfully');
            let $container = $(`.quantity-field-${cartItemId}`);
            $container.html(response);
            applyQuantityAjaxEventListeners($container);
            // Обновляем цену в карточке
            let $priceCardElem = $(`#total-price-${cartItemId}`);
            if ($priceCardElem.length === 0) {
                return;
            }
            let oldPrice = parseFloat($priceCardElem.text().replace(',', '.'));
            let productPrice = parseFloat($priceCardElem.data("price").replace(',', '.'));
            let refreshedPrice = parseFloat(oldPrice + (quantityChange * productPrice)).toFixed(2);
            $priceCardElem.text(refreshedPrice.replace('.', ','));
            // Обновляем общую цену
            updateCartPrice(productPrice, quantityChange);
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });

}
$(() => {
    applyQuantityAjaxEventListeners($(document));
});