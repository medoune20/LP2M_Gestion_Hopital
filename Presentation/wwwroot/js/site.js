/* GestionHopital — JavaScript global */

document.addEventListener('DOMContentLoaded', function () {
    const basePath = (document.body?.dataset?.pathBase || '').replace(/\/$/, '');
    const currentPath = location.pathname.toLowerCase();

    function appUrl(path) {
        return (basePath || '') + path;
    }

    function isInternalAbsolute(url) {
        return url && url.startsWith('/') && !url.startsWith('//') && !url.startsWith(basePath + '/');
    }

    function withBasePath(url) {
        if (!basePath || !isInternalAbsolute(url)) return url;
        if (url.startsWith('/gestionbar')) return url;
        if (url.startsWith('/hopital')) return url;
        return basePath + url;
    }

    function link(controller, action, icon, label, extraClass) {
        const href = appUrl('/' + controller + (action ? '/' + action : ''));
        const active = currentPath.includes(('/' + controller).toLowerCase()) ? ' actif active' : '';
        return `<a class="lp2m-link${active} ${extraClass || ''}" href="${href}"><i class="bi ${icon}"></i><span>${label}</span></a>`;
    }

    function section(id, title, html) {
        return `
<button type="button" class="lp2m-nav-section-toggle" data-nav-section="${id}" aria-expanded="true">
  <span class="lp2m-nav-sep">${title}</span><i class="bi bi-chevron-down"></i>
</button>
<div class="lp2m-nav-section" data-nav-section-content="${id}">${html}</div>`;
    }

    function buildMenuHtml() {
        return [
            section('pilotage', 'Pilotage',
                link('Accueil', 'Index', 'bi-speedometer2', 'Dashboard') +
                link('Lp2m', 'Index', 'bi-grid-1x2', 'Administration LP2M')),
            section('patients', 'Patients',
                link('Patient', 'Index', 'bi-person-lines-fill', 'Patients') +
                link('Patient', 'Nouveau', 'bi-person-plus', 'Nouveau patient') +
                '<a class="lp2m-link" href="' + appUrl('/Lp2m#patient') + '"><i class="bi bi-phone"></i><span>Accès patient</span></a>'),
            section('agenda', 'Agenda & file d\'attente',
                link('RendezVous', 'Index', 'bi-calendar3', 'Rendez-vous') +
                link('RendezVous', 'Nouveau', 'bi-calendar-plus', 'Nouveau RDV') +
                link('RendezVous', 'Calendrier', 'bi-calendar-week', 'Calendrier') +
                '<a class="lp2m-link" href="' + appUrl('/Lp2m#file') + '"><i class="bi bi-hourglass-split"></i><span>File d\'attente</span></a>'),
            section('soins', 'Soins',
                link('Consultation', 'Index', 'bi-clipboard2-pulse', 'Consultations') +
                '<a class="lp2m-link" href="' + appUrl('/Lp2m#tele') + '"><i class="bi bi-camera-video"></i><span>Consultation en ligne</span></a>' +
                link('Examen', 'Index', 'bi-droplet-half', 'Examens & analyses')),
            section('hospitalisation', 'Hospitalisation',
                link('Hospitalisation', 'Index', 'bi-hospital-fill', 'Patients hospitalisés') +
                link('Hospitalisation', 'Nouvelle', 'bi-person-check', 'Nouvelle admission') +
                link('Departement', 'Index', 'bi-diagram-3', 'Services & lits')),
            section('finances', 'Finances & analyses',
                '<a class="lp2m-link" href="' + appUrl('/Lp2m#paiements') + '"><i class="bi bi-cash-stack"></i><span>Paiements</span></a>' +
                '<a class="lp2m-link" href="' + appUrl('/Lp2m#compta') + '"><i class="bi bi-journal-text"></i><span>Comptabilité</span></a>' +
                '<a class="lp2m-link" href="' + appUrl('/Accueil/Index') + '"><i class="bi bi-graph-up"></i><span>Statistiques</span></a>' +
                '<a class="lp2m-link" href="' + appUrl('/Lp2m#alertes') + '"><i class="bi bi-bell"></i><span>Alertes</span></a>'),
            section('administration', 'Administration',
                '<a class="lp2m-link" href="' + appUrl('/Lp2m#admin') + '"><i class="bi bi-people"></i><span>Utilisateurs</span></a>' +
                '<a class="lp2m-link" href="' + appUrl('/Lp2m#admin') + '"><i class="bi bi-shield-lock"></i><span>Profils / rôles</span></a>' +
                '<a class="lp2m-link" href="' + appUrl('/Lp2m#admin') + '"><i class="bi bi-buildings"></i><span>Tenants</span></a>' +
                link('Medecin', 'Index', 'bi-person-badge', 'Médecins')),
            section('superadmin', 'Super-admin',
                '<a class="lp2m-link" href="' + appUrl('/Lp2m#admin') + '"><i class="bi bi-person-check"></i><span>Demandes d\'inscription</span></a>' +
                '<a class="lp2m-link" href="' + appUrl('/Lp2m#admin') + '"><i class="bi bi-buildings"></i><span>Tenants</span></a>' +
                '<a class="lp2m-link" href="' + appUrl('/Lp2m#com') + '"><i class="bi bi-envelope"></i><span>Envoi mail</span></a>')
        ].join('');
    }

    function applyGestionBarMenu() {
        document.body.classList.add('lp2m-app');
        document.querySelector('.gh-sidebar')?.classList.add('lp2m-sidebar');
        document.querySelector('.gh-main')?.classList.add('lp2m-main');
        document.querySelector('.gh-topbar')?.classList.add('lp2m-topbar');
        document.querySelector('.gh-bottom-nav')?.classList.add('mobile-bottom-nav');

        const sidebarNav = document.querySelector('.gh-sidebar nav');
        if (sidebarNav) {
            sidebarNav.className = 'lp2m-nav';
            sidebarNav.innerHTML = buildMenuHtml();
        }

        const drawerNav = document.querySelector('.gh-mobile-drawer nav');
        if (drawerNav) {
            drawerNav.className = 'lp2m-nav p-2';
            drawerNav.innerHTML = buildMenuHtml() + '<hr class="border-secondary"><a class="lp2m-link text-danger" href="' + appUrl('/Auth/Deconnexion') + '"><i class="bi bi-box-arrow-right"></i><span>Déconnexion</span></a>';
        }

        const bottomNav = document.querySelector('.gh-bottom-nav');
        if (bottomNav) {
            bottomNav.className = 'mobile-bottom-nav gh-bottom-nav d-lg-none no-print';
            bottomNav.innerHTML =
                '<a href="' + appUrl('/Accueil/Index') + '"><i class="bi bi-speedometer2"></i><span>Accueil</span></a>' +
                '<a href="' + appUrl('/Lp2m') + '" class="mobile-nav-pos"><i class="bi bi-grid-1x2-fill"></i><span>LP2M</span></a>' +
                '<a href="' + appUrl('/Patient/Index') + '"><i class="bi bi-person-lines-fill"></i><span>Patients</span></a>' +
                '<a href="' + appUrl('/RendezVous/Index') + '"><i class="bi bi-calendar3"></i><span>RDV</span></a>' +
                '<a href="' + appUrl('/Consultation/Index') + '"><i class="bi bi-clipboard2-pulse"></i><span>Soins</span></a>';
        }
    }

    function fixLegacyLinks() {
        document.querySelectorAll('a[href]').forEach(function (a) {
            const href = a.getAttribute('href');
            if (isInternalAbsolute(href)) a.setAttribute('href', withBasePath(href));
        });
        document.querySelectorAll('form[action]').forEach(function (form) {
            const action = form.getAttribute('action');
            if (isInternalAbsolute(action)) form.setAttribute('action', withBasePath(action));
        });
    }

    function initCollapsibleSections() {
        const storageKey = 'lp2m.sante.sidebar.collapsed.sections';
        let collapsed = [];
        try { collapsed = JSON.parse(localStorage.getItem(storageKey) || '[]'); } catch (e) { collapsed = []; }

        function setCollapsed(name, value) {
            const content = document.querySelector('[data-nav-section-content="' + name + '"]');
            const button = document.querySelector('[data-nav-section="' + name + '"]');
            if (!content || !button) return;
            content.classList.toggle('is-collapsed', value);
            button.classList.toggle('is-collapsed', value);
            button.setAttribute('aria-expanded', value ? 'false' : 'true');
        }

        document.querySelectorAll('[data-nav-section]').forEach(function (button) {
            const name = button.getAttribute('data-nav-section');
            const content = document.querySelector('[data-nav-section-content="' + name + '"]');
            const hasActive = content && content.querySelector('.lp2m-link.actif, .lp2m-link.active');
            setCollapsed(name, collapsed.indexOf(name) !== -1 && !hasActive);
            button.addEventListener('click', function () {
                const c = document.querySelector('[data-nav-section-content="' + name + '"]');
                const next = c && !c.classList.contains('is-collapsed');
                setCollapsed(name, next);
                const list = [];
                document.querySelectorAll('[data-nav-section].is-collapsed').forEach(function (b) { list.push(b.getAttribute('data-nav-section')); });
                localStorage.setItem(storageKey, JSON.stringify(list));
            });
        });
    }

    applyGestionBarMenu();
    fixLegacyLinks();
    initCollapsibleSections();

    if (currentPath.includes('/lp2m')) {
        document.querySelectorAll('a[href*="/Lp2m"]').forEach(function (a) { a.classList.add('active', 'actif'); });
    }

    // Mobile menu
    const mobileBtn = document.getElementById('mobileMenuBtn');
    const overlay = document.getElementById('mobileOverlay');
    const drawer = document.getElementById('mobileDrawer');
    const closeBtn = document.getElementById('closeDrawer');

    function openDrawer() { drawer?.classList.add('show'); overlay?.classList.add('show'); document.body.classList.add('sidebar-open'); document.body.style.overflow = 'hidden'; }
    function closeDrawer() { drawer?.classList.remove('show'); overlay?.classList.remove('show'); document.body.classList.remove('sidebar-open'); document.body.style.overflow = ''; }

    mobileBtn?.addEventListener('click', openDrawer);
    overlay?.addEventListener('click', closeDrawer);
    closeBtn?.addEventListener('click', closeDrawer);
    document.querySelectorAll('.gh-mobile-drawer a, .mobile-bottom-nav a, .lp2m-link').forEach(function (a) { a.addEventListener('click', closeDrawer); });

    document.querySelectorAll('.alert-dismissible').forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = bootstrap.Alert.getInstance(alert);
            if (bsAlert) bsAlert.close(); else alert.classList.remove('show');
        }, 5000);
    });

    const clockEl = document.getElementById('gh-clock');
    if (clockEl) {
        function updateClock() { clockEl.textContent = new Date().toLocaleTimeString('fr-FR'); }
        updateClock();
        setInterval(updateClock, 1000);
    }
});
