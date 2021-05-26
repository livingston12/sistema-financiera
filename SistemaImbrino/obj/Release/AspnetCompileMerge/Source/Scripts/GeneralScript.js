

// Obtener el nombre corto del mes

function monthName(monthNumber) {
	var months = new Array('ENE', 'FEB', 'MAR', 'ABR', 'MAY', 'JUN', 'JUL', 'AGO', 'SEP', 'OCT', 'NOV', 'DIC'),
		curMonth = months[monthNumber];
	return curMonth;

}

// Obtener el numero  del mes

function monthNumber(monthName) {
	
	if (monthName === "ENE")
		return 1;
	else if (monthName === "ENE")
		return 2;
	else if (monthName === "FEB")
		return 3;
	else if (monthName === "MAR")
		return 4;
	else if (monthName === "ABR")
		return 5;
	else if (monthName === "MAY")
		return 6;
	else if (monthName === "JUN")
		return 7;
	else if (monthName === "JUL")
		return 8;
	else if (monthName === "SEP")
		return 9;
	else if (monthName === "OCT")
		return 10;
	else if (monthName === "NOV")
		return 11;
	else if (monthName === "DIC")
		return 12;
	

}

// Abrir Cerrar modal
function Show_closeModal(id, isShow) {
    Modal = $(id);
    if (isShow) {
        Modal.modal('show');
    } else {
        Modal.modal('hide');
    }
}


// popup de mensajes

function MessageNotification(messsage, isSucces,isButton = false) {
    var typeError = "";
    if (isSucces === true) {
        typeError = "success";
    }
    else {
        typeError = "error";
    }
    if (isButton) {
       
        swal.fire({
            position: 'center',
            icon: typeError,
            title: messsage,
            showCancelButton: false,
            confirmButtonColor: '#f0ad4e',            
            confirmButtonText: 'OK'
        }).then((result) => {
            if (result.isConfirmed) {
                document.location.reload();
            }
        })

    } else {
        Swal.fire({
            position: 'center',
            icon: typeError,
            title: messsage,
            showConfirmButton: isButton,
            timer: 3500
        });
    }
  

}

function doSearch(tableId, searchID) {
    var tableReg = document.getElementById(tableId);
    var searchText = document.getElementById(searchID).value.toLowerCase();
    for (var i = 1; i < tableReg.rows.length; i++) {
        var cellsOfRow = tableReg.rows[i].getElementsByTagName('td');
        var found = false;
        var found2 = false;
        var isVisible = tableReg.rows[i].getAttribute('isVisible');

        for (var j = 0; j < cellsOfRow.length && !found; j++) {
            var compareWith = cellsOfRow[j].innerHTML.toLowerCase();
            if (searchText.length === 0) {
                found2 = true;
            }
            if (compareWith.indexOf(searchText) > -1) {
                found = true;
            }
        }
        if (found) {
            if (found2 && i % 2 === 0 || isVisible === '0') {
                tableReg.rows[i].style.display = 'none';
            } else {
                tableReg.rows[i].style.display = 'table-row';
            }
        } else {
            tableReg.rows[i].style.display = 'none';
        }
    }
}

function number_format_js(number, decimals, dec_point, thousands_point) {

    if (number === null || !isFinite(number)) {
        throw new TypeError("number is not valid");
    }

    if (!decimals) {
        var len = number.toString().split('.').length;
        decimals = len > 1 ? len : 0;
    }

    if (!dec_point) {
        dec_point = '.';
    }

    if (!thousands_point) {
        thousands_point = ',';
    }

    number = parseFloat(number).toFixed(decimals);

    number = number.replace(".", dec_point);

    var splitNum = number.split(dec_point);
    splitNum[0] = splitNum[0].replace(/\B(?=(\d{3})+(?!\d))/g, thousands_point);
    number = splitNum.join(dec_point);

    return number;
}


