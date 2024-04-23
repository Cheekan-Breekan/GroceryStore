function InfinityScroll(containerId, action, params) {
    var self = this;

    this.container = document.getElementById(containerId);
    this.action = action;
    this.params = params;
    this.loading = false;

    this.AddDivs = function (firstItem) {
        self.loading = true;
        var requestParams = Object.assign({}, self.params);

        requestParams.firstItem = firstItem;
        requestParams.sortOrder = sortOrderParam;
        requestParams.currentFilter = currentFilterParam;
        requestParams.categoryFilter = categoryFilterParam;
        var token = document.getElementsByName("__RequestVerificationToken")[0].value;

        $.ajax({
            type: 'POST',
            url: self.action,
            data: requestParams,
            dataType: "html",
            headers: {
                RequestVerificationToken: token
            }
        })
            .done(function (result) {
                if (result) {
                    self.container.innerHTML += result;
                    self.loading = false;
                }
                applyQuantityButtonsEventListeners(); //После каждого ajax запроса обновляем события для кнопок
            })
            .fail(function (xhr) {
                self.loading = false;
                if (xhr.status == 400 && xhr.responseText == "maxCount") {
                    console.log("Maximum count of products is reached");
                    return;
                }
                console.log("Error in AddDivs:", xhr.responseText);
            })
            .always(function () {
            });
    };

    window.onscroll = function (ev) {
        if ((window.innerHeight + window.scrollY) >= document.body.offsetHeight) {
            if (!self.loading) {
                var itemCount = self.container.children.length;
                self.AddDivs(itemCount);
            }
        }
    };

    this.AddDivs(0);
}

$(() => {
    InfinityScroll("products-cards-field", "/home/ProductsLoad");
});