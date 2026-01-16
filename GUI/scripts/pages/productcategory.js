(() => {
  "use strict";

  class ProductCategoryPage {
    constructor() {
      const baseUrl = window.CARADOG_API_BASE || "http://localhost:8080/api";
      this.api = new window.CaraDogApiClient(baseUrl);
      this.cart = new window.CaraDogCartStorage("caradog-cart");
      this.grid = document.getElementById("category-grid");
      this.title = document.getElementById("category-title");
      this.subtitle = document.getElementById("category-subtitle");
      this.filter = document.getElementById("category-filter");
      this.category = new URLSearchParams(window.location.search).get("category");
      this.products = [];
    }

    init() {
      if (!this.grid) {
        return;
      }

      this.api.getProducts()
        .then((products) => {
          this.products = Array.isArray(products) ? products : [];
          this.renderFilterOptions();
          this.applyFilter(this.category);
        })
        .catch((error) => this.renderError(error.message));
    }

    renderFilterOptions() {
      if (!this.filter) {
        return;
      }

      const categories = [...new Set(this.products.map((product) => product.categoryName))]
        .filter((name) => name)
        .sort((a, b) => a.localeCompare(b));

      this.filter.innerHTML = '<option value="">Alle Produkte</option>';
      categories.forEach((name) => {
        const option = document.createElement("option");
        option.value = name;
        option.textContent = name;
        this.filter.appendChild(option);
      });

      if (this.category && categories.includes(this.category)) {
        this.filter.value = this.category;
      } else {
        this.category = "";
        this.filter.value = "";
      }

      this.filter.addEventListener("change", () => {
        this.applyFilter(this.filter.value);
      });
    }

    applyFilter(category) {
      const normalized = category ? category.trim() : "";
      const filtered = normalized
        ? this.products.filter((product) => product.categoryName === normalized)
        : this.products;

      this.updateTitles(normalized);
      this.renderProducts(filtered);
    }

    updateTitles(category) {
      if (this.title) {
        this.title.textContent = category || "Alle Produkte";
      }
      if (this.subtitle) {
        this.subtitle.textContent = category
          ? "Handgemachte Auswahl für deinen Hund."
          : "Unsere handgemachten Lieblingsstücke auf einen Blick.";
      }
    }

    renderProducts(products) {
      this.grid.innerHTML = "";
      products.forEach((product, index) => {
        const card = this.createCard(product);
        card.style.animationDelay = `${index * 120}ms`;
        this.grid.appendChild(card);
      });
    }

    renderError(message) {
      this.grid.innerHTML = `<p class="product-error">${message}</p>`;
    }

    createCard(product) {
      const card = document.createElement("article");
      card.className = "product-card";

      const mediaLink = document.createElement("a");
      mediaLink.className = "product-media";
      mediaLink.href = `productdetail.html?sku=${encodeURIComponent(product.sku)}`;
      mediaLink.setAttribute("aria-label", `${product.name} ansehen`);

      const image = document.createElement("img");
      image.src = this.buildImagePath(product.sku);
      image.alt = product.name;
      image.loading = "lazy";
      image.addEventListener("error", () => {
        image.src = "assets/Logo/CaraDog_Logo.png";
      });

      mediaLink.appendChild(image);

      const title = document.createElement("h3");
      title.className = "product-title";
      const titleLink = document.createElement("a");
      titleLink.href = `productdetail.html?sku=${encodeURIComponent(product.sku)}`;
      titleLink.textContent = product.name;
      title.appendChild(titleLink);

      const description = document.createElement("p");
      description.textContent = this.truncateDescription(
        product.description || "Handgemachte Lieblingsstücke von CaraDog."
      );

      const price = document.createElement("span");
      price.className = "product-price";
      price.textContent = `${product.grossPrice.toFixed(2)} EUR`;

      const actions = document.createElement("div");
      actions.className = "product-actions";

      const addToCart = document.createElement("button");
      addToCart.className = "button button-primary";
      addToCart.type = "button";
      addToCart.textContent = "In den Warenkorb";
      addToCart.addEventListener("click", () => {
        this.cart.addItem(product.sku, 1);
        addToCart.textContent = "Hinzugefügt";
      });

      actions.append(addToCart);
      card.append(mediaLink, title, description, price, actions);
      return card;
    }

    truncateDescription(description) {
      const delimiterIndex = description.indexOf(",");
      if (delimiterIndex === -1) {
        return description.trim();
      }
      return description.slice(0, delimiterIndex).trim();
    }

    buildImagePath(sku) {
      const normalizedSku = (sku || "product").trim().toUpperCase();
      return `assets/products/${normalizedSku}-1.jpg`;
    }
  }

  document.addEventListener("DOMContentLoaded", () => {
    new ProductCategoryPage().init();
  });
})();
