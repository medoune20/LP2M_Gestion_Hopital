/* GestionHopital — JavaScript global */

document.addEventListener('DOMContentLoaded', function () {
    const basePath = (document.body?.dataset?.pathBase || '').replace(/\/$/, '');
    const currentPath = location.pathname.toLowerCase();

    function injectLp2mMenuCss() {
        if (document.getElementById('lp2m-menu-style')) return;
        const style = document.createElement('style');
        style.id = 'lp2m-menu-style';
        style.textContent = `
.gh-sidebar,.lp2m-sidebar{background:linear-gradient(180deg,#07182a 0%,#0a2540 58%,#082033 100%)!important;border-right:1px solid rgba(255,255,255,.08);box-shadow:4px 0 20px rgba(0,0,0,.18)}
.gh-mobile-drawer{background:linear-gradient(180deg,#07182a 0%,#0a2540 100%)!important;color:#fff}.gh-mobile-drawer strong{color:#fff}.gh-topbar{background:linear-gradient(135deg,#07182a,#0f3c5e 55%,#0ea5e9)!important;box-shadow:0 8px 24px rgba(7,24,42,.18)}
.lp2m-nav{padding:.6rem .65rem 1rem}.lp2m-nav-section-toggle{width:100%;border:0;background:transparent;color:rgba(255,255,255,.78);display:flex;align-items:center;justify-content:space-between;gap:.5rem;cursor:pointer;padding:.45rem .55rem;margin-top:.55rem;text-align:left}.lp2m-nav-section-toggle:hover{background:rgba(255,255,255,.06);border-radius:12px;color:#fff}.lp2m-nav-sep{margin:0;flex:1;font-size:.72rem;font-weight:800;text-transform:uppercase;letter-spacing:.08em;opacity:.72}.lp2m-nav-section-toggle .bi-chevron-down{transition:transform .18s ease;font-size:.82rem;opacity:.72}.lp2m-nav-section-toggle.is-collapsed .bi-chevron-down{transform:rotate(-90deg)}.lp2m-nav-section{display:grid;gap:.15rem}.lp2m-nav-section.is-collapsed{display:none}
.lp2m-link{min-height:42px;border:1px solid transparent;display:flex;align-items:center;gap:.72rem;color:rgba(255,255,255,.72);text-decoration:none;padding:.58rem .72rem;border-radius:12px;font-weight:650;font-size:.92rem;margin-bottom:.12rem}.lp2m-link i{font-size:1.08rem;width:1.25rem;text-align:center}.lp2m-link:hover,.lp2m-link.active,.lp2m-link.actif{background:rgba(14,165,233,.18);border-color:rgba(255,255,255,.08);color:#fff}.lp2m-link.active,.lp2m-link.actif{box-shadow:inset 3px 0 0 #38bdf8}
.mobile-bottom-nav,.gh-bottom-nav{position:fixed;left:10px;right:10px;bottom:calc(10px + env(safe-area-inset-bottom));z-index:80;height:64px;border-radius:22px;background:rgba(255,255,255,.94);border:1px solid rgba(15,23,42,.08);box-shadow:0 18px 46px rgba(15,23,42,.18);backdrop-filter:blur(18px);display:grid;grid-template-columns:repeat(5,1fr);overflow:hidden}.mobile-bottom-nav a,.gh-bottom-nav a{color:#64748b;text-decoration:none;display:flex;flex-direction:column;align-items:center;justify-content:center;gap:2px;font-size:.68rem;font-weight:800}.mobile-bottom-nav a i,.gh-bottom-nav a i{font-size:1.18rem}.mobile-nav-pos{color:#fff!important;background:linear-gradient(135deg,#0ea5e9,#0369a1)!important}
`; document.head.appendChild(style);
    }

    function appUrl(path) { return (basePath || '') + path; }
    function isInternalAbsolute(url) { return url && url.startsWith('/') && !url.startsWith('//') && !url.startsWith(basePath + '/'); }
    function withBasePath(url) { if (!basePath || !isInternalAbsolute(url)) return url; if (url.startsWith('/gestionbar')) return url; if (url.startsWith('/hopital')) return url; return basePath + url; }
    function link(controller, action, icon, label, extraClass) { const href = appUrl('/' + controller + (action ? '/' + action : '')); const active = currentPath.includes(('/' + controller).toLowerCase()) ? ' actif active' : ''; return `<a class="lp2m-link${active} ${extraClass || ''}" href="${href}"><i class="bi ${icon}"></i><span>${label}</span></a>`; }
    function section(id, title, html) { return `<button type="button" class="lp2m-nav-section-toggle" data-nav-section="${id}" aria-expanded="true"><span class="lp2m-nav-sep">${title}</span><i class="bi bi-chevron-down"></i></button><div class="lp2m-nav-section" data-nav-section-content="${id}">${html}</div>`; }
    function hash(path, label, icon) { return `<a class="lp2m-link" href="${appUrl(path)}"><i class="bi ${icon}"></i><span>${label}</span></a>`; }

    function buildMenuHtml() {
        return [
            section('pilotage', 'Pilotage', link('Accueil','Index','bi-speedometer2','Dashboard') + link('Lp2m','Index','bi-grid-1x2','Administration LP2M')),
            section('patients', 'Patients', link('Patient','Index','bi-person-lines-fill','Patients') + link('Patient','Nouveau','bi-person-plus','Nouveau patient') + hash('/Lp2m#patient','Accès patient','bi-phone')),
            section('agenda', 'Agenda & file d’attente', link('RendezVous','Index','bi-calendar3','Rendez-vous') + link('RendezVous','Nouveau','bi-calendar-plus','Nouveau RDV') + link('RendezVous','Calendrier','bi-calendar-week','Calendrier') + hash('/Lp2m#file','File d’attente','bi-hourglass-split')),
            section('soins', 'Soins', link('Consultation','Index','bi-clipboard2-pulse','Consultations') + hash('/Lp2m#tele','Consultation en ligne','bi-camera-video') + link('Examen','Index','bi-droplet-half','Examens & analyses')),
            section('hospitalisation', 'Hospitalisation', link('Hospitalisation','Index','bi-hospital-fill','Patients hospitalisés') + link('Hospitalisation','Nouvelle','bi-person-check','Nouvelle admission') + link('Departement','Index','bi-diagram-3','Services & lits')),
            section('finances', 'Finances & analyses', hash('/Lp2m#paiements','Paiements','bi-cash-stack') + hash('/Lp2m#compta','Comptabilité','bi-journal-text') + hash('/Accueil/Index','Statistiques','bi-graph-up') + hash('/Lp2m#alertes','Alertes','bi-bell')),
            section('administration', 'Administration', hash('/Lp2m#admin','Utilisateurs','bi-people') + hash('/Lp2m#admin','Profils / rôles','bi-shield-lock') + hash('/Lp2m#admin','Tenants','bi-buildings') + link('Medecin','Index','bi-person-badge','Médecins')),
            section('superadmin', 'Super-admin', hash('/Lp2m#admin','Demandes d’inscription','bi-person-check') + hash('/Lp2m#admin','Tenants','bi-buildings') + hash('/Lp2m#com','Envoi mail','bi-envelope'))
        ].join('');
    }

    function applyGestionBarMenu() {
        injectLp2mMenuCss();
        document.body.classList.add('lp2m-app');
        document.querySelector('.gh-sidebar')?.classList.add('lp2m-sidebar');
        document.querySelector('.gh-main')?.classList.add('lp2m-main');
        document.querySelector('.gh-topbar')?.classList.add('lp2m-topbar');
        const sidebarNav = document.querySelector('.gh-sidebar nav'); if (sidebarNav) { sidebarNav.className = 'lp2m-nav'; sidebarNav.innerHTML = buildMenuHtml(); }
        const drawerNav = document.querySelector('.gh-mobile-drawer nav'); if (drawerNav) { drawerNav.className = 'lp2m-nav p-2'; drawerNav.innerHTML = buildMenuHtml() + '<hr class="border-secondary"><a class="lp2m-link text-danger" href="' + appUrl('/Auth/Deconnexion') + '"><i class="bi bi-box-arrow-right"></i><span>Déconnexion</span></a>'; }
        const bottomNav = document.querySelector('.gh-bottom-nav'); if (bottomNav) { bottomNav.className = 'mobile-bottom-nav gh-bottom-nav d-lg-none no-print'; bottomNav.innerHTML = '<a href="' + appUrl('/Accueil/Index') + '"><i class="bi bi-speedometer2"></i><span>Accueil</span></a><a href="' + appUrl('/Lp2m') + '" class="mobile-nav-pos"><i class="bi bi-grid-1x2-fill"></i><span>LP2M</span></a><a href="' + appUrl('/Patient/Index') + '"><i class="bi bi-person-lines-fill"></i><span>Patients</span></a><a href="' + appUrl('/RendezVous/Index') + '"><i class="bi bi-calendar3"></i><span>RDV</span></a><a href="' + appUrl('/Consultation/Index') + '"><i class="bi bi-clipboard2-pulse"></i><span>Soins</span></a>'; }
    }

    function fixLegacyLinks() { document.querySelectorAll('a[href]').forEach(a => { const href = a.getAttribute('href'); if (isInternalAbsolute(href)) a.setAttribute('href', withBasePath(href)); }); document.querySelectorAll('form[action]').forEach(f => { const action = f.getAttribute('action'); if (isInternalAbsolute(action)) f.setAttribute('action', withBasePath(action)); }); }
    function initCollapsibleSections() { const key = 'lp2m.sante.sidebar.collapsed.sections'; let collapsed = []; try { collapsed = JSON.parse(localStorage.getItem(key) || '[]'); } catch(e) {} function set(name, value) { const c = document.querySelector('[data-nav-section-content="'+name+'"]'); const b = document.querySelector('[data-nav-section="'+name+'"]'); if (!c || !b) return; c.classList.toggle('is-collapsed', value); b.classList.toggle('is-collapsed', value); b.setAttribute('aria-expanded', value ? 'false' : 'true'); } document.querySelectorAll('[data-nav-section]').forEach(b => { const name = b.getAttribute('data-nav-section'); const c = document.querySelector('[data-nav-section-content="'+name+'"]'); const hasActive = c && c.querySelector('.lp2m-link.actif,.lp2m-link.active'); set(name, collapsed.indexOf(name) !== -1 && !hasActive); b.addEventListener('click', () => { const content = document.querySelector('[data-nav-section-content="'+name+'"]'); const next = content && !content.classList.contains('is-collapsed'); set(name, next); const list = []; document.querySelectorAll('[data-nav-section].is-collapsed').forEach(x => list.push(x.getAttribute('data-nav-section'))); localStorage.setItem(key, JSON.stringify(list)); }); }); }

    applyGestionBarMenu(); fixLegacyLinks(); initCollapsibleSections();
    if (currentPath.includes('/lp2m')) document.querySelectorAll('a[href*="/Lp2m"]').forEach(a => a.classList.add('active','actif'));

    const mobileBtn = document.getElementById('mobileMenuBtn'), overlay = document.getElementById('mobileOverlay'), drawer = document.getElementById('mobileDrawer'), closeBtn = document.getElementById('closeDrawer');
    function openDrawer() { drawer?.classList.add('show'); overlay?.classList.add('show'); document.body.classList.add('sidebar-open'); document.body.style.overflow = 'hidden'; }
    function closeDrawer() { drawer?.classList.remove('show'); overlay?.classList.remove('show'); document.body.classList.remove('sidebar-open'); document.body.style.overflow = ''; }
    mobileBtn?.addEventListener('click', openDrawer); overlay?.addEventListener('click', closeDrawer); closeBtn?.addEventListener('click', closeDrawer); document.querySelectorAll('.gh-mobile-drawer a,.mobile-bottom-nav a,.lp2m-link').forEach(a => a.addEventListener('click', closeDrawer));
    document.querySelectorAll('.alert-dismissible').forEach(alert => setTimeout(() => { const bsAlert = bootstrap.Alert.getInstance(alert); if (bsAlert) bsAlert.close(); else alert.classList.remove('show'); }, 5000));
});
