(() => {
  "use strict";

  class CartStorage {
    constructor(storageKey) {
      this.storageKey = storageKey || "caradog-cart";
    }

    getItems() {
      const raw = localStorage.getItem(this.storageKey);
      return raw ? JSON.parse(raw) : [];
    }

    addItem(sku, quantity) {
      const items = this.getItems();
      const existing = items.find((item) => item.sku === sku);

      if (existing) {
        existing.quantity += quantity;
      } else {
        items.push({ sku, quantity });
      }

      this.save(items);
    }

    setQuantity(sku, quantity) {
      const items = this.getItems();
      const existing = items.find((item) => item.sku === sku);

      if (!existing) {
        return;
      }

      existing.quantity = quantity;
      if (existing.quantity <= 0) {
        this.removeItem(sku);
        return;
      }

      this.save(items);
    }

    removeItem(sku) {
      const items = this.getItems().filter((item) => item.sku !== sku);
      this.save(items);
    }

    clear() {
      localStorage.removeItem(this.storageKey);
    }

    save(items) {
      localStorage.setItem(this.storageKey, JSON.stringify(items));
    }
  }

  window.CaraDogCartStorage = CartStorage;
})();
