function applyQuantityButtonsEventListeners() {
    // Убавляем кол-во по клику
    $('#quantity-input-inner #btn_minus').on("click", function () {
        let $input = $(this).parent().find('.quantity-input');
        let count = parseInt($input.val()) - 1;
        count = count < 1 ? 1 : count;
        $input.val(count);
    });
    // Прибавляем кол-во по клику
    $('#quantity-input-inner #btn_plus').on("click", function () {
        let $input = $(this).parent().find('.quantity-input');
        let count = parseInt($input.val()) + 1;
        count = count > parseInt($input.data('max-count')) ? parseInt($input.data('max-count')) : count;
        
        let storageQuantity = findProductStorageQuantity($(this))
        if (count > storageQuantity) {
            console.log('Cannot add more');
            count = count - 1;
        }
        $input.val(parseInt(count));
    });
    // Убираем все лишнее и невозможное при изменении поля
    $('#quantity-input-inner .quantity-input').on("change keyup input click", function () {
        if (this.value.match(/[^0-9]/g)) {
            this.value = this.value.replace(/[^0-9]/g, '');
        }
        if (this.value == "") {
            this.value = 1;
        }
        if (this.value > parseInt($(this).data('max-count'))) {
            this.value = parseInt($(this).data('max-count'));
        }
        let storageQuantity = findProductStorageQuantity($(this))
        if (this.value > storageQuantity) {
            console.log('Cannot add more');
            this.value = storageQuantity;
        }
    });
    //Проверка на количество на складе
    function findProductStorageQuantity (element) {
        let $storageQuantityParent = $(element).closest("#quantity-script");
        let storageQuantity = parseInt($storageQuantityParent.find("input[id*='product-storage-quantity-']").val());
        return storageQuantity;
    }
}
$(() => {
    applyQuantityButtonsEventListeners();
});
