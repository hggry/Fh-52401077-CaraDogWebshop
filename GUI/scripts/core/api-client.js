(() => {
  "use strict";

  class ApiClient {
    constructor(baseUrl) {
      this.baseUrl = (baseUrl || "http://localhost:8080/api").replace(/\/$/, "");
    }

    getProducts() {
      return this.getJson("/products");
    }

    getProductsBySku(skus) {
      const list = Array.isArray(skus) ? skus : [];
      const query = encodeURIComponent(list.join(","));
      return this.getJson(`/products/by-sku?skus=${query}`);
    }

    getProductsByCategory(categories) {
      const list = Array.isArray(categories) ? categories : [];
      const query = encodeURIComponent(list.join(","));
      return this.getJson(`/products/by-category?categories=${query}`);
    }

    getCartInfo(items) {
      return this.postJson("/cart/info", { items });
    }

    createOrder(payload) {
      return this.postJson("/orders", payload);
    }

    getJson(path) {
      return fetch(this.baseUrl + path, {
        headers: {
          "Accept": "application/json"
        }
      }).then(this.handleResponse);
    }

    postJson(path, payload) {
      return fetch(this.baseUrl + path, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "Accept": "application/json"
        },
        body: JSON.stringify(payload)
      }).then(this.handleResponse);
    }

    handleResponse(response) {
      if (response.ok) {
        return response.json();
      }

      return response.json().then((problem) => {
        const message = problem && problem.title ? problem.title : "Unbekannter Fehler";
        throw new Error(message);
      });
    }
  }

  window.CaraDogApiClient = ApiClient;
})();
