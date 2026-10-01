const flash = document.querySelector('#flash-messages');

if (flash?.dataset.success) {
    const notyf = new Notyf({
        duration: 4000,
        dismissible: true,
        position: { x: 'right', y: 'top' },
        types: [{ type: 'success', background: '#1cbb8c' }]
    });

    notyf.success(flash.dataset.success);
}
