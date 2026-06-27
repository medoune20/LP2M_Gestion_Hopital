/* LP2M SANTÉ — JavaScript global */

document.addEventListener('DOMContentLoaded', function () {

    // Auto-dismiss des alertes Bootstrap après 5s
    document.querySelectorAll('.alert-dismissible').forEach(function (el) {
        setTimeout(function () {
            var bsAlert = typeof bootstrap !== 'undefined' ? bootstrap.Alert.getInstance(el) : null;
            if (bsAlert) bsAlert.close();
            else { el.style.transition = 'opacity .4s'; el.style.opacity = '0'; setTimeout(function(){ el.remove(); }, 400); }
        }, 5000);
    });

    // Horloge temps réel (dashboard)
    var clockEl = document.getElementById('gh-clock');
    if (clockEl) {
        function majHorloge() { clockEl.textContent = new Date().toLocaleTimeString('fr-FR'); }
        majHorloge();
        setInterval(majHorloge, 1000);
    }

});
