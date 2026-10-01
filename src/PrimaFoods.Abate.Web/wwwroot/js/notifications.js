const flash = document.querySelector('#flash-messages');

if (flash && (flash.dataset.success || flash.dataset.warning || flash.dataset.error)) {
    const notyf = new Notyf({
        duration: 4000,
        dismissible: true,
        position: { x: 'right', y: 'top' },
        types: [
            { type: 'success', background: '#1cbb8c' },
            { type: 'warning', background: '#fcb92c', icon: false },
            { type: 'error', background: '#dc3545', duration: 8000 }
        ]
    });

    if (flash.dataset.success) {
        notyf.success(flash.dataset.success);
    }

    if (flash.dataset.warning) {
        notyf.open({ type: 'warning', message: flash.dataset.warning, duration: 8000 });
    }

    if (flash.dataset.error) {
        notyf.error(flash.dataset.error);
    }
}
