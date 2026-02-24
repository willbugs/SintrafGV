function ChangeOpen(field, tela, urli) {
    const parametro = { campo: field.name, tela: tela, id: field.value };

    switch (tela) {
    case "#formPessoas":
        $("#loading").show();
        $.ajax({ method: "get", url: urli, data: parametro, success: function(data) {
            $("select[id='PROVEDOR']").empty();
            $("select[id='PROVEDOR']").append(data);
            $("#loading").hide();
        } });
        $("#loading").hide();

        break;
    }
}

function onClickXFecha(data) {
    window.$(".sub-menu-list li").removeClass("active");
    window.$("#"+data).remove();
    return true;
}; 

function Falhou(result) {
    const obj = window.$.parseJSON(result.responseText);
    _erros(obj.message);
}

function ProgressBarModal(showHide) {
    if (showHide === "show") 
    {
        window.$("#mod-progress").modal("show");
        if (arguments.length >= 2) {
            window.$("#progressBarParagraph").text(arguments[1]);
        } else {
            window.$("#progressBarParagraph").text("........");
        }
        window.progressBarActive = true;
    } else 
    {
        window.$("#mod-progress").modal("hide");
        window.progressBarActive = false;
    }
}

function stopRKey(e) {
    var self = $(":focus"), form = self.parents("form:eq(0)");
    var focusable = form.find("input,select,textarea").filter(":visible");
    function enterKey() {
        if (e.which === 13 && !self.is("textarea")) {
            if ($.inArray(self, focusable) && !self.is("a") && !self.is("button")) { e.preventDefault(); }
            focusable.eq(focusable.index(self) + (e.shiftKey ? -1 : 1)).focus();
            return false;
        }
    }
    if (e.shiftKey) { enterKey(); } else { enterKey(); }
}

function errorfields() {
    $("span[class='field-validation-error']").each(function () {
        var inputId = $(this).attr("data-valmsg-for");
        var validationMessage = $(this).html();
        $("#" + inputId).tooltip({ 'trigger': "hover", 'title': validationMessage });
        $("#" + inputId).attr("tooltip", validationMessage);
    });

}

function mascaras() {
    $(".mDATA").mask("99/99/9999");
    $(".mCEP").mask("99999-999");

    var spMaskBehavior = function (val) {
        return val.replace(/\D/g, "").length === 11 ? "(00)00000-0000" : "(00)0000-00009";
    },
        spOptions = {
            onKeyPress: function (val, e, field, options) {
                field.mask(spMaskBehavior.apply({}, arguments), options);
            }
        };
    $("input#TELEFONE").mask(spMaskBehavior, spOptions);
    $(".mTELEFONE").mask(spMaskBehavior, spOptions);
    $(".mCNPJ").mask("99.999.999/9999-99");
    $(".mCPF").mask("999.999.999-99");
    $(".mNUMERO").mask("###.###.###", { reverse: true, maxlength: false });
    $(".mDINHEIRO").mask("#.##0,00", { reverse: true });
    $(".mPERCENTUAL").mask("##0,00", { reverse: true });
    $(".mCARTAO").mask("9999-9999-9999-9999");
    $(".mVALIDADE").mask("99/99");
//    $(".mSINDICATO").mask("9999999A");
    $(".mCVV").mask("999");
    //$(".mSINDICATO").mask("9.999.999-A");
    tmpImage($("#TempFoto").val());
    $("#botaosalvar").mouseover(function () { $(".alert").remove(); });
}

function float2moeda(num) {
    var x = 0;
    if (num < 0) { num = Math.abs(num); x = 1; }
    if (isNaN(num)) num = "0";
    var cents = Math.floor((num * 100 + 0.5) % 100);
    num = Math.floor((num * 100 + 0.5) / 100).toString();
    if (cents < 10) cents = "0" + cents;
    for (var i = 0; i < Math.floor((num.length - (1 + i)) / 3); i++) num = num.substring(0, num.length - (4 * i + 3)) + "." + num.substring(num.length - (4 * i + 3));
    var ret = num + "," + cents;
    if (x === 1) ret = " - " + ret;
    return ret;
}

function moeda2float(moeda) {
    moeda = moeda.replace(".", "");
    moeda = moeda.replace(",", ".");
    return parseFloat(moeda);
}

function gravar(form, posicao) {
    $("body, html").animate({ scrollTop: 0 }, 600);
}

function barra() {
    $(".webgrid-footer a").each(function () { $(this).addClass("btn btn-primary btn-xs"); });
    $(".webgrid-footer td").each(function () {
        $(".webgrid-footer td").attr("id", "_pbarra");
        var cellText = $(this).html();
        var posicao = cellText.split("</a>");
        var numero;
        var linha = 0;
        var ultima = false;
        var j;
        for (j = 0; j < posicao.length; j++) {
            numero = posicao[j].substring(0, posicao[j].indexOf("<a"));
            if (numero !== "" && numero !== " ") {
                linha = j;
                break;
            }
        }
        for (j = 0; j < posicao.length; j++) {
            if (posicao[j].indexOf("&lt;&lt;") > 0) {
                posicao[j] = posicao[j].replace("btn btn-primary btn-xs", "btn btn-primary btn-xs fa fa-fast-backward");
                posicao[j] = posicao[j].replace("&lt;&lt;", " ");
            }
            if (posicao[j].indexOf("&lt;") > 0) {
                posicao[j] = posicao[j].replace("btn btn-primary btn-xs", "btn btn-primary btn-xs fa fa-backward");
                posicao[j] = posicao[j].replace("&lt;", " ");
            }
            if (posicao[j].indexOf("&gt;&gt;") > 0) {
                posicao[j] = posicao[j].replace("btn btn-primary btn-xs", "btn btn-primary btn-xs fa fa-fast-forward");
                posicao[j] = posicao[j].replace("&gt;&gt;", " ");
            }
            if (posicao[j].indexOf("&gt;") > 0) {
                posicao[j] = posicao[j].replace("btn btn-primary btn-xs", "btn btn-primary btn-xs fa fa-forward");
                posicao[j] = posicao[j].replace("&gt;", " ");
            }
        }
        if (numero === "" || numero === " ") {
            var ulinha = posicao.length - 1;
            numero = posicao[ulinha];
            linha = ulinha;
            ultima = true;
        }
        if (ultima === false) {
            posicao[linha] = "<a data-swhglnk= \"true\" class=\"btn btn-success btn-xs\">" + numero.replace(/ /g, "") + "</a>" + posicao[linha].substring(posicao[linha].indexOf("<a"), posicao[linha].length);
        } else {
            posicao[linha] = "<a data-swhglnk= \"true\"class=\"btn btn-success btn-xs\">" + numero.replace(/ /g, "") + "</a>";
        }
        $("#_pbarra").html(posicao);
    });
}

function excluir(id, nclasse, idtela, updater, urli, tipo) {
    $.confirm(
        {
            closeIcon: true,
            closeIconClass: "fa fa-close",
            title: "Atenção",
            content: "Confirmar exclusão do registro ?",
            type: "red",
            typeAnimated: true,
            icon: "fa fa-warning",
            buttons: {
                confirm: {
                    text: "Sim",
                    keys: ["y"],
                    btnClass: "btn-info",
                    action: function () {
                        var parametro = {};
                        if (tipo === "0") {
                            parametro = { id: id, sguid: nclasse };
                        }
                        if (tipo === "1") {
                            parametro = { id: id, nclasse: nclasse, idtela: idtela };
                        }
                        $.ajax({
                            method: "get",
                            url: urli,
                            data: parametro,
                            success: function (result) { $("#" + updater).html(result); }
                        });
                    }
                },
                close: {
                    keys: ["n"],
                    text: "Não",
                    btnClass: "btn-default",
                    action: function () {
                        return true;
                    }
                }
            }
        }
    );
}

function _erros(data) {
    if (data !== null && data !== "") {
        data = data.replace(/(\r\n|\n\r|\r|\n)/g, "<br>");
        window.mensagem = "";
        $.alert({
            title: "Atenção",
            theme: "material",
            content: data,
            icon: "fa fa-warning",
            type: "red"
        });
    }
}

function ChangeGrupo(e, urli, urli1) {
    $.ajax({
        method: "get",
        url: urli,
        data: "id=" + e,
        success: function (result) {
            $.ajax({
                method: "GET",
                url: urli1,
                success: function (data) {
                    $("#boxUsuariosPermissoes").html(data);
                }
            });
        }
    });
}

function _bfechar(urli, id, tnames) {
    var elements = $("#board > *").length;
    if (elements > 1) {
        if (tnames !== null) {
            $.each(tnames, function (i, val) { $(val).remove(); });
        }
        $.ajax({
            method: "get",
            url: urli,
            data: { id: id },
            success: function (result) { $("#Consultas").append(result); }
        });
    } else {
        $(".sub-menu-list li").removeClass("active");
        $("#Page").remove();
        $("#Container").html("");
        superflag = 0;
    }
}

function consulta_controle() {
    var spMaskBehavior = function (val) {
        return val.replace(/\D/g, "").length === 11 ? "(00)00000-0000" : "(00)0000-00009";
    },
        spOptions = {
            onKeyPress: function (val, e, field, options) {
                field.mask(spMaskBehavior.apply({}, arguments), options);
            }
        };

    $("#Ordem").change(function () {
        $("#Chavebusca").unmask();
        var mascara = $("#Ordem").val().split(".")[2];
        if (mascara === "mTELEFONE") { $("#Chavebusca").mask(spMaskBehavior, spOptions); }
        if (mascara === "mDATA") { $("#Chavebusca").mask("00/00/0000"); }
        if (mascara === "mCNPJ") { $("#Chavebusca").mask("99.999.999/9999-99"); }
    });
}

function tmpImage(input) {
    $("#fotoimg").attr("src", input);
}

function readImage(input) {
    if (input.files && input.files[0]) {
        var fr = new FileReader();
        fr.onload = function (e) {
            $("#fotoimg").attr("src", e.target.result);
            $("#TempFoto").attr("value", e.target.result);
        };
        fr.readAsDataURL(input.files[0]);
    }
}

function Sair(urli, urli1) {
    $.confirm(
        {
            closeIcon: true,
            closeIconClass: "fa fa-close",
            title: "Atenção",
            content: "Confirmar saida do sistema ?",
            type: "red",
            typeAnimated: true,
            icon: "fa fa-warning",
            buttons: {
                confirm: {
                    text: "Sim",
                    keys: ["y"],
                    btnClass: "btn-info",
                    action: function () {
                        $.ajax({ method: "get", url: urli });
                        window.location.href = urli1;
                        return false;
                    }
                },
                close: {
                    keys: ["n"],
                    text: "Não",
                    btnClass: "btn-default",
                    action: function () {
                        return true;
                    }
                }
            }
        }
    );
}

function relatchange(url, mid) {
    var mrelatorio = window.$("#Relatorio").val();
    window.$.ajax({
        method: "get",
        url: url,
        data: { relatorio: mrelatorio, id: mid },
        success: function (data) {
            if (mrelatorio !== "") { window.$("#relatfiltros").html(data); }
            if (mrelatorio === "") { window.$("#Relatorios").html(data); }
        }
    });
}

function ChangeCombo(field, tela, urli) {
    const parametro = { campo: field.name, tela: tela, id: field.value };
    switch (tela) {
        case "#formDespesas":
            if (field.name === "FUNCIONARIO") {
                $.ajax({
                    method: "get",
                    url: urli,
                    data: parametro,
                    success: function (data) {
                        const arrayDados = data.split("|");
                        $("#NOME").val(arrayDados[0]);
                        $("#CADSTARITA").val(arrayDados[1]);
                        $("#CPF").val(arrayDados[2]);
                        $("#DATANASCIMENTO").val(arrayDados[3]);
                    }
                });
            }
            break;
    }
}

function verificarpage(tela) {
    tela = tela.substring(8, tela.length);
    var nf = new Intl.NumberFormat("pt-br", { currency: "BRL", minimumFractionDigits: 2, maximumFractionDigits: 2 });
    switch (tela) {
        case "VendasProdutos":
            $.ajax({
                url: "/padrao/Total",
                dataType: "json",
                data: "tela=VendasProdutos",
                success: function (data) {
                    $("#VALORPRODUTOS", "#formVendas").val(nf.format(data));
                    var total = float2moeda(parseFloat(moeda2float($("#VALORPRODUTOS", "#formVendas").val()) + moeda2float($("#VALORENTREGA", "#formVendas").val())).toFixed(2));
                    $("#VALORTOTAL", "#formVendas").val(total);
                }
            });
            break;
        case "ComprasProdutos":
            $.ajax({
                url: "/padrao/Total",
                dataType: "json",
                data: "tela=ComprasProdutos",
                success: function (data) {
                    $("#VALORPRODUTOS", "#formCompras").val(nf.format(data));
                    var total = float2moeda(parseFloat(moeda2float($("#VALORPRODUTOS", "#formCompras").val()) + moeda2float($("#VALORFRETE", "#formCompras").val())).toFixed(2));
                    $("#VALORTOTAL", "#formCompras").val(total);
                }
            });
            break;
    }
}

function Totalizar(tela) {
    $("#subtelas").click(function () {
        var menu = $("#subtelas a.active").attr("href");
        menu = menu.substring(1, menu.length);
        verificarpage(menu);
    });
    var nf = new Intl.NumberFormat("pt-br", { currency: "BRL", minimumFractionDigits: 2, maximumFractionDigits: 2 });
    switch (tela) {
        case "#formProdutos":
            $("#MARGEMLUCRO", tela).change(function () {
                var margem = float2moeda(parseFloat(moeda2float($("#MARGEMLUCRO", tela).val())).toFixed(2));
                var total = float2moeda(parseFloat(moeda2float($("#VALORCOMPRA", tela).val()) + moeda2float($("#VALORFRETE", tela).val())).toFixed(2));
                total = moeda2float(total) * (1 + moeda2float(margem) / 100);
                $("#VALORVENDA", tela).val(nf.format(total));
            });
            $("#MARGEMLUCRO", tela).blur(function () {
                var margem = float2moeda(parseFloat(moeda2float($("#MARGEMLUCRO", tela).val())).toFixed(2));
                var total = float2moeda(parseFloat(moeda2float($("#VALORCOMPRA", tela).val()) + moeda2float($("#VALORFRETE", tela).val())).toFixed(2));
                total = moeda2float(total) * (1 + moeda2float(margem) / 100);
                $("#VALORVENDA", tela).val(nf.format(total));
            });
            break;
        case "#formVendasProdutos":
            $("#VALORVENDA", tela).change(function () {
                var total = float2moeda(parseFloat(moeda2float($("#QUANTIDADE", tela).val()) * moeda2float($("#VALORVENDA", tela).val())).toFixed(2));
                $("#VALORTOTAL", tela).val(total);
            });
            $("#QUANTIDADE", tela).blur(function () {
                var total = float2moeda(parseFloat(moeda2float($("#QUANTIDADE", tela).val()) * moeda2float($("#VALORVENDA", tela).val())).toFixed(2));
                $("#VALORTOTAL", tela).val(total);
            });
            $("#PRODUTO", tela).blur(function () { TelaVendasAlterarEstocagem(tela, $("#PRODUTO", tela).val()); });
            break;
        case "#formVendas":
            $("#VALORENTREGA", tela).change(function () {
                var total = float2moeda(parseFloat(moeda2float($("#VALORPRODUTOS", tela).val()) + moeda2float($("#VALORENTREGA", tela).val())).toFixed(2));
                $("#VALORTOTAL", tela).val(total);
            });
            $("#VALORENTREGA", tela).blur(function () {
                var total = float2moeda(parseFloat(moeda2float($("#VALORPRODUTOS", tela).val()) + moeda2float($("#VALORENTREGA", tela).val())).toFixed(2));
                $("#VALORTOTAL", tela).val(total);
            });
            break;
        case "#formComprasProdutos":
            $("#VALORCOMPRA", tela).change(function () {
                var total = float2moeda(parseFloat(moeda2float($("#QUANTIDADE", tela).val()) * moeda2float($("#VALORCOMPRA", tela).val())).toFixed(2));
                $("#VALORTOTAL", tela).val(total);
            });
            $("#QUANTIDADE", tela).blur(function () {
                var total = float2moeda(parseFloat(moeda2float($("#QUANTIDADE", tela).val()) * moeda2float($("#VALORCOMPRA", tela).val())).toFixed(2));
                $("#VALORTOTAL", tela).val(total);
            });
            break;
        case "#formCompras":
            $("#VALORFRETE", tela).change(function () {
                var total = float2moeda(parseFloat(moeda2float($("#VALORPRODUTOS", tela).val()) + moeda2float($("#VALORFRETE", tela).val())).toFixed(2));
                $("#VALORTOTAL", tela).val(total);
            });
            $("#VALORFRETE", tela).blur(function () {
                var total = float2moeda(parseFloat(moeda2float($("#VALORPRODUTOS", tela).val()) + moeda2float($("#VALORFRETE", tela).val())).toFixed(2));
                $("#VALORTOTAL", tela).val(total);
            });
            break;
    }
}

function TelaVendasAlterarEstocagem(tela, idproduto) {
    $.ajax({
        url: "/padrao/VendasProdutosEstocagem",
        dataType: "json",
        data: "id=" + $("#PRODUTO", tela).val(),
        success: function (data) {
            var drop = $("#ESTOCAGEM", tela);
            drop.empty();
            $.each(data,
                function (index, item) {
                    var p = new Option(item.Text, item.Value);
                    drop.append(p);
                });
        }
    });
}