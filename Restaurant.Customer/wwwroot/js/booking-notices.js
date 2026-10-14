(() => {
 const panel = document.getElementById('booking-notices');
 if (!panel) return;
 let previous = '', busy = false;
 async function refresh() {
  if (busy || document.hidden) return;
  busy = true;
  try {
   const response = await fetch(panel.dataset.url, { cache: 'no-store', headers: { Accept: 'application/json' } });
   if (!response.ok || !response.headers.get('content-type')?.includes('application/json')) { panel.hidden = true; return; }
   const notices = await response.json();
   const signature = JSON.stringify(notices);
   if (signature !== previous) {
    panel.replaceChildren();
    for (const n of notices) {
     const card = document.createElement('div'); card.className = `alert ${n.due ? 'alert-warning' : 'alert-info'}`;
     const title = document.createElement('strong'); title.textContent = n.message;
     const detail = document.createElement('p'); detail.className = 'mb-1';
     detail.textContent = `${n.table} · ${n.time} · ${n.date}` + (panel.dataset.staff ? ` · ${n.customer} · ${n.phone || 'Chưa cập nhật số điện thoại'}` : '');
     const link = document.createElement('a'); link.href = n.url; link.className = 'alert-link'; link.textContent = panel.dataset.staff ? 'Xem danh sách đón khách' : 'Xem mã đặt bàn';
     card.append(title, detail, link); panel.append(card);
    }
    previous = signature;
   }
   panel.hidden = notices.length === 0;
  } catch { panel.hidden = true; }
  finally { busy = false; }
 }
 refresh(); setInterval(refresh, 30000);
 document.addEventListener('visibilitychange', refresh);
})();