
namespace DiskMonitor;

// ── Embedded HTML ─────────────────────────────────────────────────────────────

static class DiskMonitorHtml
{
    public static string GetPage() => """
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Disk Space Monitor</title>
<!-- integrity="sha512-vc58zeoGEOOYGSoACmFdNsgFR31KE07LT4bOJ9MRqUXaDiKvkHYA7MSpUGl6Vb1JHzJkMIHSBzW3vfuYbrfbQ==" .-->
<script src="diskmonitor/js/d3.min.js"
        
        crossorigin="anonymous" referrerpolicy="no-referrer"></script>
<style>
  :root {
    --bg:       #f1f5f9;
    --surface:  #ffffff;
    --border:   #e2e8f0;
    --text:     #1e293b;
    --muted:    #64748b;
    --accent:   #3b82f6;
    --green:    #22c55e;
    --yellow:   #eab308;
    --orange:   #f97316;
    --red:      #ef4444;
    --free-clr: #cbd5e1;
    --radius:   12px;
    --shadow:   0 1px 3px rgba(0,0,0,.08), 0 4px 12px rgba(0,0,0,.06);
  }
  *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
  body {
    font-family: ui-sans-serif, system-ui, -apple-system, sans-serif;
    background: var(--bg);
    color: var(--text);
    min-height: 100vh;
    padding: 0 0 48px;
  }

  /* ── Header ── */
  header {
    background: var(--surface);
    border-bottom: 1px solid var(--border);
    padding: 18px 32px;
    display: flex;
    align-items: center;
    gap: 16px;
    flex-wrap: wrap;
  }
  header h1 {
    font-size: 1.25rem;
    font-weight: 700;
    letter-spacing: -.02em;
    flex: 1;
  }
  header h1 span { color: var(--accent); }
  #ts { font-size: .8rem; color: var(--muted); }
  button {
    background: var(--accent);
    color: #fff;
    border: none;
    padding: 8px 18px;
    border-radius: 8px;
    font-size: .85rem;
    font-weight: 600;
    cursor: pointer;
    transition: opacity .15s;
  }
  button:hover { opacity: .85; }
  button.secondary {
    background: transparent;
    color: var(--accent);
    border: 1.5px solid var(--accent);
    padding: 6px 12px;
    font-size: .78rem;
    font-weight: 500;
  }

  main { max-width: 1200px; margin: 0 auto; padding: 32px 24px 0; }

  /* ── Disk Cards ── */
  #charts {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
    gap: 20px;
    margin-bottom: 36px;
  }
  .card {
    background: var(--surface);
    border: 1px solid var(--border);
    border-radius: var(--radius);
    box-shadow: var(--shadow);
    padding: 20px 20px 16px;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 4px;
  }
  .card-title {
    font-size: .95rem;
    font-weight: 700;
    text-align: center;
    letter-spacing: -.01em;
  }
  .card-sub {
    font-size: .75rem;
    color: var(--muted);
    text-align: center;
    margin-bottom: 6px;
  }
  .chart-wrap { position: relative; width: 240px; height: 240px; }
  .chart-wrap svg { overflow: visible; }
  .legend {
    display: flex;
    gap: 20px;
    margin-top: 6px;
    flex-wrap: wrap;
    justify-content: center;
  }
  .legend-item { display: flex; align-items: center; gap: 5px; font-size: .78rem; color: var(--muted); }
  .legend-dot { width: 10px; height: 10px; border-radius: 3px; flex-shrink: 0; }
  .card-actions { display: flex; gap: 6px; margin-top: 10px; }

  /* ── Table ── */
  #table-section { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius); box-shadow: var(--shadow); overflow: hidden; }
  .table-header { padding: 16px 20px 12px; border-bottom: 1px solid var(--border); display: flex; align-items: center; justify-content: space-between; }
  .table-header h2 { font-size: 1rem; font-weight: 700; }
  table { width: 100%; border-collapse: collapse; font-size: .875rem; }
  th {
    text-align: left;
    font-size: .72rem;
    font-weight: 600;
    letter-spacing: .06em;
    text-transform: uppercase;
    color: var(--muted);
    padding: 8px 16px;
    background: #f8fafc;
    border-bottom: 1px solid var(--border);
    white-space: nowrap;
  }
  th.num, td.num { text-align: right; font-variant-numeric: tabular-nums; }
  td { padding: 10px 16px; border-bottom: 1px solid var(--border); vertical-align: middle; }
  tr:last-child td { border-bottom: none; }
  tr:hover td { background: #f8fafc; }
  .fs-cell { font-weight: 600; font-family: ui-monospace, monospace; font-size: .82rem; }
  .mount-cell { color: var(--muted); font-family: ui-monospace, monospace; font-size: .8rem; }
  .badge {
    display: inline-block;
    font-size: .68rem;
    font-weight: 600;
    padding: 2px 7px;
    border-radius: 999px;
    background: #f1f5f9;
    color: var(--muted);
    margin-left: 4px;
  }

  /* ── Usage bar ── */
  .bar-wrap { width: 120px; height: 7px; background: var(--free-clr); border-radius: 4px; overflow: hidden; }
  .bar-fill  { height: 100%; border-radius: 4px; transition: width .5s; }

  /* ── Loading/Error ── */
  .status { padding: 40px; text-align: center; color: var(--muted); }
  .error  { color: var(--red); }
</style>
</head>
<body>

<header>
  <h1>💾 Disk <span>Space Monitor</span></h1>
  <span id="ts">Loading…</span>
  <button onclick="load()">⟳ Refresh</button>
</header>

<main>
  <div id="charts"><p class="status">Fetching disk data…</p></div>
  <section id="table-section">
    <div class="table-header">
      <h2>All Partitions</h2>
    </div>
    <div id="table-wrap"><p class="status">Loading…</p></div>
  </section>
</main>

<script>
// ── Number formatting (Swiss locale: ' as thousands separator) ───────────────

const CH = 'de-CH';   // Swiss German: 1'234.56

function fmtBytes(bytes, fractionDigits = 2) 
{
  if (!bytes || bytes <= 0) return fmt(0, 0) + '\u202fB';
  const units = ['B','KiB','MiB','GiB','TiB','PiB'];
  const k = 1024;
  const i = Math.min(Math.floor(Math.log(bytes) / Math.log(k)), units.length - 1);
  const val = bytes / Math.pow(k, i);
  const dec = i === 0 ? 0 : fractionDigits;
  return val.toLocaleString(CH, { minimumFractionDigits: dec, maximumFractionDigits: dec })
       + '\u202f' + units[i];
}

function fmtPct(used, total) 
{
  if (!total) return '0\u202f%';
  return (used / total * 100).toLocaleString(CH, { minimumFractionDigits: 1, maximumFractionDigits: 1 }) + '\u202f%';
}

// ── Usage colour ─────────────────────────────────────────────────────────────

function usageColor(pct) 
{
  if (pct < 60)  return getComputedStyle(document.documentElement).getPropertyValue('--green').trim();
  if (pct < 80)  return getComputedStyle(document.documentElement).getPropertyValue('--yellow').trim();
  if (pct < 90)  return getComputedStyle(document.documentElement).getPropertyValue('--orange').trim();
  return getComputedStyle(document.documentElement).getPropertyValue('--red').trim();
}

// ── Pie / Donut chart ────────────────────────────────────────────────────────

function buildChart(disk) 
{
  const W = 240, H = 240;
  const R = (Math.min(W, H) / 2) - 10;
  const Ri = R * 0.56;   // inner radius → donut

  const pct    = disk.totalBytes > 0 ? disk.usedBytes / disk.totalBytes * 100 : 0;
  const clr    = usageColor(pct);
  const freeC  = '#cbd5e1';

  const data = [
    { label: 'Used', bytes: disk.usedBytes,  color: clr },
    { label: 'Free', bytes: disk.freeBytes,  color: freeC },
  ];

  const pie  = d3.pie().value(d => d.bytes).sort(null);
  const arc  = d3.arc().innerRadius(Ri).outerRadius(R).padAngle(0.025).cornerRadius(3);
  const arcH = d3.arc().innerRadius(Ri - 4).outerRadius(R + 4).padAngle(0.025).cornerRadius(3);

  const svg = d3.create('svg')
      .attr('viewBox', `0 0 ${W} ${H}`)
      .attr('width',  W)
      .attr('height', H)
      .style('font-family', 'ui-sans-serif, system-ui, -apple-system, sans-serif');

  // White background (needed for PNG export)
  svg.append('rect').attr('width', W).attr('height', H).attr('fill', '#ffffff');

  const g = svg.append('g').attr('transform', `translate(${W/2},${H/2})`);

  // Slices
  const slices = g.selectAll('path')
      .data(pie(data))
      .enter()
      .append('path')
        .attr('d', arc)
        .attr('fill', d => d.data.color)
        .attr('stroke', '#fff')
        .attr('stroke-width', 2)
        .style('cursor', 'pointer')
        .style('transition', 'opacity .15s');

  slices
    .on('mouseover', function(e, d) 
    {
      d3.select(this).attr('d', arcH);
      centerValue.text(fmtBytes(d.data.bytes));
      centerLabel.text(d.data.label);
    })
    .on('mouseout', function(e, d) 
    {
      d3.select(this).attr('d', arc);
      centerValue.text(fmtBytes(disk.totalBytes, 1));
      centerLabel.text('total');
    });

  // Center text
  const centerGroup = g.append('g').attr('text-anchor', 'middle');

  const shortName = disk.filesystem.replace(/^\/dev\//, '');

  centerGroup.append('text')
      .attr('dy', '-2.0em')
      .attr('font-size', '12px')
      .attr('font-weight', '700')
      .attr('fill', '#1e293b')
      .attr('font-family', 'ui-monospace, monospace')
      .text(shortName.length > 14 ? shortName.slice(-14) : shortName);

  const centerValue = centerGroup.append('text')
      .attr('dy', '0.1em')
      .attr('font-size', '17px')
      .attr('font-weight', '800')
      .attr('fill', '#1e293b')
      .text(fmtBytes(disk.totalBytes, 1));

  const centerLabel = centerGroup.append('text')
      .attr('dy', '1.5em')
      .attr('font-size', '11px')
      .attr('fill', '#94a3b8')
      .text('total');

  centerGroup.append('text')
      .attr('dy', '2.9em')
      .attr('font-size', '13px')
      .attr('font-weight', '700')
      .attr('fill', clr)
      .text(fmtPct(disk.usedBytes, disk.totalBytes) + ' used');

  return svg.node();
}

// ── PNG download ─────────────────────────────────────────────────────────────

async function downloadPng(svgEl, name) 
{
  const clone = svgEl.cloneNode(true);
  // Ensure white bg rect exists
  if (!clone.querySelector('rect[fill="#ffffff"]')) 
  {
    const bg = document.createElementNS('http://www.w3.org/2000/svg', 'rect');
    const vb = clone.viewBox.baseVal;
    bg.setAttribute('width',  vb.width  || clone.getAttribute('width')  || 240);
    bg.setAttribute('height', vb.height || clone.getAttribute('height') || 240);
    bg.setAttribute('fill', '#ffffff');
    clone.insertBefore(bg, clone.firstChild);
  }
  const svgStr  = new XMLSerializer().serializeToString(clone);
  const blob    = new Blob([svgStr], { type: 'image/svg+xml;charset=utf-8' });
  const url     = URL.createObjectURL(blob);
  const scale   = 2;
  const vb      = svgEl.viewBox.baseVal;
  const w       = (vb.width  || +svgEl.getAttribute('width')  || 240) * scale;
  const h       = (vb.height || +svgEl.getAttribute('height') || 240) * scale;

  await new Promise((resolve, reject) => {
    const img = new Image();
    img.onload = () => {
      const canvas = document.createElement('canvas');
      canvas.width  = w;
      canvas.height = h;
      const ctx = canvas.getContext('2d');
      ctx.fillStyle = '#ffffff';
      ctx.fillRect(0, 0, w, h);
      ctx.drawImage(img, 0, 0, w, h);
      URL.revokeObjectURL(url);
      const a = document.createElement('a');
      a.download = name + '.png';
      a.href = canvas.toDataURL('image/png');
      a.click();
      resolve();
    };
    img.onerror = reject;
    img.src = url;
  });
}

function downloadSvg(svgEl, name) 
{
  const clone  = svgEl.cloneNode(true);
  const svgStr = new XMLSerializer().serializeToString(clone);
  const blob   = new Blob([svgStr], { type: 'image/svg+xml;charset=utf-8' });
  const url    = URL.createObjectURL(blob);
  const a      = document.createElement('a');
  a.download   = name + '.svg';
  a.href       = url;
  a.click();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}

// ── Render cards ─────────────────────────────────────────────────────────────

function renderCards(disks) 
{
  const container = document.getElementById('charts');
  container.innerHTML = '';

  if (!disks.length) 
  {
    container.innerHTML = '<p class="status">No physical drives found.</p>';
    return;
  }

  disks.forEach(disk => {
    const pct   = disk.totalBytes > 0 ? disk.usedBytes / disk.totalBytes * 100 : 0;
    const clr   = usageColor(pct);
    const name  = disk.filesystem.replace(/^\/dev\//, '');
    const safeName = name.replace(/[^a-z0-9]/gi, '_');

    const card  = document.createElement('div');
    card.className = 'card';

    const title = document.createElement('div');
    title.className = 'card-title';
    title.textContent = disk.filesystem;
    card.appendChild(title);

    const sub = document.createElement('div');
    sub.className = 'card-sub';
    sub.textContent = disk.mountPoint + (disk.fsType ? '  ·  ' + disk.fsType : '');
    card.appendChild(sub);

    const wrap = document.createElement('div');
    wrap.className = 'chart-wrap';
    const svgNode = buildChart(disk);
    wrap.appendChild(svgNode);
    card.appendChild(wrap);

    // Legend
    const legend = document.createElement('div');
    legend.className = 'legend';
    legend.innerHTML = `
      <div class="legend-item">
        <div class="legend-dot" style="background:${clr}"></div>
        <div>Used&nbsp;<strong>${fmtBytes(disk.usedBytes, 1)}</strong></div>
      </div>
      <div class="legend-item">
        <div class="legend-dot" style="background:#cbd5e1"></div>
        <div>Free&nbsp;<strong>${fmtBytes(disk.freeBytes, 1)}</strong></div>
      </div>`;
    card.appendChild(legend);

    // Action buttons
    const actions = document.createElement('div');
    actions.className = 'card-actions';

    const btnSvg = document.createElement('button');
    btnSvg.className = 'secondary';
    btnSvg.textContent = '↓ SVG';
    btnSvg.onclick = () => downloadSvg(svgNode, 'disk_' + safeName);

    const btnPng = document.createElement('button');
    btnPng.className = 'secondary';
    btnPng.textContent = '↓ PNG';
    btnPng.onclick = () => downloadPng(svgNode, 'disk_' + safeName);

    actions.appendChild(btnSvg);
    actions.appendChild(btnPng);
    card.appendChild(actions);

    container.appendChild(card);
  });
}

// ── Render table ─────────────────────────────────────────────────────────────

function barHtml(pct) 
{
  const clr = usageColor(pct);
  return `<div class="bar-wrap">
    <div class="bar-fill" style="width:${Math.min(pct,100).toFixed(1)}%;background:${clr}"></div>
  </div>`;
}

function renderTable(disks) 
{
  const wrap = document.getElementById('table-wrap');
  if (!disks.length) { wrap.innerHTML = '<p class="status">No data.</p>'; return; }

  const rows = disks.map(d => {
    const pct = d.totalBytes > 0 ? d.usedBytes / d.totalBytes * 100 : 0;
    const clr = usageColor(pct);
    return `<tr>
      <td class="fs-cell">${d.filesystem}<span class="badge">${d.fsType || '?'}</span></td>
      <td class="mount-cell">${d.mountPoint}</td>
      <td class="num">${fmtBytes(d.totalBytes, 2)}</td>
      <td class="num">${fmtBytes(d.usedBytes, 2)}</td>
      <td class="num">${fmtBytes(d.freeBytes, 2)}</td>
      <td class="num" style="color:${clr};font-weight:700">${fmtPct(d.usedBytes, d.totalBytes)}</td>
      <td>${barHtml(pct)}</td>
    </tr>`;
  }).join('');

  wrap.innerHTML = `
    <table>
      <thead>
        <tr>
          <th>Filesystem</th>
          <th>Mount point</th>
          <th class="num">Total</th>
          <th class="num">Used</th>
          <th class="num">Free</th>
          <th class="num">Use&nbsp;%</th>
          <th>Usage</th>
        </tr>
      </thead>
      <tbody>${rows}</tbody>
    </table>`;
}

// ── Main load ────────────────────────────────────────────────────────────────

async function load() 
{
  document.getElementById('ts').textContent = 'Refreshing…';
  try 
  {
    const res   = await fetch('diskmonitor/api/disks');
    if (!res.ok) throw new Error('HTTP ' + res.status);
    const disks = await res.json();
    renderCards(disks);
    renderTable(disks);
    const now = new Date().toLocaleString('de-CH', {
      year:'numeric', month:'2-digit', day:'2-digit',
      hour:'2-digit', minute:'2-digit', second:'2-digit'
    });
    document.getElementById('ts').textContent = 'Last updated: ' + now;
  } 
  catch (err) 
  {
    document.getElementById('charts').innerHTML =
      `<p class="status error">Error: ${err.message}</p>`;
    document.getElementById('table-wrap').innerHTML =
      `<p class="status error">Error: ${err.message}</p>`;
    document.getElementById('ts').textContent = 'Failed';
  }
}

// Auto-load on page open, then refresh every 30 s
load();
setInterval(load, 30_000);
</script>
</body>
</html>
""";
}