(() => {
  "use strict";

  class IndexPage {
    constructor() {
      const baseUrl = window.CARADOG_API_BASE || "http://localhost:8080/api";
      this.api = new window.CaraDogApiClient(baseUrl);
      this.grid = document.getElementById("product-grid");
      this.featuredSkus = [
        "TOY-MONKEYDOT-TARTAN",
        "HALSBAND-LONDON",
        "LEINE-ROYAL",
        "LEINE-SUMMER-BREEZE"
      ];
    }

    init() {
      if (!this.grid) {
        return;
      }

      this.api.getProducts()
        .then((products) => this.renderProducts(this.selectFeaturedProducts(products)))
        .catch((error) => this.renderError(error.message));
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

    selectFeaturedProducts(products) {
      const bySku = new Map(
        products.map((product) => [product.sku.toUpperCase(), product])
      );
      return this.featuredSkus
        .map((sku) => bySku.get(sku.toUpperCase()))
        .filter(Boolean);
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

      const category = document.createElement("span");
      category.className = "product-category";
      category.textContent = product.categoryName;

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

      card.append(mediaLink, category, title, description, price);
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
    new IndexPage().init();
  });
})();
