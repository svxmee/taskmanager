/* ==========================================================================
   TaskManager — site.js
   Логика страницы: загрузка, создание, фильтры, завершение, удаление задач.
   Работает с API /api/tasks (см. Controllers/TasksController.cs)
   ========================================================================== */

/* ---------- 1. КОНСТАНТЫ И ССЫЛКИ НА ЭЛЕМЕНТЫ ---------- */

const API_URL = '/api/tasks';

// Сервер отдаёт enum-ы строками (JsonStringEnumConverter) — переводим их на русский
const CATEGORY_LABELS = {
  Study: 'Учёба',
  Work: 'Работа',
  Shopping: 'Покупки',
  Personal: 'Личное',
  Other: 'Другое',
};

const PRIORITY_LABELS = {
  Low: 'Низкий',
  Medium: 'Средний',
  High: 'Высокий',
};

// Один раз находим нужные элементы — дальше работаем через объект el
const el = {
  form: document.getElementById('task-form'),
  text: document.getElementById('task-text'),
  textError: document.getElementById('task-text-error'),
  submit: document.getElementById('task-submit'),
  notifications: document.getElementById('notifications'),
  filterCategory: document.getElementById('filter-category'),
  filterPriority: document.getElementById('filter-priority'),
  filtersReset: document.getElementById('filters-reset'),
  list: document.getElementById('task-list'),
  count: document.getElementById('tasks-count'),
  loading: document.getElementById('tasks-loading'),
  empty: document.getElementById('empty-state'),
  template: document.getElementById('task-card-template'),
};

// «Состояние» приложения — единственный источник правды для отрисовки
let tasks = [];

/* ---------- 2. РАБОТА С API ---------- */

// Обёртка над fetch: разбирает JSON и бросает понятные ошибки
async function apiRequest(url, options = {}) {
  let response;

  try {
    response = await fetch(url, options);
  } catch {
    throw new Error('Не удалось связаться с сервером. Запущен ли backend?');
  }

  if (!response.ok) {
    throw new Error(`Сервер вернул ошибку ${response.status}`);
  }

  // DELETE отвечает 204 No Content — тела нет, парсить нечего
  if (response.status === 204) return null;

  return response.json();
}

/* ---------- 3. УВЕДОМЛЕНИЯ ---------- */

function showNotification(message, type = 'info') {
  const note = document.createElement('div');
  note.className = `notification notification--${type}`;
  note.textContent = message;
  el.notifications.appendChild(note);

  // Через несколько секунд убираем, чтобы не копились
  const lifetime = type === 'error' ? 6000 : 4000;
  setTimeout(() => note.remove(), lifetime);
}

/* ---------- 4. СОСТОЯНИЯ ФОРМЫ ---------- */

function setFormLoading(isLoading) {
  el.submit.disabled = isLoading;
  el.submit.classList.toggle('is-loading', isLoading);
}

function showFieldError(message) {
  el.textError.textContent = message;
  el.textError.hidden = false;
  el.text.classList.add('is-invalid');
  el.text.setAttribute('aria-invalid', 'true');
}

function clearFieldError() {
  el.textError.textContent = '';
  el.textError.hidden = true;
  el.text.classList.remove('is-invalid');
  el.text.removeAttribute('aria-invalid');
}

/* ---------- 5. ОТРИСОВКА СПИСКА ---------- */

// Правильная форма слова: 1 задача / 2 задачи / 5 задач
function tasksWord(n) {
  const mod10 = n % 10;
  const mod100 = n % 100;
  if (mod10 === 1 && mod100 !== 11) return 'задача';
  if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) return 'задачи';
  return 'задач';
}

function formatDeadline(iso) {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return '';
  return date.toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit', year: 'numeric' });
}

// Клонируем <template> и заполняем его данными одной задачи
function createCard(task) {
  const fragment = el.template.content.cloneNode(true);

  const card = fragment.querySelector('.task-card');
  const textEl = fragment.querySelector('.task-card__text');
  const statusEl = fragment.querySelector('.task-card__status');
  const categoryBadge = fragment.querySelector('.badge--category');
  const priorityBadge = fragment.querySelector('.badge--priority');
  const deadlineBadge = fragment.querySelector('.badge--deadline');

  card.dataset.id = task.id;
  if (task.isCompleted) card.classList.add('is-completed');

  // textContent (а не innerHTML) — текст задачи не может «внедрить» HTML
  textEl.textContent = task.text;

  categoryBadge.textContent = CATEGORY_LABELS[task.category] || task.category;

  priorityBadge.textContent = PRIORITY_LABELS[task.priority] || task.priority;
  priorityBadge.dataset.priority = task.priority; // по этому атрибуту работает цвет в CSS

  if (task.deadline) {
    deadlineBadge.hidden = false;
    deadlineBadge.dateTime = task.deadline;
    deadlineBadge.textContent = `Дедлайн: ${formatDeadline(task.deadline)}`;
  }

  if (task.isCompleted) {
    statusEl.hidden = false;
    statusEl.textContent = 'Выполнено';
  }

  // Замыкание: каждая кнопка «помнит» id своей задачи
  fragment.querySelector('.btn--complete').addEventListener('click', () => completeTask(task.id));
  fragment.querySelector('.btn--danger').addEventListener('click', () => deleteTask(task.id));

  return fragment;
}

// Применяем фильтры к полному списку задач
function getVisibleTasks() {
  const category = el.filterCategory.value;
  const priority = el.filterPriority.value;

  return tasks
    .filter((task) => !category || task.category === category)
    .filter((task) => !priority || task.priority === priority)
    // сначала незавершённые, внутри группы — новые сверху
    .sort((a, b) => Number(a.isCompleted) - Number(b.isCompleted) || b.id - a.id);
}

function setEmptyState(mode) {
  const title = el.empty.querySelector('.empty-state__title');
  const body = el.empty.querySelector('.empty-state__text');

  if (mode === 'error') {
    title.textContent = 'Не удалось загрузить задачи';
    body.textContent = 'Проверьте, что сервер запущен, и обновите страницу.';
  } else if (mode === 'no-matches') {
    title.textContent = 'Ничего не найдено';
    body.textContent = 'По выбранным фильтрам задач нет. Попробуйте сбросить фильтры.';
  } else {
    title.textContent = 'Пока задач нет';
    body.textContent = 'Добавьте первую задачу в форме выше — она появится здесь.';
  }
}

function render() {
  const visible = getVisibleTasks();

  el.list.innerHTML = '';
  for (const task of visible) {
    el.list.appendChild(createCard(task));
  }

  // Счётчик задач
  const total = tasks.length;
  if (total === 0) {
    el.count.textContent = '';
  } else if (visible.length === total) {
    el.count.textContent = `${total} ${tasksWord(total)}`;
  } else {
    el.count.textContent = `Показано ${visible.length} из ${total}`;
  }

  // Пустое состояние
  const isEmpty = visible.length === 0;
  el.empty.hidden = !isEmpty;
  if (isEmpty) {
    const filtered = Boolean(el.filterCategory.value || el.filterPriority.value);
    setEmptyState(filtered ? 'no-matches' : 'no-tasks');
  }
}

function setListLoading(isLoading) {
  el.loading.hidden = !isLoading;
}

// Блокируем кнопки конкретной карточки, пока идёт запрос
function setCardBusy(id, isBusy) {
  const card = el.list.querySelector(`.task-card[data-id='${id}']`);
  if (!card) return;
  card.querySelectorAll('button').forEach((btn) => { btn.disabled = isBusy; });
}

/* ---------- 6. ДЕЙСТВИЯ (запросы к API) ---------- */

// GET /api/tasks — загрузка при открытии страницы
async function loadTasks() {
  setListLoading(true);

  try {
    tasks = await apiRequest(API_URL);
  } catch (error) {
    setListLoading(false);
    showNotification(error.message, 'error');
    el.empty.hidden = false;
    setEmptyState('error');
    return;
  }

  setListLoading(false);
  render();
}

// POST /api/tasks — создание задачи
async function createTask(event) {
  event.preventDefault();
  clearFieldError();

  const text = el.text.value.trim();
  if (!text) {
    showFieldError('Введите текст задачи.');
    el.text.focus();
    return;
  }

  setFormLoading(true);

  try {
    // Отправляем только текст: категорию, приоритет и дедлайн
    // сервер определит сам (TaskService.CreateTask -> AiTaskClassifier)
    const created = await apiRequest(API_URL, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ text: text }),
    });

    tasks.push(created);
    el.text.value = '';
    render();
    showNotification('Задача добавлена.', 'success');
  } catch (error) {
    showNotification(error.message, 'error');
  } finally {
    setFormLoading(false);
  }
}

// PUT /api/tasks/{id} — отметить задачу выполненной
async function completeTask(id) {
  const task = tasks.find((t) => t.id === id);
  if (!task) return;

  setCardBusy(id, true);

  try {
    // ВАЖНО: PUT заменяет задачу целиком, поэтому отправляем ВСЕ поля.
    // Если отправить только isCompleted, сервер обнулит категорию и приоритет.
    const updated = await apiRequest(`${API_URL}/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        text: task.text,
        category: task.category,
        priority: task.priority,
        deadline: task.deadline,
        isCompleted: true,
      }),
    });

    tasks = tasks.map((t) => (t.id === id ? updated : t));
    render();
    showNotification('Задача завершена.', 'success');
  } catch (error) {
    showNotification(error.message, 'error');
    setCardBusy(id, false);
  }
}

// DELETE /api/tasks/{id} — удаление задачи
async function deleteTask(id) {
  const task = tasks.find((t) => t.id === id);
  if (!task) return;

  if (!confirm(`Удалить задачу «${task.text}»?`)) return;

  setCardBusy(id, true);

  try {
    await apiRequest(`${API_URL}/${id}`, { method: 'DELETE' });
    tasks = tasks.filter((t) => t.id !== id);
    render();
    showNotification('Задача удалена.', 'success');
  } catch (error) {
    showNotification(error.message, 'error');
    setCardBusy(id, false);
  }
}

/* ---------- 7. ИНИЦИАЛИЗАЦИЯ ---------- */

el.form.addEventListener('submit', createTask);

// Ошибку под полем убираем, как только пользователь начал печатать
el.text.addEventListener('input', clearFieldError);

// Фильтры применяются мгновенно, без запросов к серверу
el.filterCategory.addEventListener('change', render);
el.filterPriority.addEventListener('change', render);
el.filtersReset.addEventListener('click', () => {
  el.filterCategory.value = '';
  el.filterPriority.value = '';
  render();
});

// Первая загрузка списка
loadTasks();