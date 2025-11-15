// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

$('table[data-table]').DataTable();

$('.EditTables').DataTable({
    "columnDefs": [
        { "orderable": false, "targets": [4,5]}
    ]
});

$('.OtherInfoTables').DataTable({
    "columnDefs": [
        { "orderable": false, "targets": [1] }
    ]
});

$('.MovieActorTable').DataTable({
    "columnDefs": [
        { "orderable": false, "targets": [2] }
    ]
});