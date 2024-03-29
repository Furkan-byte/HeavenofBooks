﻿var dataTable;

$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("inprocess")) {
        loadDataTable("inprocess");
    }
    else
    {
        if (url.includes("completed")) {
            loadDataTable("completed");
        }
        else
        {
            if (url.includes("pending")) {
                loadDataTable("pending");
            }
            else
            {
                if (url.includes("approved")) {
                    loadDataTable("approved");
                }
                else {
                    if (url.includes("paymentdelayed")) {
                        loadDataTable("paymentdelayed");
                    }
                    else {
                        loadDataTable("all");
                    }
                }              
            }
        }
    }
});

function loadDataTable(status)
{
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Order/GetAll?status=" + status
        },
        "columns": [
            { "data": "id", "width": "5%" },
            { "data": "name", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "appUser.email", "width": "15%" },
            { "data": "orderStatus", "width": "15%" },
            { "data": "orderTotal", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                         <div class="w-25 btn-group" role="group">
                        <a href="/Admin/Order/Details?orderId=${data}"
                        class="btn btn-primary mx-1"><i class="bi bi-pen"></i></a>
                    </div>
                    
                                `
                },
                "width" : "15%"
            },
        ]
    });
}
