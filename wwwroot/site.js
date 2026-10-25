/* ==========================================================================
   TaskManager — site.js   (работает с API /api/Tasks)
   ========================================================================== */

const API_URL = "/api/Tasks";

const CATEGORY_LABELS = { Study: "Учёба", Work: "Работа", Shopping: "Покупки", Personal: "Личное", Other: "Другое" };
const PRIORITY_LABELS = { Low: "Низкий", Medium: "Средний", High: "Высокий" };

const el = {
  form: document.getElementById("task-form"),
  text: document.getElementById("task-text"),
  textError: document.getElementById("task-error"),
  submit: document.getElementById("submit-btn"),
  submitLabel: document.getElementById("submit-label"),
  filterCategory: document.getElementById("filter-category"),
  filterPriority: document.getElementById("filter-priority"),
  filtersReset: document.getElementById("filters-reset"),
  list: document.getElementById("task-list"),
  count: document.getElementById("tasks-count"),
  loading: document.getElementById("tasks-loading"),
  empty: document.getElementById("empty-state"),
  template: document.getElementById("task-card-template"),
  toasts: document.getElementById("toasts"),
  modal: document.getElementById("modal"),
  modalText: document.getElementById("modal-text"),
  modalOk: document.getElementById("modal-confirm"),
  modalCancel: document.getElementById("modal-cancel"),
  modalOverlay: document.querySelector("#modal .modal__overlay"),
};

let tasks = [];

/* ---------- API ---------- */
async function apiRequest(url, options = {}) {
  let response;
  try {
    response = await fetch(url, options);
  } catch {
    throw new Error("Нет связи с сервером.");
  }
  if (!response.ok) {
    let errorMessage = "";
    try {
      const data = await response.json();
      errorMessage = data.error || data.title || "";
    } catch {
      // Ответ сервера не содержит JSON
    }
    if (errorMessage) {
      throw new Error(errorMessage);
    }
    if (response.status === 400) {
      throw new Error("Не удалось создать задачу. Проверьте текст задачи");
    }
    if (response.status === 404) {
      throw new Error("Задача не найдена");
    }
    if (response.status >= 500) {
      throw new Error("Не удалось обработать задачу. Попробуйте ещё раз");
    }
    throw new Error("Не удалось выполнить запрос. Попробуйте ещё раз");
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

function buildListUrl() {
  const params = new URLSearchParams();
  if (el.filterCategory.value) params.append("category", el.filterCategory.value);
  if (el.filterPriority.value) params.append("priority", el.filterPriority.value);
  const query = params.toString();
  return query ? `${API_URL}?${query}` : API_URL;
}

/* ---------- ТОСТЫ ---------- */
function toast(message, type = "info") {
  const node = document.createElement("div");
  node.className = `toast toast--${type}`;
  const dot = document.createElement("span");
  dot.className = "toast__dot";
  dot.setAttribute("aria-hidden", "true");
  const text = document.createElement("span");
  text.className = "toast__text";
  text.textContent = message;
  node.append(dot, text);
  el.toasts.appendChild(node);

  const life = type === "error" ? 6000 : 4000;
  setTimeout(() => {
    node.classList.add("is-hiding");
    setTimeout(() => node.remove(), 400);
  }, life);
}

/* ---------- ФОРМА ---------- */
function setFormLoading(isLoading) {
  el.submit.disabled = isLoading;
  el.submit.classList.toggle("is-loading", isLoading);
  el.submit.setAttribute("aria-busy", String(isLoading));
  el.form.setAttribute("aria-busy", String(isLoading));
  el.submitLabel.textContent = isLoading ? "Классифицирую задачу…" : "Добавить задачу";
}

function showError(message) {
  el.textError.textContent = message;
  el.textError.hidden = false;
  el.text.classList.add("is-invalid");
  el.text.setAttribute("aria-invalid", "true");
}

function clearError() {
  el.textError.textContent = "";
  el.textError.hidden = true;
  el.text.classList.remove("is-invalid");
  el.text.removeAttribute("aria-invalid");
}

function selectedClassifier() {
  const checked = document.querySelector("input[name=classifier]:checked");
  return checked ? checked.value : "AI";
}

function setListLoading(isLoading) {
  el.loading.hidden = !isLoading;
}

/* ---------- ОТРИСОВКА ---------- */
function tasksWord(n) {
  const d = n % 10;
  const h = n % 100;
  if (d === 1 && h !== 11) return "задача";
  if (d >= 2 && d <= 4 && (h < 12 || h > 14)) return "задачи";
  return "задач";
}

function formatDeadline(iso) {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return "";
  return date.toLocaleDateString("ru-RU", { day: "2-digit", month: "short", year: "numeric" });
}

function createCard(task, index) {
  const fragment = el.template.content.cloneNode(true);
  const card = fragment.querySelector(".task");
  const title = fragment.querySelector(".task__title");
  const catChip = fragment.querySelector(".chip--cat");
  const prioChip = fragment.querySelector(".chip--prio");
  const dateChip = fragment.querySelector(".chip--date");

  card.dataset.id = task.id;
  card.style.animationDelay = `${Math.min(index, 12) * 55}ms`;
  if (task.isCompleted) card.classList.add("is-done");

  title.textContent = task.text;
  catChip.textContent = CATEGORY_LABELS[task.category] || task.category;
  prioChip.textContent = PRIORITY_LABELS[task.priority] || task.priority;
  prioChip.dataset.prio = task.priority;

  if (task.deadline) {
    dateChip.hidden = false;
    dateChip.dateTime = task.deadline;
    dateChip.textContent = `Срок: ${formatDeadline(task.deadline)}`;
  }

  fragment.querySelector("[data-action=complete]").addEventListener("click", () => toggleTask(task.id));
  fragment.querySelector("[data-action=delete]").addEventListener("click", () => deleteTask(task.id));
  return fragment;
}

// Ключ сортировки по сроку: чем раньше дедлайн, тем меньше значение.
// Задачи без срока (или с некорректной датой) считаются «бесконечными» и уезжают вниз.
function deadlineTime(task) {
  if (!task.deadline) return Number.POSITIVE_INFINITY;
  const time = new Date(task.deadline).getTime();
  return Number.isNaN(time) ? Number.POSITIVE_INFINITY : time;
}

// Сортировка: сначала активные (выполненные — вниз), затем по сроку (ближе срок — выше)
function compareTasks(a, b) {
  if (a.isCompleted !== b.isCompleted) {
    return Number(a.isCompleted) - Number(b.isCompleted);
  }
  const deadlineA = deadlineTime(a);
  const deadlineB = deadlineTime(b);
  if (deadlineA === deadlineB) return 0;
  return deadlineA - deadlineB;
}

function render() {
  el.list.innerHTML = "";
  // Ближе срок — выше задача; выполненные и без срока — в конце
  const ordered = [...tasks].sort(compareTasks);
  ordered.forEach((task, index) => el.list.appendChild(createCard(task, index)));

  el.count.textContent = tasks.length ? `${tasks.length} ${tasksWord(tasks.length)}` : "";

  const isEmpty = tasks.length === 0;
  el.empty.hidden = !isEmpty;
  if (isEmpty) {
    const filtered = Boolean(el.filterCategory.value || el.filterPriority.value);
    const title = el.empty.querySelector(".empty__title");
    const text = el.empty.querySelector(".empty__text");
    title.textContent = filtered ? "Ничего не найдено" : "Пока задач нет";
    text.textContent = filtered
      ? "Под выбранные фильтры задач нет. Попробуйте сбросить фильтры."
      : "Добавьте первую задачу — она появится здесь.";
  }
}

function setCardBusy(id, busy) {
  const card = el.list.querySelector(`.task[data-id="${id}"]`);
  if (!card) return;
  card.querySelectorAll("button").forEach((btn) => { btn.disabled = busy; });
}

/* ---------- ЗАГРУЗКА ---------- */
async function loadTasks() {
  setListLoading(true);
  try {
    tasks = await apiRequest(buildListUrl());
  } catch (error) {
    setListLoading(false);
    toast(error.message, "error");
    el.empty.hidden = false;
    el.empty.querySelector(".empty__title").textContent = "Не удалось загрузить задачи";
    el.empty.querySelector(".empty__text").textContent = "Проверьте, что сервер запущен, и обновите страницу.";
    return;
  }
  setListLoading(false);
  render();
}

/* ---------- ДЕЙСТВИЯ ---------- */
async function createTask(event) {
  event.preventDefault();
  clearError();

  const text = el.text.value.trim();
  if (!text) {
    showError("Введите текст задачи.");
    el.text.focus();
    return;
  }

  setFormLoading(true);
  const classifierType = selectedClassifier();

  try {
    const created = await apiRequest(API_URL, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ text: text, classifierType: classifierType }),
    });
    el.text.value = "";
    tasks.unshift(created);
    render();
    toast(`Задача добавлена (${classifierType}).`, "success");
  } catch (error) {
    toast(error.message, "error");
  } finally {
    setFormLoading(false);
  }
}

async function toggleTask(id) {
  const task = tasks.find((t) => t.id === id);
  if (!task) return;

  setCardBusy(id, true);
  try {
    // PUT заменяет задачу целиком — берём актуальную версию с сервера и меняем статус
    const fresh = await apiRequest(`${API_URL}/${id}`);
    fresh.isCompleted = !fresh.isCompleted;

    const updated = await apiRequest(`${API_URL}/${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(fresh),
    });

    tasks = tasks.map((t) => (t.id === id ? updated : t));
    render();
    toast(updated.isCompleted ? "Задача выполнена." : "Задача возвращена в работу.", "success");
  } catch (error) {
    toast(error.message, "error");
    setCardBusy(id, false);
  }
}

async function deleteTask(id) {
  const task = tasks.find((t) => t.id === id);
  if (!task) return;

  const confirmed = await openConfirm(`Задача «${task.text}» будет удалена безвозвратно.`);
  if (!confirmed) return;

  setCardBusy(id, true);
  try {
    await apiRequest(`${API_URL}/${id}`, { method: "DELETE" });
    tasks = tasks.filter((t) => t.id !== id);
    render();
    toast("Задача удалена.", "success");
  } catch (error) {
    toast(error.message, "error");
    setCardBusy(id, false);
  }
}


/* ---------- МОДАЛЬНОЕ ОКНО ПОДТВЕРЖДЕНИЯ ---------- */
let modalResolve = null;
let lastFocused = null;

function openConfirm(message) {
  el.modalText.textContent = message;
  el.modal.hidden = false;
  document.body.classList.add("is-locked");

  lastFocused = document.activeElement;
  el.modalCancel.focus();

  return new Promise((resolve) => { modalResolve = resolve; });
}

function closeConfirm(result) {
  if (el.modal.hidden) return;

  el.modal.hidden = true;
  document.body.classList.remove("is-locked");

  if (lastFocused && typeof lastFocused.focus === "function") lastFocused.focus();
  lastFocused = null;

  const resolve = modalResolve;
  modalResolve = null;
  if (resolve) resolve(result);
}

/* ---------- ИНИЦИАЛИЗАЦИЯ ---------- */
el.form.addEventListener("submit", createTask);

// Enter — отправить задачу, Shift+Enter — перенос строки
el.text.addEventListener("keydown", (event) => {
  if (event.key === "Enter" && !event.shiftKey) {
    event.preventDefault();
    el.form.requestSubmit();
  }
});

el.text.addEventListener("input", clearError);

// Фильтры применяются на сервере (GET /api/Tasks?category=&priority=)
el.filterCategory.addEventListener("change", loadTasks);
el.filterPriority.addEventListener("change", loadTasks);
el.filtersReset.addEventListener("click", () => {
  el.filterCategory.value = "";
  el.filterPriority.value = "";
  loadTasks();
});

// Модальное окно: кнопки, клик по затемнению и Esc
el.modalCancel.addEventListener("click", () => closeConfirm(false));
el.modalOk.addEventListener("click", () => closeConfirm(true));
el.modalOverlay.addEventListener("click", () => closeConfirm(false));

document.addEventListener("keydown", (event) => {
  if (el.modal.hidden) return;

  if (event.key === "Escape") {
    event.preventDefault();
    closeConfirm(false);
    return;
  }

  // Простой фокус-трап: Tab не выходит за пределы окна
  if (event.key === "Tab") {
    const buttons = el.modal.querySelectorAll("button");
    const first = buttons[0];
    const last = buttons[buttons.length - 1];

    if (event.shiftKey && document.activeElement === first) {
      event.preventDefault();
      last.focus();
    } else if (!event.shiftKey && document.activeElement === last) {
      event.preventDefault();
      first.focus();
    }
  }
});

// Первая загрузка
loadTasks();

