$(() => {
    $.fn.dataTable.moment("DD/MM/YYYY HH:mm:ss");
    $.fn.dataTable.moment("DD/MM/YYYY");

    $("#test-registers").DataTable({
        // ServerSide Setups
        processing: true,
        serverSide: true,
        // Paging Setups
        paging: true,
        // Ajax Filter
        ajax: {
            url: "/TestRegisters/LoadTable",
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            data: function (d) {
                return JSON.stringify(d);
            }
        },
        // Columns Setups
        columns: [
            { data: "id", searchable: false, orderable: false },
            { data: "name" },
            { data: "firstSurname" },
            { data: "secondSurname" },
            { data: "street" },
            { data: "phone" },
            { data: "zipCode" },
            { data: "country" },
            { data: "notes" },
            {
                data: "creationDate",
                render: function (data, type, row) {
                    // If display or filter data is requested, format the date
                    if (type === "display" || type === "filter") {
                        return moment(data).format("ddd DD/MM/YYYY HH:mm:ss");
                    }
                    // Otherwise the data type requested (`type`) is type detection or
                    // sorting data, for which we want to use the raw date value, so just return
                    // that, unaltered
                    return data;
                }
            }
        ],
        // Column Definitions
        columnDefs: [
            { targets: "date-type", type: "date-eu" }
        ],
        order: []
    });
});
