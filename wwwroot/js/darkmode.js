
(function () {
  const key = "flashcards_darkmode";
  const body = document.body;

  function apply(on) {
    if (on) body.classList.add("dark-mode");
    else body.classList.remove("dark-mode");
  }

  const initial = localStorage.getItem(key) === "1";
  apply(initial);

  const btn = document.getElementById("dark-toggle");
  if (!btn) return;

  btn.addEventListener("click", function () {
    const on = !body.classList.contains("dark-mode");
    apply(on);
    localStorage.setItem(key, on ? "1" : "0");
  });
})();
