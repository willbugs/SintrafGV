
function initialiseUI(uri) {
    $(document).on("click", "#setMensagem",
        function requestPushNotification() {
            var $ctrl = $(this);
            if ($ctrl.is(":checked")) {
                subscribeUser(uri);

            } else {
                unsubscribeUser(uri);
            }
        });
}

function subscribeUser(uri) {
    var isSubscribed = false;
    var messaging = window.firebase.messaging();
    messaging.requestPermission()
        .then(function () {
            messaging.getToken()
                .then(function (currentToken) {
                    if (currentToken) {
                        updateSubscriptionOnServer(currentToken, uri, "0");
                        isSubscribed = true;
                    } else {
                        updateSubscriptionOnServer(currentToken, uri, "1");
                    }
                    $("#setMensagem").prop('checked', isSubscribed);
                })
                .catch(function (err) {
                    isSubscribed = false;
                    updateSubscriptionOnServer(window.currentToken, uri, "1");
                });
        })
        .catch(function (err) {
            console.log('Unable to get permission to notify.', err);
        });
}

function unsubscribeUser(uri) {
    var messaging = window.firebase.messaging();
    messaging.getToken()
        .then(function (currentToken) {
            messaging.deleteToken(currentToken)
                .then(function () {
                    updateSubscriptionOnServer(currentToken, uri, "1");
                })
                .catch(function (err) {
                    console.log('Unable to delete token. ', err);
                });
        })
        .catch(function (err) {
            console.log('Error retrieving Instance ID token. ', err);
        });
}

function updateSubscriptionOnServer(subscription, uri, acao) {

    if (acao === "0") {
        $("#IdToken").val(subscription);
    } else {
        $("#IdToken").val("");
    }
    //var subscriptionDetail = { key: subscription, action: acao };
    //var apiUrl = uri;
    //var dateToSent = subscriptionDetail;
    //$.ajax({
    //    url: apiUrl,
    //    type: 'POST',
    //    data: dateToSent,
    //    cache: true,
    //    dataType: 'json',
    //    success: function (json) { },
    //    error: function (xmlHttpRequest, textStatus, errorThrown) {
    //        console.log('some error occured', textStatus, errorThrown);
    //    },
    //    always: function () {
    //    }
    //});

}