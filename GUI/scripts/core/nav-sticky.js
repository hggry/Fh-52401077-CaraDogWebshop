(() => {
  "use strict";

  const logoWrap = document.querySelector(".logo-wrap");
  if (!logoWrap) {
    return;
  }

  const body = document.body;

  const setSticky = (isSticky) => {
    body.classList.toggle("has-sticky-logo", isSticky);
  };

  if ("IntersectionObserver" in window) {
    const observer = new IntersectionObserver(
      ([entry]) => {
        setSticky(!entry.isIntersecting);
      },
      { threshold: 0 }
    );
    observer.observe(logoWrap);
  } else {
    const onScroll = () => {
      const rect = logoWrap.getBoundingClientRect();
      setSticky(rect.bottom <= 0);
    };
    window.addEventListener("scroll", onScroll, { passive: true });
    window.addEventListener("resize", onScroll);
    onScroll();
  }
})();
