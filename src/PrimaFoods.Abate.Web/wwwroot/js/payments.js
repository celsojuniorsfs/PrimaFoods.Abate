const animalsTable = document.querySelector('#animals-table');

if (animalsTable) {
    const dataTable = new DataTable(animalsTable, {
        pageLength: 25,
        order: [[0, 'asc']],
        scrollX: true,
        language: {
            decimal: ',',
            thousands: '.',
            search: 'Pesquisar:',
            lengthMenu: '_MENU_ animais por página',
            info: 'Mostrando _START_ a _END_ de _TOTAL_ animais',
            infoEmpty: 'Nenhum animal encontrado',
            infoFiltered: '(filtrado de _MAX_ animais)',
            zeroRecords: 'Nenhum animal encontrado',
            paginate: { first: 'Primeira', previous: 'Anterior', next: 'Próxima', last: 'Última' }
        }
    });

    document.querySelector('#animals-tab')
        ?.addEventListener('shown.bs.tab', () => dataTable.columns.adjust());
}