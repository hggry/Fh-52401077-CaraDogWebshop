(() => {
  "use strict";

  class ProductDetailPage {
    constructor() {
      const baseUrl = window.CARADOG_API_BASE || "http://localhost:8080/api";
      this.api = new window.CaraDogApiClient(baseUrl);
      this.cart = new window.CaraDogCartStorage("caradog-cart");
      this.sku = new URLSearchParams(window.location.search).get("sku");
      this.container = document.getElementById("product-detail");
      this.imagePaths = [];
    }

    init() {
      if (!this.container || !this.sku) {
        return;
      }

      this.api.getProductsBySku([this.sku])
        .then((products) => {
          const product = products[0];
          if (!product) {
            this.renderError("Produkt nicht gefunden.");
            return;
          }
          return this.loadAvailableImages(product.sku)
            .then((imagePaths) => {
              this.renderProduct(product, imagePaths);
            });
        })
        .catch((error) => this.renderError(error.message));
    }

    loadAvailableImages(sku) {
      const maxImages = 6;
      const normalizedSku = (sku || "product").trim().toUpperCase();
      const candidates = [];
      for (let i = 1; i <= maxImages; i += 1) {
        candidates.push(`assets/products/${normalizedSku}-${i}.jpg`);
      }

      const checks = candidates.map((path) => new Promise((resolve) => {
        const probe = new Image();
        probe.onload = () => resolve(path);
        probe.onerror = () => resolve(null);
        probe.src = path;
      }));

      return Promise.all(checks)
        .then((results) => results.filter((path) => Boolean(path)));
    }

    renderProduct(product, imagePaths) {
      this.container.innerHTML = "";

      const layout = document.createElement("div");
      layout.className = "product-detail-main";

      const media = document.createElement("div");
      media.className = "product-detail-media";

      this.imagePaths = Array.isArray(imagePaths) ? imagePaths : [];

      const mainImage = document.createElement("img");
      mainImage.className = "product-main-image is-ready";
      mainImage.alt = product.name;
      mainImage.addEventListener("load", () => {
        mainImage.classList.add("is-ready");
      });
      mainImage.addEventListener("error", () => {
        mainImage.src = "assets/Logo/CaraDog_Logo.png";
      });

      const setMainImage = (path) => {
        mainImage.classList.remove("is-ready");
        mainImage.src = path;
      };

      const thumbnails = document.createElement("div");
      thumbnails.className = "product-thumbnails";

      let activeThumb = null;

      const selectImage = (path, button) => {
        if (activeThumb) {
          activeThumb.classList.remove("is-active");
        }
        activeThumb = button;
        if (activeThumb) {
          activeThumb.classList.add("is-active");
        }
        setMainImage(path);
      };

      this.imagePaths.forEach((path, index) => {
        const button = document.createElement("button");
        button.type = "button";
        button.className = "product-thumb";
        button.dataset.imagePath = path;
        button.setAttribute("aria-label", `Bild ${index + 1} von ${product.name}`);

        const thumbImage = document.createElement("img");
        thumbImage.src = path;
        thumbImage.alt = product.name;
        thumbImage.loading = "lazy";
        button.addEventListener("click", () => {
          selectImage(path, button);
        });

        button.appendChild(thumbImage);
        thumbnails.appendChild(button);
        if (index === 0) {
          activeThumb = button;
          button.classList.add("is-active");
        }
      });

      if (this.imagePaths.length > 0) {
        setMainImage(this.imagePaths[0]);
      } else {
        setMainImage("assets/Logo/CaraDog_Logo.png");
      }

      media.append(mainImage, thumbnails);

      const content = document.createElement("div");
      content.className = "product-detail-content";

      const title = document.createElement("h1");
      title.textContent = product.name;

      const meta = document.createElement("p");
      meta.className = "product-detail-meta";
      meta.textContent = `${product.categoryName} - SKU ${product.sku}`;

      const description = this.buildDescriptionList(product.description);

      const price = document.createElement("div");
      price.className = "product-detail-price";
      price.textContent = `${product.grossPrice.toFixed(2)} EUR inkl. Steuer`;

      const qtyWrap = document.createElement("div");
      qtyWrap.className = "product-qty";
      qtyWrap.innerHTML = `
        <label for="quantity">Menge</label>
        <input id="quantity" type="number" min="1" value="1" />
      `;

      const actions = document.createElement("div");
      actions.className = "product-actions";

      const addButton = document.createElement("button");
      addButton.className = "button button-primary";
      addButton.type = "button";
      addButton.textContent = "In den Warenkorb";
      addButton.addEventListener("click", () => {
        const quantity = Number.parseInt(document.getElementById("quantity").value, 10) || 1;
        this.cart.addItem(product.sku, Math.max(1, quantity));
        addButton.textContent = "Hinzugefügt";
      });

      const cartLink = document.createElement("a");
      cartLink.className = "button button-outline";
      cartLink.href = "cart.html";
      cartLink.textContent = "Zum Warenkorb";

      actions.append(addButton, cartLink);
      content.append(title, meta, description, price, qtyWrap, actions);

      layout.append(media, content);
      this.container.append(layout);

      const tagSection = this.buildTagSection(product.tags);
      if (tagSection) {
        this.container.append(tagSection);
      }
    }

    buildDescriptionList(description) {
      const list = document.createElement("ul");
      list.className = "product-detail-list";
      const items = (description || "Handgemachte Lieblingsstücke von CaraDog.")
        .split(",")
        .map((item) => item.trim())
        .filter((item) => item.length > 0);

      items.forEach((item) => {
        const li = document.createElement("li");
        li.textContent = item;
        list.appendChild(li);
      });

      return list;
    }

    buildTagSection(tags) {
      const tagCatalog = window.CaraDogTagCatalog || {};
      const activeTags = (Array.isArray(tags) ? tags : [])
        .filter((tag) => tagCatalog[tag]);

      if (activeTags.length === 0) {
        return null;
      }

      const section = document.createElement("section");
      section.className = "product-tag-section";

      const header = document.createElement("div");
      header.className = "section-header";
      header.innerHTML = `
        <h2>Material &amp; Besonderheiten</h2>
        <p>Mehr zu den Materialien und Details deines Produkts.</p>
      `;

      const grid = document.createElement("div");
      grid.className = "product-tag-grid";

      activeTags.forEach((tagKey) => {
        const info = tagCatalog[tagKey];
        const card = document.createElement("article");
        card.className = "product-tag-card";

        const image = document.createElement("img");
        image.src = info.image;
        image.alt = info.title;
        image.loading = "lazy";

        const text = document.createElement("div");
        text.className = "product-tag-text";

        const title = document.createElement("h3");
        title.textContent = info.title;

        const body = document.createElement("p");
        body.textContent = info.text;

        text.append(title, body);
        card.append(image, text);
        grid.appendChild(card);
      });

      section.append(header, grid);
      return section;
    }

    renderError(message) {
      this.container.innerHTML = `<p class="product-error">${message}</p>`;
    }
  }

  document.addEventListener("DOMContentLoaded", () => {
    new ProductDetailPage().init();
  });
})();
