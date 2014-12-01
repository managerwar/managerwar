const MIN_REFRESH_INTERVAL = 3000;

const MAX_REFRESH_INTERVAL = 20000;

var _rounds;

var _roundID;

var _roundIndexByID;

var _lastResponseTime;

var _requestInProgress;

var _currentRefreshInterval;

function handleResponse(data) {
    var result = data["result"];
    if (result != null) {
        var r = result["rounds"];
        if (r != null) {
            _rounds = r;
            if (_rounds.length > 0) {
                $("#main").show();
                _roundIndexByID = new Object();
                for (var i = 0; i < _rounds.length; i++) {
                    _roundIndexByID[_rounds[i]["id"]] = i;
                }
                if (_roundID == -1) {
                    _roundID = _rounds[0]["id"];
                }
                refreshRound();
            }
        }
    }
}

function refreshPayment(element, payment, managerBalance, othermanagerBalance, jackpot, status) {
    var tomanager = payment[1];
    var fee = payment[2];
    var feeString = satoshiToBTCString(fee, true);
    var feeRateInt = parseInt(payment[3] * 1000);
    var feeRateString = (parseFloat(feeRateInt) / 1000).toString();
    while (feeRateString.length < 4) {
        feeRateString = "0" + feeRateString;
    }
    feeString += " (" + feeRateString + " %)"; //feeRateString.substr(0, feeRateString.length - 4) + "." + feeRateString.substr(feeRateString.length - 4, 4) + " %)";
    var type = payment[5];
    if (type == 0 || type == 2 || type == 3) {
        // unconfirmed or too late
        element.find(".unconfirmed").show();
        element.find(".confirmed").hide();
        if (type == 0) {
            element.find(".unconfirmed .comment").html("unconfirmed");
        }
        else if (type == 2) {
            element.find(".unconfirmed .comment").html("confirmed too late - returned to sender");
        }
        else if (type == 3) {
            element.find(".unconfirmed .comment").html("Deposited after round finished.");
        }
        if (status == 3) {
            element.find(".unconfirmed .values").hide();
        }
        else {
            element.find(".unconfirmed .values").show();
        }
    }
    else {
        element.find(".confirmed").show();
        element.find(".unconfirmed").hide();
        var share = tomanager / managerBalance;
        element.find(".share .value").html(decimalNumberToString(share * 100, 1));
        element.find(".estimate .label").html(status == 3 ? "result: " : "status: ");
        var estimate = "?";
        if (managerBalance > othermanagerBalance) {
            estimate = satoshiToBTCString(tomanager + share * (othermanagerBalance + jackpot)) + " USD payout";
        }
        else if (othermanagerBalance > managerBalance) {
            estimate = "LOSS";
        }
        element.find(".estimate .value").html(estimate);
    }
    element.find(".head .value").html(satoshiToBTCString(tomanager + fee, true) + " USD");
    element.find(".head .addr").html(payment[0]);
    element.find(".tomanager .value").html(satoshiToBTCString(tomanager));
    element.find(".fee .value").html(feeString);
}

function refreshmanager(index, data, status, standing, othermanagerBalance, jackpot) {
    var id = "#manager" + index + "summary";
    var addr = data["a"];
    $(id).find(".addr a").html(addr);
    $(id).find(".addrLink").attr("href", "bitcoin:" + addr);
    $(id + " .qrImg").attr("src", "https://blockchain.info/qr?data=" + addr + "&size=200");
    var tomanagerTotal = data["f"];
    showDecimalString($(id + " .balance"), satoshiToBTCString(tomanagerTotal));
    $(id + " .unconfirmed .value").html(satoshiToBTCString(tomanagerTotal + data["u"]));
    for (var i = 0; i < 5; i++) {
        var img = $(id + " .img" + i);
        var smallImg = $("#payments" + index + " .img" + i);
        visible = false;
        switch (i) {
            case 0:
                visible = (standing == 0);
                break;
            case 1:
                visible = (status < 3 && standing == -1);
                break;
            case 2:
                visible = (status < 3 && standing == 1);
                break;
            case 3:
                visible = (status >= 3 && standing == 1);
                break;
            case 4:
                visible = (status >= 3 && standing == -1);
                break;
        }
        if (visible) {
            img.show();
            smallImg.show();
        }
        else {
            img.hide();
            smallImg.hide();
        }
    }
    // payments:
    var uBalance = data["u"];
    if (uBalance > 0) {
        $("#payments" + index + " .unconfirmed").show();
        $("#payments" + index + " .unconfirmed .value").html(satoshiToBTCString(uBalance));
    }
    else {
        $("#payments" + index + " .unconfirmed").hide();
    }
    var payments = data["payments"];
    for (i = 0; i < 10; i++) {
        var element = $("#payments" + index + " .payment" + i);
        var payment = payments[i];
        if (payment != null) {
            element.show();
            refreshPayment(element, payment, tomanagerTotal, othermanagerBalance, jackpot, status);
        }
        else {
            element.hide();
        }
    }
    var more = data["t"] - 10;
    if (more > 0) {
        $("#payments" + index + " .more").show();
        $("#payments" + index + " .more .value").html(more);
    }
    else {
        $("#payments" + index + " .more").hide();
    }
    $("#payments" + index + " .blockChainLink .addrHref").attr("href", "https://blockchain.info/address/" + addr);
}

function refreshRound() {
    $("#roundHeader .roundID").html("ROUND #" + _roundID + " | ");
    var round = _rounds[_roundIndexByID[_roundID]];
    var status = round["status"];
    for (var i = 1; i < 4; i++) {
        if (status == i) {
            $(".status" + i).show();
        }
        else {
            $(".status" + i).hide();
        }
    }
    switch (status) {
        case 1:
            if (_gameID == 3) {
                refreshTimeValues(new Date().getTime() - _lastResponseTime, 5);
            }
            else {
                refreshTimeValues(new Date().getTime() - _lastResponseTime);
                var cf2 = document.getElementById("cf2");
                if (cf2 != null)
                    cf2.setAttribute("value", _roundID);
                var cf3 = document.getElementById("cf3");
                if (cf3 != null)
                    cf3.setAttribute("value", _roundID);
            }
            break;
        case 2:
            break;
        case 3:
            $("#prizesTx a").attr("href", "https://blockchain.info/tx/" + round["prizesTx"]);
    }
    var jackpot = round["jackpot"];
    var standing0;
    var standing1;
    var addresses = round["addresses"];
    var fBalance0 = addresses[0]["f"];
    var fBalance1 = addresses[1]["f"];
    if (fBalance0 == fBalance1) {
        standing0 = standing1 = 0;
    }
    else if (fBalance0 > fBalance1) {
        standing0 = 1;
        standing1 = -1;
    }
    else {
        standing0 = -1;
        standing1 = 1;
    }
    refreshmanager(0, addresses[0], status, standing0, fBalance1, jackpot);
    refreshmanager(1, addresses[1], status, standing1, fBalance0, jackpot);
    if (getPreviousRound() == null) {
        $("#previousRound").hide();
    }
    else {
        $("#previousRound").show();
    }
    if (getNextRound() == null) {
        $("#nextRound").hide();
    }
    else {
        $("#nextRound").show();
    }
    if (status > 1) {
        if (_gameID == 1) {
            refreshFeeInfo(50);
        }
        else if (_gameID == 3) {
            refreshFeeInfo(5);
        }
    }
    if (jackpot > 0) {
        $("#jackpot").show();
        showDecimalString($("#jackpot .value"), satoshiToBTCString(jackpot), true);
    }
    else {
        $("#jackpot").hide();
    }
    if (status != 1) {
        $(".calculator").hide();
    }
}

function refreshTimeValues(msSinceLastResponse, fixedFee) {
    var round = _rounds[_roundIndexByID[_roundID]];
    $(".needed").hide();
    var status = round["status"];
    if (status == 1) {
        var remainingMS = round["remainingSeconds"] * 1000 - msSinceLastResponse;
        if (remainingMS < 0) {
            remainingMS = 0;
        }
        var remainingSeconds = Math.floor(remainingMS / 1000);
        var elapsedSeconds = (((round["endTime"] - round["startTime"]) * 1000) - remainingMS) / 1000;
        var minutes = Math.floor(remainingSeconds / 60);
        remainingSeconds -= minutes * 60;
        var hours = Math.floor(minutes / 60);
        minutes -= hours * 60;
        $("#remainingHours").html((hours < 10 ? "0" : "") + hours);
        $("#remainingMinutes").html((minutes < 10 ? "0" : "") + minutes);
        $("#remainingSeconds").html((remainingSeconds < 10 ? "0" : "") + remainingSeconds);
        var fee;
        if (fixedFee) {
            fee = fixedFee;
        }
        else {
            if (_gameID == 1) {
                fee = elapsedSeconds * 0.002316;
                if (fee > 50) {
                    fee = 50;
                }
            }
            else if (_gameID == 3) {
                fee = 5;
            }
        }
        refreshFeeInfo(fee);
        var f0 = round["addresses"][0]["f"];
        var f1 = round["addresses"][1]["f"];
        var jackpot = round["jackpot"];
        refreshCalculator(0, status, f0, f1, jackpot, fee);
        refreshCalculator(1, status, f1, f0, jackpot, fee);
        var paymentNeeded;
        if (f0 != f1) {
            $(".needed").show();
            var fd = Math.abs(f0 - f1) + 0.0004;
            var newWinningBalance;
            var newLoosingBalance;
            var needed;
            var winning;
            if (f0 > f1) {
                paymentNeeded = $("#payments1 .paymentNeededBox .payment");
                needed = $("#payments1 .paymentNeededBox");
                winning = $("#payments0 .paymentNeededBox");
                newLoosingBalance = f0;
                newWinningBalance = f1 + fd;
            }
            else {
                paymentNeeded = $("#payments0 .paymentNeededBox .payment");
                needed = $("#payments0 .paymentNeededBox");
                winning = $("#payments1 .paymentNeededBox");
                newLoosingBalance = f1;
                newWinningBalance = f0 + fd;
            }
            winning.css("display", "none");
            needed.css("display", "block");
            needed.find(".fValue").html(satoshiToBTCString(fd));
            var dWithFee = (fd / (1 - fee * 0.01));
            needed.find(".value").html(satoshiToBTCString(dWithFee));
            refreshPayment(paymentNeeded, ["you", fd, dWithFee - fd, fee, 0, 1], newWinningBalance, newLoosingBalance, jackpot, status);
            document.getElementById("divnothing0").setAttribute("style", "display:none");
            document.getElementById("divnothing1").setAttribute("style", "display:none");
        }
        else
        {
            document.getElementById("divnothing0").setAttribute("style", "display:block");
            document.getElementById("divnothing1").setAttribute("style", "display:block");
        }
        var cacheSeconds = round["cacheSeconds"] + Math.floor(msSinceLastResponse / 1000);
        $("#lastRefreshedValue").html(Math.round(msSinceLastResponse / 1000));
        $("#cacheSecondsValue").html(cacheSeconds);
        $("#syncWarning").hide();
        $("#refreshWarning").hide();
        if (cacheSeconds > 150) {
            if (msSinceLastResponse > 40000) {
                $("#refreshWarning").show();
            }
            else {
                $("#syncWarning").show();
            }
        }
    }
}

function refreshFeeInfo(rate) {
    if (rate == 5) {
        // fixed
        showDecimalString($("#currentFeeValue"), decimalNumberToString(rate, 0), true);
        showDecimalString($("#tomanagerValue"), decimalNumberToString(100 - rate, 0), true);
        $("#feeComment").html("The fee is fixed during this round.");
    }
    else {
        showDecimalString($("#currentFeeValue"), decimalNumberToString(rate, 3));
        showDecimalString($("#tomanagerValue"), decimalNumberToString(100 - rate, 3));
        var feeComment = "The fee is slowly raising during this round, it starts at 0 % and ends at " + (_gameID == 1 ? "50" : "100") + " % rate.<br/>Calculate carefully but act quickly! Or perhaps you'd better wait for the next round...";
        /* if (rate == 100) {
         feeComment = "Since this round has finished (at 100 % fee rate), it makes no sense to send any more bitcoins.";
         }
         else if (rate > 50) {
         feeComment = "Yes, the fee is currently very high. You may want to wait for the next round &mdash; we will start again at 0 %!"
         }
         else if (rate > 6) {
         feeComment = "Is the fee too high? Wait for the next round as it will start from 0 % again!"
         }
         else {
         feeComment = "Act quickly - the fee rate is rising slowly.";
         }  */
        $("#feeComment").html(feeComment);
    }
}

function showDecimalString(element, value, omitZeros) {
    var a = value.split(".");
    var intStr = a[0];
    var decStr = "";
    if (a.length > 1) {
        decStr = a[1];
    }
    if (omitZeros) {
        while (decStr.charAt(decStr.length - 1) == "0") {
            decStr = decStr.substr(0, decStr.length - 1);
        }
    }
    if (decStr == "")
        decStr = "00";
    element.find(".int").html(intStr);
    if (decStr.length > 0) {
        element.find(".dot").show();
        element.find(".dec").show();
        element.find(".dec").html(decStr);
    }
    else {
        element.find(".dot").hide();
        element.find(".dec").hide();
    }
}

function decimalNumberToString(value, decPlaces) {
    var n = 1;
    for (var i = 0; i < decPlaces; i++) {
        n = n * 10;
    }
    value = Math.round(value * n);
    var s = value.toString();
    while (s.length < (decPlaces + 1)) {
        s = "0" + s;
    }
    var l = s.length;
    return s.substr(0, l - decPlaces) + "." + s.substr(l - decPlaces, decPlaces);
}

function satoshiToBTCString(satoshi, omitZeros) {
    satoshi = Math.floor(satoshi * 10000);
    var s = satoshi.toString();
    while (s.length < 5) {
        s = "0" + s;
    }
    var l = s.length;
    s = s.substr(0, l - 4) + "." + s.substr(l - 4, 4);
    if (omitZeros) {
        while (s.charAt(s.length - 1) == "0" || s.charAt(s.length - 1) == ".") {
            s = s.substr(0, s.length - 1);
        }
    }
    return s;
}

function getPreviousRound() {
    var r;
    var i = _roundIndexByID[_roundID];
    if (i == null) {
        r = null;
    }
    else {
        if (i < _rounds.length - 1) {
            r = _rounds[i + 1];
        }
        else {
            r = null;
        }
    }
    return r;
}

function getNextRound() {
    var r;
    var i = _roundIndexByID[_roundID];
    if (i == null) {
        r = _rounds[_rounds.length - 1];
    }
    else {
        if (i == 0) {
            r = null;
        }
        else {
            r = _rounds[i - 1];
        }
    }
    return r;
}

function goToPreviousRound() {
    _roundID = getPreviousRound()["id"];
    refreshRound();
}

function goToNextRound() {
    _roundID = getNextRound()["id"];
    refreshRound();
}

function tick() {
    var msSinceLastResponse = new Date().getTime() - _lastResponseTime;
    if (msSinceLastResponse > _currentRefreshInterval && !_requestInProgress) {
        if (_currentRefreshInterval < MAX_REFRESH_INTERVAL) {
            _currentRefreshInterval += 1000;
        }
        _requestInProgress = true;
        $.get("Home/GetGameData")
            .done(function (data) {
                _requestInProgress = false;
                _lastResponseTime = new Date().getTime();
                handleResponse(data);
            })
            .fail(function () {

            })
    }
    if (_roundID > 0) {
        if (_gameID == 0) {
            refreshTimeValues(msSinceLastResponse, 2);
        }
        else {
            refreshTimeValues(msSinceLastResponse);
        }
    }
    setTimeout(tick, 250);
}

function refreshCalculator(id, status, managerBalance, othermanagerBalance, jackpot, feeRate) {
    if (status == 1) {
        $(".calculator").show();
        var value = parseFloat($("#payments" + id + " .calcInput").val());
        var payment = $("#payments" + id + " .calculator .payment");
        if (!isNaN(value) && value > 0) {
            payment.show();
            var total = value;
            var fee = total * feeRate / 100;
            var tomanager = total - fee;
            var newmanagerBalance = managerBalance + tomanager;
            refreshPayment(payment, ["you", tomanager, fee, feeRate, 0, 1], newmanagerBalance, othermanagerBalance, jackpot, status);
        }
        else {
            payment.hide();
        }
    }
    else {
        $(".calculator").hide();
    }
}

$(document).ready(function () {
    _currentRefreshInterval = MIN_REFRESH_INTERVAL;
    _lastResponseTime = 0;
    _roundID = -1;
    tick();
});
