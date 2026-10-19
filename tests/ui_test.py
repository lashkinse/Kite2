# -*- coding: utf-8 -*-
"""UI-тест Kite2 (pywinauto, UIA): состояние читается из контролов и журнала, скриншоты не используются.

Запуск:  python tests/ui_test.py [php начала фазы, напр. save|fragment|ask]
Требования: pywinauto, приложение собрано в src/Kite2.App/bin/x64/Release/net48.
"""
import ctypes
import ctypes.wintypes as wt
import os
import sys
import time

from pywinauto import Application, Desktop, keyboard

EXE = r"D:\Dev\C_Sharp\Kite2\src\Kite2.App\bin\x64\Release\net48\Kite2.exe"
LOG = os.path.join(os.environ["APPDATA"], "Kite2", "logs", time.strftime("Kite2-%Y%m%d.log"))
TMP = r"C:\Temp"

_failures = []
_logpos = 0


def log_init():
    global _logpos
    _logpos = os.path.getsize(LOG) if os.path.exists(LOG) else 0


def log_newlines():
    """Новые строки журнала с момента последнего чтения."""
    global _logpos
    if not os.path.exists(LOG):
        return []
    with open(LOG, "r", encoding="utf-8", errors="replace") as f:
        f.seek(_logpos)
        data = f.read()
        _logpos = f.tell()
    return data.splitlines()


def wait_log(pattern, timeout=10.0, contains=None):
    """Ждать строку журнала с подстрокой (или regex-подтекстом). Возвращает строку или None."""
    deadline = time.time() + timeout
    while time.time() < deadline:
        for line in log_newlines():
            ok = pattern in line if contains is None else contains(line)
            if ok:
                return line
        time.sleep(0.3)
    return None


def check(name, cond, detail=""):
    mark = "OK  " if cond else "FAIL"
    line = f"[{mark}] {name}" + (f" — {detail}" if detail else "")
    print(line, flush=True)
    if not cond:
        _failures.append(line)
    return cond


# ---------- окна процесса (своё EnumWindows: владеемые окна UIA-Desktop не показывает) ----------

def enum_windows_of(pid):
    out = []
    proto = ctypes.WINFUNCTYPE(wt.BOOL, wt.HWND, wt.LPARAM)

    def cb(hwnd, _):
        owner = wt.DWORD()
        ctypes.windll.user32.GetWindowThreadProcessId(hwnd, ctypes.byref(owner))
        if owner.value == pid and ctypes.windll.user32.IsWindowVisible(hwnd):
            length = ctypes.windll.user32.GetWindowTextLengthW(hwnd)
            buf = ctypes.create_unicode_buffer(length + 1)
            ctypes.windll.user32.GetWindowTextW(hwnd, buf, length + 1)
            out.append((hwnd, buf.value))
        return True

    ctypes.windll.user32.EnumWindows(proto(cb), 0)
    return out


def foreground(hwnd, timeout=3.0):
    """Активация окна с ожиданием: у системы переключение бывает ленивым."""
    U = ctypes.windll.user32
    U.SetForegroundWindow(hwnd)
    deadline = time.time() + timeout
    while time.time() < deadline:
        if U.GetForegroundWindow() == hwnd:
            return True
        U.SetForegroundWindow(hwnd)
        time.sleep(0.2)
    return False


def wrap_uia(hwnd):
    """Обернуть HWND через UIA (поиск по title падает на владеемых окнах WPF, поиск по handle — нет)."""
    desktop = Desktop(backend="uia")
    return desktop.window(handle=hwnd)


def find_dlg(title, timeout=8.0):
    """Диалог процесса: окно с точным заголовком среди видимых окон Kite2."""
    pid = _pid[0]
    deadline = time.time() + timeout
    while time.time() < deadline:
        for hwnd, text in enum_windows_of(pid):
            if text == title:
                return wrap_uia(hwnd), hwnd
        time.sleep(0.3)
    return None, None


def click_in_dlg(dlg, hwnd, caption, aliases=(), fallback_enter=True):
    """Кнопку диалога — сначала по подписи через UIA, иначе Enter/клавиши по умолчанию."""
    if dlg is not None:
        for cap in (caption,) + tuple(aliases):
            btns = [b for b in dlg.descendants(control_type="Button")]
            for b in btns:
                name = b.element_info.name or ""
                if name.startswith(cap):
                    b.click_input()
                    time.sleep(1.5)
                    return f"клик «{name}»"
    if fallback_enter and hwnd:
        foreground(hwnd)
        keyboard.send_keys("{ENTER}")
        time.sleep(1.5)
        return "Enter (кнопка по умолчанию)"
    return None


def dlg_button_click(hwnd, caption):
    """Клик по кнопке диалога по началу подписи (окно ищется по handle заново)."""
    for cand, text in enum_windows_of(_pid[0]):
        if cand == hwnd:
            dlg = wrap_uia(hwnd)
            return click_in_dlg(dlg, hwnd, caption, fallback_enter=False)
    return None


# ---------- служебное ----------

_pid = [0]
_proc = [None]
PIDFILE = os.path.join(os.environ["TEMP"], "kite2-ui-test.pid")


def pid_load():
    if os.path.exists(PIDFILE):
        with open(PIDFILE) as f:
            _pid[0] = int(f.read().strip() or 0)


def pid_save():
    with open(PIDFILE, "w") as f:
        f.write(str(_pid[0]))


def start_app(extra_args=()):
    stop_app()
    import subprocess as sp
    args = [EXE, "--test"] + list(extra_args)
    _proc[0] = sp.Popen(args, cwd=os.path.dirname(EXE))
    _pid[0] = _proc[0].pid
    pid_save()
    time.sleep(4)


def stop_app():
    import subprocess as sp
    sp.run(["taskkill", "/F", "/IM", "Kite2.exe"], capture_output=True)
    time.sleep(1)
    pid_load()


pid_load()


def main_win():
    pid_load()
    app = Application(backend="uia").connect(process=_pid[0])
    return app.window(title_re="Kite2.*тестовый режим")


def page_count(win):
    return len(win.descendants(control_type="ListItem"))


def click_btn(win, caption):
    win.child_window(title=caption, control_type="Button").click_input()


def wait_pages(win, expected, timeout=12.0):
    deadline = time.time() + timeout
    n = -1
    while time.time() < deadline:
        n = page_count(win)
        if n == expected:
            return n
        time.sleep(0.5)
    return n


def press_vk(vk):
    """Чистая виртуальная клавиша без Shift —_MATCH KeyBinding только без модификаторов."""
    ctypes.windll.user32.keybd_event(vk, 0, 0, 0)
    ctypes.windll.user32.keybd_event(vk, 0, 2, 0)


VK_ADD = 0x6B
VK_DELETE = 0x2E


def combo_key(vk, ctrl=False, shift=False, alt=False):
    """Ctrl/Shift+клавиши для WPF: keybd_event с паузами — PostMessage и SendInput-
    пачки pywinauto доходят без модификаторов, KeyBinding не срабатывает."""
    U = ctypes.windll.user32
    down, up = 0, 0x0002
    for code, want in ((0x11, ctrl), (0x10, shift), (0x12, alt)):
        if want:
            U.keybd_event(code, 0, down, 0)
            time.sleep(0.1)
    U.keybd_event(vk, 0, down, 0)
    time.sleep(0.05)
    U.keybd_event(vk, 0, up, 0)
    for code, want in ((0x11, ctrl), (0x10, shift), (0x12, alt)):
        if want:
            U.keybd_event(code, 0, up, 0)
    time.sleep(0.2)


def send_combo(win, combo):
    """Обновляет фокус окна; сами клавиши всегда через combo_key (см. пояснение)."""
    win.set_focus()
    time.sleep(0.4)


# ---------- фазы тестирования ----------

def phase_start():
    win = main_win()
    w = win.window_text()
    check("Запуск: окно тестового режима", "(тестовый режим)" in w, w)
    check("Запуск: 0 страниц на старте", page_count(win) == 0, f"страниц={page_count(win)}")
    return win


def phase_scan(win):
    click_btn(win, "Ч/Б")
    n = wait_pages(win, 3)
    check("Скан Ч/Б: +3 страницы", n == 3, f"страниц={n}")
    line = wait_log("Вопрос о продолжении пропущен: тестовый режим", 8)
    check("Скан Ч/Б: лог пропуска вопроса", line is not None)

    click_btn(win, "Цветное")
    n = wait_pages(win, 6)
    check("Скан Цветное: +3 страницы", n == 6, f"страниц={n}")

    # «+» — повторное сканирование прежней цветностью (цветной)
    win.set_focus()
    time.sleep(0.4)
    press_vk(VK_ADD)
    n = wait_pages(win, 9)
    check("Ключ «+»: +3 страницы", n == 9, f"страниц={n}")
    return win


def phase_save(win, idx=1):
    """Сохранить в ZIP и PDF (по одному), затем отмену."""
    results = []
    for ext in ("zip", "pdf"):
        target = os.path.join(TMP, f"kite2-ui-{idx}.{ext}")
        if os.path.exists(target):
            os.remove(target)
        open_save(win)
        ok = save_dialog_type(target, timeout=10)
        if not ok:
            click_btn(win, "Сохранить")
            time.sleep(2)
            ok = save_dialog_type(target, timeout=10)
        existed = os.path.exists(target)
        line = wait_log("Экспорт завершён", 8)
        check(f"Сохранение {ext.upper()}: файл создан", existed,
              f"{target} ({os.path.getsize(target) if existed else 0} байт)")
        check(f"Сохранение {ext.upper()}: лог экспорта",
              line is not None and ext in line.lower(), line or "нет строки")
        results.append(target)
    # отмена: диалог открыт, Esc — файл не появляется
    open_save(win)
    dlg, _ = find_dlg("Сохранение", 5)
    if dlg is None:
        click_btn(win, "Сохранить")
        time.sleep(2)
        dlg, _ = find_dlg("Сохранение", 5)
    check("Отмена: диалог открыт", dlg is not None)
    if dlg is not None:
        foreground(dlg.handle)
        keyboard.send_keys("{ESC}")
        time.sleep(1.2)
        gone, _ = find_dlg("Сохранение", 3)
        check("Отмена: диалог закрылся", gone is None)
        line = wait_log("Экспорт завершён", 2)
        check("Отмена: экспорта нет", line is None, line or "")
    return win


def open_save(win):
    """Открыть окно «Сохранение»: сначала Ctrl+S (комбо), запасной путь — кнопка тулбара."""
    win.set_focus()
    time.sleep(0.4)
    combo_key(0x53, ctrl=True)
    time.sleep(1.5)
    dlg, _ = find_dlg("Сохранение", 4)
    if dlg is None:
        click_btn(win, "Сохранить")
        time.sleep(2)
        dlg, _ = find_dlg("Сохранение", 4)
    return dlg


def save_dialog_type(path, timeout=10):
    dlg, _ = find_dlg("Сохранение", timeout)
    if dlg is None:
        return False
    edits = [e for e in dlg.descendants() if e.class_name() == "Edit"]
    if not edits:
        return False
    edit = max(edits, key=lambda e: e.rectangle().top)
    edit.click_input()
    time.sleep(0.4)
    edit.set_edit_text("")
    edit.type_keys(path.replace("{", "{{").replace("}", "}}"))
    time.sleep(0.3)
    edit.type_keys("{ENTER}")
    time.sleep(2.5)
    return True


def phase_editing(win):
    """Поворот/перемещение/удаление выделенной страницы; фокус заведомо в колонке страниц."""
    items = win.descendants(control_type="ListItem")
    check("Редактирование: 9 страниц", len(items) == 9, f"страниц={len(items)}")
    live = win.descendants(control_type="ListItem")
    live[1].click_input()
    time.sleep(0.6)
    sel = lambda: [i.is_selected() for i in win.descendants(control_type="ListItem")].index(1)
    before = sel()
    win.set_focus()
    combo_key(0x26, ctrl=True)  # Ctrl+Up — выше по колонке
    time.sleep(1.2)
    after = sel()
    check("Ctrl+Up: страница поднялась", after == before - 1, f"{before} → {after}")
    combo_key(0x28, ctrl=True)  # Ctrl+Down — вернуть
    time.sleep(1.0)
    check("Ctrl+Down: страница опустилась", sel() == before, f"→ {sel()}")
    press_vk(VK_DELETE)  # удалить выделенную
    time.sleep(1.5)
    n = wait_pages(win, 8)
    check("Delete: -1 страница", n == 8, f"страниц={n}")
    return win


def phase_new_doc(win):
    win.set_focus()
    time.sleep(0.4)
    combo_key(0x4E, ctrl=True)  # Ctrl+N
    dlg, hwnd = find_dlg("Создать документ", 6)
    check("Новый документ: вопрос о потере", dlg is not None)
    if dlg is not None:
        act = click_in_dlg(dlg, hwnd, "Да")
        check("Новый документ: подтверждён", act is not None, act or "")
        n = wait_pages(win, 0)
        check("Новый документ: страницы очищены", n == 0, f"страниц={n}")
        line = wait_log("Ответ в окне «Создать документ»", 4)
        check("Новый документ: ответ в журнале", line is not None, line or "нет строки")
    return win


def phase_settings(win):
    click_btn(win, "Настройки")
    dlg, hwnd = find_dlg("Источник сканирования", 6)
    check("Настройки: окно источника открылось", dlg is not None)
    if dlg is not None:
        names = [w.window_text() for w in dlg.descendants(control_type="ListItem")]
        check("Настройки: тестовый источник в списке", any("Тестовый сканер" in (n or "") for n in names), str(names))
        close_via_esc = False
        try:
            cancel = [b for b in dlg.descendants(control_type="Button") if (b.element_info.name or "").startswith("Отмена")]
            if cancel:
                cancel[0].click_input()
                act = "клик «Отмена»"
            else:
                close_via_esc = True
        except Exception:
            close_via_esc = True
        if close_via_esc:
            ctypes.windll.user32.SetForegroundWindow(hwnd)
            time.sleep(0.3)
            keyboard.send_keys("{ESC}")
            act = "Esc"
        time.sleep(1.0)
        gone, _ = find_dlg("Источник сканирования", 3)
        check("Настройки: окно закрылось", gone is None, act if not close_via_esc else act)
    return win


def phase_fragment(win):
    """Фрагмент: протянуть рамку в просмотрщике, сохранить PNG, затем отменить второй."""
    if page_count(win) == 0:
        click_btn(win, "Ч/Б")
        wait_pages(win, 3)
    base = page_count(win)
    check("Фрагмент: документ есть", base > 0, f"страниц={base}")
    win.set_focus()
    time.sleep(0.4)

    # 1) сохранение
    fragment_drag(win)
    btn = fragment_button(win)
    check("Фрагмент: кнопка сохранения видна", btn is not None)
    if btn is None:
        return win
    btn.click_input()
    time.sleep(2.5)
    target = os.path.join(TMP, "kite2-fragment.png")
    if os.path.exists(target):
        os.remove(target)
    save_in_open_dlg(target)
    time.sleep(1.5)
    line = wait_log("Фрагмент сохранён", 6)
    check("Фрагмент: PNG сохранён", os.path.exists(target), target)
    check("Фрагмент: лог сохранения", line is not None, line or "нет строки")
    check("Фрагмент: документ не изменился", page_count(win) == base, f"страниц={page_count(win)}")

    # 2) отмена: Esc снимает выделение, диалог не открывается
    fragment_drag(win)
    btn = fragment_button(win)
    check("Фрагмент: вторая рамка есть", btn is not None)
    if btn is not None:
        keyboard.send_keys("{ESC}")
        time.sleep(1.0)
        check("Фрагмент: Esc снял выделение", fragment_button(win) is None)
        line = wait_log("Фрагмент сохранён", 2)
        check("Фрагмент: отменён без сохранения", line is None)
    return win


def fragment_drag(win):
    """Рамка на листе: протянуть мышь в центральной зоне просмотрщика."""
    viewer = None
    for c in win.descendants(control_type="Custom"):
        if c.class_name() == "PageViewer":
            viewer = c.rectangle()
    check("Фрагмент: область просмотрщика найдена", viewer is not None)
    if viewer is None:
        return
    x1 = viewer.left + int(viewer.width() * 0.40)
    y1 = viewer.top + int(viewer.height() * 0.38)
    t_drag(x1, y1, x1 + int(viewer.width() * 0.12), y1 + int(viewer.height() * 0.15))


def t_drag(x1, y1, x2, y2):
    ctypes.windll.user32.SetCursorPos(int(x1), int(y1))
    ctypes.windll.user32.mouse_event(0x0002, 0, 0, 0, 0)  # LEFTDOWN
    steps = 14
    for i in range(1, steps + 1):
        xi = x1 + (x2 - x1) * i / steps
        yi = y1 + (y2 - y1) * i / steps
        ctypes.windll.user32.SetCursorPos(int(xi), int(yi))
        time.sleep(0.02)
    ctypes.windll.user32.mouse_event(0x0004, 0, 0, 0, 0)  # LEFTUP


def fragment_button(win):
    """Кнопка сохранения фрагмента (иконка): единственная кнопка внутри PageViewer
    появляется только вместе с выделением."""
    for c in win.descendants(control_type="Custom"):
        if c.class_name() == "PageViewer":
            btns = c.descendants(control_type="Button")
            return btns[0] if btns else None
    return None


def save_in_open_dlg(path):
    """Поле имени в уже открытом диалоге «Сохранение» + Enter."""
    dlg, _ = find_dlg("Сохранение", 3)
    if dlg is None:
        return False
    edits = [e for e in dlg.descendants() if e.class_name() == "Edit"]
    edit = max(edits, key=lambda e: e.rectangle().top)
    edit.click_input()
    time.sleep(0.4)
    edit.set_edit_text("")
    edit.type_keys(path.replace("{", "{{").replace("}", "}}"))
    edit.type_keys("{ENTER}")
    time.sleep(2.0)
    return True


def phase_ask():
    """Второй запуск с --ask: окно «Сканирование» ведёт серию и завершает её."""
    start_app(extra_args=("--ask",))
    win = main_win()
    click_btn(win, "Ч/Б")
    time.sleep(3)
    dlg, hwnd = find_dlg("Сканирование", 8)
    check("--ask: окно сканирования появилось", dlg is not None)
    if dlg is None:
        return win
    dlg_button_click(hwnd, "Сканировать ещё")
    wait_pages(win, 6, timeout=15)
    check("--ask: серия расширена до 6", page_count(win) == 6, f"страниц={page_count(win)}")
    # после второй серии цикл снова спросит — сейчас «Завершить» останавливает,
    # а не выбрасывает набранное
    dlg2, hwnd2 = find_dlg("Сканирование", 8)
    check("--ask: вопрос повторился после второй серии", dlg2 is not None)
    if dlg2 is not None:
        dlg_button_click(hwnd2, "Завершить")
        time.sleep(1.5)
        gone, _ = find_dlg("Сканирование", 3)
        check("--ask: окно закрылось", gone is None)
        check("--ask: страницы целы", page_count(win) == 6, f"страниц={page_count(win)}")
        line = wait_log("Вопрос о продолжении", 3)
        check("--ask: вопрос не попал в тестовый лог", line is None)
    return win


def phase_close():
    win = main_win()
    try:
        close = [b for b in win.descendants(control_type="Button") if (b.element_info.name or "") == "Закрыть"]
        if close:
            close[0].click_input()
        else:
            ctypes.windll.user32.PostMessageW(win.handle, 0x0010, 0, 0)  # WM_CLOSE
    except Exception:
        ctypes.windll.user32.PostMessageW(win.handle, 0x0010, 0, 0)
    time.sleep(2)
    import subprocess as sp
    # вывод tasklist приходит в кодировке консоли: читаем байты — тесту важен
    # только факт исчезновения процесса.
    r = sp.run(["tasklist", "/FI", f"PID eq {_pid[0]}"], capture_output=True)
    out = (r.stdout or b"").decode("cp866", errors="replace")
    check("Закрытие: процесс завершён", "Kite2.exe" not in out, out.strip()[:80])
    errs = [l for l in log_newlines() if "]FATAL" in l or "]ERROR" in l]
    check("Журнал: нет FATAL/ERROR", not errs, "; ".join(errs[:3]))


def main():
    phases = sys.argv[1:] or ["all"]
    log_init()
    os.makedirs(TMP, exist_ok=True)
    start_app()

    if "all" in phases or "start" in phases:
        win = phase_start()
    else:
        win = main_win()

    if "all" in phases or "scan" in phases:
        win = phase_scan(win)
    if "all" in phases or "save" in phases:
        win = phase_save(win)
    if "all" in phases or "edit" in phases:
        win = phase_editing(win)
    if "all" in phases or "newdoc" in phases:
        win = phase_new_doc(win)
    if "all" in phases or "settings" in phases:
        win = phase_settings(win)
    if "all" in phases or "fragment" in phases:
        win = phase_fragment(win)
    if "all" in phases or "ask" in phases:
        win = phase_ask()
    if "all" in phases or "close" in phases:
        phase_close()

    print("\n=== ИТОГ ===")
    if _failures:
        print(f"Провалов: {len(_failures)}")
        for f in _failures:
            print(" *", f)
        sys.exit(1)
    print("Все проверки пройдены.")


if __name__ == "__main__":
    main()
