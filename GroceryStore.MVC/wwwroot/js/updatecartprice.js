function updateCartPrice(productPrice, quantityChange) {
        let $cartPriceElem = $("#cart-price");
        let cartOldPrice = parseFloat($cartPriceElem.text().replace(',', '.'));
        let refreshedCartPrice = parseFloat(cartOldPrice + (quantityChange * productPrice)).toFixed(2);
        $cartPriceElem.text(refreshedCartPrice.replace('.', ','));
    }