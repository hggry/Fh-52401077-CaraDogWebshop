(() => {
  "use strict";

  const navShells = document.querySelectorAll(".nav-shell");
  navShells.forEach((shell) => {
    const toggle = shell.querySelector(".nav-toggle");
    const nav = shell.querySelector(".site-nav");
    if (!toggle || !nav) {
      return;
    }

    const closeNav = () => {
      nav.classList.remove("is-open");
      toggle.setAttribute("aria-expanded", "false");
    };

    const openNav = () => {
      nav.classList.add("is-open");
      toggle.setAttribute("aria-expanded", "true");
    };

    toggle.addEventListener("click", () => {
      if (nav.classList.contains("is-open")) {
        closeNav();
        return;
      }
      openNav();
    });

    nav.querySelectorAll(".nav-link").forEach((link) => {
      link.addEventListener("click", () => closeNav());
    });

    document.addEventListener("click", (event) => {
      if (!nav.classList.contains("is-open")) {
        return;
      }
      if (!shell.contains(event.target)) {
        closeNav();
      }
    });

    const mediaQuery = window.matchMedia("(min-width: 901px)");
    mediaQuery.addEventListener("change", (event) => {
      if (event.matches) {
        closeNav();
      }
    });
  });

  const header = document.querySelector(".site-header");
  const logoWrap = header ? header.querySelector(".logo-wrap") : null;
  if (header) {
    let stickyThreshold = logoWrap ? logoWrap.offsetHeight : header.offsetHeight;
    let ticking = false;

    const applySticky = () => {
      const shouldStick = window.scrollY > stickyThreshold;
      header.classList.toggle("is-sticky", shouldStick);
      document.body.classList.toggle("has-sticky-header", shouldStick);
      if (shouldStick) {
        document.documentElement.style.setProperty("--sticky-header-offset", `${header.offsetHeight}px`);
      } else {
        document.documentElement.style.setProperty("--sticky-header-offset", "0px");
      }
    };

    const onScroll = () => {
      if (ticking) {
        return;
      }
      ticking = true;
      window.requestAnimationFrame(() => {
        applySticky();
        ticking = false;
      });
    };

    window.addEventListener("scroll", onScroll);
    window.addEventListener("resize", () => {
      stickyThreshold = logoWrap ? logoWrap.offsetHeight : header.offsetHeight;
      applySticky();
    });
    applySticky();
  }
})();
