/* GestionHopital — JavaScript global */

document.addEventListener('DOMContentLoaded', function () {
    const basePath = (document.body?.dataset?.pathBase || '').replace(/\/$/, '');

    function isInternalAbsolute(url) {
        return url && url.startsWith('/') && !url.startsWith('//') && !url.startsWith(basePath + '/');
    }

    function withBasePath(url) {
        if (!basePath || !isInternalAbsolute(url)) return url;
        if (url.startsWith('/gestionbar')) return url;
        if (url.startsWith('/hopital')) return url;
        return basePath + url;
    }

    // Sécurise les anciens liens/formulaires écrits en dur (/Patient, /RendezVous/Nouveau, etc.).
    // Cela évite les écrans iPhone de type « télécharger Nouveau / Connexion » quand l'app est hébergée sous /hopital.
    document.querySelectorAll('a[href]').forEach(function (a) {
        const href = a.getAttribute('href');
        if (isInternalAbsolute(href)) a.setAttribute('href', withBasePath(href));
    });

    document.querySelectorAll('form[action]').forEach(function (form) {
        const action = form.getAttribute('action');
        if (isInternalAbsolute(action)) form.setAttribute('action', withBasePath(action));
    });

    // Mobile menu
    const mobileBtn = document.getElementById('mobileMenuBtn');
    const overlay = document.getElementById('mobileOverlay');
    const drawer = document.getElementById('mobileDrawer');
    const closeBtn = document.getElementById('closeDrawer');

    function openDrawer() {
        drawer?.classList.add('show');
        overlay?.classList.add('show');
        document.body.style.overflow = 'hidden';
    }
    function closeDrawer() {
        drawer?.classList.remove('show');
        overlay?.classList.remove('show');
        document.body.style.overflow = '';
    }

    mobileBtn?.addEventListener('click', openDrawer);
    overlay?.addEventListener('click', closeDrawer);
    closeBtn?.addEventListener('click', closeDrawer);
    document.querySelectorAll('.gh-mobile-drawer a, .gh-bottom-nav a').forEach(function (a) {
        a.addEventListener('click', closeDrawer);
    });

    // Auto-dismiss alerts
    document.querySelectorAll('.alert-dismissible').forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = bootstrap.Alert.getInstance(alert);
            if (bsAlert) bsAlert.close();
            else alert.classList.remove('show');
        }, 5000);
    });

    // Horloge temps réel dans le dashboard
    const clockEl = document.getElementById('gh-clock');
    if (clockEl) {
        function updateClock() {
            const now = new Date();
            clockEl.textContent = now.toLocaleTimeString('fr-FR');
        }
        updateClock();
        setInterval(updateClock, 1000);
    }
});
