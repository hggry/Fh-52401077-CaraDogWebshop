(function () {
  "use strict";

  function ApiClient() {
    this.getFeaturedProducts = function () {
      return Promise.resolve([
        {
          id: "leinen-marina",
          title: "Leine Marina",
          category: "Leinen",
          price: "49,00 EUR",
          description: "Geflochten, robust und handgenäht."
        },
        {
          id: "halsband-aramani",
          title: "Halsband Armani",
          category: "Halsbänder",
          price: "39,00 EUR",
          description: "Weiches Leder mit sauberer Naht."
        },
        {
          id: "spielzeug-seil",
          title: "Spielzeug Seil",
          category: "Spielzeug",
          price: "19,00 EUR",
          description: "Naturfaser für gemeinsames Spielen."
        },
        {
          id: "set-welle",
          title: "Set Wellenmuster",
          category: "Sets",
          price: "79,00 EUR",
          description: "Leine und Halsband im Set."
        }
      ]);
    };

    this.sendContactMessage = function (payload) {
      return new Promise(function (resolve) {
        window.setTimeout(function () {
          resolve({ ok: true, data: payload });
        }, 700);
      });
    };
  }

  function CartStorage(storageKey) {
    this.storageKey = storageKey;

    this.getItems = function () {
      var raw = localStorage.getItem(this.storageKey);
      return raw ? JSON.parse(raw) : [];
    };

    this.addItem = function (item) {
      var items = this.getItems();
      items.push(item);
      localStorage.setItem(this.storageKey, JSON.stringify(items));
    };
  }

  function createProductCard(product) {
    var card = document.createElement("article");
    card.className = "product-card";

    var category = document.createElement("span");
    category.className = "product-category";
    category.textContent = product.category;

    var title = document.createElement("h3");
    title.className = "product-title";
    title.textContent = product.title;

    var description = document.createElement("p");
    description.textContent = product.description;

    var price = document.createElement("span");
    price.className = "product-price";
    price.textContent = product.price;

    var button = document.createElement("button");
    button.className = "button button-outline";
    button.type = "button";
    button.textContent = "In den Warenkorb";

    button.addEventListener("click", function () {
      window.caraDogCart.addItem(product);
      button.textContent = "Hinzugefügt";
    });

    card.appendChild(category);
    card.appendChild(title);
    card.appendChild(description);
    card.appendChild(price);
    card.appendChild(button);

    return card;
  }

  function renderProducts(products) {
    var grid = document.getElementById("product-grid");
    if (!grid) {
      return;
    }

    grid.innerHTML = "";
    products.forEach(function (product, index) {
      var card = createProductCard(product);
      card.style.animationDelay = (index * 120) + "ms";
      grid.appendChild(card);
    });
  }

  function bindContactForm() {
    var form = document.getElementById("contact-form");
    if (!form) {
      return;
    }

    var status = document.getElementById("contact-status");

    form.addEventListener("submit", function (event) {
      event.preventDefault();

      var payload = {
        name: form.elements.name.value.trim(),
        email: form.elements.email.value.trim(),
        message: form.elements.message.value.trim()
      };

      if (status) {
        status.textContent = "Nachricht wird gesendet...";
      }

      window.caraDogApi.sendContactMessage(payload).then(function () {
        if (status) {
          status.textContent = "Danke! Wir melden uns bald per E-Mail.";
        }
        form.reset();
      }).catch(function () {
        if (status) {
          status.textContent = "Das hat leider nicht geklappt. Bitte erneut senden.";
        }
      });
    });
  }

  function init() {
    window.caraDogApi = new ApiClient();
    window.caraDogCart = new CartStorage("caradog-cart");

    window.caraDogApi.getFeaturedProducts().then(renderProducts);
    bindContactForm();
  }

  document.addEventListener("DOMContentLoaded", init);
})();
