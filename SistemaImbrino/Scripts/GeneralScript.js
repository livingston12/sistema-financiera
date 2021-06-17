

$(document).ready(function () {	
	$(".select").selectMania({
		width: '100%',
		size: 'small',
		search: true

	});
	$(".dataTable").DataTable();	
	

	$('.datepicker').datepicker({
		onSelect: function (date, datepicker) {
			ValidarFechas(this, date, datepicker);
		},
		dateFormat: 'dd-mm-yy',
		monthNames: nameMonthComplete()
	});
});
// insertar valor fecha
function ValidarFechas(currentInput,date, datepicker)
{
	
	if (date.length !== 0) {
		var indexcurrentMonth = Number(datepicker.currentMonth);
		var currentDay = datepicker.currentDay;
		var currentYear = datepicker.currentYear;
		var currentDate = currentDay + "-" + monthName(indexcurrentMonth) + "-" + currentYear;
		$(currentInput).val(currentDate);
		validarTexto(currentInput);
		
	}


}

// Llenar drowdowns	
function llenarDropdows(data, IdObject, texto, addFirt = true,isSelectMania = true) {
	
	var IdDw = "#" + IdObject;
	$(IdDw).empty();
	if (addFirt)
	{
		$(IdDw).append($('<option />', {
			text: '-- Selecciona un ' + texto + ' --',
			value: 0
		}));
	}
	$(data).each(function (index, element) {
		$(IdDw).append($('<option />', {
			text: element.value,
			value: element.id
		}));
	});
	if (isSelectMania)
	{
		$(IdDw).selectMania('update');
	}
}

// update select mania
function updateSelectMania(id)
{
	$("#" + id).selectMania('update');
} 

// Validar Texto
function validarTexto(input)
{
	var currentinput = $(input);
	if (currentinput.hasClass("notValidate"))
		return;
	
	var texto = currentinput.val().trim();
	var inputLenth = texto.length;
	var is_tel = currentinput.hasClass("telefono");
	var is_ced = currentinput.hasClass("cedula");
	var currentDiv = $("#dv" + currentinput.attr("id"));
	var currentSpan = $("#span" + currentinput.attr("id"));

	if (is_ced || is_tel)
	{
		texto = texto.replaceAll("-", "");
		currentinput.val("");
		
		var len = 11;		
		len = is_tel ? 10 : len;
		
		if (texto.length !== len)
		{	
			currentDiv.removeClass("has-success");
			currentDiv.addClass("has-error");
			currentSpan.removeClass("glyphicon-ok");
			currentSpan.addClass("glyphicon-remove");
			return;
		}
		if (is_ced) {
			texto = texto.substring(0, 3) + "-" + texto.substring(3, 10) + "-" + texto.substring(10, 11);
		} else if (is_tel)
		{
			texto = texto.substring(0, 3) + "-" + texto.substring(3, 6)+ "-" + texto.substring(6, 10);

		}
		currentinput.val(texto);
	}		
	
	// Validar si el campo acepta texto
	if (currentinput.hasClass("texto") && inputLenth === 0)
		currentinput.val("");

	if (inputLenth === 0) {		
		currentDiv.removeClass("has-success");
		currentDiv.addClass("has-error");
		currentSpan.removeClass("glyphicon-ok");
		currentSpan.addClass("glyphicon-remove");

	} else {
		currentDiv.removeClass("has-error");
		currentDiv.addClass("has-success");
		currentSpan.removeClass("glyphicon-remove");
		currentSpan.addClass("glyphicon-ok");
	}
}
// Validar numeros
function validarNumero(input) {

	var currentinput = $(input);
	var number = currentinput.val().replace(",", "");
	number = number === "" || number < 0 ? "0" : number;

	if (currentinput.hasClass("notValidate"))
		return;

	// Validar si el campo acepta solo numero
	if ( currentinput.hasClass("number") && isNaN(number) || number === 0)
		currentinput.val("");
	else if (currentinput.hasClass("notPoint")) 
		currentinput.val(number_format_js(parseInt(number), 0,0,0));
	else if (currentinput.hasClass("number") && currentinput.hasClass("numberDecimal")   )
		currentinput.val(number_format_js(number, 2));


	var inputLenth = number < 1 ? 0 : currentinput.val().length;
	var currentDiv = $("#dv" + currentinput.attr("id"));
	var currentSpan = $("#span" + currentinput.attr("id"));



	if (inputLenth === 0) {

		currentDiv.removeClass("has-success");
		currentDiv.addClass("has-error");
		currentSpan.removeClass("glyphicon-ok");
		currentSpan.addClass("glyphicon-remove");

	} else {
		currentDiv.removeClass("has-error");
		currentDiv.addClass("has-success");
		currentSpan.removeClass("glyphicon-remove");
		currentSpan.addClass("glyphicon-ok");
	}

}

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
	else if (monthName === "FEB")
		return 2;
	else if (monthName === "MAR")
		return 3;
	else if (monthName === "ABR")
		return 4;
	else if (monthName === "MAY")
		return 5;
	else if (monthName === "JUN")
		return 6;
	else if (monthName === "JUL")
		return 7;
	else if (monthName === "AGO")
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
function MessageNotification(messsage, isSucces, isButton = false, isButtonError = false,goinTo = "") {
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
			html: messsage,
			showCancelButton: false,
			confirmButtonColor: '#0293b2',
			confirmButtonText: 'OK'
		}).then((result) => {
			if (result.isConfirmed) {
				if (isSucces)
				{
					if (goinTo === "")
						document.location.reload();
					else
						window.location.href = goinTo;
				}				
					
			}
		});

	}

	else {
        Swal.fire({
            position: 'center',
            icon: typeError,
			html: messsage,
            showConfirmButton: isButton,
            timer: 8000
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
				tableReg.rows[i].classList.add("finded");
            }
        } else {
			tableReg.rows[i].style.display = 'none';
			tableReg.rows[i].classList.remove("finded");
        }
    }
}

// formatear valores de numeros 
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

//Agregar item de un array
function AddItemArray2(_value, array, json) {

	// validar si existe
	if (array.length === 0) {
		array.push(json);
	} else {


		if (array.find((element) => element.numCuota === _value)) {

			removeItemArray(array);
			array.push(json);
		} else {
			array.push(json);
		}

		//array.forEach(function (index, key) {
		//Obtener Header key
		//if (index.numCuota === _value) {
		//	removeItemArray(key, array);
		//	//array.push(json);
		//} else {
		//	array.push(json);
		//}



		//});

	}
	return array;




}
//Agregar item de un array
function AddItemArray(_value, array, json) {
	//debugger;
	// validar si existe
	if (array.length === 0) {
		array.push(json);
	} else {
		array.forEach(function (index, key) {
			//Obtener Header key
			if (index.numCuota === _value) {
				removeItemArray(key, array);
				array.push(json);
			} else {
				array.push(json);
			}

		});

	}
	return array;




}


function nameMonthComplete() {
	return ["ENERO", "FEBRERO", "MARZO", "ABRIL", "MAYO", "JUNIO", "JULIO", "AGOSTO", "SEPTIEMBRE", "OCTUBRE", "NOVIEMBRE", "DICIEMBRE"];
}


function sumarDias(fecha, dias) {
	fecha.setDate(fecha.getDate() + dias);
	return fecha;
}




function GetDayOfFormaPago(id) {
	
	var formasPagosDays = new Array(0,30,15,7,1),
		curPago = formasPagosDays[id];
	return curPago;	
}


function enterAsTab() {
	var keyPressed = event.keyCode; // get the Key that is pressed
	if (keyPressed === 13) {
		//case the KeyPressed is the [Enter]
		var inputs = $('input'); // storage a array of Inputs
		var a = inputs.index(document.activeElement);
		//get the Index of Active Element Input inside the Inputs(array)

		if (inputs[a + 1] !== null) {
			// case the next index of array is not null
			var nextBox = inputs[a + 1];
			nextBox.focus(); // Focus the next input element. Make him an Active Element
			event.preventDefault();
		}

		return false;
	}

	else { return keyPressed; }
}


