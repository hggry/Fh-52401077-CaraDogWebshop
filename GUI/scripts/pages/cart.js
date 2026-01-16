(() => {
  "use strict";

  class CartPage {
    constructor() {
      const baseUrl = window.CARADOG_API_BASE || "http://localhost:8080/api";
      this.api = new window.CaraDogApiClient(baseUrl);
      this.cart = new window.CaraDogCartStorage("caradog-cart");
      this.itemsContainer = document.getElementById("cart-items");
      this.emptyState = document.getElementById("cart-empty");
      this.subtotalEl = document.getElementById("cart-subtotal");
      this.taxEl = document.getElementById("cart-tax");
      this.shippingEl = document.getElementById("cart-shipping");
      this.totalEl = document.getElementById("cart-total");
      this.form = document.getElementById("checkout-form");
      this.status = document.getElementById("order-status");
      this.products = [];
    }

    init() {
      const items = this.cart.getItems();
      if (!items.length) {
        this.renderEmpty();
        return;
      }

      const skus = items.map((item) => item.sku);
      this.api.getProductsBySku(skus)
        .then((products) => {
          this.products = products;
          this.renderItems(items);
          return this.refreshTotals();
        })
        .catch((error) => this.renderStatus(error.message, "error"));

      if (this.form) {
        this.form.addEventListener("submit", (event) => {
          event.preventDefault();
          this.submitOrder();
        });
      }
    }

    renderEmpty() {
      if (this.emptyState) {
        this.emptyState.hidden = false;
      }
      if (this.itemsContainer) {
        this.itemsContainer.innerHTML = "";
      }
      if (this.form) {
        this.form.querySelector("button[type='submit']").disabled = true;
      }
      this.updateTotals(null);
    }

    renderItems(items) {
      if (!this.itemsContainer) {
        return;
      }

      this.itemsContainer.innerHTML = "";
      items.forEach((item) => {
        const product = this.products.find((p) => p.sku === item.sku);
        if (!product) {
          return;
        }

        const row = document.createElement("article");
        row.className = "cart-item";

        row.innerHTML = `
          <div class="cart-thumb">
            <img src="assets/Set/DSC01626.jpg" alt="${product.name}" />
          </div>
          <div class="cart-item-meta">
            <h2 class="cart-item-title">${product.name}</h2>
            <p class="cart-item-options">SKU ${product.sku}</p>
          </div>
          <div class="cart-item-controls">
            <div class="cart-qty">
              <button type="button" data-action="decrease">-</button>
              <span>${item.quantity}</span>
              <button type="button" data-action="increase">+</button>
            </div>
            <span class="cart-item-price">${product.grossPrice.toFixed(2)} EUR</span>
            <button class="cart-remove" type="button" aria-label="Remove item">&times;</button>
          </div>
        `;

        const qtyDisplay = row.querySelector(".cart-qty span");
        const decrease = row.querySelector("[data-action='decrease']");
        const increase = row.querySelector("[data-action='increase']");
        const remove = row.querySelector(".cart-remove");

        decrease.addEventListener("click", () => {
          const newQty = item.quantity - 1;
          this.cart.setQuantity(item.sku, newQty);
          if (newQty <= 0) {
            row.remove();
          } else {
            item.quantity = newQty;
            qtyDisplay.textContent = String(newQty);
          }
          this.refreshTotals();
        });

        increase.addEventListener("click", () => {
          const newQty = item.quantity + 1;
          this.cart.setQuantity(item.sku, newQty);
          item.quantity = newQty;
          qtyDisplay.textContent = String(newQty);
          this.refreshTotals();
        });

        remove.addEventListener("click", () => {
          this.cart.removeItem(item.sku);
          row.remove();
          this.refreshTotals();
        });

        this.itemsContainer.appendChild(row);
      });
    }

    refreshTotals() {
      const items = this.cart.getItems();
      if (!items.length) {
        this.renderEmpty();
        return Promise.resolve();
      }

      return this.api.getCartInfo(items)
        .then((cartInfo) => {
          this.updateTotals(cartInfo);
        })
        .catch((error) => this.renderStatus(error.message, "error"));
    }

    updateTotals(cartInfo) {
      const format = (value) => value ? `${value.toFixed(2)} EUR` : "-";
      if (this.subtotalEl) {
        this.subtotalEl.textContent = cartInfo ? format(cartInfo.subtotalNet) : "-";
      }
      if (this.taxEl) {
        this.taxEl.textContent = cartInfo ? format(cartInfo.taxAmount) : "-";
      }
      if (this.shippingEl) {
        this.shippingEl.textContent = cartInfo ? format(cartInfo.shippingCost) : "-";
      }
      if (this.totalEl) {
        this.totalEl.textContent = cartInfo ? format(cartInfo.totalGross) : "-";
      }
    }

    submitOrder() {
      const items = this.cart.getItems();
      if (!items.length) {
        this.renderStatus("Dein Warenkorb ist leer.", "error");
        return;
      }

      const payload = {
        customer: this.getCustomerPayload(),
        items: items,
        paymentProvider: this.getPaymentProvider()
      };

      this.renderStatus("Bestellung wird verarbeitet...", "info");
      this.api.createOrder(payload)
        .then((order) => {
          this.cart.clear();
          this.renderStatus(
            `Bestellung ${order.id} erfolgreich. Bestätigung wird per E-Mail gesendet.`,
            "success"
          );
          this.renderEmpty();
        })
        .catch((error) => this.renderStatus(error.message, "error"));
    }

    getCustomerPayload() {
      const formData = new FormData(this.form);
      return {
        firstName: formData.get("firstName"),
        lastName: formData.get("lastName"),
        email: formData.get("email"),
        phone: formData.get("phone"),
        street: formData.get("street"),
        houseNumber: formData.get("houseNumber"),
        addressLine2: formData.get("addressLine2"),
        cityName: formData.get("cityName"),
        postalCode: formData.get("postalCode"),
        countryCode: formData.get("countryCode")
      };
    }

    getPaymentProvider() {
      const selected = this.form.querySelector("input[name='payment']:checked");
      return selected ? selected.value : "Paypal";
    }

    renderStatus(message, type) {
      if (!this.status) {
        return;
      }
      this.status.textContent = message;
      this.status.className = `cart-status ${type}`;
    }
  }

  document.addEventListener("DOMContentLoaded", () => {
    new CartPage().init();
  });
})();
